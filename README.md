# GeoGame3D - 3D Flight Simulation Game

[![Build Multi-Platform](https://github.com/pduchesne/geogame3d/actions/workflows/build.yml/badge.svg)](https://github.com/pduchesne/geogame3d/actions/workflows/build.yml)
[![Unity Version](https://img.shields.io/badge/Unity-6000.3.2f1-blue.svg)](https://unity.com/)
[![License](https://img.shields.io/badge/license-TBD-lightgrey.svg)](LICENSE)

A high-performance 3D flight simulation game built with Unity and Cesium for Unity, featuring dynamic terrain and building loading from open standard datasets.

## Overview

GeoGame3D allows players to fly aircraft over real-world terrain and buildings with realistic physics and a strong sense of speed and maneuverability. The game leverages open geospatial standards like 3D Tiles and WMTS for dynamic content streaming.

## Tech Stack

- **Unity 6** (or Unity 2022 LTS) - Game engine
- **Cesium for Unity** - Geospatial 3D rendering and terrain streaming
- **C#** - Game logic and scripting
- **Unity Input System** - Modern input handling
- **3D Tiles** - Streaming 3D building models
- **WMTS** - Terrain texture streaming

## Project Milestones

### Milestone 1: PoC - Basic Flight & Terrain ✓ (Current)
- [ ] Unity project initialization
- [ ] Cesium for Unity integration
- [ ] Basic terrain loading
- [ ] Simple aircraft controller
- [ ] Keyboard/mouse flight controls
- [ ] Performance validation with real-world data

### Milestone 2: Flight Mechanics
- Realistic flight physics (pitch, roll, yaw)
- Speed control and momentum
- Terrain collision detection
- Camera effects (motion blur, FOV changes)
- HUD with altitude, speed, heading

### Milestone 3: Dynamic Content Loading
- Stream 3D building tilesets
- Multiple terrain data sources
- Dynamic LOD management
- Performance optimization

### Milestone 4: Game Features
- Mission/objective system
- Points of interest
- Weather effects
- Audio system
- Settings UI

### Milestone 5: Polish & Release
- Performance profiling
- Multiple aircraft models
- Save/load system
- Documentation and deployment

## Setup Instructions

### Prerequisites

1. **Unity Hub** - Download from [unity.com](https://unity.com/download)
2. **Unity Editor** - Install Unity 6 or Unity 2022 LTS via Unity Hub
3. **Git** - For version control
4. **Cesium ion account** (free) - Create at [cesium.com/ion](https://cesium.com/ion/)

### Initial Setup

1. **Create Unity Project**
   ```bash
   # Option 1: Via Unity Hub (Recommended)
   # - Open Unity Hub
   # - Click "New Project"
   # - Select "3D Core" template
   # - Name: GeoGame3D
   # - Location: This directory
   # - Unity Version: Unity 6 or Unity 2022 LTS

   # Option 2: Via Command Line
   # Unity -createProject "/path/to/geogame3d" -projectType 3D
   ```

2. **Install Cesium for Unity**
   - Open the project in Unity
   - Go to Window → Package Manager
   - Click the "+" button → "Add package from git URL"
   - Enter: `https://github.com/CesiumGS/cesium-unity.git`
   - Or add to `Packages/manifest.json`:
     ```json
     {
       "dependencies": {
         "com.cesium.unity": "https://github.com/CesiumGS/cesium-unity.git"
       }
     }
     ```

3. **Configure Cesium ion Token**
   - Sign up at [cesium.com/ion](https://cesium.com/ion/)
   - Create an access token
   - In Unity: Window → Cesium → Cesium ion Assets
   - Paste your token when prompted

4. **Install Unity Input System**
   - Package Manager → Unity Registry
   - Find "Input System"
   - Click Install
   - Allow Unity to restart when prompted

### Project Structure

```
geogame3d/
├── Assets/
│   ├── Scenes/           # Game scenes
│   ├── Scripts/          # C# scripts
│   │   ├── Aircraft/     # Aircraft controllers
│   │   ├── Camera/       # Camera systems
│   │   └── UI/           # User interface
│   ├── Prefabs/          # Reusable game objects
│   └── Materials/        # Visual materials
├── Packages/             # Unity packages
├── ProjectSettings/      # Unity project settings
└── README.md
```

## Development

### Running the Game

1. Open the project in Unity
2. Open the main scene: `Assets/Scenes/MainScene.unity`
3. Click the Play button in the Unity Editor

### Building

The project includes an automated multi-platform build system for Windows, macOS, and Linux.

#### Quick Build (Local)

```bash
# Build for specific platform
./scripts/build.sh windows 1.0.0
./scripts/build.sh mac 1.0.0
./scripts/build.sh linux 1.0.0

# Build all platforms
./scripts/build.sh all 1.0.0
```

Output: `Builds/FlightSim_{Platform}_v{Version}.{ext}`

#### Automated Builds (CI/CD)

Push a git tag to trigger automated builds for all platforms:

```bash
git tag v1.0.0
git push origin v1.0.0
```

This creates a GitHub Release with Windows, macOS, and Linux builds.

#### Requirements

- **Unity 6000.3.2f1**
- **CMake 3.18+**
- **.NET SDK 6.0+**
- **Platform-specific compilers** (MSVC/Xcode/GCC)

See [Build System Documentation](docs/BUILD_SYSTEM.md) for detailed setup and troubleshooting.

#### Manual Build (Unity Editor)

1. File → Build Settings
2. Select target platform (Windows, macOS, Linux)
3. Click "Build" or "Build and Run"

## Key Features

### Dynamic Terrain Loading
- Real-world terrain via Cesium World Terrain or custom WMTS sources
- Automatic level-of-detail (LOD) streaming
- Efficient memory management

### 3D Tiles Integration
- Stream building models from Cesium ion or custom sources
- Support for OGC 3D Tiles specification
- Optimized rendering for large datasets

### Flight Physics
- Realistic aircraft movement
- Physics-based controls
- Smooth camera following with banking effects

## Resources

- [Unity Documentation](https://docs.unity3d.com/)
- [Cesium for Unity Documentation](https://cesium.com/learn/unity/)
- [3D Tiles Specification](https://www.ogc.org/standards/3dtiles)
- [Unity Input System Guide](https://docs.unity3d.com/Packages/com.unity.inputsystem@latest)

## License

TBD

## Contributors

TBD
