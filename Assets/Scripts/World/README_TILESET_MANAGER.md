# Tileset Manager System

This document explains how to use and configure the TilesetManager system for loading multiple 3D tilesets and positioning the aircraft at different locations.

## Overview

The TilesetManager allows you to:
- Load multiple 3D tilesets from different sources (Cesium ion, ArcGIS, custom URLs)
- Switch between different geographic locations
- Automatically position the aircraft at the selected location
- Manage tileset visibility and loading

## Setup

### 1. TilesetManager GameObject

The TilesetManager is configured on a GameObject in the scene with:
- Reference to CesiumGeoreference
- Reference to the Aircraft transform
- List of tileset configurations

### 2. Tileset Configuration

Each tileset is configured with:

```csharp
TilesetConfig {
    name                     // Display name (e.g., "Utrecht 3D Buildings")
    source                   // TilesetSource.CesiumIon or TilesetSource.CustomUrl

    // For Cesium ion assets
    ionAssetId              // Asset ID from Cesium ion
    ionAccessToken          // Optional custom access token

    // For custom URLs (ArcGIS, etc.)
    tilesetUrl              // Full URL to tileset.json

    // Starting position
    latitude                // Latitude in degrees
    longitude               // Longitude in degrees
    height                  // Starting altitude in meters

    // Display settings
    showOnStart             // Load automatically on startup
    maximumScreenSpaceError // LOD quality (lower = higher quality)
}
```

## Current Tilesets

### Utrecht 3D Buildings (Index 0)
- **Source**: ArcGIS 3D Tiles (Custom URL)
- **Location**: Utrecht, Netherlands
- **Coordinates**: 52.0907°N, 5.1214°E
- **Starting Height**: 100m
- **URL**: https://tiles.arcgis.com/tiles/V6ZHFr6zdgNZuVG0/arcgis/rest/services/Utrecht_3D_Tiles_Integrated_Mesh/3DTilesServer/tileset.json

## Adding New Tilesets

### Option 1: Via Inspector (Easiest)

1. Select the TilesetManager GameObject in the scene
2. In the Inspector, expand the "Tileset Configurations" list
3. Click "+" to add a new element
4. Fill in the configuration:
   - Name: Give it a descriptive name
   - Source: Choose CesiumIon or CustomUrl
   - If CustomUrl: Paste the tileset.json URL
   - If CesiumIon: Enter the Asset ID
   - Set latitude, longitude, and height for the starting position
   - Check "Show On Start" if you want it to load automatically

### Option 2: Via Code

```csharp
var manager = FindObjectOfType<TilesetManager>();

var newTileset = new TilesetManager.TilesetConfig();
newTileset.name = "New York City";
newTileset.source = TilesetManager.TilesetSource.CesiumIon;
newTileset.ionAssetId = 75343; // Cesium OSM Buildings
newTileset.latitude = 40.7128;
newTileset.longitude = -74.0060;
newTileset.height = 200.0;
newTileset.showOnStart = false;

manager.AddTileset(newTileset);
```

## Switching Tilesets

### During Gameplay

```csharp
var manager = FindObjectOfType<TilesetManager>();

// Switch to tileset by index
manager.SwitchToTileset(0); // Utrecht
manager.SwitchToTileset(1); // Second tileset

// This will:
// 1. Load the tileset if not already loaded
// 2. Position the aircraft at the tileset's starting location
// 3. Reset aircraft velocity and rotation
```

### From Inspector

You can set the "Current Tileset Index" field in the Inspector before entering Play mode.

## Aircraft Positioning

When switching tilesets or starting the game, the TilesetManager:

1. Finds or adds a CesiumGlobeAnchor component to the aircraft
2. Sets the position using latitude, longitude, and height
3. Resets the aircraft rotation to level flight (heading north)
4. Clears the aircraft velocity (if it has a Rigidbody)

## Finding Tileset URLs

### ArcGIS 3D Tiles
1. Browse https://www.arcgis.com/home/search.html?q=3D%20tiles
2. Look for "Scene Service" items
3. Open the service details and find the REST endpoint
4. The URL should end with `/SceneServer` or `/3DTilesServer/tileset.json`

### Cesium ion
1. Go to https://ion.cesium.com/assets
2. Browse or search for assets
3. Note the Asset ID (shown in the asset details)
4. Use TilesetSource.CesiumIon with the asset ID

### Custom 3D Tiles
Any server hosting a valid 3D Tiles tileset.json can be used with the CustomUrl source.

## Performance Tips

- **maximumScreenSpaceError**: Higher values (24-32) = better performance, lower quality
- **maximumScreenSpaceError**: Lower values (8-16) = worse performance, higher quality
- Default of 16 is a good balance

- Only set **showOnStart = true** for tilesets you need immediately
- Other tilesets will load on demand when you switch to them

## API Reference

### Public Methods

```csharp
// Load a tileset by index (if not already loaded)
void LoadTileset(int index)

// Unload a tileset by index
void UnloadTileset(int index)

// Switch to a tileset and position aircraft there
void SwitchToTileset(int index)

// Toggle visibility without unloading
void ToggleTilesetVisibility(int index, bool visible)

// Get tileset configuration
TilesetConfig GetTileset(int index)
TilesetConfig GetCurrentTileset()

// Add new tileset at runtime
void AddTileset(TilesetConfig config)
```

### Public Properties

```csharp
int CurrentTilesetIndex { get; }
int TilesetCount { get; }
```

## Example Scenarios

### Scenario 1: Tour Multiple Cities

Configure multiple tilesets for different cities:
- Index 0: Utrecht, Netherlands
- Index 1: New York City, USA
- Index 2: Paris, France
- Index 3: Tokyo, Japan

Create a UI with buttons to switch between them:

```csharp
public class TilesetSelector : MonoBehaviour
{
    private TilesetManager manager;

    void Start()
    {
        manager = FindObjectOfType<TilesetManager>();
    }

    public void OnCitySelected(int cityIndex)
    {
        manager.SwitchToTileset(cityIndex);
    }
}
```

### Scenario 2: Compare Data Sources

Load the same location from different providers:
- Cesium OSM Buildings
- Google 3D Tiles
- Custom photogrammetry data

Toggle between them to compare quality and coverage.

## Troubleshooting

**Tileset doesn't load**
- Check the Console for errors
- Verify the URL is accessible (test in browser)
- For Cesium ion: Verify the asset ID and access token

**Aircraft spawns in wrong location**
- Check latitude/longitude values (use Google Maps to verify)
- Latitude: -90 to +90 (negative = South, positive = North)
- Longitude: -180 to +180 (negative = West, positive = East)

**Aircraft falls through ground**
- Increase the starting height value
- Wait for terrain to load before flying (tilesets stream progressively)

**Poor performance**
- Increase maximumScreenSpaceError value
- Unload unused tilesets
- Reduce number of simultaneously loaded tilesets
