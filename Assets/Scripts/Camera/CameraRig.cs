using UnityEngine;

namespace GeoGame3D.Camera
{
    /// <summary>
    /// Smooth camera follow system with banking effects for aircraft
    /// </summary>
    public class CameraRig : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target;

        [Header("Follow Settings")]
        [SerializeField] private Vector3 offset = new Vector3(0f, 5f, -15f);
        [SerializeField] private float followSpeed = 5f;
        [SerializeField] private float rotationSpeed = 4f;
        [SerializeField] private float maxAngularVelocity = 180f; // degrees/s
        [SerializeField] private bool followVelocity = true; // Follow velocity vector instead of forward

        [Header("Banking Effect")]
        [SerializeField] private bool enableBanking = true;
        [SerializeField] private float bankAmount = 0.3f;

        [Header("Dynamic FOV")]
        [SerializeField] private bool enableDynamicFOV = true;
        [SerializeField] private float baseFOV = 60f;
        [SerializeField] private float maxFOV = 75f;
        [SerializeField] private float fovSpeed = 2f;
        [SerializeField] private float speedThreshold = 100f; // Speed at which FOV starts increasing

        private UnityEngine.Camera cam;
        private GeoGame3D.Aircraft.AircraftController aircraft;
        private Quaternion lastRotation;
        private Rigidbody targetRigidbody;

        private void Awake()
        {
            cam = GetComponent<UnityEngine.Camera>();

            if (cam != null)
            {
                cam.fieldOfView = baseFOV;
            }

            lastRotation = transform.rotation;
        }

        private void Start()
        {
            if (target != null)
            {
                aircraft = target.GetComponent<GeoGame3D.Aircraft.AircraftController>();
                targetRigidbody = target.GetComponent<Rigidbody>();
            }

            if (target == null)
            {
                Debug.LogWarning("CameraRig: No target assigned!");
            }
        }

        private void LateUpdate()
        {
            if (target == null) return;

            UpdatePosition();
            UpdateRotation();

            if (enableDynamicFOV && cam != null && aircraft != null)
            {
                UpdateDynamicFOV();
            }
        }

        private void UpdatePosition()
        {
            // Determine which direction to use for camera positioning
            Quaternion referenceRotation;

            if (followVelocity && targetRigidbody != null && targetRigidbody.linearVelocity.sqrMagnitude > 1f)
            {
                // Use velocity direction (where the aircraft is actually moving)
                Vector3 velocityDirection = targetRigidbody.linearVelocity.normalized;
                referenceRotation = Quaternion.LookRotation(velocityDirection);
            }
            else
            {
                // Fall back to aircraft's forward direction at low speeds
                referenceRotation = target.rotation;
            }

            // Calculate desired position based on reference rotation and offset
            Vector3 desiredPosition = target.position + referenceRotation * offset;

            // Smoothly move camera to desired position
            transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
        }

        private void UpdateRotation()
        {
            // Look at target
            Quaternion desiredRotation = Quaternion.LookRotation(target.position - transform.position);

            // Apply banking effect if enabled
            if (enableBanking)
            {
                // Get the roll angle from the target
                float targetRoll = target.localEulerAngles.z;
                if (targetRoll > 180f) targetRoll -= 360f;

                // Apply a portion of the roll to the camera
                Vector3 bankRotation = new Vector3(0f, 0f, targetRoll * bankAmount);
                desiredRotation *= Quaternion.Euler(bankRotation);
            }

            // Limit rotation speed to prevent camera spinning wildly
            float maxRotation = maxAngularVelocity * Time.deltaTime;
            Quaternion newRotation = Quaternion.RotateTowards(transform.rotation, desiredRotation, maxRotation);

            // Apply smooth interpolation
            transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, rotationSpeed * Time.deltaTime);
            lastRotation = transform.rotation;
        }

        private void UpdateDynamicFOV()
        {
            // Calculate target FOV based on speed
            float speedFactor = Mathf.Clamp01((aircraft.Speed - speedThreshold) / speedThreshold);
            float targetFOV = Mathf.Lerp(baseFOV, maxFOV, speedFactor);

            // Smoothly transition FOV
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, fovSpeed * Time.deltaTime);
        }

        #region Public Methods

        /// <summary>
        /// Set the target to follow
        /// </summary>
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            if (target != null)
            {
                aircraft = target.GetComponent<GeoGame3D.Aircraft.AircraftController>();
                targetRigidbody = target.GetComponent<Rigidbody>();
            }
        }

        #endregion
    }
}
