using UnityEngine;
using UnityEngine.InputSystem;

namespace GeoGame3D.Aircraft
{
    /// <summary>
    /// Handles flight input using the new Input System's Keyboard class
    /// Directly reads keyboard state and sets input values on AircraftController
    /// </summary>
    [RequireComponent(typeof(AircraftController))]
    public class FlightInputHandler : MonoBehaviour
    {
        private AircraftController controller;

        private void Awake()
        {
            controller = GetComponent<AircraftController>();
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            // Pitch and Roll from WASD or arrow keys
            Vector2 pitchRoll = Vector2.zero;

            // Horizontal (Roll): A/D or Left/Right arrows
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
                pitchRoll.x = -1f;
            else if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
                pitchRoll.x = 1f;

            // Vertical (Pitch): W/S or Up/Down arrows
            // Flight sim convention: S/Down = pull back = pitch UP
            //                        W/Up = push forward = pitch DOWN
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
                pitchRoll.y = 1f;  // Pull back = pitch up
            else if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
                pitchRoll.y = -1f; // Push forward = pitch down
            
            SendPitchRoll(pitchRoll);
            
            // Yaw from Q/E keys
            float yaw = 0f;
            if (keyboard.qKey.isPressed) yaw = -1f;
            if (keyboard.eKey.isPressed) yaw = 1f;
            SendYaw(yaw);
            
            // Throttle from Shift/Ctrl
            float throttle = 0f;
            if (keyboard.leftShiftKey.isPressed) throttle = 1f;
            if (keyboard.leftCtrlKey.isPressed) throttle = -1f;
            SendThrottle(throttle);
        }
        
        private void SendPitchRoll(Vector2 value)
        {
            // Access private field using reflection
            var field = typeof(AircraftController).GetField("pitchRollInput", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(controller, value);
            }
        }
        
        private void SendYaw(float value)
        {
            var field = typeof(AircraftController).GetField("yawInput", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(controller, value);
            }
        }
        
        private void SendThrottle(float value)
        {
            var field = typeof(AircraftController).GetField("throttleInput", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(controller, value);
            }
        }
    }
}
