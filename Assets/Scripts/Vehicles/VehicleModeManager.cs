using UnityEngine;
using CesiumForUnity;
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
        [SerializeField] private float groundSpawnHeight = 0.1f;  // Height above terrain to spawn (small offset to prevent penetration)
        [SerializeField] private float colliderReenableDelay = 0.5f;  // Delay before re-enabling collider after spawn
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
            Debug.LogWarning($"[VehicleMode] START: Switching from {previousMode} to {currentMode} mode");

            if (performTransition)
            {
                if (currentMode == VehicleMode.Ground)
                {
                    Debug.LogWarning("[VehicleMode] Calling TransitionToGroundMode...");
                    TransitionToGroundMode();
                    Debug.LogWarning("[VehicleMode] TransitionToGroundMode COMPLETED");

                    // Don't enable ground controller yet - wait for collider to re-enable
                    // UpdateComponentStates will be called from the coroutine after delay
                    return;
                }
                else
                {
                    TransitionToAircraftMode();
                }
            }

            // Enable/disable appropriate components
            Debug.LogWarning("[VehicleMode] Calling UpdateComponentStates...");
            UpdateComponentStates(currentMode);
            Debug.LogWarning("[VehicleMode] UpdateComponentStates COMPLETED");

            // Notify listeners (camera, HUD, etc.)
            OnModeChanged?.Invoke(currentMode);

            SimpleLogger.Info("Vehicle", $"Mode switch to {currentMode} complete");
            Debug.LogWarning($"[VehicleMode] COMPLETE: Mode switch to {currentMode}");
        }

        /// <summary>
        /// Transition from aircraft to ground mode
        /// </summary>
        private void TransitionToGroundMode()
        {
            Vector3 aircraftPosition = transform.position;

            SimpleLogger.Info("Vehicle", $"Aircraft position: {aircraftPosition}");
            SimpleLogger.Info("Vehicle", $"Raycasting from {aircraftPosition} down to find terrain");

            // CRITICAL: Set kinematic FIRST before any position/rotation changes
            // This prevents Unity physics from applying corrective forces
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            SimpleLogger.Info("Vehicle", "Set Rigidbody to kinematic BEFORE position change");

            // Temporarily disable CesiumGlobeAnchor to prevent geospatial coordinate interference
            CesiumGlobeAnchor globeAnchor = GetComponent<CesiumGlobeAnchor>();
            if (globeAnchor != null)
            {
                globeAnchor.enabled = false;
                SimpleLogger.Info("Vehicle", "Disabled CesiumGlobeAnchor during ground spawn");
            }

            // Temporarily disable collider to prevent physics penetration during spawn
            CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();
            if (capsuleCollider != null)
            {
                capsuleCollider.enabled = false;
                SimpleLogger.Info("Vehicle", "Disabled CapsuleCollider during ground spawn");
            }

            // Find terrain below current position
            RaycastHit hit;

            if (Physics.Raycast(aircraftPosition, Vector3.down, out hit, maxTerrainCheckDistance, terrainLayer))
            {
                SimpleLogger.Info("Vehicle", $"Hit terrain: {hit.collider.name} at point {hit.point}, distance {hit.distance}");

                // Position vehicle on terrain (safe now that rigidbody is kinematic)
                Vector3 spawnPosition = hit.point + Vector3.up * groundSpawnHeight;
                transform.position = spawnPosition;

                // Align rotation with terrain slope (but keep horizontal heading)
                Vector3 currentEuler = transform.eulerAngles;
                Quaternion groundRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                Quaternion finalRotation = Quaternion.Euler(groundRotation.eulerAngles.x, currentEuler.y, groundRotation.eulerAngles.z);
                transform.rotation = finalRotation;

                // CRITICAL: Force Unity to immediately update physics system with new transform
                // Without this, the physics engine may still use the old position for several frames
                Physics.SyncTransforms();

                SimpleLogger.Info("Vehicle", $"Spawned ground vehicle at {spawnPosition} (physics synced)");

                // Verify position was actually set
                Vector3 verifyPos = transform.position;
                SimpleLogger.Info("Vehicle", $"Position verification: {verifyPos}, delta from spawn: {(verifyPos - spawnPosition).magnitude:F3}m");
            }
            else
            {
                // No terrain found - just level the rotation
                Vector3 currentEuler = transform.eulerAngles;
                transform.rotation = Quaternion.Euler(0f, currentEuler.y, 0f);
                SimpleLogger.Warning("Vehicle", "No terrain found below aircraft - remaining at current position with level rotation");
            }

            // Disable aircraft controller immediately to stop gravity during spawn delay
            if (aircraftController != null)
            {
                aircraftController.enabled = false;
                SimpleLogger.Info("Vehicle", "Disabled AircraftController during ground spawn delay");
            }
            if (flightInputHandler != null)
            {
                flightInputHandler.enabled = false;
            }

            // Re-enable collider and physics after brief delay
            if (capsuleCollider != null)
            {
                StartCoroutine(ReenableColliderAfterDelay(colliderReenableDelay));
            }
        }

        /// <summary>
        /// Coroutine to re-enable collider after spawn delay and activate ground controller
        /// </summary>
        private System.Collections.IEnumerator ReenableColliderAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);

            // Verify position remained stable during kinematic period
            Vector3 positionAfterDelay = transform.position;
            SimpleLogger.Info("Vehicle", $"Position after {delay}s kinematic delay: {positionAfterDelay}");

            // Re-enable physics and configure for ground mode
            rb.isKinematic = false;
            rb.mass = 1500f;
            rb.linearDamping = 0.1f;
            rb.angularDamping = 1f;
            rb.useGravity = false;  // GroundVehicleController applies custom gravity
            SimpleLogger.Info("Vehicle", "Restored Rigidbody physics for ground mode");

            CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();
            if (capsuleCollider != null)
            {
                capsuleCollider.enabled = true;

                // Force physics to immediately detect collisions with ground
                Physics.SyncTransforms();

                SimpleLogger.Info("Vehicle", "Re-enabled CapsuleCollider after ground spawn (physics synced)");
            }

            // Re-enable CesiumGlobeAnchor to restore geospatial positioning
            CesiumGlobeAnchor globeAnchor = GetComponent<CesiumGlobeAnchor>();
            if (globeAnchor != null)
            {
                globeAnchor.enabled = true;
                SimpleLogger.Info("Vehicle", "Re-enabled CesiumGlobeAnchor after ground spawn");
            }

            // Now it's safe to enable ground vehicle controller
            Debug.LogWarning("[VehicleMode] Calling UpdateComponentStates after collider re-enable...");
            UpdateComponentStates(currentMode);
            Debug.LogWarning("[VehicleMode] UpdateComponentStates COMPLETED");

            // Notify listeners (camera, HUD, etc.)
            OnModeChanged?.Invoke(currentMode);

            SimpleLogger.Info("Vehicle", $"Mode switch to {currentMode} complete");
            Debug.LogWarning($"[VehicleMode] COMPLETE: Mode switch to {currentMode}");
        }

        /// <summary>
        /// Transition from ground to aircraft mode
        /// </summary>
        private void TransitionToAircraftMode()
        {
            // Ensure collider is enabled for aircraft mode
            CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();
            if (capsuleCollider != null && !capsuleCollider.enabled)
            {
                capsuleCollider.enabled = true;
                SimpleLogger.Info("Vehicle", "Re-enabled CapsuleCollider for aircraft mode");
            }

            // Ensure rigidbody is not kinematic
            rb.isKinematic = false;

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

            Debug.LogWarning($"[VehicleMode] UpdateComponentStates: Setting to {mode} mode (isAircraft={isAircraft})");

            // Aircraft components
            if (aircraftController != null)
            {
                aircraftController.enabled = isAircraft;
                Debug.LogWarning($"[VehicleMode]   AircraftController: {aircraftController.enabled}");
            }
            else
            {
                Debug.LogError("[VehicleMode]   AircraftController is NULL!");
            }

            if (flightInputHandler != null)
            {
                flightInputHandler.enabled = isAircraft;
                Debug.LogWarning($"[VehicleMode]   FlightInputHandler: {flightInputHandler.enabled}");
            }
            else
            {
                Debug.LogError("[VehicleMode]   FlightInputHandler is NULL!");
            }

            // Ground vehicle components
            if (groundVehicleController != null)
            {
                groundVehicleController.enabled = !isAircraft;
                Debug.LogWarning($"[VehicleMode]   GroundVehicleController: {groundVehicleController.enabled}");
            }
            else
            {
                Debug.LogError("[VehicleMode]   GroundVehicleController is NULL!");
            }

            if (groundVehicleInputHandler != null)
            {
                groundVehicleInputHandler.enabled = !isAircraft;
                Debug.LogWarning($"[VehicleMode]   GroundVehicleInputHandler: {groundVehicleInputHandler.enabled}");
            }
            else
            {
                Debug.LogError("[VehicleMode]   GroundVehicleInputHandler is NULL!");
            }
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
