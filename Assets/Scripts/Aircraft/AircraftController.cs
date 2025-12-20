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
        [Header("Flight Physics")]
        [SerializeField] private float maxSpeed = 200f; // m/s
        [SerializeField] private float acceleration = 20f; // m/s²
        [SerializeField] private float pitchSpeed = 45f; // degrees/s
        [SerializeField] private float rollSpeed = 90f; // degrees/s
        [SerializeField] private float yawSpeed = 30f; // degrees/s

        [Header("Control Settings")]
        [SerializeField] private float throttleChangeSpeed = 0.5f;
        [SerializeField] private float minThrottle = 0.3f;

        [Header("Physics")]
        [SerializeField] private float drag = 0.3f;
        [SerializeField] private float angularDrag = 0.5f;

        // Input values
        private Vector2 pitchRollInput;
        private float yawInput;
        private float throttleInput;

        // State
        private Rigidbody rb;
        private float currentThrottle = 0.5f;

        // Public properties for HUD
        public float Speed => rb.linearVelocity.magnitude;
        public float Altitude { get; private set; }
        public float Heading => transform.eulerAngles.y;
        public float ThrottlePercent => currentThrottle * 100f;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.useGravity = false; // We'll handle our own physics
            rb.linearDamping = drag;
            rb.angularDamping = angularDrag;
        }

        private void Start()
        {
            // Set initial velocity
            rb.linearVelocity = transform.forward * maxSpeed * currentThrottle;
        }

        private void FixedUpdate()
        {
            UpdateThrottle();
            ApplyMovement();
            ApplyRotation();
            UpdateAltitude();
        }

        private void UpdateThrottle()
        {
            currentThrottle = Mathf.Clamp(currentThrottle + throttleInput * throttleChangeSpeed * Time.fixedDeltaTime, minThrottle, 1f);
        }

        private void ApplyMovement()
        {
            // Calculate target speed based on throttle
            float targetSpeed = maxSpeed * currentThrottle;

            // Apply forward thrust
            Vector3 targetVelocity = transform.forward * targetSpeed;
            Vector3 velocityDelta = targetVelocity - rb.linearVelocity;
            Vector3 force = velocityDelta * acceleration;

            rb.AddForce(force, ForceMode.Acceleration);
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
                // Draw velocity vector
                Gizmos.color = Color.green;
                Gizmos.DrawRay(transform.position, rb.linearVelocity);

                // Draw forward direction
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(transform.position, transform.forward * 50f);
            }
        }

        #endregion
    }
}
