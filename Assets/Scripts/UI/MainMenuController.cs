using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace GeoGame3D.UI
{
    /// <summary>
    /// Manages the main menu overlay that can be toggled with ESC key
    /// Handles pause/resume and game exit functionality
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject menuPanel;

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
        /// Check if menu is currently active
        /// </summary>
        public bool IsMenuActive => isMenuActive;
    }
}
