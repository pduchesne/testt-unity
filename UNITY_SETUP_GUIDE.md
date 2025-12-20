# Unity Setup Guide for GeoGame3D

This guide will walk you through installing Unity and setting up the GeoGame3D project.

## Prerequisites

- Linux system (Ubuntu/Debian-based)
- ~10 GB free disk space
- Broadband internet connection

## Step 1: Install Unity Hub

Unity Hub is the centralized tool for managing Unity versions and projects.

### Option A: Download from Unity Website (Recommended)

1. Visit: https://unity.com/download
2. Click "Download Unity Hub"
3. Download the Linux AppImage or .deb package
4. Install:

```bash
# For AppImage (no installation needed, just make executable):
chmod +x UnityHub.AppImage
./UnityHub.AppImage

# For .deb package:
sudo dpkg -i UnityHub.deb
sudo apt-get install -f  # Fix any dependency issues
```

### Option B: Install via APT (if available)

```bash
# Add Unity repository (check Unity's official instructions for latest commands)
wget -qO - https://hub.unity3d.com/linux/keys/public | gpg --dearmor | sudo tee /usr/share/keyrings/unity-archive-keyring.gpg > /dev/null
echo 'deb [signed-by=/usr/share/keyrings/unity-archive-keyring.gpg] https://hub.unity3d.com/linux/repos/deb stable main' | sudo tee /etc/apt/sources.list.d/unityhub.list
sudo apt update
sudo apt install unityhub
```

### Verify Installation

```bash
# If installed system-wide:
unityhub --version

# If using AppImage, launch it and check Help → About
```

## Step 2: Install Unity Editor

Once Unity Hub is running:

1. **Launch Unity Hub**
2. **Sign in or create Unity account** (free account is fine)
3. **Get a free personal license**:
   - Click on the gear icon (Settings)
   - Go to "License Management"
   - Click "Activate New License"
   - Choose "Unity Personal" → "I don't use Unity in a professional capacity"

4. **Install Unity Editor**:
   - Click "Installs" tab in Unity Hub
   - Click "Install Editor" or "Add" button
   - **Recommended:** Choose **Unity 6** (latest LTS) or **Unity 2022 LTS**
   - Select modules to install:
     - ✅ Linux Build Support (Mono)
     - ✅ Documentation
     - ✅ Linux IL2CPP Build Support (optional, for production builds)
     - ✅ WebGL Build Support (optional, if you want web builds)
   - Click "Install" and wait (this will take 10-30 minutes depending on your connection)

## Step 3: Create Unity Project

### Method 1: Via Unity Hub (Recommended)

1. In Unity Hub, click "Projects" tab
2. Click "New Project" button
3. Select the Unity version you just installed
4. Choose template: **"3D Core"** or **"3D (URP)"** (Universal Render Pipeline)
5. **Project Name:** GeoGame3D
6. **Location:** `/home/pduchesne/Sources/claude/hilats/geogame3d`
7. Click "Create Project"
8. Wait for Unity Editor to open and complete initial setup

### Method 2: Via Command Line (Alternative)

```bash
cd /home/pduchesne/Sources/claude/hilats/geogame3d

# Find Unity installation path (example):
UNITY_PATH="/opt/Unity/Hub/Editor/2022.3.XX/Editor/Unity"  # Adjust version

# Create project:
$UNITY_PATH -createProject "$(pwd)" -projectType 3D -quit
```

### Verify Project Creation

After project creation, you should see new directories:
```bash
ls -la
# Should show:
# Assets/
# Library/
# Packages/
# ProjectSettings/
# Temp/
# UserSettings/
```

## Step 4: Install Cesium for Unity

With the Unity project open in Unity Editor:

1. **Via Package Manager (Easiest)**:
   - In Unity Editor: `Window → Package Manager`
   - Click the `+` button (top-left)
   - Select "Add package from git URL..."
   - Enter: `https://github.com/CesiumGS/cesium-unity.git`
   - Click "Add"
   - Wait for installation (2-5 minutes)

2. **Via manifest.json (Alternative)**:
   - Close Unity Editor
   - Edit `Packages/manifest.json`
   - Add to dependencies:
     ```json
     {
       "dependencies": {
         "com.cesium.unity": "https://github.com/CesiumGS/cesium-unity.git",
         ...existing packages...
       }
     }
     ```
   - Reopen Unity Editor
   - Unity will auto-install the package

3. **Verify Installation**:
   - Check Unity menu bar for: `Cesium` menu item
   - In Package Manager, you should see "Cesium for Unity"

## Step 5: Get Cesium ion Access Token

1. **Create Cesium ion Account**:
   - Visit: https://cesium.com/ion/signup
   - Sign up for a free account
   - Verify your email

2. **Create Access Token**:
   - Log in to https://cesium.com/ion/
   - Go to "Access Tokens" (in your account menu)
   - Click "Create Token"
   - Name: "GeoGame3D Development"
   - Scopes: Select "assets:read" (default is fine)
   - Click "Create"
   - **Copy the token** (you won't see it again!)

3. **Configure Token in Unity**:
   - In Unity Editor: `Window → Cesium`
   - In the Cesium panel, click "Connect to Cesium ion"
   - Paste your access token
   - Click "Connect"

## Step 6: Verify Installation

### Create a Test Scene:

1. In Unity Editor: `File → New Scene`
2. In Hierarchy window, right-click → `Cesium → Cesium Georeference`
3. Right-click again → `Cesium → Cesium World Terrain`
4. Position the Scene view camera to see the globe
5. Click Play button
6. You should see Earth's terrain streaming!

### If you see terrain → ✅ Setup Complete!

## Next Steps

After completing this setup, return to the terminal and let me know. I will:
1. Configure the Unity project structure for GeoGame3D
2. Install Unity Input System package
3. Set up the initial scene with proper components
4. Begin implementing the aircraft controller

## Troubleshooting

### Unity Hub won't start
- Try running from terminal to see error messages
- Check if required graphics drivers are installed

### Cesium package installation fails
- Check internet connection
- Try manual download from https://github.com/CesiumGS/cesium-unity/releases
- Import as local package via Package Manager

### Terrain not loading
- Verify Cesium ion token is configured correctly
- Check Unity Console for error messages
- Ensure internet connection is active

### Performance issues
- Update graphics drivers
- Lower Quality settings: `Edit → Project Settings → Quality`
- Reduce Cesium's "Maximum Screen Space Error" value

## Useful Resources

- Unity Manual: https://docs.unity3d.com/Manual/
- Cesium for Unity Quickstart: https://cesium.com/learn/unity/unity-quickstart/
- Unity Hub Manual: https://docs.unity3d.com/hub/manual/

---

**Estimated Time:** 45-90 minutes (depending on download speeds)

Once you complete these steps, we'll proceed with automated project configuration!
