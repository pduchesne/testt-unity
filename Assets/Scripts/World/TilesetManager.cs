using UnityEngine;
using System.Collections.Generic;
using CesiumForUnity;

namespace GeoGame3D.World
{
    /// <summary>
    /// Manages multiple 3D tilesets and allows switching between different locations
    /// Supports both Cesium ion assets and custom tileset URLs (ArcGIS, etc.)
    /// </summary>
    public class TilesetManager : MonoBehaviour
    {
        [System.Serializable]
        public class TilesetConfig
        {
            public string name;
            public TilesetSource source;

            [Header("Cesium Ion Asset (if source = CesiumIon)")]
            public long ionAssetId;
            public string ionAccessToken;

            [Header("Custom URL (if source = CustomUrl)")]
            public string tilesetUrl;

            [Header("Starting Position")]
            public double latitude;
            public double longitude;
            public double height = 100.0; // Starting height above ground

            [Header("Display Settings")]
            public bool showOnStart = true;
            public float maximumScreenSpaceError = 16.0f;
        }

        public enum TilesetSource
        {
            CesiumIon,
            CustomUrl
        }

        [Header("References")]
        [SerializeField] private CesiumGeoreference georeference;
        [SerializeField] private Transform aircraftTransform;

        [Header("Tileset Configurations")]
        [SerializeField] private List<TilesetConfig> tilesets = new List<TilesetConfig>();

        [Header("Current Selection")]
        [SerializeField] private int currentTilesetIndex = 0;

        private Dictionary<int, Cesium3DTileset> loadedTilesets = new Dictionary<int, Cesium3DTileset>();

        private void Start()
        {
            if (georeference == null)
            {
                georeference = FindObjectOfType<CesiumGeoreference>();
                if (georeference == null)
                {
                    Debug.LogError("TilesetManager: No CesiumGeoreference found in scene!");
                    return;
                }
            }

            // Load and display initial tilesets
            for (int i = 0; i < tilesets.Count; i++)
            {
                if (tilesets[i].showOnStart)
                {
                    LoadTileset(i);
                }
            }

            // Position aircraft at current tileset
            if (currentTilesetIndex >= 0 && currentTilesetIndex < tilesets.Count)
            {
                PositionAircraftAtTileset(currentTilesetIndex);
            }
        }

        /// <summary>
        /// Load a tileset by index
        /// </summary>
        public void LoadTileset(int index)
        {
            if (index < 0 || index >= tilesets.Count)
            {
                Debug.LogError($"TilesetManager: Invalid tileset index {index}");
                return;
            }

            if (loadedTilesets.ContainsKey(index))
            {
                Debug.Log($"TilesetManager: Tileset {tilesets[index].name} already loaded");
                loadedTilesets[index].gameObject.SetActive(true);
                return;
            }

            TilesetConfig config = tilesets[index];

            // Create GameObject for tileset
            GameObject tilesetObj = new GameObject(config.name);
            tilesetObj.transform.SetParent(georeference.transform);
            tilesetObj.transform.localPosition = Vector3.zero;
            tilesetObj.transform.localRotation = Quaternion.identity;

            // Add Cesium3DTileset component
            Cesium3DTileset tileset = tilesetObj.AddComponent<Cesium3DTileset>();

            // Configure based on source type
            if (config.source == TilesetSource.CesiumIon)
            {
                // Use Cesium ion asset
                tileset.tilesetSource = CesiumDataSource.FromCesiumIon;
                tileset.ionAssetID = config.ionAssetId;

                if (!string.IsNullOrEmpty(config.ionAccessToken))
                {
                    tileset.ionAccessToken = config.ionAccessToken;
                }
            }
            else if (config.source == TilesetSource.CustomUrl)
            {
                // Use custom URL
                tileset.tilesetSource = CesiumDataSource.FromUrl;
                tileset.url = config.tilesetUrl;
            }

            // Set display settings
            tileset.maximumScreenSpaceError = config.maximumScreenSpaceError;

            loadedTilesets[index] = tileset;

            Debug.Log($"TilesetManager: Loaded tileset '{config.name}' from {config.source}");
        }

        /// <summary>
        /// Unload a tileset by index
        /// </summary>
        public void UnloadTileset(int index)
        {
            if (loadedTilesets.ContainsKey(index))
            {
                Destroy(loadedTilesets[index].gameObject);
                loadedTilesets.Remove(index);
                Debug.Log($"TilesetManager: Unloaded tileset {tilesets[index].name}");
            }
        }

        /// <summary>
        /// Switch to a different tileset and position aircraft there
        /// </summary>
        public void SwitchToTileset(int index)
        {
            if (index < 0 || index >= tilesets.Count)
            {
                Debug.LogError($"TilesetManager: Invalid tileset index {index}");
                return;
            }

            currentTilesetIndex = index;

            // Load if not already loaded
            if (!loadedTilesets.ContainsKey(index))
            {
                LoadTileset(index);
            }

            // Position aircraft
            PositionAircraftAtTileset(index);

            Debug.Log($"TilesetManager: Switched to tileset '{tilesets[index].name}'");
        }

        /// <summary>
        /// Position aircraft at the starting location of a tileset
        /// </summary>
        private void PositionAircraftAtTileset(int index)
        {
            if (index < 0 || index >= tilesets.Count)
            {
                return;
            }

            if (aircraftTransform == null)
            {
                aircraftTransform = FindObjectOfType<GeoGame3D.Aircraft.AircraftController>()?.transform;
                if (aircraftTransform == null)
                {
                    Debug.LogWarning("TilesetManager: No aircraft found to position");
                    return;
                }
            }

            TilesetConfig config = tilesets[index];

            // Update CesiumGeoreference origin to center the world at this location
            if (georeference != null)
            {
                georeference.latitude = config.latitude;
                georeference.longitude = config.longitude;
                georeference.height = config.height;
                Debug.Log($"TilesetManager: Set georeference origin to {config.latitude}, {config.longitude}, {config.height}m");
            }

            // Use CesiumGlobeAnchor to position aircraft
            CesiumGlobeAnchor anchor = aircraftTransform.GetComponent<CesiumGlobeAnchor>();
            if (anchor == null)
            {
                anchor = aircraftTransform.gameObject.AddComponent<CesiumGlobeAnchor>();
            }

            // Set position (same as georeference origin for aircraft to start at world center)
            anchor.longitudeLatitudeHeight = new Unity.Mathematics.double3(
                config.longitude,
                config.latitude,
                config.height
            );

            // Reset aircraft rotation to level flight heading north
            aircraftTransform.rotation = Quaternion.Euler(0, 0, 0);

            // Reset aircraft velocity if it has a Rigidbody
            Rigidbody rb = aircraftTransform.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            Debug.Log($"TilesetManager: Positioned aircraft at {config.latitude}, {config.longitude}, {config.height}m");
        }

        /// <summary>
        /// Toggle visibility of a tileset
        /// </summary>
        public void ToggleTilesetVisibility(int index, bool visible)
        {
            if (loadedTilesets.ContainsKey(index))
            {
                loadedTilesets[index].gameObject.SetActive(visible);
            }
            else if (visible)
            {
                LoadTileset(index);
            }
        }

        /// <summary>
        /// Get the current tileset configuration
        /// </summary>
        public TilesetConfig GetCurrentTileset()
        {
            if (currentTilesetIndex >= 0 && currentTilesetIndex < tilesets.Count)
            {
                return tilesets[currentTilesetIndex];
            }
            return null;
        }

        /// <summary>
        /// Add a new tileset configuration at runtime
        /// </summary>
        public void AddTileset(TilesetConfig config)
        {
            tilesets.Add(config);
            Debug.Log($"TilesetManager: Added tileset '{config.name}'");
        }

        #region Public Accessors

        public int CurrentTilesetIndex => currentTilesetIndex;
        public int TilesetCount => tilesets.Count;
        public TilesetConfig GetTileset(int index) => (index >= 0 && index < tilesets.Count) ? tilesets[index] : null;

        #endregion
    }
}
