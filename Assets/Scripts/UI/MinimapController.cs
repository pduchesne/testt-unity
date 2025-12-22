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
        [SerializeField] private int tileGridSize = 3; // Load 3x3 grid of tiles (9 tiles total)
        [SerializeField] private float updateInterval = 1.0f; // Update map every N seconds
        [SerializeField] private string tileServerUrl = "https://tile.openstreetmap.org/{z}/{x}/{y}.png";

        [Header("Display Settings")]
        [SerializeField] private float mapScale = 1.0f; // Scale factor for the minimap
        [SerializeField] private bool rotateWithAircraft = true; // Rotate map or keep north up
        [SerializeField] private Color aircraftIconColor = Color.red;

        private Texture2D mapTexture;
        private int currentCenterTileX;
        private int currentCenterTileY;
        private float lastUpdateTime;
        private bool isFetchingTiles = false;

        // Current aircraft position in tile coordinates (for precise positioning)
        private double currentLongitude;
        private double currentLatitude;

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

            // Initialize map texture for 3x3 tile grid
            int textureSize = tileSize * tileGridSize;
            mapTexture = new Texture2D(textureSize, textureSize, TextureFormat.RGB24, false);
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

            // Update aircraft icon rotation every frame
            UpdateAircraftIcon();

            // Update map every frame (only fetches new tile when crossing boundaries)
            UpdateMap(false);
        }

        private void UpdateMap(bool forceUpdate)
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

            double longitude = lla.x;
            double latitude = lla.y;

            // Store current position for map positioning
            currentLongitude = longitude;
            currentLatitude = latitude;

            // Convert lat/lon to tile coordinates (center tile)
            int centerTileX = LonToTileX(longitude, zoomLevel);
            int centerTileY = LatToTileY(latitude, zoomLevel);

            // Check if we need to fetch new tiles (center tile changed)
            if (forceUpdate || centerTileX != currentCenterTileX || centerTileY != currentCenterTileY)
            {
                currentCenterTileX = centerTileX;
                currentCenterTileY = centerTileY;

                // Fetch 3x3 grid of tiles
                if (!isFetchingTiles)
                {
                    StartCoroutine(FetchTileGrid(centerTileX, centerTileY, zoomLevel));
                }
            }
        }

        private void UpdateAircraftIcon()
        {
            if (aircraftIcon == null || aircraftTransform == null)
            {
                return;
            }

            // ALWAYS keep aircraft icon centered and pointing upward
            aircraftIcon.anchoredPosition = Vector2.zero;
            aircraftIcon.localRotation = Quaternion.identity; // Point upward (no rotation)

            // Calculate aircraft's position within the 3x3 tile grid
            // The center tile is at grid position (1, 1), surrounded by 8 tiles
            int gridOffset = tileGridSize / 2; // For 3x3, this is 1

            // Get bounds of the center tile
            double centerTileMinLon = TileXToLon(currentCenterTileX, zoomLevel);
            double centerTileMaxLon = TileXToLon(currentCenterTileX + 1, zoomLevel);
            double centerTileMaxLat = TileYToLat(currentCenterTileY, zoomLevel);
            double centerTileMinLat = TileYToLat(currentCenterTileY + 1, zoomLevel);

            // Calculate fractional position within center tile (0-1)
            double fracX = (currentLongitude - centerTileMinLon) / (centerTileMaxLon - centerTileMinLon);
            double fracY = (currentLatitude - centerTileMinLat) / (centerTileMaxLat - centerTileMinLat);

            // Convert to pixel offset within the 3x3 grid texture
            // Center tile starts at (tileSize, tileSize) in the 3x3 grid
            float textureSize = tileSize * tileGridSize;
            float centerTileStartX = tileSize * gridOffset;
            float centerTileStartY = tileSize * gridOffset;

            // Calculate pixel position in the full 3x3 texture
            float pixelX = centerTileStartX + (float)(fracX * tileSize);
            float pixelY = centerTileStartY + (float)((1.0 - fracY) * tileSize); // Invert Y

            // Calculate offset to center aircraft in the view
            // We want to translate the map so the aircraft's position appears at the center
            float offsetX = textureSize / 2f - pixelX;
            float offsetY = textureSize / 2f - pixelY;

            // Apply translation to map (moves map so aircraft position is centered)
            mapImage.rectTransform.anchoredPosition = new Vector2(offsetX, offsetY);

            // Apply rotation to map based on aircraft heading
            if (rotateWithAircraft)
            {
                float heading = aircraftTransform.eulerAngles.y;
                mapImage.rectTransform.localRotation = Quaternion.Euler(0, 0, -heading);
            }
            else
            {
                mapImage.rectTransform.localRotation = Quaternion.identity;
            }
        }

        private IEnumerator FetchTileGrid(int centerX, int centerY, int zoom)
        {
            isFetchingTiles = true;
            int gridOffset = tileGridSize / 2;

            // Fetch 3x3 grid of tiles
            for (int dy = -gridOffset; dy <= gridOffset; dy++)
            {
                for (int dx = -gridOffset; dx <= gridOffset; dx++)
                {
                    int tileX = centerX + dx;
                    int tileY = centerY + dy;

                    // Fetch individual tile
                    yield return FetchSingleTile(tileX, tileY, zoom, dx + gridOffset, dy + gridOffset);
                }
            }

            isFetchingTiles = false;
        }

        private IEnumerator FetchSingleTile(int x, int y, int zoom, int gridX, int gridY)
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
                Texture2D downloadedTexture = DownloadHandlerTexture.GetContent(request);

                // Copy pixels to the correct position in the 3x3 grid texture
                if (downloadedTexture.width == tileSize && downloadedTexture.height == tileSize)
                {
                    Color[] pixels = downloadedTexture.GetPixels();

                    // Calculate position in the grid texture
                    int startX = gridX * tileSize;
                    int startY = gridY * tileSize;

                    // Copy pixels to the grid position
                    for (int py = 0; py < tileSize; py++)
                    {
                        for (int px = 0; px < tileSize; px++)
                        {
                            int srcIndex = py * tileSize + px;
                            int dstX = startX + px;
                            int dstY = startY + py;
                            mapTexture.SetPixel(dstX, dstY, pixels[srcIndex]);
                        }
                    }

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
