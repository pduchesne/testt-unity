using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using CesiumForUnity;

namespace GeoGame3D.UI
{
    /// <summary>
    /// Minimap with OSM basemap showing aircraft position and heading
    /// Dynamically fetches OSM tiles based on aircraft location
    /// </summary>
    public class MinimapController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform aircraftTransform;
        [SerializeField] private CesiumGeoreference georeference;
        [SerializeField] private RawImage mapImage;
        [SerializeField] private RectTransform aircraftIcon;

        [Header("Map Settings")]
        [SerializeField] private int zoomLevel = 15; // OSM zoom level (higher = more detail)
        [SerializeField] private int tileSize = 256; // OSM tile size in pixels
        [SerializeField] private float updateInterval = 1.0f; // Update map every N seconds
        [SerializeField] private string tileServerUrl = "https://tile.openstreetmap.org/{z}/{x}/{y}.png";

        [Header("Display Settings")]
        [SerializeField] private float mapScale = 1.0f; // Scale factor for the minimap
        [SerializeField] private bool rotateWithAircraft = true; // Rotate map or keep north up
        [SerializeField] private Color aircraftIconColor = Color.red;

        private Texture2D mapTexture;
        private int currentTileX;
        private int currentTileY;
        private float lastUpdateTime;
        private Coroutine fetchCoroutine;

        private void Start()
        {
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

            if (mapImage == null)
            {
                Debug.LogError("MinimapController: Map image not assigned!");
                return;
            }

            // Initialize map texture
            mapTexture = new Texture2D(tileSize, tileSize, TextureFormat.RGB24, false);
            mapTexture.filterMode = FilterMode.Bilinear;
            mapImage.texture = mapTexture;

            // Set aircraft icon color
            if (aircraftIcon != null)
            {
                var iconImage = aircraftIcon.GetComponent<Image>();
                if (iconImage != null)
                {
                    iconImage.color = aircraftIconColor;
                }
            }

            UpdateMap(true);
        }

        private void Update()
        {
            if (aircraftTransform == null || mapImage == null)
            {
                return;
            }

            // Update aircraft icon rotation
            UpdateAircraftIcon();

            // Periodically update the map
            if (Time.time - lastUpdateTime >= updateInterval)
            {
                UpdateMap(false);
                lastUpdateTime = Time.time;
            }
        }

        private void UpdateMap(bool forceUpdate)
        {
            if (aircraftTransform == null || georeference == null)
            {
                return;
            }

            // Get aircraft position in lat/lon
            var anchor = aircraftTransform.GetComponent<CesiumGlobeAnchor>();
            if (anchor == null)
            {
                return;
            }

            var pos = anchor.longitudeLatitudeHeight;
            double latitude = pos.y;
            double longitude = pos.x;

            // If anchor shows 0,0,0 (at georeference origin), use georeference coordinates
            if (latitude == 0.0 && longitude == 0.0)
            {
                latitude = georeference.latitude;
                longitude = georeference.longitude;
            }

            // Convert lat/lon to tile coordinates
            int tileX = LonToTileX(longitude, zoomLevel);
            int tileY = LatToTileY(latitude, zoomLevel);

            // Check if we need to fetch a new tile
            if (forceUpdate || tileX != currentTileX || tileY != currentTileY)
            {
                currentTileX = tileX;
                currentTileY = tileY;

                // Stop any existing fetch
                if (fetchCoroutine != null)
                {
                    StopCoroutine(fetchCoroutine);
                }

                // Fetch new tile
                fetchCoroutine = StartCoroutine(FetchOSMTile(tileX, tileY, zoomLevel));
            }
        }

        private void UpdateAircraftIcon()
        {
            if (aircraftIcon == null || aircraftTransform == null)
            {
                return;
            }

            // Keep aircraft icon centered
            aircraftIcon.anchoredPosition = Vector2.zero;

            // Update rotation
            if (rotateWithAircraft)
            {
                // Rotate the entire map to keep north up relative to aircraft
                float heading = aircraftTransform.eulerAngles.y;
                mapImage.rectTransform.localRotation = Quaternion.Euler(0, 0, -heading);
            }
            else
            {
                // Keep map north-up, rotate only the aircraft icon
                float heading = aircraftTransform.eulerAngles.y;
                aircraftIcon.localRotation = Quaternion.Euler(0, 0, -heading);
            }
        }

        private IEnumerator FetchOSMTile(int x, int y, int zoom)
        {
            // Construct tile URL
            string url = tileServerUrl
                .Replace("{z}", zoom.ToString())
                .Replace("{x}", x.ToString())
                .Replace("{y}", y.ToString());

            // Set user agent (required by OSM tile usage policy)
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
            request.SetRequestHeader("User-Agent", "GeoGame3D/1.0 (Unity Flight Simulator)");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                // Get the downloaded texture
                Texture2D downloadedTexture = DownloadHandlerTexture.GetContent(request);

                // Copy pixels to our map texture (avoid mipmap issues)
                if (downloadedTexture.width == mapTexture.width && downloadedTexture.height == mapTexture.height)
                {
                    mapTexture.SetPixels(downloadedTexture.GetPixels());
                    mapTexture.Apply();
                }
                else
                {
                    Debug.LogWarning($"MinimapController: Tile size mismatch. Expected {tileSize}x{tileSize}, got {downloadedTexture.width}x{downloadedTexture.height}");
                }

                Destroy(downloadedTexture);
            }
            else
            {
                Debug.LogWarning($"MinimapController: Failed to fetch tile {x}/{y}/{zoom}: {request.error}");
            }

            request.Dispose();
            fetchCoroutine = null;
        }

        #region Tile Coordinate Conversion

        /// <summary>
        /// Convert longitude to tile X coordinate
        /// </summary>
        private int LonToTileX(double lon, int zoom)
        {
            int n = 1 << zoom; // 2^zoom
            return (int)System.Math.Floor((lon + 180.0) / 360.0 * n);
        }

        /// <summary>
        /// Convert latitude to tile Y coordinate
        /// </summary>
        private int LatToTileY(double lat, int zoom)
        {
            int n = 1 << zoom; // 2^zoom
            double latRad = lat * System.Math.PI / 180.0;
            return (int)System.Math.Floor((1.0 - System.Math.Log(System.Math.Tan(latRad) + 1.0 / System.Math.Cos(latRad)) / System.Math.PI) / 2.0 * n);
        }

        /// <summary>
        /// Convert tile X to longitude
        /// </summary>
        private double TileXToLon(int x, int zoom)
        {
            int n = 1 << zoom;
            return x / (double)n * 360.0 - 180.0;
        }

        /// <summary>
        /// Convert tile Y to latitude
        /// </summary>
        private double TileYToLat(int y, int zoom)
        {
            int n = 1 << zoom;
            double latRad = System.Math.Atan(System.Math.Sinh(System.Math.PI * (1 - 2 * y / (double)n)));
            return latRad * 180.0 / System.Math.PI;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Set the zoom level (1-19, higher = more detail)
        /// </summary>
        public void SetZoomLevel(int zoom)
        {
            zoomLevel = Mathf.Clamp(zoom, 1, 19);
            UpdateMap(true);
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
            tileServerUrl = url;
            UpdateMap(true);
        }

        #endregion

        private void OnDestroy()
        {
            if (mapTexture != null)
            {
                Destroy(mapTexture);
            }
        }
    }
}
