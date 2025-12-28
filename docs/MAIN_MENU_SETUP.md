# Main Menu Setup Guide

This guide explains how to set up the main menu overlay system in the Unity Editor.

## Overview

The main menu system provides:
- **ESC key toggle** - Press ESC to open/close the menu
- **Game pause** - Time.timeScale = 0 when menu is active
- **Cursor control** - Cursor shown in menu, hidden during gameplay
- **Start/Resume button** - Resumes the game
- **Exit button** - Closes the application

## Setup Steps

### 1. Create the Menu Canvas

1. In the **Hierarchy**, right-click and select **UI > Canvas**
2. Rename it to **"MainMenuCanvas"**
3. Select the Canvas and configure in Inspector:
   - **Render Mode**: Screen Space - Overlay
   - **Canvas Scaler**:
     - UI Scale Mode: Scale With Screen Size
     - Reference Resolution: 1920 x 1080
     - Match: 0.5 (balance between width and height)
   - **Order in Layer**: 100 (ensure it appears above HUD)

### 2. Create the Menu Panel (Semi-transparent Background)

1. Right-click on **MainMenuCanvas** → **UI > Panel**
2. Rename it to **"MenuPanel"**
3. Configure the RectTransform:
   - **Anchors**: Stretch (all corners)
   - **Left/Right/Top/Bottom**: 0
4. Configure the Image component:
   - **Color**: Black with alpha 180-200 (semi-transparent overlay)
   - **Material**: None (default)

### 3. Create the Menu Container

1. Right-click on **MenuPanel** → **UI > Panel** or **Empty**
2. Rename it to **"MenuContainer"**
3. Configure the RectTransform:
   - **Anchors**: Center Middle
   - **Width**: 400
   - **Height**: 300
   - **Pos X/Y/Z**: 0, 0, 0
4. Configure the Image (if using Panel):
   - **Color**: Dark gray (RGB: 40, 40, 40, 255)
   - Or use a custom panel sprite

5. Add **Vertical Layout Group** component:
   - **Padding**: 20 on all sides
   - **Spacing**: 20
   - **Child Alignment**: Upper Center
   - **Child Controls Size**: Width = ✓, Height = ✗
   - **Child Force Expand**: Width = ✓, Height = ✗

### 4. Create Menu Title (Optional)

1. Right-click on **MenuContainer** → **UI > Text - TextMeshPro**
2. Rename it to **"MenuTitle"**
3. Configure TextMeshPro:
   - **Text**: "PAUSED" or "MENU"
   - **Font Size**: 48
   - **Alignment**: Center
   - **Color**: White
4. Configure RectTransform:
   - **Height**: 60 (will be auto-sized by layout group)

### 5. Create Resume Button

1. Right-click on **MenuContainer** → **UI > Button - TextMeshPro**
2. Rename it to **"ResumeButton"**
3. Configure the Button component:
   - Find the **MainMenuController** script in the scene
   - **On Click ()**:
     - Click **+** to add event
     - Drag the **MainMenuCanvas** (or GameObject with MainMenuController) into the object field
     - Select **MainMenuController > ResumeGame()**
4. Configure Button colors (in Transition: Color Tint):
   - **Normal**: Light gray
   - **Highlighted**: White
   - **Pressed**: Dark gray
   - **Selected**: White
   - **Disabled**: Very dark gray
5. Select the child **Text (TMP)** object and set:
   - **Text**: "START / RESUME"
   - **Font Size**: 24
   - **Alignment**: Center
   - **Color**: White
6. Configure RectTransform on the button:
   - **Height**: 60

### 6. Create Exit Button

1. Right-click on **MenuContainer** → **UI > Button - TextMeshPro**
2. Rename it to **"ExitButton"**
3. Configure the Button component:
   - **On Click ()**:
     - Click **+**
     - Drag the **MainMenuCanvas** into the object field
     - Select **MainMenuController > ExitGame()**
4. Configure Button colors (same as Resume button)
5. Select the child **Text (TMP)** object and set:
   - **Text**: "EXIT"
   - **Font Size**: 24
   - **Alignment**: Center
   - **Color**: White or Red
6. Configure RectTransform on the button:
   - **Height**: 60

### 7. Add MainMenuController Script

1. Select **MainMenuCanvas** in the Hierarchy
2. In the Inspector, click **Add Component**
3. Search for **"MainMenuController"** and add it
4. Configure the script:
   - **Menu Panel**: Drag the **MenuPanel** GameObject here
   - **Start Paused**: ✓ if you want game to start paused, ✗ for normal start

### 8. Configure Event System

Unity should have automatically created an **EventSystem** GameObject when you created the first Canvas.

1. Verify **EventSystem** exists in the Hierarchy
2. If not, create it: Right-click Hierarchy → **UI > Event System**

### 9. Test the Menu

1. **Play the game** in Unity Editor
2. Press **ESC** - menu should appear
3. Click **START/RESUME** - menu should close, game resumes
4. Press **ESC** again - menu reappears
5. Click **EXIT** - play mode should stop (in editor) or app should quit (in build)

## Visual Customization

### Styling Buttons

You can enhance button appearance:

1. Add **outline** to buttons:
   - Select button → **Add Component** → **Outline**
   - Set color and thickness

2. Add **shadow** to text:
   - Select Text (TMP) → **Add Component** → **Shadow**
   - Set offset and color

3. Use custom **sprites** for buttons:
   - Create 9-slice sprites for button backgrounds
   - Assign to Button → Image → Source Image

### Adding Background Image

Instead of a solid color panel, use an image:

1. Create a blurred screenshot of your game or custom background
2. Select **MenuPanel** → Image component
3. Assign your image to **Source Image**
4. Set **Color** to white with reduced alpha

### Advanced Layout

For more complex menus (future expansion):

1. Use **Grid Layout Group** for grid-based button layouts
2. Use **Content Size Fitter** for auto-sizing containers
3. Nest multiple containers for complex hierarchies

## Future Enhancements

The current setup supports easy addition of:

- **Settings Panel**: Create similar panel with settings buttons
- **Geocoding Field**: Add TMP Input Field for location entry
- **Credits/About**: Additional panels with information
- **Sound Volume Sliders**: UI sliders for audio control
- **Graphics Options**: Dropdown menus for quality settings

### Adding Geocoding Field (Preview)

For future reference, to add a location search field:

```
1. Create a TMP Input Field in MenuContainer
2. Add a "Search" button next to it
3. Create a GeocodingController script
4. Connect button click to GeocodingController.SearchLocation(string location)
5. Use a geocoding API (OpenStreetMap Nominatim, Google Geocoding, etc.)
```

## Troubleshooting

### Menu doesn't appear when pressing ESC
- Check MainMenuController is attached to a GameObject
- Verify MenuPanel reference is assigned in Inspector
- Check console for errors

### Buttons don't respond to clicks
- Verify EventSystem exists in scene
- Check button OnClick events are properly configured
- Ensure Canvas is set to Screen Space - Overlay

### Game doesn't pause
- MainMenuController automatically sets Time.timeScale
- Check if other scripts are overriding Time.timeScale
- Verify MainMenuController.UpdateGameState() is being called

### Flight controls still work when menu is open
- Verify FlightInputHandler has been updated with menu check
- Check mainMenu reference is found in Awake()
- Look for console warnings about missing MainMenuController

## File References

- **Script**: `Assets/Scripts/UI/MainMenuController.cs`
- **Updated**: `Assets/Scripts/Aircraft/FlightInputHandler.cs`
- **Documentation**: `docs/MAIN_MENU_SETUP.md`

## Summary

You now have a fully functional pause menu that:
- ✅ Toggles with ESC key
- ✅ Pauses the game when active
- ✅ Shows/hides cursor appropriately
- ✅ Blocks flight input when open
- ✅ Allows resuming or exiting the game
- ✅ Ready for future expansion (settings, geocoding, etc.)
