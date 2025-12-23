# Minimap Implementation

## Overview

The minimap system is built using a modular architecture with two main components:

1. **OSMMapDisplay** - A reusable component that renders north-oriented OpenStreetMap tiles
2. **MinimapController** - HUD integration that handles rotation, cropping, and aircraft overlay

## Architecture

### OSMMapDisplay.cs
A standalone, reusable component for displaying OSM map tiles:
- **Input**: mapCenter (lat/lon), zoomLevel, pixelSize
- **Output**: North-oriented map display
- **Features**:
  - Fetches OSM tiles on demand
  - Handles tile grid management (dynamic sizing)
  - Uses UV rect manipulation for precise centering
  - No knowledge of aircraft or game logic

### MinimapController.cs
HUD integration controller:
- Uses OSMMapDisplay for rendering
- Converts aircraft Unity position to geospatial coordinates (via CesiumGeoreference)
- Updates map center based on aircraft position
- Applies rotation to entire minimap container based on aircraft heading
- Manages aircraft icon overlay (always centered, pointing up)

## Hierarchy Structure

```
Minimap (GameObject with MinimapController)
└── MapDisplay (GameObject with OSMMapDisplay + RawImage)
    └── MinimapContainer (RectTransform with RectMask2D for clipping)
        └── AircraftIcon (Image, centered, no rotation)
```

## Setup Instructions

### Automatic Setup (Recommended)

1. Open Unity Editor
2. Go to menu: **GeoGame3D → Setup Minimap**
3. The setup script will automatically:
   - Find the Minimap GameObject
   - Create the proper hierarchy
   - Add required components
   - Wire up all references
   - Configure transforms and sizing

### Manual Setup

If you need to set it up manually:

1. **Create Minimap root**:
   - Add MinimapController component
   - Assign aircraftTransform and georeference references

2. **Create MapDisplay child**:
   - Add RectTransform
   - Add RawImage component
   - Add OSMMapDisplay component
   - Assign mapImage reference to RawImage
   - Position in bottom-left corner (anchoredPosition: 100, 100)
   - Size: 200x200 pixels

3. **Create MinimapContainer child of MapDisplay**:
   - Add RectTransform
   - Add RectMask2D for clipping
   - Center it (anchor: 0.5, 0.5)
   - Size: 200x200 pixels

4. **Create AircraftIcon child of MinimapContainer**:
   - Add RectTransform
   - Add Image component
   - Center it (anchor: 0.5, 0.5)
   - Size: 20x20 pixels
   - Set color to red (or your preference)

5. **Wire up MinimapController references**:
   - mapDisplay → OSMMapDisplay component
   - minimapContainer → MinimapContainer RectTransform
   - aircraftIcon → AircraftIcon RectTransform

## Configuration

### MinimapController Settings

- **zoomLevel**: OSM zoom level (1-19, default: 15)
  - Lower values = wider area, less detail
  - Higher values = smaller area, more detail
  - 15 is good for general flight navigation

- **mapPixelSize**: Size of the map texture in pixels (default: 512)
  - Larger values = smoother map, more memory
  - Should be power of 2 for best performance

- **rotateWithAircraft**: Whether to rotate map with aircraft heading
  - true: Map rotates, aircraft icon always points up (better for navigation)
  - false: Map stays north-up, aircraft icon rotates

- **aircraftIconColor**: Color of the aircraft marker

### OSMMapDisplay Settings

- **tileSize**: OSM tile size (256 pixels, standard)
- **tileServerUrl**: URL template for tile server
  - Default: `https://tile.openstreetmap.org/{z}/{x}/{y}.png`
  - Can use other tile servers (ensure you follow their usage policies)

## How It Works

### Update Flow (Every Frame)

1. **UpdateAircraftPosition()**:
   - Get aircraft Unity position
   - Transform to ECEF coordinates (CesiumGeoreference)
   - Convert ECEF to LLA (longitude, latitude, altitude)
   - Store currentLongitude and currentLatitude

2. **UpdateMapDisplay()**:
   - Call OSMMapDisplay.UpdateMap(lat, lon, zoom, size)
   - OSMMapDisplay calculates which tiles are needed
   - Fetches tiles if center tile changed
   - Updates UV rect to center map on exact coordinate

3. **UpdateAircraftIcon()**:
   - Keep aircraft icon centered in container
   - Rotate entire minimapContainer based on aircraft heading
   - Aircraft icon itself never rotates (always points up)

### Tile Fetching

OSMMapDisplay manages tile fetching:
- Calculates required tile grid size based on pixelSize
- Fetches tiles in a grid centered on the map center
- Only fetches new tiles when crossing tile boundaries
- Uses coroutines to avoid blocking the main thread
- Respects OSM tile usage policy (sets User-Agent header)

### Coordinate Systems

The implementation handles multiple coordinate systems:

1. **Unity World Space**: Aircraft position in Unity scene
2. **ECEF (Earth-Centered, Earth-Fixed)**: 3D Cartesian coordinates
3. **LLA (Longitude, Latitude, Altitude)**: Geographic coordinates
4. **Tile Coordinates**: OSM slippy map tile indices (x, y, zoom)
5. **Pixel Coordinates**: Position within tile grid texture

Conversions:
- Unity → ECEF: `CesiumGeoreference.TransformUnityPositionToEarthCenteredEarthFixed()`
- ECEF → LLA: `CesiumWgs84Ellipsoid.EarthCenteredEarthFixedToLongitudeLatitudeHeight()`
- LLA → Tile: Web Mercator projection formulas

## Customization

### Using Different Tile Servers

You can use alternative tile servers:

```csharp
minimapController.SetTileServer("https://your-tile-server.com/{z}/{x}/{y}.png");
```

Popular alternatives:
- **OpenTopoMap**: `https://tile.opentopomap.org/{z}/{x}/{y}.png` (topographic)
- **CartoDB**: `https://cartodb-basemaps-{s}.global.ssl.fastly.net/light_all/{z}/{x}/{y}.png` (light theme)

Note: Always check and comply with the tile server's usage policies and terms of service.

### Adjusting Zoom Level

You can change zoom level at runtime:

```csharp
minimapController.SetZoomLevel(17); // More detail
minimapController.SetZoomLevel(13); // Wider area
```

### Toggle Rotation Mode

```csharp
minimapController.ToggleRotationMode(); // Switch between rotating and north-up
```

## Performance Considerations

- **Tile Fetching**: Tiles are fetched asynchronously and cached in the texture
- **Update Frequency**: Map updates every frame, but only fetches new tiles when needed
- **Memory**: Texture size is `(tileSize * tileGridSize)^2 * 3 bytes`
  - Default: (256 * 4)^2 * 3 ≈ 3 MB (with mapPixelSize=512)
- **Network**: Only fetches tiles when crossing tile boundaries
  - With zoom 15, a tile covers ~4.8 km at equator
  - Typical flight might fetch 10-20 tiles total

## Troubleshooting

### Map not visible
- Check that mapImage RawImage has a texture assigned
- Verify OSMMapDisplay component is active and enabled
- Check console for tile fetch errors

### Aircraft icon not rotating
- Verify rotateWithAircraft is true
- Check that minimapContainer reference is assigned
- Ensure aircraftTransform is valid

### Map not updating
- Check that georeference is assigned
- Verify aircraft is moving in the scene
- Check console for coordinate conversion errors

### Tiles not loading
- Check internet connection
- Verify tile server URL is correct
- Check console for HTTP errors
- Ensure User-Agent header is set (OSM requirement)

## Future Enhancements

Possible improvements:
- Tile caching to disk for offline use
- Multiple map layers (streets, satellite, terrain)
- Waypoint markers on minimap
- Adjustable minimap size/position
- Minimap visibility toggle
- Custom aircraft icon sprites
- Airport/landmark labels
