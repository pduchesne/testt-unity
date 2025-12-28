using UnityEngine;
using UnityEngine.InputSystem;
using GeoGame3D.UI;

namespace GeoGame3D.Vehicles
{
    /// <summary>
    /// Handles ground vehicle input using the new Input System's Keyboard class
    /// Directly reads keyboard state and sets input values on GroundVehicleController
    /// </summary>
    [RequireComponent(typeof(GroundVehicleController))]
    public class GroundVehicleInputHandler : MonoBehaviour
    {
        private GroundVehicleController controller;
        private MainMenuController mainMenu;

        private void Awake()
        {
            controller = GetComponent<GroundVehicleController>();
            mainMenu = FindFirstObjectByType<MainMenuController>();

            if (mainMenu == null)
            {
                Debug.LogWarning("[GroundVehicleInput] MainMenuController not found - input will not be blocked by menu");
            }
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            // Don't process input if menu is active
            if (mainMenu != null && mainMenu.IsMenuActive) return;

            // Acceleration/Reverse from WASD or arrow keys
            float acceleration = 0f;
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
                acceleration = 1f;  // Forward
            else if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
                acceleration = -1f;  // Reverse

            SendAcceleration(acceleration);

            // Steering from A/D or Left/Right arrows
            float steering = 0f;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
                steering = -1f;  // Turn left
            else if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
                steering = 1f;  // Turn right

            SendSteering(steering);

            // Brake from Spacebar
            bool brake = keyboard.spaceKey.isPressed;
            SendBrake(brake);
        }

        private void SendAcceleration(float value)
        {
            var field = typeof(GroundVehicleController).GetField("accelerationInput",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(controller, value);
            }
        }

        private void SendSteering(float value)
        {
            var field = typeof(GroundVehicleController).GetField("steeringInput",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(controller, value);
            }
        }

        private void SendBrake(bool value)
        {
            var field = typeof(GroundVehicleController).GetField("brakeInput",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(controller, value);
            }
        }
    }
}
