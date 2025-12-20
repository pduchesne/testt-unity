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

        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI speedText;
        [SerializeField] private TextMeshProUGUI altitudeText;
        [SerializeField] private TextMeshProUGUI headingText;
        [SerializeField] private TextMeshProUGUI throttleText;

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
            }

            ValidateUIElements();
        }

        private void Update()
        {
            if (aircraft == null) return;

            UpdateSpeed();
            UpdateAltitude();
            UpdateHeading();
            UpdateThrottle();
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

        private void ValidateUIElements()
        {
            if (speedText == null) Debug.LogWarning("FlightHUD: Speed text not assigned");
            if (altitudeText == null) Debug.LogWarning("FlightHUD: Altitude text not assigned");
            if (headingText == null) Debug.LogWarning("FlightHUD: Heading text not assigned");
            if (throttleText == null) Debug.LogWarning("FlightHUD: Throttle text not assigned");
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
