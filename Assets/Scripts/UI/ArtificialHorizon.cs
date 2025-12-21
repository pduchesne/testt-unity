using UnityEngine;
using UnityEngine.UI;

namespace GeoGame3D.UI
{
    /// <summary>
    /// Artificial horizon gauge showing pitch and roll
    /// Modern arcade style with smooth animations
    /// </summary>
    public class ArtificialHorizon : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform horizonLine;
        [SerializeField] private RectTransform rollIndicator;
        [SerializeField] private RectTransform centerReticle;

        [Header("Settings")]
        [SerializeField] private float pitchSensitivity = 5f; // Pixels per degree
        [SerializeField] private float smoothing = 10f;

        private float targetPitch;
        private float targetRoll;
        private float currentPitch;
        private float currentRoll;

        public void UpdateHorizon(float pitch, float roll)
        {
            targetPitch = pitch;
            targetRoll = roll;
        }

        private void Update()
        {
            // Smooth interpolation
            currentPitch = Mathf.Lerp(currentPitch, targetPitch, Time.deltaTime * smoothing);
            currentRoll = Mathf.Lerp(currentRoll, targetRoll, Time.deltaTime * smoothing);

            if (horizonLine != null)
            {
                // Move horizon line based on pitch (vertical movement)
                Vector2 pos = horizonLine.anchoredPosition;
                pos.y = currentPitch * pitchSensitivity;
                horizonLine.anchoredPosition = pos;

                // Rotate horizon line based on roll
                horizonLine.localRotation = Quaternion.Euler(0, 0, -currentRoll);
            }

            if (rollIndicator != null)
            {
                // Rotate roll indicator
                rollIndicator.localRotation = Quaternion.Euler(0, 0, -currentRoll);
            }
        }
    }
}
