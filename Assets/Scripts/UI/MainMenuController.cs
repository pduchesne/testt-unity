using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;
using GeoGame3D.Vehicles;

namespace GeoGame3D.UI
{
    /// <summary>
    /// Manages the main menu overlay that can be toggled with ESC key
    /// Handles pause/resume, game exit, and vehicle mode switching
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject menuPanel;
        [SerializeField] private TextMeshProUGUI modeDisplayText;

        [Header("Vehicle Mode")]
        [SerializeField] private VehicleModeManager vehicleModeManager;
        [SerializeField] private FlightHUD flightHUD;
        [SerializeField] private DrivingHUD drivingHUD;

        [Header("Settings")]
        [SerializeField] private bool startPaused = false;

        private bool isMenuActive = false;
        private bool wasInitialized = false;

        private void Start()
        {
            // Ensure menu is initially hidden unless specified
            if (menuPanel != null)
            {
                isMenuActive = startPaused;
                menuPanel.SetActive(isMenuActive);
                UpdateGameState();
            }
            else
            {
                Debug.LogError("MainMenuController: Menu panel reference not assigned!");
            }

            // Find VehicleModeManager if not assigned
            if (vehicleModeManager == null)
            {
                vehicleModeManager = FindFirstObjectByType<VehicleModeManager>();
                if (vehicleModeManager == null)
                {
                    Debug.LogWarning("MainMenuController: No VehicleModeManager found - mode switching disabled");
                }
            }

            // Find HUDs if not assigned
            if (flightHUD == null)
            {
                flightHUD = FindFirstObjectByType<FlightHUD>();
            }
            if (drivingHUD == null)
            {
                drivingHUD = FindFirstObjectByType<DrivingHUD>();
            }

            // Subscribe to mode changes
            if (vehicleModeManager != null)
            {
                vehicleModeManager.OnModeChanged += OnVehicleModeChanged;
                // Update display to match initial mode
                OnVehicleModeChanged(vehicleModeManager.CurrentMode);
            }

            wasInitialized = true;
            Debug.Log("MainMenuController: Initialized");
        }

        private void Update()
        {
            if (!wasInitialized) return;

            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            // Toggle menu with ESC key
            if (keyboard.escapeKey.wasPressedThisFrame)
            {
                ToggleMenu();
            }
        }

        /// <summary>
        /// Toggle the menu on/off
        /// </summary>
        public void ToggleMenu()
        {
            isMenuActive = !isMenuActive;

            if (menuPanel != null)
            {
                menuPanel.SetActive(isMenuActive);
            }

            UpdateGameState();

            Debug.Log($"MainMenuController: Menu {(isMenuActive ? "opened" : "closed")}");
        }

        /// <summary>
        /// Resume the game (called by Resume button)
        /// </summary>
        public void ResumeGame()
        {
            isMenuActive = false;

            if (menuPanel != null)
            {
                menuPanel.SetActive(false);
            }

            UpdateGameState();

            Debug.Log("MainMenuController: Game resumed");
        }

        /// <summary>
        /// Exit the game (called by Exit button)
        /// </summary>
        public void ExitGame()
        {
            Debug.Log("MainMenuController: Exiting game...");

            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }

        /// <summary>
        /// Update game state based on menu visibility
        /// </summary>
        private void UpdateGameState()
        {
            if (isMenuActive)
            {
                // Pause the game
                Time.timeScale = 0f;

                // Show cursor
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                // Resume the game
                Time.timeScale = 1f;

                // Hide cursor (typical for flight sim)
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        /// <summary>
        /// Switch vehicle mode (called by mode switch button)
        /// </summary>
        public void OnSwitchModeClicked()
        {
            if (vehicleModeManager == null)
            {
                Debug.LogWarning("MainMenuController: Cannot switch mode - VehicleModeManager not found");
                return;
            }

            vehicleModeManager.SwitchMode();
            Debug.Log($"MainMenuController: Switching to {vehicleModeManager.CurrentMode} mode");
        }

        /// <summary>
        /// Handle mode change event from VehicleModeManager
        /// </summary>
        private void OnVehicleModeChanged(VehicleMode newMode)
        {
            UpdateModeDisplay(newMode);
            UpdateHUDVisibility(newMode);
        }

        /// <summary>
        /// Update the mode display text
        /// </summary>
        private void UpdateModeDisplay(VehicleMode mode)
        {
            if (modeDisplayText != null)
            {
                string modeString = mode == VehicleMode.Aircraft ? "AIRCRAFT" : "GROUND VEHICLE";
                modeDisplayText.text = $"Mode: {modeString}";
            }
        }

        /// <summary>
        /// Update HUD visibility based on vehicle mode
        /// </summary>
        private void UpdateHUDVisibility(VehicleMode mode)
        {
            bool isAircraft = (mode == VehicleMode.Aircraft);

            if (flightHUD != null)
            {
                flightHUD.gameObject.SetActive(isAircraft);
            }

            if (drivingHUD != null)
            {
                drivingHUD.gameObject.SetActive(!isAircraft);
            }

            Debug.Log($"MainMenuController: Updated HUD visibility for {mode} mode");
        }

        /// <summary>
        /// Check if menu is currently active
        /// </summary>
        public bool IsMenuActive => isMenuActive;
    }
}
