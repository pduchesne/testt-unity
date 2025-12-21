using UnityEngine;
using TMPro;

namespace GeoGame3D.UI
{
    /// <summary>
    /// Heads-Up Display for flight information
    /// Shows speed, altitude, heading, and throttle
    /// </summary>
    public class FlightHUD : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GeoGame3D.Aircraft.AircraftController aircraft;

        [Header("UI Elements - Basic")]
        [SerializeField] private TextMeshProUGUI speedText;
        [SerializeField] private TextMeshProUGUI altitudeText;
        [SerializeField] private TextMeshProUGUI headingText;
        [SerializeField] private TextMeshProUGUI throttleText;

        [Header("UI Elements - Advanced Instruments")]
        [SerializeField] private TextMeshProUGUI attitudeText;
        [SerializeField] private TextMeshProUGUI verticalSpeedText;
        [SerializeField] private TextMeshProUGUI angleOfAttackText;
        [SerializeField] private TextMeshProUGUI gForceText;
        [SerializeField] private TextMeshProUGUI stallWarningText;

        [Header("Display Settings")]
        [SerializeField] private bool useMetric = true;
        [SerializeField] private int decimalPlaces = 0;

        private void Start()
        {
            if (aircraft == null)
            {
                aircraft = FindObjectOfType<GeoGame3D.Aircraft.AircraftController>();

                if (aircraft == null)
                {
                    Debug.LogError("FlightHUD: No AircraftController found in scene!");
                }
                else
                {
                    Debug.Log($"FlightHUD: Found AircraftController via FindObjectOfType");
                }
            }
            else
            {
                Debug.Log($"FlightHUD: AircraftController already assigned in Inspector");
            }

            ValidateUIElements();
            Debug.Log("FlightHUD: Start() complete");
        }

        private void Update()
        {
            if (aircraft == null)
            {
                if (Time.frameCount % 60 == 0) // Log every 60 frames
                {
                    Debug.LogWarning("FlightHUD: Aircraft reference is null in Update()");
                }
                return;
            }

            UpdateSpeed();
            UpdateAltitude();
            UpdateHeading();
            UpdateThrottle();
            UpdateAttitude();
            UpdateVerticalSpeed();
            UpdateAngleOfAttack();
            UpdateGForce();
            UpdateStallWarning();
        }

        private void UpdateSpeed()
        {
            if (speedText == null) return;

            float speed = aircraft.Speed;

            if (useMetric)
            {
                // Convert m/s to km/h
                float speedKmh = speed * 3.6f;
                speedText.text = $"SPD: {speedKmh.ToString($"F{decimalPlaces}")} km/h";
            }
            else
            {
                // Convert m/s to mph
                float speedMph = speed * 2.237f;
                speedText.text = $"SPD: {speedMph.ToString($"F{decimalPlaces}")} mph";
            }
        }

        private void UpdateAltitude()
        {
            if (altitudeText == null) return;

            float altitude = aircraft.Altitude;

            if (useMetric)
            {
                altitudeText.text = $"ALT: {altitude.ToString($"F{decimalPlaces}")} m";
            }
            else
            {
                // Convert m to feet
                float altitudeFeet = altitude * 3.281f;
                altitudeText.text = $"ALT: {altitudeFeet.ToString($"F{decimalPlaces}")} ft";
            }
        }

        private void UpdateHeading()
        {
            if (headingText == null) return;

            float heading = aircraft.Heading;
            headingText.text = $"HDG: {heading.ToString("000")}°";
        }

        private void UpdateThrottle()
        {
            if (throttleText == null) return;

            float throttle = aircraft.ThrottlePercent;
            throttleText.text = $"THR: {throttle.ToString("F0")}%";
        }

        private void UpdateAttitude()
        {
            if (attitudeText == null) return;

            float pitch = aircraft.Pitch;
            float roll = aircraft.Roll;
            attitudeText.text = $"ATT: P{pitch:+00;-00}° R{roll:+00;-00}°";
        }

        private void UpdateVerticalSpeed()
        {
            if (verticalSpeedText == null) return;

            float verticalSpeed = aircraft.VerticalSpeed;

            if (useMetric)
            {
                verticalSpeedText.text = $"V/S: {verticalSpeed:+00.0;-00.0} m/s";
            }
            else
            {
                // Convert m/s to feet/minute
                float feetPerMinute = verticalSpeed * 196.85f;
                verticalSpeedText.text = $"V/S: {feetPerMinute:+0000;-0000} ft/min";
            }
        }

        private void UpdateAngleOfAttack()
        {
            if (angleOfAttackText == null) return;

            float aoa = aircraft.AngleOfAttack;
            angleOfAttackText.text = $"AOA: {aoa:+00.0;-00.0}°";

            // Color code based on AOA (green = good, yellow = high, red = stall)
            if (Mathf.Abs(aoa) > 15f)
            {
                angleOfAttackText.color = Color.red;
            }
            else if (Mathf.Abs(aoa) > 10f)
            {
                angleOfAttackText.color = Color.yellow;
            }
            else
            {
                angleOfAttackText.color = Color.green;
            }
        }

        private void UpdateGForce()
        {
            if (gForceText == null) return;

            float gForce = aircraft.GForce;
            gForceText.text = $"G: {gForce:F1}";

            // Color code based on G-force (green = normal, yellow = moderate, red = high)
            if (gForce > 4f || gForce < 0f)
            {
                gForceText.color = Color.red;
            }
            else if (gForce > 2.5f)
            {
                gForceText.color = Color.yellow;
            }
            else
            {
                gForceText.color = Color.green;
            }
        }

        private void UpdateStallWarning()
        {
            if (stallWarningText == null) return;

            if (aircraft.IsStalled)
            {
                // Flash the warning
                bool flash = Mathf.PingPong(Time.time * 2f, 1f) > 0.5f;
                stallWarningText.enabled = flash;
                stallWarningText.text = "*** STALL ***";
                stallWarningText.color = Color.red;
            }
            else
            {
                stallWarningText.enabled = false;
            }
        }

        private void ValidateUIElements()
        {
            // Basic elements
            if (speedText == null) Debug.LogWarning("FlightHUD: Speed text not assigned");
            if (altitudeText == null) Debug.LogWarning("FlightHUD: Altitude text not assigned");
            if (headingText == null) Debug.LogWarning("FlightHUD: Heading text not assigned");
            if (throttleText == null) Debug.LogWarning("FlightHUD: Throttle text not assigned");

            // Advanced instruments (optional, no warning)
            // These are optional and will simply not display if not assigned
        }

        #region Public Methods

        /// <summary>
        /// Set the aircraft to monitor
        /// </summary>
        public void SetAircraft(GeoGame3D.Aircraft.AircraftController newAircraft)
        {
            aircraft = newAircraft;
        }

        #endregion
    }
}
