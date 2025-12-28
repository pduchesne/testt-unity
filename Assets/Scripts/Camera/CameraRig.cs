using UnityEngine;
using GeoGame3D.Vehicles;

namespace GeoGame3D.Camera
{
    /// <summary>
    /// Smooth camera follow system with banking effects for aircraft
    /// Mode-aware behavior for different vehicle types
    /// </summary>
    public class CameraRig : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target;

        [Header("Mode-Specific Settings")]
        [SerializeField] private Vector3 aircraftBaseOffset = new Vector3(0f, 5f, -15f);
        [SerializeField] private Vector3 groundBaseOffset = new Vector3(0f, 3f, -8f);
        [SerializeField] private float aircraftBaseFOV = 60f;
        [SerializeField] private float groundBaseFOV = 65f;
        [SerializeField] private float aircraftFollowSpeed = 8f;
        [SerializeField] private float groundFollowSpeed = 12f;

        [Header("Follow Settings")]
        [SerializeField] private Vector3 baseOffset = new Vector3(0f, 5f, -15f); // Active offset (updated by mode)
        [SerializeField] private float followSpeed = 8f;  // Active follow speed (updated by mode)
        [SerializeField] private float rotationSpeed = 6f;
        [SerializeField] private float maxDeviationAngle = 30f; // Max degrees camera can deviate from aircraft axis
        [SerializeField] private float minDistance = 10f; // Minimum distance from aircraft
        [SerializeField] private float centeringSpeed = 4f; // Speed at which camera returns to center when not turning
        [SerializeField] private float centeringThreshold = 10f; // Angular velocity below which centering activates (deg/s)

        [Header("Dynamic Distance")]
        [SerializeField] private bool enableDynamicDistance = true;
        [SerializeField] private float minSpeedForDistance = 30f; // Speed below which distance doesn't change
        [SerializeField] private float maxSpeedForDistance = 150f; // Speed at which max distance is reached
        [SerializeField] private float distanceMultiplier = 1.5f; // How much to multiply distance at max speed

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
        private GroundVehicleController groundVehicle;
        private Quaternion lastRotation;
        private Rigidbody targetRigidbody;
        private VehicleMode currentMode = VehicleMode.Aircraft;

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
                groundVehicle = target.GetComponent<GroundVehicleController>();
                targetRigidbody = target.GetComponent<Rigidbody>();

                // Subscribe to mode changes
                VehicleModeManager modeManager = target.GetComponent<VehicleModeManager>();
                if (modeManager != null)
                {
                    modeManager.OnModeChanged += SetVehicleMode;
                    // Set initial mode
                    SetVehicleMode(modeManager.CurrentMode);
                }
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

            if (enableDynamicFOV && cam != null)
            {
                UpdateDynamicFOV();
            }
        }

        private void UpdatePosition()
        {
            // Calculate dynamic offset based on speed
            Vector3 currentOffset = baseOffset;
            if (enableDynamicDistance)
            {
                float speed = 0f;
                if (currentMode == VehicleMode.Aircraft && aircraft != null)
                {
                    speed = aircraft.Speed;
                }
                else if (currentMode == VehicleMode.Ground && groundVehicle != null)
                {
                    speed = groundVehicle.Speed;
                }

                float speedFactor = Mathf.Clamp01((speed - minSpeedForDistance) / (maxSpeedForDistance - minSpeedForDistance));
                float distanceScale = Mathf.Lerp(1f, distanceMultiplier, speedFactor);
                currentOffset = baseOffset * distanceScale;
            }

            // Calculate ideal centered position using only forward direction (ignore roll)
            // This ensures the camera is directly behind the aircraft, not offset by banking
            Vector3 forward = target.forward;
            Vector3 up = Vector3.up; // Use world up, not aircraft's up
            Vector3 right = Vector3.Cross(up, forward).normalized;
            up = Vector3.Cross(forward, right).normalized;

            Quaternion levelRotation = Quaternion.LookRotation(forward, up);
            Vector3 idealCenteredPosition = target.position + levelRotation * currentOffset;

            // Check if aircraft is turning (based on angular velocity)
            bool isTurning = targetRigidbody != null && targetRigidbody.angularVelocity.magnitude * Mathf.Rad2Deg > centeringThreshold;

            // Start with current position
            Vector3 smoothedPosition = transform.position;

            // Smoothly move toward ideal centered position
            if (isTurning)
            {
                // When turning, use normal follow speed
                smoothedPosition = Vector3.Lerp(smoothedPosition, idealCenteredPosition, followSpeed * Time.deltaTime);
            }
            else
            {
                // When not turning, use faster centering speed to return to axis
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

            // Apply banking effect if enabled (only for aircraft mode)
            if (enableBanking && currentMode == VehicleMode.Aircraft)
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
            float speed = 0f;
            if (currentMode == VehicleMode.Aircraft && aircraft != null)
            {
                speed = aircraft.Speed;
            }
            else if (currentMode == VehicleMode.Ground && groundVehicle != null)
            {
                speed = groundVehicle.Speed;
            }

            // Calculate target FOV based on speed
            float speedFactor = Mathf.Clamp01((speed - speedThreshold) / speedThreshold);
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
                groundVehicle = target.GetComponent<GroundVehicleController>();
                targetRigidbody = target.GetComponent<Rigidbody>();
            }
        }

        /// <summary>
        /// Set vehicle mode for camera behavior
        /// </summary>
        public void SetVehicleMode(VehicleMode mode)
        {
            currentMode = mode;

            // Update camera settings based on mode
            if (mode == VehicleMode.Aircraft)
            {
                baseOffset = aircraftBaseOffset;
                followSpeed = aircraftFollowSpeed;
                baseFOV = aircraftBaseFOV;
            }
            else // Ground
            {
                baseOffset = groundBaseOffset;
                followSpeed = groundFollowSpeed;
                baseFOV = groundBaseFOV;
            }

            // Immediately update camera FOV
            if (cam != null)
            {
                cam.fieldOfView = baseFOV;
            }

            Debug.Log($"CameraRig: Switched to {mode} mode with offset {baseOffset}, FOV {baseFOV}");
        }

        #endregion
    }
}
