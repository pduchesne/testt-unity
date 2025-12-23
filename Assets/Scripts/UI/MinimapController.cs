using UnityEngine;
using UnityEngine.UI;
using CesiumForUnity;

namespace GeoGame3D.UI
{
    /// <summary>
    /// Minimap HUD controller that displays aircraft position on an OSM map.
    /// Uses OSMMapDisplay for rendering and handles rotation, cropping, and aircraft overlay.
    /// </summary>
    public class MinimapController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform aircraftTransform;
        [SerializeField] private CesiumGeoreference georeference;
        [SerializeField] private OSMMapDisplay mapDisplay;
        [SerializeField] private RectTransform minimapContainer; // Container that applies rotation/clipping
        [SerializeField] private RectTransform aircraftIcon;

        [Header("Map Settings")]
        [SerializeField] private int zoomLevel = 15; // OSM zoom level (higher = more detail)
        [SerializeField] private int mapPixelSize = 512; // Size of the map display in pixels

        [Header("Display Settings")]
        [SerializeField] private bool rotateWithAircraft = true; // Rotate map or keep north up
        [SerializeField] private Color aircraftIconColor = Color.red;
        [SerializeField] private float updateThreshold = 0.00001f; // Minimum position change to trigger update (degrees)

        // Current aircraft geospatial position
        private double currentLongitude;
        private double currentLatitude;
        private double previousLongitude;
        private double previousLatitude;

        private void Start()
        {
            // Find dependencies if not assigned
            if (georeference == null)
            {
                georeference = FindObjectOfType<CesiumGeoreference>();
            }

            if (aircraftTransform == null)
            {
                var controller = FindObjectOfType<GeoGame3D.Aircraft.AircraftController>();
                if (controller != null)
                {
                    aircraftTransform = controller.transform;
                }
            }

            if (mapDisplay == null)
            {
                Debug.LogError("MinimapController: OSMMapDisplay not assigned!");
                return;
            }

            // Set aircraft icon color
            if (aircraftIcon != null)
            {
                var iconImage = aircraftIcon.GetComponent<Image>();
                if (iconImage != null)
                {
                    iconImage.color = aircraftIconColor;
                }
            }

            // Initialize with current position
            UpdateAircraftPosition();
            previousLatitude = currentLatitude;
            previousLongitude = currentLongitude;
        }

        private void Update()
        {
            if (aircraftTransform == null || mapDisplay == null)
            {
                return;
            }

            // Update aircraft position and map every frame
            UpdateAircraftPosition();
            UpdateMapDisplay();
            UpdateAircraftIcon();
        }

        /// <summary>
        /// Update the current aircraft geospatial position
        /// </summary>
        private void UpdateAircraftPosition()
        {
            if (aircraftTransform == null || georeference == null)
            {
                return;
            }

            // Convert Unity world position to geospatial coordinates
            Vector3 unityPos = aircraftTransform.position;
            Unity.Mathematics.double3 unityPosDouble = new Unity.Mathematics.double3(unityPos.x, unityPos.y, unityPos.z);

            Unity.Mathematics.double3 ecefPosition = georeference.TransformUnityPositionToEarthCenteredEarthFixed(
                unityPosDouble
            );

            Unity.Mathematics.double3 lla = CesiumForUnity.CesiumWgs84Ellipsoid.EarthCenteredEarthFixedToLongitudeLatitudeHeight(
                ecefPosition
            );

            currentLongitude = lla.x;
            currentLatitude = lla.y;
        }

        /// <summary>
        /// Update the OSM map display with current aircraft position
        /// </summary>
        private void UpdateMapDisplay()
        {
            if (mapDisplay == null)
            {
                return;
            }

            // Only update if position changed significantly to reduce flickering
            double latDiff = System.Math.Abs(currentLatitude - previousLatitude);
            double lonDiff = System.Math.Abs(currentLongitude - previousLongitude);

            if (latDiff > updateThreshold || lonDiff > updateThreshold)
            {
                // Update map to center on aircraft position
                mapDisplay.UpdateMap(currentLatitude, currentLongitude, zoomLevel, mapPixelSize);

                previousLatitude = currentLatitude;
                previousLongitude = currentLongitude;
            }
        }

        /// <summary>
        /// Update the aircraft icon position and rotation
        /// </summary>
        private void UpdateAircraftIcon()
        {
            if (aircraftIcon == null || aircraftTransform == null || minimapContainer == null)
            {
                return;
            }

            float heading = aircraftTransform.eulerAngles.y;

            // Aircraft icon is always centered on the MapDisplay
            aircraftIcon.anchoredPosition = Vector2.zero;

            if (rotateWithAircraft)
            {
                // Rotate map so aircraft heading is "up"
                minimapContainer.localRotation = Quaternion.Euler(0, 0, -heading);

                // Icon doesn't rotate - it always points up (which is the aircraft's heading)
                aircraftIcon.localRotation = Quaternion.identity;
            }
            else
            {
                // Map stays north-up
                minimapContainer.localRotation = Quaternion.identity;

                // Icon rotates to show aircraft heading
                aircraftIcon.localRotation = Quaternion.Euler(0, 0, -heading);
            }
        }

        #region Public Methods

        /// <summary>
        /// Set the zoom level (1-19, higher = more detail)
        /// </summary>
        public void SetZoomLevel(int zoom)
        {
            zoomLevel = Mathf.Clamp(zoom, 1, 19);
        }

        /// <summary>
        /// Toggle map rotation mode
        /// </summary>
        public void ToggleRotationMode()
        {
            rotateWithAircraft = !rotateWithAircraft;
        }

        /// <summary>
        /// Set custom tile server URL
        /// </summary>
        public void SetTileServer(string url)
        {
            if (mapDisplay != null)
            {
                mapDisplay.SetTileServer(url);
            }
        }

        /// <summary>
        /// Set the map pixel size
        /// </summary>
        public void SetMapPixelSize(int size)
        {
            mapPixelSize = size;
        }

        #endregion
    }
}
