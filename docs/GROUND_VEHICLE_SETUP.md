# Ground Vehicle Setup Guide

## Important: Unity Compilation Required

The ground vehicle scripts have been created but Unity needs to compile them before they can be used. Follow these steps to complete the setup.

## Step 1: Trigger Unity Compilation

Unity-MCP's asset refresh didn't trigger C# compilation. You need to:

1. **Switch to Unity Editor window** (this will trigger compilation)
2. **Wait for Unity to finish compiling** (watch the bottom-right corner for "Compiling..." to disappear)
3. **Check Console for any compilation errors** (Window → General → Console)

The following scripts should compile successfully:
- `Assets/Scripts/Vehicles/GroundVehicleController.cs`
- `Assets/Scripts/Vehicles/GroundVehicleInputHandler.cs`
- `Assets/Scripts/Vehicles/VehicleModeManager.cs`
- `Assets/Scripts/UI/DrivingHUD.cs`

## Step 2: Add Components to Aircraft GameObject

Once compilation is complete:

1. **Find the Aircraft GameObject** in the Hierarchy
2. **Add the following components** (Add Component button in Inspector):
   - `GeoGame3D.Vehicles.GroundVehicleController`
   - `GeoGame3D.Vehicles.GroundVehicleInputHandler`
   - `GeoGame3D.Vehicles.VehicleModeManager`

3. **Disable the ground vehicle components initially**:
   - Uncheck `GroundVehicleController`
   - Uncheck `GroundVehicleInputHandler`
   - Leave `VehicleModeManager` enabled
   - Leave `AircraftController` and `FlightInputHandler` enabled

## Step 3: Create Wheel Position Transforms

The GroundVehicleController needs 4 wheel position transforms:

1. **Right-click Aircraft GameObject** → Create Empty
2. **Rename to "WheelPositions"**
3. **Create 4 child transforms** under WheelPositions:
   - `FrontLeft` - Position: (-0.9, -0.5, 1.0)
   - `FrontRight` - Position: (0.9, -0.5, 1.0)
   - `RearLeft` - Position: (-0.9, -0.5, -1.0)
   - `RearRight` - Position: (0.9, -0.5, -1.0)

These positions are relative to the vehicle and represent typical car wheel positions.

## Step 4: Configure GroundVehicleController

Select the Aircraft GameObject and configure the GroundVehicleController component:

### Wheel Configuration
- **Front Left Wheel**: Drag `WheelPositions/FrontLeft` transform
- **Front Right Wheel**: Drag `WheelPositions/FrontRight` transform
- **Rear Left Wheel**: Drag `WheelPositions/RearLeft` transform
- **Rear Right Wheel**: Drag `WheelPositions/RearRight` transform
- **Wheel Radius**: 0.35
- **Wheelbase**: 2.5
- **Track**: 1.8

### Suspension
- **Suspension Distance**: 0.5
- **Suspension Stiffness**: 25000
- **Suspension Damping**: 2000
- **Terrain Layer**: Select "Terrain" layer (create if needed - see Step 5)

### Movement
- **Max Speed**: 30 (m/s ≈ 108 km/h)
- **Acceleration**: 15
- **Brake Force**: 30
- **Reverse Speed**: 10

### Steering
- **Max Steer Angle**: 35
- **Steer Speed**: 3

### Physics
- **Forward Friction**: 0.95
- **Lateral Friction**: 0.98
- **Gravity**: 20
- **Slope Alignment Speed**: 5

## Step 5: Create Terrain Layer

The ground vehicle uses raycasts to detect terrain:

1. **Edit → Project Settings → Tags and Layers**
2. **Find an empty layer** (e.g., Layer 8)
3. **Name it "Terrain"**
4. **Click on your Cesium tileset GameObjects** in the hierarchy
5. **Set their Layer to "Terrain"** (this may need to be done for child tiles too)

## Step 6: Configure VehicleModeManager

Select the Aircraft GameObject and configure VehicleModeManager:

- **Start Mode**: Aircraft
- **Ground Spawn Height**: 2.0
- **Max Terrain Check Distance**: 1000
- **Terrain Layer**: Select "Terrain"
- **Aircraft Spawn Velocity**: 50

## Step 7: Create DrivingHUD Canvas

1. **Find or Create DrivingHUD Canvas**:
   - If you have an existing HUD canvas, duplicate it
   - Or create: GameObject → UI → Canvas
   - Name it "DrivingHUD"

2. **Add DrivingHUD Component** to the canvas:
   - Add Component → `GeoGame3D.UI.DrivingHUD`

3. **Create UI Elements** (as children of DrivingHUD canvas):

### Speed Display
- Create Text (TMP): "SpeedText"
- Position: Top-left area
- Initial text: "SPD: 0 km/h"

### Heading Display
- Create Text (TMP): "HeadingText"
- Position: Top-center area
- Initial text: "HDG: 000°"

### Brake Indicator
- Create Text (TMP): "BrakeText"
- Position: Center of screen
- Initial text: "BRAKE"
- Color: Red
- Font Size: Large (48+)
- Initially disabled

### Airborne Warning
- Create Text (TMP): "GroundedText"
- Position: Center of screen (below brake)
- Initial text: "*** AIRBORNE ***"
- Color: Yellow
- Font Size: Large (36+)
- Initially disabled

4. **Assign References** in DrivingHUD component:
   - Drag text elements to corresponding fields
   - Optionally assign CircularGauge and HeadingCompass if you have them

5. **Initially Disable DrivingHUD GameObject**:
   - Uncheck the GameObject (it will be enabled when switching to ground mode)

## Step 8: Configure MainMenuCanvas

Find your MainMenuCanvas and:

1. **Add Mode Switch Button**:
   - Create Button (TMP): "SwitchModeButton"
   - Text: "Switch to Ground Vehicle"
   - Position: In your menu panel

2. **Add Mode Display Text**:
   - Create Text (TMP): "ModeDisplayText"
   - Text: "Mode: AIRCRAFT"
   - Position: Top of menu or near switch button

3. **Configure MainMenuController** component:
   - **Mode Display Text**: Drag ModeDisplayText
   - **Vehicle Mode Manager**: Drag Aircraft GameObject (contains VehicleModeManager)
   - **Flight HUD**: Drag your FlightHUD GameObject
   - **Driving HUD**: Drag your DrivingHUD GameObject

4. **Wire Button Click Event**:
   - Select SwitchModeButton
   - In Button component → On Click():
     - Click **+** to add event
     - Drag **MainMenuCanvas** (with MainMenuController) to object field
     - Select **MainMenuController → OnSwitchModeClicked()**

## Step 9: Configure CameraRig

The CameraRig should automatically subscribe to mode changes. If needed, verify:

1. **Find CameraRig** GameObject in hierarchy
2. **Check CameraRig component** has these mode-specific settings:
   - **Aircraft Base Offset**: (0, 5, -15)
   - **Ground Base Offset**: (0, 3, -8)
   - **Aircraft Base FOV**: 60
   - **Ground Base FOV**: 65
   - **Aircraft Follow Speed**: 8
   - **Ground Follow Speed**: 12

## Step 10: Test Mode Switching

1. **Enter Play Mode**
2. **Press ESC** to open menu
3. **Click "Switch to Ground Vehicle"**
4. **Verify**:
   - Aircraft is positioned on terrain
   - DrivingHUD is visible, FlightHUD is hidden
   - Camera is closer and lower
   - Mode display shows "GROUND VEHICLE"
5. **Test Driving**:
   - W/S or Up/Down: Accelerate/Reverse
   - A/D or Left/Right: Steer
   - Space: Brake
6. **Switch back to Aircraft**:
   - ESC → "Switch to Aircraft"
   - Verify aircraft mode activates

## Troubleshooting

### Scripts Not Compiling
- Check Console for errors
- Verify file permissions (scripts should be readable)
- Try: Assets → Reimport All

### Components Not Available
- Ensure compilation finished
- Check namespace is correct: `GeoGame3D.Vehicles`
- Restart Unity Editor if needed

### Vehicle Falls Through Terrain
- Verify Terrain layer is created
- Check Cesium tiles have colliders (should by default)
- Verify GroundVehicleController's Terrain Layer is set
- Check wheel positions are correct (below vehicle center)

### Mode Switch Doesn't Work
- Verify VehicleModeManager is enabled
- Check MainMenuController has all references assigned
- Look for errors in Console when clicking button
- Verify OnModeChanged event is wired to Camera and HUD

### Camera Doesn't Adjust
- Verify CameraRig has mode-specific offset values set
- Check if CameraRig is finding VehicleModeManager (see Console logs)

### HUD Doesn't Switch
- Verify MainMenuController has both HUD references
- Check UpdateHUDVisibility is being called (add Debug.Log if needed)
- Ensure both HUD GameObjects exist and are initially correct state

## Summary

After setup, you'll have:
- ✅ Fully functional ground vehicle with physics-based driving
- ✅ Raycast suspension system that follows terrain
- ✅ Seamless mode switching via menu
- ✅ Mode-aware camera behavior
- ✅ Appropriate HUD for each mode
- ✅ Complete control system (WASD + Space)

Press **M** in the menu (once wired) or use the button to switch between flying and driving!
