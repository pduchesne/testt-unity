using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GeoGame3D.UI
{
    /// <summary>
    /// Heading compass with rotating dial
    /// Modern arcade style with smooth animations
    /// </summary>
    public class HeadingCompass : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform compassRose; // Rotating compass background
        [SerializeField] private RectTransform compassTape; // Horizontal scrolling tape (alternative)
        [SerializeField] private TextMeshProUGUI headingText;

        [Header("Settings")]
        [SerializeField] private bool useRotatingRose = true; // True = rotating rose, False = scrolling tape
        [SerializeField] private float smoothing = 10f;
        [SerializeField] private float tapeScale = 5f; // Pixels per degree for tape mode

        private float targetHeading;
        private float currentHeading;

        public void SetHeading(float heading)
        {
            // Normalize to 0-360
            targetHeading = (heading % 360f + 360f) % 360f;
        }

        private void Update()
        {
            // Smooth interpolation with wrap-around handling
            currentHeading = SmoothAngle(currentHeading, targetHeading, smoothing * Time.deltaTime);

            if (useRotatingRose && compassRose != null)
            {
                // Rotate the compass rose (counter-clockwise as heading increases)
                compassRose.localRotation = Quaternion.Euler(0, 0, -currentHeading);
            }
            else if (!useRotatingRose && compassTape != null)
            {
                // Scroll the compass tape horizontally
                Vector2 pos = compassTape.anchoredPosition;
                pos.x = -currentHeading * tapeScale;
                compassTape.anchoredPosition = pos;
            }

            // Update heading text
            if (headingText != null)
            {
                headingText.text = Mathf.RoundToInt(currentHeading).ToString("000") + "°";
            }
        }

        /// <summary>
        /// Smooth angle interpolation with wrap-around
        /// </summary>
        private float SmoothAngle(float current, float target, float speed)
        {
            // Calculate shortest angular distance
            float delta = Mathf.DeltaAngle(current, target);

            // Interpolate
            float newAngle = current + delta * speed;

            // Normalize to 0-360
            return (newAngle % 360f + 360f) % 360f;
        }
    }
}
