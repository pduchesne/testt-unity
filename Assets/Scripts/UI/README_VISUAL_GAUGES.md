# Visual HUD Gauges Setup Guide

This guide explains how to set up the modern/arcade style visual gauges for the Flight HUD.

## Quick Setup (Automated)

1. **Open the HUD Gauge Setup Window**:
   - In Unity, go to menu: `GeoGame3D > Setup HUD Visual Gauges`

2. **Select the FlightHUD Canvas**:
   - In the Hierarchy, click on "FlightHUD Canvas"

3. **Create Gauges**:
   - Click "Create All Gauges" button to automatically create all primary gauges
   - OR click individual buttons to create specific gauges

4. **Link Gauges to FlightHUD Script**:
   - Select "FlightHUD Canvas" in Hierarchy
   - In the Inspector, find the FlightHUD component
   - Drag the created gauge GameObjects to the corresponding fields:
     - Artificial Horizon → `artificialHorizon`
     - SpeedGauge → `speedGauge`
     - AltitudeGauge → `altitudeGauge`
     - HeadingCompass → `headingCompass`
     - ThrottleGauge → `throttleGauge`

5. **Save and Test**:
   - Save the scene (Ctrl+S)
   - Enter Play mode to see the gauges in action!

## Gauge Types Created

### 1. Artificial Horizon
- **Location**: Center of screen
- **Purpose**: Shows aircraft pitch and roll
- **Visual**: Cyan horizon line that tilts and moves
- **Components**:
  - Background (dark blue)
  - Horizon line (cyan, rotates with roll)
  - Center reticle (yellow, fixed)

### 2. Speed Gauge
- **Location**: Bottom-left
- **Range**: 0-400 km/h (or mph)
- **Visual**: Circular fill gauge with value text
- **Color**: Cyan fill

### 3. Altitude Gauge
- **Location**: Bottom-right
- **Range**: 0-5000m (or feet)
- **Visual**: Circular fill gauge with value text
- **Color**: Cyan fill

### 4. Heading Compass
- **Location**: Top-center
- **Range**: 0-360°
- **Visual**: Digital heading display
- **Features**: Smooth rotation with wrap-around

### 5. Throttle Gauge
- **Location**: Top-left
- **Range**: 0-100%
- **Visual**: Circular fill gauge
- **Color**: Cyan fill

## Customization

### Adjusting Gauge Ranges
Select a gauge GameObject and modify the CircularGauge component:
- `Min Value`: Minimum value for the gauge
- `Max Value`: Maximum value for the gauge
- `Smoothing`: How quickly the gauge responds (higher = slower/smoother)

### Color Coding
Enable `Use Color Coding` on CircularGauge to have the gauge change color:
- **Low Color**: Green (safe range)
- **Mid Color**: Yellow (caution range)
- **High Color**: Red (danger range)
- **Mid Threshold**: 0-1 value where color changes from low to mid
- **High Threshold**: 0-1 value where color changes from mid to high

Example for Speed Gauge:
- Set `Use Color Coding` = true
- `Low Color` = Green (0-50% speed)
- `Mid Color` = Yellow (50-80% speed)
- `High Color` = Red (80-100% speed)
- `Mid Threshold` = 0.5
- `High Threshold` = 0.8

### Repositioning Gauges
- Select the gauge GameObject
- Adjust the RectTransform `Anchored Position` in the Inspector
- Default positions are designed for 1920x1080 screens

### Adjusting Artificial Horizon Sensitivity
- Select the ArtificialHorizon GameObject
- Modify `Pitch Sensitivity` (default: 5 pixels per degree)
- Modify `Smoothing` (default: 10 - higher = smoother but slower response)

## Manual Setup (Advanced)

If you prefer to create gauges manually:

1. Create an empty GameObject as a child of FlightHUD Canvas
2. Add one of the gauge components:
   - `ArtificialHorizon`
   - `CircularGauge`
   - `HeadingCompass`
3. Add required UI elements (Images, TextMeshPro texts)
4. Link the UI elements to the component's fields
5. Configure the component settings
6. Link the gauge to the FlightHUD component

## Additional Gauges

The system supports additional gauges (not created by default):
- Vertical Speed Indicator (VSI)
- Angle of Attack (AOA) Gauge
- G-Force Meter

To create these, use the CircularGauge component with appropriate ranges:
- **VSI**: Range -20 to +20 m/s
- **AOA**: Range -20 to +20 degrees
- **G-Force**: Range 0 to 6 G

## Troubleshooting

**Gauges don't appear**:
- Check that the FlightHUD Canvas is active
- Verify the gauge GameObjects are enabled
- Check Canvas render mode is set correctly

**Gauges don't update**:
- Ensure gauges are linked to the FlightHUD component
- Check that the AircraftController reference is set on FlightHUD
- Look for errors in the Console

**Gauges appear but values are wrong**:
- Check the min/max range settings on CircularGauge
- Verify the `useMetric` setting on FlightHUD matches your preference

**Performance issues**:
- Reduce `smoothing` values on gauges for faster updates
- Disable unused gauges
- Use fewer gauges on screen at once
