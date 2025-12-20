# GeoGame3D - Quick Start

## TL;DR

1. Install Unity Hub + Unity 6 (or Unity 2022 LTS)
2. Create new 3D project in this directory via Unity Hub
3. Install Cesium for Unity: `https://github.com/CesiumGS/cesium-unity.git`
4. Sign up for Cesium ion, get access token
5. Connect token in Unity: Window → Cesium
6. Add to scene: Cesium Georeference + Cesium World Terrain
7. Press Play

## What You'll Get

A working 3D view of Earth's terrain that you can navigate with the camera.

## Current Status

- ✅ Project structure and documentation created
- ⏳ Unity project needs to be created (manual step via Unity Hub)
- ⏳ Cesium for Unity needs to be installed
- ⏳ Aircraft controller needs to be implemented

## Next Immediate Steps

1. **Create Unity Project** - See `SETUP_GUIDE.md` Step 2
2. **Install Cesium** - See `SETUP_GUIDE.md` Step 5
3. **Configure Cesium ion** - See `SETUP_GUIDE.md` Step 6
4. **Create Main Scene** - See `SETUP_GUIDE.md` Step 8
5. **Implement Aircraft Controller** - Coming next

## Files Created So Far

```
.gitignore           # Unity-specific git ignores
README.md            # Main project documentation
ARCHITECTURE.md      # Technical architecture and design
PROJECT_STATUS.md    # Current progress and milestones
SETUP_GUIDE.md       # Detailed setup instructions
QUICK_START.md       # This file
package.json         # NPM scripts and build info
```

## Tech Stack

- **Engine**: Unity 6 or Unity 2022 LTS
- **3D Terrain**: Cesium for Unity
- **Language**: C#
- **Input**: Unity Input System
- **Platform**: Desktop (Windows/macOS/Linux)

## Performance Goals

- **FPS**: 60+ on GTX 1060 equivalent
- **Memory**: <1GB VRAM
- **Network**: Broadband for tile streaming

## Project Structure (After Unity Creation)

```
geogame3d/
├── Assets/
│   ├── Scenes/
│   │   └── MainScene.unity
│   ├── Scripts/
│   │   ├── Aircraft/
│   │   │   ├── AircraftController.cs
│   │   │   └── FlightPhysics.cs
│   │   ├── Camera/
│   │   │   └── CameraRig.cs
│   │   └── UI/
│   │       └── FlightHUD.cs
│   └── Prefabs/
├── Packages/
│   └── manifest.json (will include Cesium)
├── ProjectSettings/
└── [Documentation files]
```

## Useful Commands

```bash
# Install npm dependencies (for build info)
npm install

# Generate build info
npm run build-info

# Unity project creation (via CLI, adjust path)
Unity -createProject "$PWD" -projectType 3D
```

## Help & Resources

- **Unity Docs**: https://docs.unity3d.com/
- **Cesium Docs**: https://cesium.com/learn/unity/
- **Project Status**: See `PROJECT_STATUS.md`
- **Detailed Setup**: See `SETUP_GUIDE.md`
- **Architecture**: See `ARCHITECTURE.md`

## Common Issues

**Q: Unity Hub doesn't show my project**
A: Use "Add" button and navigate to this directory after Unity creates the project files

**Q: Cesium terrain not loading**
A: Check Cesium ion token, internet connection, and Unity console for errors

**Q: Performance is low**
A: Adjust Quality Settings in Unity, reduce tile cache size in Cesium settings

## Milestone 1: PoC Checklist

- [ ] Unity project created
- [ ] Cesium for Unity installed
- [ ] Cesium ion token configured
- [ ] Main scene with terrain
- [ ] Aircraft controller implemented
- [ ] Input system configured
- [ ] Camera follow system
- [ ] 3D building tiles loaded
- [ ] HUD with speed/altitude
- [ ] 60 FPS achieved

**Target**: 1-2 weeks from now

---

For detailed instructions, see `SETUP_GUIDE.md`
