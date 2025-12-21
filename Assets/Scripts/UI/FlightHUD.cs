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

        [Header("Visual Gauges")]
        [SerializeField] private ArtificialHorizon artificialHorizon;
        [SerializeField] private CircularGauge speedGauge;
        [SerializeField] private CircularGauge altitudeGauge;
        [SerializeField] private HeadingCompass headingCompass;
        [SerializeField] private CircularGauge throttleGauge;
        [SerializeField] private CircularGauge verticalSpeedGauge;
        [SerializeField] private CircularGauge aoaGauge;
        [SerializeField] private CircularGauge gForceGauge;

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
            float speed = aircraft.Speed;
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

        private void UpdateAltitude()
        {
            float altitude = aircraft.Altitude;
            float displayAltitude = useMetric ? altitude : altitude * 3.281f; // m or feet

            // Update visual gauge
            if (altitudeGauge != null)
            {
                altitudeGauge.SetValue(displayAltitude);
            }

            // Update text display (fallback)
            if (altitudeText != null)
            {
                string unit = useMetric ? "m" : "ft";
                altitudeText.text = $"ALT: {displayAltitude.ToString($"F{decimalPlaces}")} {unit}";
            }
        }

        private void UpdateHeading()
        {
            float heading = aircraft.Heading;

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

        private void UpdateThrottle()
        {
            float throttle = aircraft.ThrottlePercent;

            // Update visual gauge
            if (throttleGauge != null)
            {
                throttleGauge.SetValue(throttle);
            }

            // Update text display (fallback)
            if (throttleText != null)
            {
                throttleText.text = $"THR: {throttle.ToString("F0")}%";
            }
        }

        private void UpdateAttitude()
        {
            float pitch = aircraft.Pitch;
            float roll = aircraft.Roll;

            // Update artificial horizon
            if (artificialHorizon != null)
            {
                artificialHorizon.UpdateHorizon(pitch, roll);
            }

            // Update text display (fallback)
            if (attitudeText != null)
            {
                attitudeText.text = $"ATT: P{pitch:+00;-00}° R{roll:+00;-00}°";
            }
        }

        private void UpdateVerticalSpeed()
        {
            float verticalSpeed = aircraft.VerticalSpeed;

            // Update visual gauge
            if (verticalSpeedGauge != null)
            {
                verticalSpeedGauge.SetValue(verticalSpeed);
            }

            // Update text display (fallback)
            if (verticalSpeedText != null)
            {
                if (useMetric)
                {
                    verticalSpeedText.text = $"V/S: {verticalSpeed:+00.0;-00.0} m/s";
                }
                else
                {
                    float feetPerMinute = verticalSpeed * 196.85f;
                    verticalSpeedText.text = $"V/S: {feetPerMinute:+0000;-0000} ft/min";
                }
            }
        }

        private void UpdateAngleOfAttack()
        {
            float aoa = aircraft.AngleOfAttack;

            // Update visual gauge
            if (aoaGauge != null)
            {
                aoaGauge.SetValue(aoa);
            }

            // Update text display (fallback)
            if (angleOfAttackText != null)
            {
                angleOfAttackText.text = $"AOA: {aoa:+00.0;-00.0}°";

                // Color code based on AOA
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
        }

        private void UpdateGForce()
        {
            float gForce = aircraft.GForce;

            // Update visual gauge
            if (gForceGauge != null)
            {
                gForceGauge.SetValue(gForce);
            }

            // Update text display (fallback)
            if (gForceText != null)
            {
                gForceText.text = $"G: {gForce:F1}";

                // Color code based on G-force
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
