# GeoGame3D Scene Setup Guide

This guide will walk you through setting up the main flight scene in Unity Editor.

## Prerequisites

- ✅ Unity project open
- ✅ Cesium for Unity installed
- ✅ Cesium ion token configured
- ✅ Scripts created (AircraftController, CameraRig, FlightHUD)

## Step 1: Create Main Scene

1. In Unity Editor: **File → New Scene**
2. Choose **"Basic (Built-in)"** or **"Basic (URP)"** template
3. Save the scene: **File → Save As**
   - Location: `Assets/Scenes/`
   - Name: `MainScene`

## Step 2: Add Cesium Components

### Add Cesium Georeference

1. In **Hierarchy** window, right-click → **Cesium → Cesium Georeference**
2. This creates a "CesiumGeoreference" GameObject
3. In the **Inspector**, configure:
   - **Latitude:** 50.85 (Brussels, or choose your preferred location)
   - **Longitude:** 4.35
   - **Height:** 1000 (starting altitude in meters)

### Add Cesium World Terrain

1. In **Hierarchy**, right-click → **Cesium → Cesium World Terrain**
2. This creates terrain that streams from Cesium ion
3. No configuration needed - it should work automatically

### (Optional) Add Cesium 3D Tiles for Buildings

1. In **Hierarchy**, right-click → **Cesium → Cesium 3D Tileset**
2. Rename it to "Buildings"
3. In **Inspector**:
   - Click **"Select from Cesium ion"**
   - Choose **"Cesium OSM Buildings"** (free)
   - Or **"Google Photorealistic 3D Tiles"** if available

## Step 3: Create Aircraft GameObject

1. In **Hierarchy**, right-click → **Create Empty**
2. Rename it to **"Aircraft"**
3. Set **Position:** (0, 0, 0) - will be positioned by Georeference
4. Add components:
   - Click **Add Component** → search for "Rigidbody"
   - Set **Mass:** 1000
   - Set **Drag:** 0.3
   - Set **Angular Drag:** 0.5
   - **Uncheck** "Use Gravity"
5. Add the controller:
   - Click **Add Component** → search for "Aircraft Controller"
   - Configure in Inspector:
     - **Max Speed:** 200
     - **Acceleration:** 20
     - **Pitch Speed:** 45
     - **Roll Speed:** 90
     - **Yaw Speed:** 30

### Add Visual Model (Temporary)

For the PoC, use a simple cube as aircraft placeholder:

1. Right-click **Aircraft** → **3D Object → Cube**
2. Rename to "Model"
3. Set **Scale:** (2, 0.5, 4) - makes it look more like an aircraft
4. **Optional:** Change color:
   - Create material: **Assets → Create → Material**
   - Name it "AircraftMaterial"
   - Set **Albedo** color to blue or red
   - Drag material onto the Cube

## Step 4: Set Up Camera

1. Select the existing **"Main Camera"** in Hierarchy
2. Add component:
   - **Add Component** → search for "Camera Rig"
3. Configure in Inspector:
   - **Target:** Drag the "Aircraft" GameObject here
   - **Offset:** (0, 5, -15)
   - **Follow Speed:** 5
   - **Rotation Speed:** 3
   - **Enable Banking:** ✅ Checked
   - **Enable Dynamic FOV:** ✅ Checked

## Step 5: Create HUD Canvas

1. In **Hierarchy**, right-click → **UI → Canvas**
2. Rename to "FlightHUD"
3. Configure Canvas:
   - **Render Mode:** Screen Space - Overlay
   - **Canvas Scaler → UI Scale Mode:** Scale With Screen Size
   - **Reference Resolution:** 1920 x 1080

### Add HUD Script

1. With "FlightHUD" selected:
   - **Add Component** → search for "Flight HUD"
   - **Aircraft:** Drag the "Aircraft" GameObject here

### Create HUD Text Elements

For each text element (Speed, Altitude, Heading, Throttle):

1. Right-click **FlightHUD** → **UI → Text - TextMeshPro**
   - If prompted to import TMP Essentials, click "Import"
2. Name them:
   - "SpeedText"
   - "AltitudeText"
   - "HeadingText"
   - "ThrottleText"

3. Configure each text element:
   - **Font Size:** 24-32
   - **Color:** White (or green for HUD feel)
   - **Alignment:** Left/Top
   - **Text:** "SPD: 000" (just for preview)

4. Position them using **Rect Transform**:
   - **SpeedText:** Top-left (X: 20, Y: -20)
   - **AltitudeText:** Below speed (X: 20, Y: -60)
   - **HeadingText:** Top-right (X: -20, Y: -20)
   - **ThrottleText:** Below heading (X: -20, Y: -60)

5. Link to HUD script:
   - Select "FlightHUD" Canvas
   - In Inspector, find "Flight HUD" component
   - Drag each text element to its corresponding field:
     - **Speed Text** → SpeedText GameObject
     - **Altitude Text** → AltitudeText GameObject
     - **Heading Text** → HeadingText GameObject
     - **Throttle Text** → ThrottleText GameObject

## Step 6: Configure Input System

1. Open the Input Actions asset:
   - **Assets → InputSystem_Actions**
2. In the Inspector, click **"Edit Asset"**
3. Create a new Action Map:
   - Click **"+"** → Name it "Flight"
4. Add actions:
   - **PitchRoll** (Value, Vector2)
     - Binding: Keyboard WASD
     - Up: W, Down: S, Left: A, Right: D
   - **Yaw** (Value, Axis)
     - Positive: Q
     - Negative: E
   - **Throttle** (Value, Axis)
     - Positive: Left Shift
     - Negative: Left Ctrl
5. **Save Asset**

### Link Input to Aircraft

1. Select **Aircraft** GameObject
2. **Add Component** → **Player Input**
3. Configure:
   - **Actions:** Drag "InputSystem_Actions" asset here
   - **Default Map:** Flight
   - **Behavior:** Invoke Unity Events

4. In **Events → Flight**:
   - **PitchRoll** → Click "+" → Drag Aircraft → Select "AircraftController.OnPitchRoll"
   - **Yaw** → Click "+" → Drag Aircraft → Select "AircraftController.OnYaw"
   - **Throttle** → Click "+" → Drag Aircraft → Select "AircraftController.OnThrottle"

## Step 7: Test the Scene

1. **Save everything:** Ctrl+S
2. Click **Play** button (▶️ at top)
3. **Expected behavior:**
   - You should see Earth's terrain loading
   - Aircraft should be flying forward
   - Camera should follow smoothly
   - HUD should show speed, altitude, heading
   - Controls should respond:
     - **W/S:** Pitch up/down
     - **A/D:** Roll left/right
     - **Q/E:** Yaw left/right
     - **Shift/Ctrl:** Throttle up/down

## Troubleshooting

### Terrain not loading
- Check Cesium ion token in `Window → Cesium`
- Check Console for errors
- Ensure internet connection active

### Aircraft falls or doesn't move
- Check Rigidbody "Use Gravity" is unchecked
- Verify AircraftController is attached
- Check Input System is configured

### Camera doesn't follow
- Verify Camera Rig has target set to Aircraft
- Check follow/rotation speeds aren't 0

### HUD doesn't update
- Verify FlightHUD script has Aircraft reference
- Check all text elements are assigned
- Look for errors in Console

### Controls don't work
- Verify Player Input component is on Aircraft
- Check Input Actions asset is assigned
- Ensure events are linked to correct methods

## Next Steps

Once the scene is working:
1. Test flight controls and physics
2. Adjust speeds/sensitivities to feel good
3. Add 3D building tiles
4. Performance test (aim for 60 FPS)

**When ready, report back** and I'll help with:
- Fine-tuning physics
- Adding more features
- Performance optimization
- Building the game

---

**Need help?** Check the Console for error messages and let me know what you see!
