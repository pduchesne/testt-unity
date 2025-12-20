# GeoGame3D Setup Guide

This guide walks you through setting up the Unity project for GeoGame3D.

## Step 1: Install Unity

### Option A: Unity Hub (Recommended)

1. Download Unity Hub from [unity.com/download](https://unity.com/download)
2. Install Unity Hub
3. Open Unity Hub
4. Go to "Installs" tab
5. Click "Install Editor"
6. Select one of:
   - **Unity 6** (recommended for new features)
   - **Unity 2022 LTS** (recommended for stability)
7. In the modules selection, make sure to include:
   - **Visual Studio** (Windows) or **Visual Studio for Mac** (macOS)
   - **Linux Build Support** (if on Windows/Mac)
   - **Documentation** (optional but helpful)

### Option B: Unity Editor Standalone

Download directly from [unity.com](https://unity.com/releases/editor/archive)

## Step 2: Create the Unity Project

### Option A: Via Unity Hub (Easiest)

1. Open Unity Hub
2. Click the "Projects" tab
3. Click "New Project"
4. In the creation dialog:
   - **Template**: Select "3D Core" or "3D"
   - **Project Name**: GeoGame3D
   - **Location**: Navigate to and select `/home/pduchesne/Sources/claude/hilats/geogame3d`
   - **Unity Version**: Select Unity 6 or Unity 2022 LTS
5. Click "Create Project"

Unity will create the project structure and open the editor. This may take a few minutes.

### Option B: Via Command Line

```bash
# Set Unity editor path (adjust for your installation)
UNITY_PATH="/Applications/Unity/Hub/Editor/2022.3.50f1/Unity.app/Contents/MacOS/Unity"  # macOS
# or
UNITY_PATH="C:/Program Files/Unity/Hub/Editor/2022.3.50f1/Editor/Unity.exe"  # Windows
# or
UNITY_PATH="~/Unity/Hub/Editor/2022.3.50f1/Editor/Unity"  # Linux

# Create the project
"$UNITY_PATH" -createProject "/home/pduchesne/Sources/claude/hilats/geogame3d" -projectType 3D

# Or with Unity 6
"$UNITY_PATH" -createProject "/home/pduchesne/Sources/claude/hilats/geogame3d"
```

## Step 3: Verify Project Structure

After creation, you should see:

```
geogame3d/
├── Assets/              # Your game assets (scenes, scripts, prefabs)
├── Library/            # Unity's cache (ignored by git)
├── Packages/           # Package dependencies
├── ProjectSettings/    # Unity project configuration
├── Temp/              # Temporary files (ignored by git)
├── .gitignore         # Already created
├── README.md          # Already created
└── ARCHITECTURE.md    # Already created
```

## Step 4: Configure Project Settings

1. Open the project in Unity Editor
2. Go to **Edit → Project Settings**
3. Configure the following:

### Player Settings
- **Company Name**: Your name or studio
- **Product Name**: GeoGame3D
- **Version**: 0.1.0

### Quality Settings
- Set default quality level to "High" or "Ultra"
- Enable VSync for now (can disable later for performance testing)

### Time Settings
- **Fixed Timestep**: 0.02 (50 physics updates/second)

## Step 5: Install Cesium for Unity

### Method 1: Git URL (Recommended)

1. In Unity Editor, go to **Window → Package Manager**
2. Click the **"+" button** in the top-left
3. Select **"Add package from git URL"**
4. Enter: `https://github.com/CesiumGS/cesium-unity.git`
5. Click **"Add"**
6. Wait for package to download and install

### Method 2: Manual Package Manifest

1. Close Unity Editor
2. Open `Packages/manifest.json` in a text editor
3. Add this line to the `"dependencies"` section:
   ```json
   "com.cesium.unity": "https://github.com/CesiumGS/cesium-unity.git"
   ```
4. Save the file
5. Reopen Unity (it will download and install the package)

### Verify Installation

1. In Unity, go to **Window → Package Manager**
2. Change dropdown from "Packages: In Project" to "Packages: All"
3. You should see "Cesium for Unity" in the list
4. Click on it to see version and documentation

## Step 6: Set Up Cesium ion

Cesium ion provides free access to terrain and 3D building data.

1. Create a free account at [cesium.com/ion](https://cesium.com/ion/signup)
2. After logging in, go to **Access Tokens**
3. Click **"Create Token"**
4. Name it: "GeoGame3D Development"
5. Leave default scopes (assets:read, assets:list, etc.)
6. Click **"Create"**
7. **Copy the token** (you won't be able to see it again)

### Configure Token in Unity

1. In Unity, go to **Window → Cesium**
2. If prompted, click **"Sign in to Cesium ion"**
3. Click **"Connect to Cesium ion"**
4. **Paste your access token**
5. Click **"Connect"**

You should see "Connected to Cesium ion" with your account info.

## Step 7: Install Unity Input System

1. Go to **Window → Package Manager**
2. Change to **"Packages: Unity Registry"**
3. Find **"Input System"**
4. Click **"Install"**
5. Unity will show a warning about the old input system
6. Click **"Yes"** to enable the new Input System
7. Unity will restart

## Step 8: Create Initial Scene

1. In the Project window, go to `Assets/`
2. Right-click → **Create → Folder**, name it `Scenes`
3. Right-click in `Scenes/` → **Create → Scene**
4. Name it `MainScene`
5. Double-click to open it
6. **File → Save** (or Ctrl/Cmd + S)

## Step 9: Add Cesium Components to Scene

1. In the Hierarchy, right-click → **3D Object → Cesium → Cesium Georeference**
   - This sets up the world coordinate system
2. Right-click again → **3D Object → Cesium → Cesium World Terrain**
   - This adds global terrain streaming
3. Select the Main Camera in Hierarchy
4. In Inspector, set position:
   - X: 0, Y: 1000, Z: 0 (start 1km above ground)

## Step 10: Test the Scene

1. Click the **Play button** at the top of the editor
2. You should see terrain loading below the camera
3. Press **Play** again to stop

If you see terrain, congratulations! Your Unity + Cesium setup is working.

## Step 11: Set Up Version Control (Optional but Recommended)

Already configured via .gitignore!

```bash
git add .
git commit -m "Initial Unity project setup with Cesium for Unity"
```

## Troubleshooting

### "Package resolution error" for Cesium
- Make sure you have internet connection
- Try removing and re-adding the package
- Check [Cesium for Unity GitHub](https://github.com/CesiumGS/cesium-unity) for latest URL

### "Failed to load terrain"
- Verify Cesium ion token is correctly configured
- Check internet connection
- Try refreshing the connection: Window → Cesium → Disconnect, then reconnect

### "Input System errors"
- Make sure you installed Input System package
- Restart Unity after installation
- Check Project Settings → Player → Active Input Handling is set to "Both" or "Input System Package (New)"

### Unity crashes on startup
- Check Unity version compatibility
- Try creating with Unity 2022 LTS instead of Unity 6
- Verify system meets minimum requirements

## Next Steps

Once setup is complete, proceed to:
1. Implement aircraft controller script
2. Set up flight controls
3. Add camera system
4. Create HUD

See `ARCHITECTURE.md` for technical details and `PROJECT_STATUS.md` for current progress.

## Resources

- [Unity Learn](https://learn.unity.com/)
- [Cesium for Unity Quickstart](https://cesium.com/learn/unity/unity-quickstart/)
- [Unity Input System Documentation](https://docs.unity3d.com/Packages/com.unity.inputsystem@latest)
- [Cesium Community Forum](https://community.cesium.com/)
