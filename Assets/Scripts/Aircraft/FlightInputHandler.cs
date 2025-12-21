using UnityEngine;

namespace GeoGame3D.Aircraft
{
    /// <summary>
    /// Handles flight input using Unity's Input Manager (legacy input system)
    /// This is a temporary solution until the new Input System is properly configured
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
            // Pitch and Roll from WASD or arrow keys
            Vector2 pitchRoll = new Vector2(
                Input.GetAxis("Horizontal"),  // A/D for roll (left/right)
                Input.GetAxis("Vertical")     // W/S for pitch (up/down)
            );
            
            // Simulate Input Action callback
            var pitchRollContext = new UnityEngine.InputSystem.InputAction.CallbackContext();
            controller.OnPitchRoll(pitchRollContext);
            
            // Manually set the pitch/roll values using reflection since we can't properly create CallbackContext
            // Instead, let's directly access the private fields
            SendPitchRoll(pitchRoll);
            
            // Yaw from Q/E keys
            float yaw = 0f;
            if (Input.GetKey(KeyCode.Q)) yaw = -1f;
            if (Input.GetKey(KeyCode.E)) yaw = 1f;
            SendYaw(yaw);
            
            // Throttle from Shift/Ctrl
            float throttle = 0f;
            if (Input.GetKey(KeyCode.LeftShift)) throttle = 1f;
            if (Input.GetKey(KeyCode.LeftControl)) throttle = -1f;
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
