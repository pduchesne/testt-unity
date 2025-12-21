using UnityEngine;
using UnityEngine.InputSystem;

namespace GeoGame3D.Aircraft
{
    /// <summary>
    /// Physics-based aircraft controller for GeoGame3D
    /// Handles pitch, roll, yaw, and throttle controls
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class AircraftController : MonoBehaviour
    {
        [Header("Engine & Thrust")]
        [SerializeField] private float maxThrust = 50000f; // Newtons
        [SerializeField] private float pitchSpeed = 80f; // degrees/s
        [SerializeField] private float rollSpeed = 120f; // degrees/s
        [SerializeField] private float yawSpeed = 40f; // degrees/s

        [Header("Control Settings")]
        [SerializeField] private float throttleChangeSpeed = 0.5f;
        [SerializeField] private float minThrottle = 0.0f;

        [Header("Aerodynamics")]
        [SerializeField] private float wingArea = 35f; // m²
        [SerializeField] private float baseDragCoefficient = 0.25f; // High drag for speed control
        [SerializeField] private float inducedDragFactor = 0.12f; // Drag from lift generation
        [SerializeField] private AnimationCurve liftCurve = AnimationCurve.Linear(-15f, -0.5f, 15f, 1.5f);
        [SerializeField] private float stallAngle = 20f; // degrees
        [SerializeField] private float stallLiftMultiplier = 0.5f; // Lift multiplier when stalled
        [SerializeField] private float airDensity = 1.225f; // kg/m³ at sea level

        [Header("Physics")]
        [SerializeField] private float angularDrag = 0.5f;
        [SerializeField] private float gravity = 9.81f;

        // Input values
        private Vector2 pitchRollInput;
        private float yawInput;
        private float throttleInput;

        // State
        private Rigidbody rb;
        private float currentThrottle = 0.0f; // Start with engines off
        private float previousAltitude;

        // Aerodynamic state
        private float angleOfAttack;
        private bool isStalled;

        // Public properties for HUD
        public float Speed => rb.linearVelocity.magnitude;
        public float Altitude { get; private set; }
        public float Heading => transform.eulerAngles.y;
        public float ThrottlePercent => currentThrottle * 100f;
        public float AngleOfAttack => angleOfAttack;
        public float VerticalSpeed { get; private set; }
        public bool IsStalled => isStalled;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.useGravity = false; // We'll handle our own physics
            rb.linearDamping = 0f; // Drag is calculated in aerodynamics
            rb.angularDamping = angularDrag;

            // Force throttle settings to correct values
            minThrottle = 0.0f;
            currentThrottle = 0.0f;
            Debug.Log($"Aircraft initialized: minThrottle={minThrottle}, currentThrottle={currentThrottle}");

            // Initialize lift curve if not set
            if (liftCurve == null || liftCurve.length == 0)
            {
                liftCurve = new AnimationCurve();
                liftCurve.AddKey(-20f, -1.0f);   // Strong negative lift when inverted
                liftCurve.AddKey(-10f, -0.5f);   // Moderate negative lift
                liftCurve.AddKey(0f, 0.4f);      // Base lift at 0° AOA
                liftCurve.AddKey(5f, 1.2f);      // Good lift at low AOA
                liftCurve.AddKey(12f, 2.2f);     // Peak lift at optimal AOA
                liftCurve.AddKey(20f, 1.5f);     // Reduced lift near stall angle
            }
        }

        private void Start()
        {
            // Set initial velocity
            rb.linearVelocity = transform.forward * 50f; // Start with some speed
            previousAltitude = transform.position.y;
        }

        private void FixedUpdate()
        {
            UpdateThrottle();
            CalculateAerodynamics();
            ApplyAerodynamicForces();
            ApplyRotation();
            UpdateAltitude();
            UpdateVerticalSpeed();
        }

        private void UpdateThrottle()
        {
            float previousThrottle = currentThrottle;
            currentThrottle = Mathf.Clamp(currentThrottle + throttleInput * throttleChangeSpeed * Time.fixedDeltaTime, minThrottle, 1f);

            // Debug logging to track throttle changes
            if (Time.frameCount % 120 == 0 || Mathf.Abs(currentThrottle - previousThrottle) > 0.01f)
            {
                Debug.Log($"Throttle: {currentThrottle:F2} ({currentThrottle*100:F0}%) | Input: {throttleInput:F1} | Min: {minThrottle} | Thrust: {(maxThrust * currentThrottle):F0}N");
            }
        }

        private void CalculateAerodynamics()
        {
            // Calculate angle of attack
            // AOA is the angle between the aircraft's longitudinal axis and the relative wind
            // When nose is UP relative to velocity, localVelocity.y is negative (wind from below)
            // We negate to get positive AOA when nose is up
            Vector3 localVelocity = transform.InverseTransformDirection(rb.linearVelocity);
            angleOfAttack = -Mathf.Atan2(localVelocity.y, localVelocity.z) * Mathf.Rad2Deg;

            // Check for stall condition
            isStalled = Mathf.Abs(angleOfAttack) > stallAngle || Speed < 20f;
        }

        private void ApplyAerodynamicForces()
        {
            float speed = Speed;
            float speedSquared = speed * speed;

            // 1. Thrust force
            Vector3 thrust = transform.forward * maxThrust * currentThrottle;

            // 2. Lift force (perpendicular to velocity and wing span)
            // Lift acts perpendicular to both velocity and the aircraft's right axis (wing span)
            // This gives us a vector pointing "up" relative to the wing
            Vector3 velocityNormalized = rb.linearVelocity.normalized;
            Vector3 liftDirection = Vector3.Cross(velocityNormalized, transform.right).normalized;

            // Get lift coefficient from curve
            float liftCoefficient = liftCurve.Evaluate(angleOfAttack);

            // Apply stall penalty
            if (isStalled)
            {
                liftCoefficient *= stallLiftMultiplier;
            }

            // Lift formula: L = 0.5 * ρ * v² * S * CL
            float liftForce = 0.5f * airDensity * speedSquared * wingArea * liftCoefficient;
            Vector3 lift = liftDirection * liftForce;

            // 3. Drag force (opposite to velocity)
            Vector3 dragDirection = -velocityNormalized;

            // Total drag = base drag + induced drag (related to lift)
            float inducedDrag = inducedDragFactor * Mathf.Abs(liftCoefficient);
            float totalDragCoefficient = baseDragCoefficient + inducedDrag;

            // Drag formula: D = 0.5 * ρ * v² * S * CD
            float dragForce = 0.5f * airDensity * speedSquared * wingArea * totalDragCoefficient;
            Vector3 drag = dragDirection * dragForce;

            // 4. Gravity
            Vector3 gravityForce = Vector3.down * (rb.mass * gravity);

            // Debug logging for forces
            if (Time.frameCount % 120 == 0)
            {
                Debug.Log($"Speed: {speed:F1} m/s ({speed*3.6f:F0} km/h) | Drag: {dragForce:F0}N | Thrust: {thrust.magnitude:F0}N | CD: {totalDragCoefficient:F3}");
            }

            // Apply all forces
            rb.AddForce(thrust + lift + drag + gravityForce);
        }

        private void ApplyRotation()
        {
            // Pitch (nose up/down) - around X axis
            float pitch = -pitchRollInput.y * pitchSpeed * Time.fixedDeltaTime;

            // Roll (bank left/right) - around Z axis
            float roll = -pitchRollInput.x * rollSpeed * Time.fixedDeltaTime;

            // Yaw (turn left/right) - around Y axis
            float yaw = yawInput * yawSpeed * Time.fixedDeltaTime;

            // Apply rotation
            Quaternion deltaRotation = Quaternion.Euler(pitch, yaw, roll);
            rb.MoveRotation(rb.rotation * deltaRotation);
        }

        private void UpdateAltitude()
        {
            // Raycast down to find altitude above terrain
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, 20000f))
            {
                // Altitude is the distance from aircraft to ground
                Altitude = hit.distance;
            }
            else
            {
                // If no terrain hit, raycast up to find if we're below terrain
                if (Physics.Raycast(transform.position, Vector3.up, out hit, 20000f))
                {
                    // We're below terrain - altitude is negative
                    Altitude = -hit.distance;
                }
                else
                {
                    // No terrain in either direction - use absolute height
                    // This handles edge cases where we're very far from any terrain
                    Altitude = Mathf.Max(0f, transform.position.y);
                }
            }
        }

        private void UpdateVerticalSpeed()
        {
            // Calculate vertical speed in m/s
            float currentAltitude = transform.position.y;
            VerticalSpeed = (currentAltitude - previousAltitude) / Time.fixedDeltaTime;
            previousAltitude = currentAltitude;
        }

        #region Input System Callbacks

        public void OnPitchRoll(InputAction.CallbackContext context)
        {
            pitchRollInput = context.ReadValue<Vector2>();
        }

        public void OnYaw(InputAction.CallbackContext context)
        {
            yawInput = context.ReadValue<float>();
        }

        public void OnThrottle(InputAction.CallbackContext context)
        {
            throttleInput = context.ReadValue<float>();
        }

        #endregion

        #region Debug

        private void OnDrawGizmos()
        {
            if (Application.isPlaying && rb != null)
            {
                Vector3 pos = transform.position;

                // Draw velocity vector (green)
                Gizmos.color = Color.green;
                Gizmos.DrawRay(pos, rb.linearVelocity);

                // Draw forward direction (blue)
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(pos, transform.forward * 50f);

                // Draw lift direction (cyan)
                Vector3 liftDir = Vector3.Cross(rb.linearVelocity.normalized, transform.right).normalized;
                Gizmos.color = Color.cyan;
                Gizmos.DrawRay(pos, liftDir * 30f);

                // Draw stall indicator (red when stalled)
                if (isStalled)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(pos, 5f);
                }
            }
        }

        #endregion
    }
}
