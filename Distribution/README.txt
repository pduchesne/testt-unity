================================================================================
                        3D FLIGHT SIMULATOR
                          Version {{VERSION}}
================================================================================

ABOUT
-----
A realistic 3D flight simulator featuring:
- Real-world terrain streaming using Cesium for Unity
- Physics-based flight dynamics with realistic aerodynamics
- 3D building overlays from various tileset providers
- Comprehensive flight instrumentation
- Missile firing system
- Real-time minimap with OpenStreetMap integration


SYSTEM REQUIREMENTS
-------------------

Minimum:
  - OS: Windows 10/11, macOS 10.15+, Ubuntu 20.04+
  - Processor: Intel Core i5 or AMD Ryzen 5
  - Memory: 8 GB RAM
  - Graphics: NVIDIA GTX 1050 / AMD RX 560 / Intel UHD 630
  - DirectX: Version 11 (Windows)
  - Storage: 2 GB available space
  - Network: Broadband internet connection (for terrain streaming)

Recommended:
  - Processor: Intel Core i7 or AMD Ryzen 7
  - Memory: 16 GB RAM
  - Graphics: NVIDIA GTX 1660 / AMD RX 580 or better
  - Storage: SSD with 5 GB available space
  - Network: High-speed internet connection


INSTALLATION
------------

Windows:
  1. Extract the ZIP file to your desired location
  2. Run FlightSim.exe
  3. Allow firewall access if prompted (required for terrain streaming)

macOS:
  1. Mount the DMG file
  2. Drag FlightSim.app to your Applications folder
  3. On first launch, right-click the app and select "Open"
     (Required due to macOS security - you only need to do this once)
  4. Allow network access if prompted

Linux:
  1. Extract the tar.gz file: tar xzf FlightSim_Linux_*.tar.gz
  2. Make the executable runnable: chmod +x FlightSim.x86_64
  3. Run: ./FlightSim.x86_64
  4. Install required libraries if prompted:
     sudo apt-get install libgl1-mesa-glx libx11-6


CONTROLS
--------

Flight Controls:
  W / S         - Pitch Up / Down
  A / D         - Roll Left / Right
  Q / E         - Yaw Left / Right
  Shift / Ctrl  - Increase / Decrease Throttle
  Space         - Fire Missile
  R             - Reset Aircraft Position

Camera:
  Mouse         - Look Around (when right-click held)
  Mouse Scroll  - Zoom In / Out
  C             - Toggle Camera Mode

UI:
  Tab           - Toggle HUD
  M             - Toggle Minimap
  T             - Toggle Tileset Selector
  Escape        - Pause Menu / Exit

General:
  F11           - Toggle Fullscreen
  F12           - Take Screenshot


GAMEPLAY TIPS
-------------

1. Starting Flight:
   - Press Shift to increase throttle to at least 60%
   - Use W to pitch up once you reach takeoff speed (~80 km/h)
   - Maintain altitude by adjusting pitch and throttle

2. Basic Maneuvers:
   - Banking turns: Use A/D to roll, then W to pitch into the turn
   - Level flight: Keep the attitude indicator centered
   - Landing: Reduce throttle, descend gradually, and keep nose up

3. Avoiding Stalls:
   - Maintain sufficient airspeed (>60 km/h)
   - Watch the angle of attack indicator
   - If you stall, pitch down and increase throttle to recover

4. Using Missiles:
   - You have 10 missiles per session
   - Aim at buildings or terrain features
   - Wait for explosion before firing next missile

5. Navigation:
   - Use the minimap to see your position
   - The compass shows your heading
   - Altitude is displayed in meters above sea level


INTERNET CONNECTION
-------------------
This game requires an active internet connection to stream:
- High-resolution terrain data (Cesium World Terrain)
- 3D building tilesets (OSM Buildings, Google 3D Tiles, ArcGIS)
- OpenStreetMap tiles for the minimap

Without internet:
- The game will still run with cached terrain data
- Some areas may not load properly
- Minimap will show a placeholder


TROUBLESHOOTING
---------------

Performance Issues:
  - Lower graphics quality in the settings menu
  - Close other applications consuming network bandwidth
  - Ensure your GPU drivers are up to date
  - Reduce screen resolution

Terrain Not Loading:
  - Check your internet connection
  - Verify firewall isn't blocking the game
  - Try restarting the game
  - Move to a different location (some areas have better tile coverage)

Controls Not Responding:
  - Ensure the game window has focus
  - Check if a gamepad is connected (may override keyboard)
  - Try restarting the game

Crashes on Startup:
  - Update your graphics drivers
  - Verify all files extracted correctly
  - Check system requirements
  - Run as administrator (Windows) or with appropriate permissions


CREDITS
-------
Built with:
  - Unity 6
  - Cesium for Unity (terrain and 3D tiles streaming)
  - OpenStreetMap (minimap tiles)

Terrain Data: Cesium World Terrain
3D Buildings: OpenStreetMap Buildings, Google 3D Tiles, ArcGIS


SUPPORT & FEEDBACK
------------------
For bug reports, feature requests, or questions:
  - GitHub: https://github.com/pduchesne/geogame3d/issues
  - Email: [Your support email]


LICENSE
-------
[Your license information]


================================================================================
                          Enjoy your flight!
================================================================================
