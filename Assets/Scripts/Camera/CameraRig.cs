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
        [SerializeField] private float rotationSpeed = 3f;

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

        private void Awake()
        {
            cam = GetComponent<UnityEngine.Camera>();

            if (cam != null)
            {
                cam.fieldOfView = baseFOV;
            }
        }

        private void Start()
        {
            if (target != null)
            {
                aircraft = target.GetComponent<GeoGame3D.Aircraft.AircraftController>();
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
            // Calculate desired position based on target's rotation and offset
            Vector3 desiredPosition = target.position + target.rotation * offset;

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

            // Smoothly rotate camera
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSpeed * Time.deltaTime);
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
            }
        }

        #endregion
    }
}
