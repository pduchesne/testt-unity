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
        [SerializeField] private float followSpeed = 8f;
        [SerializeField] private float rotationSpeed = 6f;
        [SerializeField] private float maxDeviationAngle = 30f; // Max degrees camera can deviate from aircraft axis
        [SerializeField] private float minDistance = 10f; // Minimum distance from aircraft
        [SerializeField] private float centeringSpeed = 2f; // Speed at which camera returns to center when not turning
        [SerializeField] private float centeringThreshold = 20f; // Angular velocity below which centering activates (deg/s)

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
            // Calculate ideal centered position behind aircraft's forward axis
            Quaternion aircraftRotation = target.rotation;
            Vector3 idealCenteredPosition = target.position + aircraftRotation * offset;

            // Check if aircraft is turning (based on angular velocity)
            bool isTurning = targetRigidbody != null && targetRigidbody.angularVelocity.magnitude * Mathf.Rad2Deg > centeringThreshold;

            // Smoothly move toward ideal position
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, idealCenteredPosition, followSpeed * Time.deltaTime);

            // Auto-center when not turning: pull camera back to centerline
            if (!isTurning)
            {
                smoothedPosition = Vector3.Lerp(smoothedPosition, idealCenteredPosition, centeringSpeed * Time.deltaTime);
            }

            // Apply angular constraint: limit deviation from aircraft axis
            Vector3 fromAircraft = smoothedPosition - target.position;
            Vector3 aircraftBackward = -target.forward;

            // Calculate angle between camera direction and aircraft backward axis
            float currentAngle = Vector3.Angle(fromAircraft, aircraftBackward);

            // If exceeding max angle, clamp to max angle
            if (currentAngle > maxDeviationAngle)
            {
                // Project camera position onto a cone around the aircraft's backward axis
                Vector3 constrainedDirection = Vector3.RotateTowards(aircraftBackward, fromAircraft.normalized, maxDeviationAngle * Mathf.Deg2Rad, 0f);
                smoothedPosition = target.position + constrainedDirection * fromAircraft.magnitude;
            }

            // Apply minimum distance constraint
            float currentDistance = fromAircraft.magnitude;
            if (currentDistance < minDistance)
            {
                smoothedPosition = target.position + fromAircraft.normalized * minDistance;
            }

            transform.position = smoothedPosition;
        }

        private void UpdateRotation()
        {
            // Always look at the aircraft
            Vector3 lookDirection = target.position - transform.position;
            Quaternion desiredRotation = Quaternion.LookRotation(lookDirection);

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

            // Smoothly rotate toward desired rotation
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSpeed * Time.deltaTime);
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
