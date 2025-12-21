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
        [SerializeField] private float pitchSpeed = 45f; // degrees/s
        [SerializeField] private float rollSpeed = 90f; // degrees/s
        [SerializeField] private float yawSpeed = 30f; // degrees/s

        [Header("Control Settings")]
        [SerializeField] private float throttleChangeSpeed = 0.5f;
        [SerializeField] private float minThrottle = 0.0f;

        [Header("Aerodynamics")]
        [SerializeField] private float wingArea = 20f; // m²
        [SerializeField] private float baseDragCoefficient = 0.02f;
        [SerializeField] private float inducedDragFactor = 0.05f;
        [SerializeField] private AnimationCurve liftCurve = AnimationCurve.Linear(-15f, -0.5f, 15f, 1.5f);
        [SerializeField] private float stallAngle = 15f; // degrees
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
        private float currentThrottle = 0.5f;
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

            // Initialize lift curve if not set
            if (liftCurve == null || liftCurve.length == 0)
            {
                liftCurve = new AnimationCurve();
                liftCurve.AddKey(-15f, -0.5f);
                liftCurve.AddKey(0f, 0f);
                liftCurve.AddKey(15f, 1.5f);
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
            currentThrottle = Mathf.Clamp(currentThrottle + throttleInput * throttleChangeSpeed * Time.fixedDeltaTime, minThrottle, 1f);
        }

        private void CalculateAerodynamics()
        {
            // Calculate angle of attack
            Vector3 localVelocity = transform.InverseTransformDirection(rb.linearVelocity);
            angleOfAttack = Mathf.Atan2(localVelocity.y, localVelocity.z) * Mathf.Rad2Deg;

            // Check for stall condition
            isStalled = Mathf.Abs(angleOfAttack) > stallAngle || Speed < 20f;
        }

        private void ApplyAerodynamicForces()
        {
            float speed = Speed;
            float speedSquared = speed * speed;

            // 1. Thrust force
            Vector3 thrust = transform.forward * maxThrust * currentThrottle;

            // 2. Lift force (perpendicular to velocity, in the "up" direction of the aircraft)
            Vector3 liftDirection = Vector3.Cross(Vector3.Cross(rb.linearVelocity, transform.right), rb.linearVelocity).normalized;
            float liftCoefficient = liftCurve.Evaluate(angleOfAttack);

            // Apply stall penalty
            if (isStalled)
            {
                liftCoefficient *= 0.3f; // Greatly reduced lift when stalled
            }

            // Lift formula: L = 0.5 * ρ * v² * S * CL
            float liftForce = 0.5f * airDensity * speedSquared * wingArea * liftCoefficient;
            Vector3 lift = liftDirection * liftForce;

            // 3. Drag force (opposite to velocity)
            Vector3 dragDirection = -rb.linearVelocity.normalized;

            // Total drag = base drag + induced drag (related to lift)
            float inducedDrag = inducedDragFactor * Mathf.Abs(liftCoefficient);
            float totalDragCoefficient = baseDragCoefficient + inducedDrag;

            // Drag formula: D = 0.5 * ρ * v² * S * CD
            float dragForce = 0.5f * airDensity * speedSquared * wingArea * totalDragCoefficient;
            Vector3 drag = dragDirection * dragForce;

            // 4. Gravity
            Vector3 gravityForce = Vector3.down * (rb.mass * gravity);

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
            if (Physics.Raycast(transform.position, Vector3.down, out hit, 10000f))
            {
                Altitude = hit.distance;
            }
            else
            {
                // Fallback to absolute height if no terrain below
                Altitude = transform.position.y;
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
                Vector3 liftDir = Vector3.Cross(Vector3.Cross(rb.linearVelocity, transform.right), rb.linearVelocity).normalized;
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
