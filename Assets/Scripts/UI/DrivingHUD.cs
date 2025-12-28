using UnityEngine;
using TMPro;
using GeoGame3D.Vehicles;

namespace GeoGame3D.UI
{
    /// <summary>
    /// Heads-Up Display for ground vehicle driving
    /// Shows speed, heading, brake status, and grounding status
    /// </summary>
    public class DrivingHUD : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GroundVehicleController vehicle;

        [Header("UI Elements - Basic")]
        [SerializeField] private TextMeshProUGUI speedText;
        [SerializeField] private TextMeshProUGUI headingText;
        [SerializeField] private TextMeshProUGUI brakeText;
        [SerializeField] private TextMeshProUGUI groundedText;

        [Header("Visual Gauges")]
        [SerializeField] private CircularGauge speedGauge;
        [SerializeField] private HeadingCompass headingCompass;

        [Header("Display Settings")]
        [SerializeField] private bool useMetric = true;
        [SerializeField] private int decimalPlaces = 0;

        private void Start()
        {
            if (vehicle == null)
            {
                vehicle = FindFirstObjectByType<GroundVehicleController>();

                if (vehicle == null)
                {
                    Debug.LogError("DrivingHUD: No GroundVehicleController found in scene!");
                }
                else
                {
                    Debug.Log("DrivingHUD: Found GroundVehicleController via FindObjectOfType");
                }
            }
            else
            {
                Debug.Log("DrivingHUD: GroundVehicleController already assigned in Inspector");
            }

            ValidateUIElements();
            Debug.Log("DrivingHUD: Start() complete");
        }

        private void Update()
        {
            if (vehicle == null)
            {
                if (Time.frameCount % 60 == 0) // Log every 60 frames
                {
                    Debug.LogWarning("DrivingHUD: Vehicle reference is null in Update()");
                }
                return;
            }

            UpdateSpeed();
            UpdateHeading();
            UpdateBrakeStatus();
            UpdateGroundedStatus();
        }

        private void UpdateSpeed()
        {
            float speed = vehicle.Speed;
            float displaySpeed = useMetric ? speed * 3.6f : speed * 2.237f; // km/h or mph

            // Update visual gauge
            if (speedGauge != null)
            {
                speedGauge.SetValue(displaySpeed);
            }

            // Update text display (fallback)
            if (speedText != null)
            {
                string unit = useMetric ? "km/h" : "mph";
                speedText.text = $"SPD: {displaySpeed.ToString($"F{decimalPlaces}")} {unit}";
            }
        }

        private void UpdateHeading()
        {
            float heading = vehicle.Heading;

            // Update visual compass
            if (headingCompass != null)
            {
                headingCompass.SetHeading(heading);
            }

            // Update text display (fallback)
            if (headingText != null)
            {
                headingText.text = $"HDG: {heading.ToString("000")}°";
            }
        }

        private void UpdateBrakeStatus()
        {
            if (brakeText == null) return;

            if (vehicle.IsBraking)
            {
                // Flash the brake indicator
                bool flash = Mathf.PingPong(Time.time * 3f, 1f) > 0.5f;
                brakeText.enabled = flash;
                brakeText.text = "BRAKE";
                brakeText.color = Color.red;
            }
            else
            {
                brakeText.enabled = false;
            }
        }

        private void UpdateGroundedStatus()
        {
            if (groundedText == null) return;

            if (!vehicle.IsGrounded)
            {
                // Flash the airborne warning
                bool flash = Mathf.PingPong(Time.time * 2f, 1f) > 0.5f;
                groundedText.enabled = flash;
                groundedText.text = "*** AIRBORNE ***";
                groundedText.color = Color.yellow;
            }
            else
            {
                groundedText.enabled = false;
            }
        }

        private void ValidateUIElements()
        {
            // Basic elements
            if (speedText == null) Debug.LogWarning("DrivingHUD: Speed text not assigned");
            if (headingText == null) Debug.LogWarning("DrivingHUD: Heading text not assigned");

            // Warning elements (optional)
            if (brakeText == null) Debug.LogWarning("DrivingHUD: Brake text not assigned (optional)");
            if (groundedText == null) Debug.LogWarning("DrivingHUD: Grounded text not assigned (optional)");

            // Visual gauges (optional)
            if (speedGauge == null) Debug.LogWarning("DrivingHUD: Speed gauge not assigned (optional)");
            if (headingCompass == null) Debug.LogWarning("DrivingHUD: Heading compass not assigned (optional)");
        }

        #region Public Methods

        /// <summary>
        /// Set the vehicle to monitor
        /// </summary>
        public void SetVehicle(GroundVehicleController newVehicle)
        {
            vehicle = newVehicle;
        }

        #endregion
    }
}
