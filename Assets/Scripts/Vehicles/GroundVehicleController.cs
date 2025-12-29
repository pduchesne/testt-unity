using UnityEngine;

namespace GeoGame3D.Vehicles
{
    /// <summary>
    /// Physics-based ground vehicle controller with raycast suspension
    /// Provides arcade-style driving with terrain following
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class GroundVehicleController : MonoBehaviour
    {
        [Header("Wheel Configuration")]
        [SerializeField] private Transform frontLeftWheel;
        [SerializeField] private Transform frontRightWheel;
        [SerializeField] private Transform rearLeftWheel;
        [SerializeField] private Transform rearRightWheel;
        [SerializeField] private float wheelRadius = 0.35f;
        [SerializeField] private float wheelbase = 2.5f;  // Distance between front and rear axles
        [SerializeField] private float track = 1.8f;      // Distance between left and right wheels

        [Header("Suspension")]
        [SerializeField] private float suspensionDistance = 0.5f;  // How far raycasts go
        [SerializeField] private float suspensionStiffness = 25000f;  // Spring force
        [SerializeField] private float suspensionDamping = 2000f;     // Damping force
        [SerializeField] private float maxGroundDetectionDistance = 100f;  // Max distance for initial ground detection (spawning/falling)
        [SerializeField] private LayerMask terrainLayer;  // What to raycast against

        [Header("Movement")]
        [SerializeField] private float maxSpeed = 30f;  // m/s (~108 km/h)
        [SerializeField] private float acceleration = 15f;  // m/s²
        [SerializeField] private float brakeForce = 30f;  // m/s²
        [SerializeField] private float reverseSpeed = 10f;  // m/s

        [Header("Steering")]
        [SerializeField] private float maxSteerAngle = 35f;  // degrees
        [SerializeField] private float steerSpeed = 3f;  // How fast steering responds

        [Header("Physics")]
        [SerializeField] private float forwardFriction = 0.95f;  // Longitudinal friction (0-1)
        [SerializeField] private float lateralFriction = 0.98f;  // Sideways friction (0-1)
        [SerializeField] private float gravity = 20f;  // Custom gravity
        [SerializeField] private float slopeAlignmentSpeed = 5f;  // How fast to align to terrain

        [Header("Failsafe")]
        [SerializeField] private float failsafeCheckInterval = 1f;  // Check every N seconds
        [SerializeField] private float failsafeRespawnHeight = 1f;  // Height above terrain to respawn
        [SerializeField] private int consecutiveAirborneLimitBeforeRespawn = 100;  // Number of consecutive airborne frames before respawn

        // Physics state
        private Rigidbody rb;
        private WheelState[] wheels;
        private float currentSteerAngle = 0f;

        // Input (set via reflection or public methods)
        private float accelerationInput = 0f;  // -1 to 1
        private float steeringInput = 0f;      // -1 to 1
        private bool brakeInput = false;

        // Failsafe state
        private float lastFailsafeCheckTime = 0f;
        private int consecutiveAirborneFrames = 0;

        // Public properties for HUD
        public float Speed => rb.linearVelocity.magnitude;
        public float SpeedKmh => Speed * 3.6f;
        public float Heading => transform.eulerAngles.y;
        public bool IsBraking => brakeInput;
        public bool IsGrounded => GroundedWheelCount > 0;
        public int GroundedWheelCount { get; private set; }

        private struct WheelState
        {
            public Transform wheelTransform;
            public bool isGrounded;
            public float compression;
            public float previousCompression;
            public Vector3 groundNormal;
            public Vector3 hitPoint;
        }

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();

            // Initialize wheel states
            wheels = new WheelState[4];
            wheels[0].wheelTransform = frontLeftWheel;
            wheels[1].wheelTransform = frontRightWheel;
            wheels[2].wheelTransform = rearLeftWheel;
            wheels[3].wheelTransform = rearRightWheel;

            // Configure rigidbody for ground vehicle
            rb.mass = 1500f;  // kg
            rb.linearDamping = 0.1f;
            rb.angularDamping = 1f;
            rb.useGravity = false;  // We'll apply custom gravity
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        private void FixedUpdate()
        {
            UpdateWheelGroundDetection();
            ApplySuspensionForces();
            ApplyDrivingForces();
            ApplySteering();
            ApplyGroundAlignment();
            ApplyFriction();
            ApplyGravity();
            CheckFailsafeRespawn();
        }

        /// <summary>
        /// Raycast from each wheel position to detect ground
        /// </summary>
        private void UpdateWheelGroundDetection()
        {
            GroundedWheelCount = 0;
            int frameModCheck = Time.frameCount % 60; // Log every 60 frames when debugging

            for (int i = 0; i < wheels.Length; i++)
            {
                if (wheels[i].wheelTransform == null)
                {
                    if (frameModCheck == 0)
                        Debug.LogWarning($"[GroundVehicle] Wheel {i} transform is NULL!");
                    continue;
                }

                Vector3 wheelPos = wheels[i].wheelTransform.position;
                Vector3 rayStart = wheelPos + transform.up * (wheelRadius * 0.5f);
                // Use longer distance to detect ground even when falling/spawning
                float rayDistance = maxGroundDetectionDistance;

                RaycastHit hit;
                if (Physics.Raycast(rayStart, -transform.up, out hit, rayDistance, terrainLayer))
                {
                    wheels[i].isGrounded = true;
                    wheels[i].hitPoint = hit.point;
                    wheels[i].groundNormal = hit.normal;

                    // Calculate compression
                    float hitDistance = hit.distance - wheelRadius;
                    wheels[i].previousCompression = wheels[i].compression;
                    wheels[i].compression = Mathf.Max(0f, suspensionDistance - hitDistance);

                    GroundedWheelCount++;
                }
                else
                {
                    wheels[i].isGrounded = false;
                    wheels[i].compression = 0f;
                    wheels[i].previousCompression = 0f;

                    if (frameModCheck == 0)
                    {
                        Debug.LogWarning($"[GroundVehicle] Wheel {i} raycast MISS: from {rayStart} down {rayDistance}m, terrainLayer={terrainLayer.value}");
                    }
                }
            }

            if (frameModCheck == 0 && GroundedWheelCount == 0)
            {
                Debug.LogWarning($"[GroundVehicle] NO WHEELS GROUNDED! Position: {transform.position}, terrainLayer mask: {terrainLayer.value}");
            }
        }

        /// <summary>
        /// Apply spring and damper forces at each wheel
        /// </summary>
        private void ApplySuspensionForces()
        {
            for (int i = 0; i < wheels.Length; i++)
            {
                if (!wheels[i].isGrounded) continue;

                // Spring force (Hooke's law)
                float springForce = wheels[i].compression * suspensionStiffness;

                // Damping force (velocity-based)
                float compressionVelocity = (wheels[i].compression - wheels[i].previousCompression) / Time.fixedDeltaTime;
                float dampingForce = compressionVelocity * suspensionDamping;

                // Total force in direction of ground normal
                float totalForce = springForce + dampingForce;
                Vector3 suspensionForce = wheels[i].groundNormal * totalForce;

                // Apply force at wheel position
                rb.AddForceAtPosition(suspensionForce, wheels[i].wheelTransform.position);
            }
        }

        /// <summary>
        /// Apply acceleration and braking forces
        /// </summary>
        private void ApplyDrivingForces()
        {
            if (!IsGrounded) return;

            Vector3 forwardDir = transform.forward;
            float currentSpeed = Vector3.Dot(rb.linearVelocity, forwardDir);

            if (brakeInput)
            {
                // Braking
                rb.AddForce(-forwardDir * brakeForce * rb.mass);
            }
            else if (accelerationInput != 0f)
            {
                // Acceleration or reverse
                float targetSpeed = accelerationInput > 0 ? maxSpeed : -reverseSpeed;

                // Only apply force if we're below target speed
                if (Mathf.Abs(currentSpeed) < Mathf.Abs(targetSpeed))
                {
                    Vector3 driveForce = forwardDir * (accelerationInput * acceleration * rb.mass);
                    rb.AddForce(driveForce);
                }
            }
        }

        /// <summary>
        /// Apply steering by rotating the vehicle
        /// </summary>
        private void ApplySteering()
        {
            if (!IsGrounded) return;

            // Smooth steering angle
            float targetSteerAngle = steeringInput * maxSteerAngle;
            currentSteerAngle = Mathf.Lerp(currentSteerAngle, targetSteerAngle, Time.fixedDeltaTime * steerSpeed);

            // Convert speed to angular velocity
            float speed = rb.linearVelocity.magnitude;
            if (speed > 1f)  // Only steer when moving
            {
                float turnRadius = wheelbase / Mathf.Tan(currentSteerAngle * Mathf.Deg2Rad);
                float angularVelocity = speed / turnRadius;

                rb.angularVelocity = new Vector3(0f, angularVelocity, 0f);
            }
        }

        /// <summary>
        /// Align vehicle rotation to terrain slope
        /// </summary>
        private void ApplyGroundAlignment()
        {
            if (GroundedWheelCount == 0) return;

            // Average ground normal from all grounded wheels
            Vector3 averageNormal = Vector3.zero;
            int count = 0;

            for (int i = 0; i < wheels.Length; i++)
            {
                if (wheels[i].isGrounded)
                {
                    averageNormal += wheels[i].groundNormal;
                    count++;
                }
            }

            if (count > 0)
            {
                averageNormal /= count;
                averageNormal.Normalize();

                // Calculate target rotation aligned with ground
                Quaternion targetRotation = Quaternion.FromToRotation(transform.up, averageNormal) * transform.rotation;

                // Smoothly rotate towards target
                rb.MoveRotation(Quaternion.Slerp(
                    rb.rotation,
                    targetRotation,
                    Time.fixedDeltaTime * slopeAlignmentSpeed
                ));
            }
        }

        /// <summary>
        /// Apply friction to slow down the vehicle
        /// </summary>
        private void ApplyFriction()
        {
            if (!IsGrounded) return;

            Vector3 velocity = rb.linearVelocity;

            // Forward friction (along vehicle forward axis)
            Vector3 forwardVel = Vector3.Project(velocity, transform.forward);
            rb.linearVelocity -= forwardVel * (1f - forwardFriction) * Time.fixedDeltaTime * 50f;

            // Lateral friction (along vehicle right axis)
            Vector3 lateralVel = Vector3.Project(velocity, transform.right);
            rb.linearVelocity -= lateralVel * (1f - lateralFriction) * Time.fixedDeltaTime * 50f;
        }

        /// <summary>
        /// Apply custom gravity force
        /// </summary>
        private void ApplyGravity()
        {
            rb.AddForce(Vector3.down * gravity * rb.mass);
        }

        /// <summary>
        /// Failsafe: Check if vehicle has fallen below ground and respawn if needed
        /// </summary>
        private void CheckFailsafeRespawn()
        {
            // Track consecutive airborne frames
            if (!IsGrounded)
            {
                consecutiveAirborneFrames++;
            }
            else
            {
                consecutiveAirborneFrames = 0;
                return;  // Vehicle is grounded, no failsafe needed
            }

            // Only check periodically to reduce performance impact
            if (Time.time - lastFailsafeCheckTime < failsafeCheckInterval)
            {
                return;
            }

            lastFailsafeCheckTime = Time.time;

            // If vehicle has been airborne for too long, attempt respawn
            if (consecutiveAirborneFrames > consecutiveAirborneLimitBeforeRespawn)
            {
                Debug.LogWarning($"[GroundVehicle] FAILSAFE TRIGGERED: Vehicle airborne for {consecutiveAirborneFrames} frames, attempting respawn...");

                // Try raycasting DOWN first to find terrain
                RaycastHit hit;
                Vector3 rayStart = transform.position;
                bool terrainFound = false;

                if (Physics.Raycast(rayStart, Vector3.down, out hit, maxGroundDetectionDistance, terrainLayer))
                {
                    terrainFound = true;
                    Debug.LogWarning($"[GroundVehicle] FAILSAFE: Found terrain BELOW at {hit.point}");
                }
                // If not found below, try raycasting UP (vehicle may have fallen through terrain)
                else if (Physics.Raycast(rayStart, Vector3.up, out hit, maxGroundDetectionDistance, terrainLayer))
                {
                    terrainFound = true;
                    Debug.LogWarning($"[GroundVehicle] FAILSAFE: Found terrain ABOVE at {hit.point}");
                }

                if (terrainFound)
                {
                    // Found terrain, respawn above it
                    Vector3 respawnPos = hit.point + Vector3.up * failsafeRespawnHeight;

                    Debug.LogWarning($"[GroundVehicle] FAILSAFE: Terrain found at {hit.point}, respawning at {respawnPos}");

                    // Make kinematic temporarily to prevent physics interference
                    bool wasKinematic = rb.isKinematic;
                    rb.isKinematic = true;

                    // Set position and level rotation
                    transform.position = respawnPos;
                    Vector3 euler = transform.eulerAngles;
                    transform.rotation = Quaternion.Euler(0f, euler.y, 0f);

                    // Zero velocities
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;

                    // Restore physics
                    rb.isKinematic = wasKinematic;

                    Physics.SyncTransforms();

                    // Reset counter
                    consecutiveAirborneFrames = 0;

                    Debug.LogWarning($"[GroundVehicle] FAILSAFE COMPLETE: Vehicle respawned successfully");
                }
                else
                {
                    Debug.LogError($"[GroundVehicle] FAILSAFE FAILED: No terrain found within {maxGroundDetectionDistance}m below vehicle!");
                    // Reset counter to prevent spam
                    consecutiveAirborneFrames = 0;
                }
            }
        }

        /// <summary>
        /// Debug visualization of wheel positions and raycasts
        /// </summary>
        private void OnDrawGizmos()
        {
            if (wheels == null || wheels.Length == 0) return;

            for (int i = 0; i < wheels.Length; i++)
            {
                if (wheels[i].wheelTransform == null) continue;

                Vector3 wheelPos = wheels[i].wheelTransform.position;

                // Draw wheel sphere
                Gizmos.color = wheels[i].isGrounded ? Color.green : Color.red;
                Gizmos.DrawWireSphere(wheelPos, wheelRadius);

                // Draw raycast line
                Vector3 rayStart = wheelPos + transform.up * (wheelRadius * 0.5f);
                Vector3 rayEnd = rayStart - transform.up * (suspensionDistance + wheelRadius);
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(rayStart, rayEnd);

                // Draw ground contact point and normal
                if (wheels[i].isGrounded)
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawSphere(wheels[i].hitPoint, 0.1f);
                    Gizmos.DrawLine(wheels[i].hitPoint, wheels[i].hitPoint + wheels[i].groundNormal * 0.5f);
                }
            }
        }
    }
}
