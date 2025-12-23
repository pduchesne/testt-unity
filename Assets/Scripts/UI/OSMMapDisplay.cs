using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

namespace GeoGame3D.UI
{
    /// <summary>
    /// Reusable OSM map display component that renders a north-oriented map
    /// given a center point, zoom level, and pixel size.
    /// This component is display-only and doesn't know about aircraft or game logic.
    /// </summary>
    public class OSMMapDisplay : MonoBehaviour
    {
        [Header("Display Settings")]
        [SerializeField] private RawImage mapImage;
        [SerializeField] private int tileSize = 256; // OSM tile size in pixels
        [SerializeField] private string tileServerUrl = "https://tile.openstreetmap.org/{z}/{x}/{y}.png";

        // Map state
        private double centerLatitude;
        private double centerLongitude;
        private int zoomLevel = 15;
        private int pixelSize = 512; // Total size of the map in pixels

        private Texture2D mapTexture;
        private int tileGridSize; // Number of tiles needed to cover pixelSize
        private int currentCenterTileX;
        private int currentCenterTileY;
        private bool isFetchingTiles = false;
        private bool initialized = false;

        /// <summary>
        /// Initialize or update the map display
        /// </summary>
        /// <param name="latitude">Center latitude in degrees</param>
        /// <param name="longitude">Center longitude in degrees</param>
        /// <param name="zoom">OSM zoom level (1-19)</param>
        /// <param name="size">Display size in pixels (width and height)</param>
        public void UpdateMap(double latitude, double longitude, int zoom, int size)
        {
            // Update parameters
            centerLatitude = latitude;
            centerLongitude = longitude;
            zoomLevel = Mathf.Clamp(zoom, 1, 19);
            pixelSize = size;

            // Calculate how many tiles we need to cover the display size
            // Add 2 extra tiles (1 on each side) to allow for panning
            tileGridSize = Mathf.CeilToInt(pixelSize / (float)tileSize) + 2;

            // Initialize texture if needed or resize if size changed
            if (!initialized || mapTexture == null || mapTexture.width != tileGridSize * tileSize)
            {
                InitializeTexture();
            }

            // Calculate center tile coordinates
            int centerTileX = LonToTileX(centerLongitude, zoomLevel);
            int centerTileY = LatToTileY(centerLatitude, zoomLevel);

            // Fetch tiles if center tile changed
            if (!initialized || centerTileX != currentCenterTileX || centerTileY != currentCenterTileY)
            {
                currentCenterTileX = centerTileX;
                currentCenterTileY = centerTileY;

                if (!isFetchingTiles)
                {
                    StartCoroutine(FetchTileGrid(centerTileX, centerTileY, zoomLevel));
                }
            }

            // Update UV offset to center the map on the exact coordinate
            UpdateMapOffset();

            initialized = true;
        }

        private void InitializeTexture()
        {
            if (mapImage == null)
            {
                Debug.LogError("OSMMapDisplay: Map image not assigned!");
                return;
            }

            // Create texture for tile grid
            int textureSize = tileSize * tileGridSize;
            if (mapTexture != null)
            {
                Destroy(mapTexture);
            }

            mapTexture = new Texture2D(textureSize, textureSize, TextureFormat.RGB24, false);
            mapTexture.filterMode = FilterMode.Bilinear;
            mapTexture.wrapMode = TextureWrapMode.Clamp;
            mapImage.texture = mapTexture;

            // Set the RawImage size to match pixelSize
            if (mapImage.rectTransform != null)
            {
                mapImage.rectTransform.sizeDelta = new Vector2(pixelSize, pixelSize);
            }
        }

        private void UpdateMapOffset()
        {
            if (mapImage == null || mapTexture == null)
            {
                return;
            }

            // Calculate the exact pixel position of the center point within the tile grid
            int gridOffset = tileGridSize / 2;

            // Get bounds of the center tile
            double centerTileMinLon = TileXToLon(currentCenterTileX, zoomLevel);
            double centerTileMaxLon = TileXToLon(currentCenterTileX + 1, zoomLevel);
            double centerTileMaxLat = TileYToLat(currentCenterTileY, zoomLevel);
            double centerTileMinLat = TileYToLat(currentCenterTileY + 1, zoomLevel);

            // Calculate fractional position within center tile (0-1)
            double fracX = (centerLongitude - centerTileMinLon) / (centerTileMaxLon - centerTileMinLon);
            double fracY = (centerLatitude - centerTileMinLat) / (centerTileMaxLat - centerTileMinLat);

            // Convert to pixel position in the texture
            int textureSize = tileSize * tileGridSize;
            float centerTileStartX = tileSize * gridOffset;
            float centerTileStartY = tileSize * gridOffset;

            float pixelX = centerTileStartX + (float)(fracX * tileSize);
            float pixelY = centerTileStartY + (float)((1.0 - fracY) * tileSize); // Invert Y for texture coordinates

            // Calculate UV rect to display the portion of the texture centered on the point
            // The UV rect should show a pixelSize x pixelSize area centered on (pixelX, pixelY)
            float uvWidth = pixelSize / (float)textureSize;
            float uvHeight = pixelSize / (float)textureSize;

            float uvCenterX = pixelX / textureSize;
            float uvCenterY = pixelY / textureSize;

            Rect uvRect = new Rect(
                uvCenterX - uvWidth / 2f,
                uvCenterY - uvHeight / 2f,
                uvWidth,
                uvHeight
            );

            mapImage.uvRect = uvRect;
        }

        private IEnumerator FetchTileGrid(int centerX, int centerY, int zoom)
        {
            isFetchingTiles = true;
            int gridOffset = tileGridSize / 2;

            // Fetch grid of tiles centered on the center tile
            for (int dy = -gridOffset; dy <= gridOffset; dy++)
            {
                for (int dx = -gridOffset; dx <= gridOffset; dx++)
                {
                    int tileX = centerX + dx;
                    int tileY = centerY + dy;

                    // Calculate grid position
                    int gridX = dx + gridOffset;
                    int gridY = dy + gridOffset;

                    // Fetch individual tile
                    yield return FetchSingleTile(tileX, tileY, zoom, gridX, gridY);
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

                // Copy pixels to the correct position in the grid texture
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
                    Debug.LogWarning($"OSMMapDisplay: Tile size mismatch. Expected {tileSize}x{tileSize}, got {downloadedTexture.width}x{downloadedTexture.height}");
                }

                Destroy(downloadedTexture);
            }
            else
            {
                Debug.LogWarning($"OSMMapDisplay: Failed to fetch tile {x}/{y}/{zoom}: {request.error}");
            }

            request.Dispose();
        }

        #region Tile Coordinate Conversion

        private int LonToTileX(double lon, int zoom)
        {
            int n = 1 << zoom; // 2^zoom
            return (int)System.Math.Floor((lon + 180.0) / 360.0 * n);
        }

        private int LatToTileY(double lat, int zoom)
        {
            int n = 1 << zoom; // 2^zoom
            double latRad = lat * System.Math.PI / 180.0;
            return (int)System.Math.Floor((1.0 - System.Math.Log(System.Math.Tan(latRad) + 1.0 / System.Math.Cos(latRad)) / System.Math.PI) / 2.0 * n);
        }

        private double TileXToLon(int x, int zoom)
        {
            int n = 1 << zoom;
            return x / (double)n * 360.0 - 180.0;
        }

        private double TileYToLat(int y, int zoom)
        {
            int n = 1 << zoom;
            double latRad = System.Math.Atan(System.Math.Sinh(System.Math.PI * (1 - 2 * y / (double)n)));
            return latRad * 180.0 / System.Math.PI;
        }

        #endregion

        #region Public API

        /// <summary>
        /// Set custom tile server URL
        /// </summary>
        public void SetTileServer(string url)
        {
            tileServerUrl = url;
        }

        /// <summary>
        /// Get the current center latitude
        /// </summary>
        public double CenterLatitude => centerLatitude;

        /// <summary>
        /// Get the current center longitude
        /// </summary>
        public double CenterLongitude => centerLongitude;

        /// <summary>
        /// Get the current zoom level
        /// </summary>
        public int ZoomLevel => zoomLevel;

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
