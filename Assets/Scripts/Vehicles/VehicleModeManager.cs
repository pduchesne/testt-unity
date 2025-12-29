using UnityEngine;
using GeoGame3D.Aircraft;
using GeoGame3D.Utils;

namespace GeoGame3D.Vehicles
{
    /// <summary>
    /// Vehicle mode enumeration
    /// </summary>
    public enum VehicleMode
    {
        Aircraft,
        Ground
    }

    /// <summary>
    /// Manages switching between aircraft and ground vehicle modes
    /// Handles controller enable/disable, physics transitions, and notifications
    /// </summary>
    public class VehicleModeManager : MonoBehaviour
    {
        [Header("Mode Settings")]
        [SerializeField] private VehicleMode startMode = VehicleMode.Aircraft;

        [Header("Ground Mode Spawn")]
        [SerializeField] private float groundSpawnHeight = 2f;  // Height above terrain to spawn
        [SerializeField] private float maxTerrainCheckDistance = 1000f;  // Max raycast distance
        [SerializeField] private LayerMask terrainLayer;  // Terrain layer for raycasting

        [Header("Aircraft Mode Spawn")]
        [SerializeField] private float aircraftSpawnVelocity = 50f;  // Initial forward velocity

        // Component references
        private AircraftController aircraftController;
        private FlightInputHandler flightInputHandler;
        private GroundVehicleController groundVehicleController;
        private GroundVehicleInputHandler groundVehicleInputHandler;
        private Rigidbody rb;

        // Current mode
        private VehicleMode currentMode;
        public VehicleMode CurrentMode => currentMode;

        // Events for camera and HUD updates
        public delegate void ModeChangedHandler(VehicleMode newMode);
        public event ModeChangedHandler OnModeChanged;

        private void Awake()
        {
            // Get all required components
            aircraftController = GetComponent<AircraftController>();
            flightInputHandler = GetComponent<FlightInputHandler>();
            groundVehicleController = GetComponent<GroundVehicleController>();
            groundVehicleInputHandler = GetComponent<GroundVehicleInputHandler>();
            rb = GetComponent<Rigidbody>();

            // Validate components
            if (aircraftController == null)
            {
                SimpleLogger.Error("Vehicle", "AircraftController not found on VehicleModeManager GameObject");
            }
            if (groundVehicleController == null)
            {
                SimpleLogger.Error("Vehicle", "GroundVehicleController not found on VehicleModeManager GameObject");
            }

            // Set initial mode
            SetMode(startMode, false);  // Don't trigger transition on startup
        }

        /// <summary>
        /// Toggle between aircraft and ground modes
        /// </summary>
        public void SwitchMode()
        {
            VehicleMode newMode = currentMode == VehicleMode.Aircraft ? VehicleMode.Ground : VehicleMode.Aircraft;
            SetMode(newMode, true);
        }

        /// <summary>
        /// Set specific vehicle mode
        /// </summary>
        public void SetMode(VehicleMode mode, bool performTransition = true)
        {
            if (mode == currentMode && performTransition == false)
            {
                // Just ensure correct components are enabled/disabled
                UpdateComponentStates(mode);
                return;
            }

            VehicleMode previousMode = currentMode;
            currentMode = mode;

            SimpleLogger.Info("Vehicle", $"Switching from {previousMode} to {currentMode} mode");

            if (performTransition)
            {
                if (currentMode == VehicleMode.Ground)
                {
                    TransitionToGroundMode();
                }
                else
                {
                    TransitionToAircraftMode();
                }
            }

            // Enable/disable appropriate components
            UpdateComponentStates(currentMode);

            // Notify listeners (camera, HUD, etc.)
            OnModeChanged?.Invoke(currentMode);

            SimpleLogger.Info("Vehicle", $"Mode switch to {currentMode} complete");
        }

        /// <summary>
        /// Transition from aircraft to ground mode
        /// </summary>
        private void TransitionToGroundMode()
        {
            // Find terrain below current position
            Vector3 rayStart = transform.position;
            RaycastHit hit;

            if (Physics.Raycast(rayStart, Vector3.down, out hit, maxTerrainCheckDistance, terrainLayer))
            {
                // Position vehicle on terrain
                Vector3 spawnPosition = hit.point + Vector3.up * groundSpawnHeight;
                transform.position = spawnPosition;

                // Align rotation with terrain slope
                Quaternion groundRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                transform.rotation = groundRotation;

                SimpleLogger.Info("Vehicle", $"Spawned ground vehicle at {spawnPosition}, aligned to terrain slope");
            }
            else
            {
                // No terrain found, spawn at current position with level rotation
                transform.rotation = Quaternion.identity;
                SimpleLogger.Warning("Vehicle", "No terrain found below aircraft, spawning ground vehicle at current position");
            }

            // Zero all physics velocities
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // Configure rigidbody for ground mode
            rb.mass = 1500f;
            rb.linearDamping = 0.1f;
            rb.angularDamping = 1f;
            rb.useGravity = false;  // GroundVehicleController applies custom gravity
        }

        /// <summary>
        /// Transition from ground to aircraft mode
        /// </summary>
        private void TransitionToAircraftMode()
        {
            // Level the rotation (zero pitch and roll, keep heading)
            Vector3 eulerAngles = transform.eulerAngles;
            transform.rotation = Quaternion.Euler(0f, eulerAngles.y, 0f);

            // Add forward velocity for stable flight
            rb.linearVelocity = transform.forward * aircraftSpawnVelocity;
            rb.angularVelocity = Vector3.zero;

            // Configure rigidbody for aircraft mode
            rb.mass = 10000f;
            rb.linearDamping = 0f;
            rb.angularDamping = 0.5f;
            rb.useGravity = false;  // AircraftController applies custom gravity

            SimpleLogger.Info("Vehicle", $"Transitioned to aircraft mode with velocity {aircraftSpawnVelocity} m/s");
        }

        /// <summary>
        /// Enable/disable appropriate components based on mode
        /// </summary>
        private void UpdateComponentStates(VehicleMode mode)
        {
            bool isAircraft = (mode == VehicleMode.Aircraft);

            // Aircraft components
            if (aircraftController != null)
                aircraftController.enabled = isAircraft;
            if (flightInputHandler != null)
                flightInputHandler.enabled = isAircraft;

            // Ground vehicle components
            if (groundVehicleController != null)
                groundVehicleController.enabled = !isAircraft;
            if (groundVehicleInputHandler != null)
                groundVehicleInputHandler.enabled = !isAircraft;
        }

        /// <summary>
        /// Debug visualization
        /// </summary>
        private void OnDrawGizmos()
        {
            if (currentMode == VehicleMode.Ground)
            {
                // Draw terrain check raycast
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(transform.position, transform.position + Vector3.down * maxTerrainCheckDistance);
            }
        }
    }
}
