using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GeoGame3D.UI
{
    /// <summary>
    /// Circular gauge with fill and needle indicator
    /// Modern arcade style with smooth animations
    /// </summary>
    public class CircularGauge : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image fillImage; // Radial fill image
        [SerializeField] private RectTransform needle; // Optional rotating needle
        [SerializeField] private TextMeshProUGUI valueText;
        [SerializeField] private TextMeshProUGUI labelText;

        [Header("Gauge Settings")]
        [SerializeField] private float minValue = 0f;
        [SerializeField] private float maxValue = 100f;
        [SerializeField] private string unit = "";
        [SerializeField] private string format = "F0"; // Number format
        [SerializeField] private float smoothing = 10f;

        [Header("Visual Settings")]
        [SerializeField] private bool useNeedle = false;
        [SerializeField] private float needleMinAngle = -135f; // Starting angle (bottom-left)
        [SerializeField] private float needleMaxAngle = 135f;  // Ending angle (bottom-right)

        [Header("Color Coding")]
        [SerializeField] private bool useColorCoding = false;
        [SerializeField] private Color lowColor = Color.green;
        [SerializeField] private Color midColor = Color.yellow;
        [SerializeField] private Color highColor = Color.red;
        [SerializeField] private float midThreshold = 0.5f; // 0-1 range
        [SerializeField] private float highThreshold = 0.8f; // 0-1 range

        private float targetValue;
        private float currentValue;

        private void Start()
        {
            if (fillImage != null)
            {
                fillImage.fillAmount = 0f;
                fillImage.type = Image.Type.Filled;
                fillImage.fillMethod = Image.FillMethod.Radial360;
                fillImage.fillOrigin = (int)Image.Origin360.Bottom; // Start from bottom
            }

            if (labelText != null && !string.IsNullOrEmpty(unit))
            {
                labelText.text = unit;
            }
        }

        public void SetValue(float value)
        {
            targetValue = Mathf.Clamp(value, minValue, maxValue);
        }

        public void SetRange(float min, float max)
        {
            minValue = min;
            maxValue = max;
        }

        private void Update()
        {
            // Smooth interpolation
            currentValue = Mathf.Lerp(currentValue, targetValue, Time.deltaTime * smoothing);

            // Calculate normalized value (0-1)
            float normalizedValue = Mathf.InverseLerp(minValue, maxValue, currentValue);

            // Update fill amount
            if (fillImage != null)
            {
                fillImage.fillAmount = normalizedValue;

                // Apply color coding
                if (useColorCoding)
                {
                    fillImage.color = GetColorForValue(normalizedValue);
                }
            }

            // Update needle rotation
            if (useNeedle && needle != null)
            {
                float angle = Mathf.Lerp(needleMinAngle, needleMaxAngle, normalizedValue);
                needle.localRotation = Quaternion.Euler(0, 0, angle);
            }

            // Update text
            if (valueText != null)
            {
                valueText.text = currentValue.ToString(format);
            }
        }

        private Color GetColorForValue(float normalizedValue)
        {
            if (normalizedValue < midThreshold)
            {
                // Interpolate between low and mid
                float t = normalizedValue / midThreshold;
                return Color.Lerp(lowColor, midColor, t);
            }
            else if (normalizedValue < highThreshold)
            {
                // Interpolate between mid and high
                float t = (normalizedValue - midThreshold) / (highThreshold - midThreshold);
                return Color.Lerp(midColor, highColor, t);
            }
            else
            {
                return highColor;
            }
        }
    }
}
