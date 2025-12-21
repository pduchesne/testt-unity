# Realistic Gauge Sprite Creation Guide

This guide covers how to create realistic aviation instrument gauge sprites using AI generation and other resources.

## Quick Reference: AI Prompts for Gauge Generation

### Speed Gauge (Airspeed Indicator)
```
circular aviation instrument gauge face, airspeed indicator 0-400 km/h,
black background, white markings and numbers, mechanical dial,
vintage aircraft cockpit, centered, top view, highly detailed,
clean edges, transparent background, professional instrument design
```

### Altitude Gauge (Altimeter)
```
circular altimeter gauge face, aviation instrument, 0-5000 meter markings,
black background, white numbers and tick marks, mechanical dial,
classic cockpit instrument, centered view, highly detailed,
clean sharp edges, transparent background
```

### Throttle Gauge
```
circular throttle gauge, 0-100 percent indicator, aviation instrument,
black face with white markings, arc dial from bottom to top,
mechanical gauge design, centered, transparent background,
professional aircraft cockpit style
```

### Heading Compass
```
circular compass rose, aviation heading indicator, 0-360 degrees,
black background, white cardinal directions and degree markings,
mechanical compass design, top view centered, highly detailed,
transparent background, professional instrument
```

### Needle/Pointer (for all gauges)
```
aircraft gauge needle pointer, metallic red with white tip,
simple arrow shape, centered pivot point at base,
transparent background, sharp clean edges, thin elegant design,
professional cockpit instrument pointer
```

## AI Generation Tools

### Free Options

#### 1. Leonardo.ai (Recommended for Beginners)
- **URL**: https://leonardo.ai
- **Free Tier**: 150 credits/day
- **Best for**: Quick gauge generation with good quality
- **Steps**:
  1. Sign up for free account
  2. Click "Create" → "AI Image Generation"
  3. Paste gauge prompt
  4. Settings: 512x512, High Quality
  5. Generate (4 variations per generation)
  6. Download PNG with transparent background

#### 2. Stable Diffusion (Civitai)
- **URL**: https://civitai.com
- **Free Tier**: Unlimited generations
- **Best for**: Full control, many model options
- **Recommended Models**:
  - "Realistic Vision" for photorealistic gauges
  - "DreamShaper" for clean illustrated style

#### 3. Playground.ai
- **URL**: https://playground.com
- **Free Tier**: Available
- **Best for**: Easy to use, good for beginners

### Paid Options (Better Quality)

#### Midjourney ($10/month)
```
/imagine aircraft instrument gauge, airspeed indicator,
circular dial, black background, white numbers,
clean vector style, transparent background --v 6 --ar 1:1
```

#### DALL-E 3 (ChatGPT Plus $20/month)
- Generally excellent at technical illustrations
- Good at following precise requirements

#### Adobe Firefly
- Copyright-safe for commercial use
- Good "Generative Fill" for cleanup

## Free Resources (Ready-Made)

### 1. OpenGameArt.org
- **URL**: https://opengameart.org
- **Search**: "gauge", "instrument", "cockpit", "HUD"
- **License**: Check individual assets (mostly CC0/Public Domain)
- **Quality**: Variable, good for prototyping

### 2. Freepik.com
- **URL**: https://freepik.com
- **Search**: "aircraft gauge vector", "aviation instrument"
- **Free tier**: Available (requires attribution)
- **Quality**: Professional vector graphics

### 3. Kenney.nl
- **URL**: https://kenney.nl
- **All assets**: CC0 (Public Domain)
- **Quality**: Clean, stylized (not photorealistic)
- **Best for**: Modern minimalist style

### 4. Wikimedia Commons
- **URL**: https://commons.wikimedia.org
- **Search**: "aircraft instrument panel"
- **License**: Public domain available
- **Quality**: Real photographs (need extraction/editing)

## Post-Processing (Making AI Output Unity-Ready)

### Using GIMP (Free Photoshop Alternative)

1. **Remove Background Artifacts**:
   - Layer → Transparency → Remove Alpha Channel
   - Layer → Transparency → Add Alpha Channel
   - Select → By Color → Click background
   - Delete

2. **Resize to Power of 2**:
   - Image → Scale Image
   - Recommended sizes: 512x512 or 1024x1024

3. **Center the Gauge**:
   - Filters → Align Layers → Center
   - Ensure pivot point is center of image

4. **Export for Unity**:
   - File → Export As → PNG
   - Name: `GaugeFace_Speed.png`, `Needle_Speed.png`, etc.

### Unity Import Settings

```
Texture Type: Sprite (2D and UI)
Sprite Mode: Single
Pixels Per Unit: 100
Mesh Type: Tight
Extrude Edges: 0
Pivot: Center
Generate Mip Maps: OFF (for crisp gauges)
Filter Mode: Bilinear
Max Size: 2048 (or 4096 for very detailed gauges)
Compression: None (for crisp edges)
```

## Gauge Layer Structure

For each gauge, create this hierarchy:

```
SpeedGauge (GameObject)
├─ Bezel (Image)           - Outer ring/frame
├─ Face (Image)            - Gauge face with markings
├─ Needle (Image)          - Rotating pointer
├─ Glass (Image)           - Reflection overlay (optional)
└─ ValueText (TMP)         - Digital backup readout
```

## Enhanced CircularGauge Script Requirements

The script should support:

1. **Sprite References**:
   ```csharp
   [SerializeField] private Image bezelImage;
   [SerializeField] private Image faceImage;
   [SerializeField] private Image needleImage;
   [SerializeField] private Image glassImage;
   ```

2. **Needle Rotation**:
   ```csharp
   [SerializeField] private float needleMinAngle = -135f;  // 7 o'clock
   [SerializeField] private float needleMaxAngle = 135f;   // 5 o'clock
   ```

3. **Visual Polish**:
   - Smooth rotation with interpolation
   - Optional needle glow effect
   - Shadow for depth

## Color Palette (Classical Aviation Instruments)

### Standard Color Schemes

**Traditional Dark**:
- Face: `#0A0A0F` (very dark blue-black)
- Markings: `#FFFFFF` (white)
- Needle: `#FF3333` (red) or `#FFCC00` (amber)
- Bezel: `#2A2A30` (dark grey)

**EFIS Style (Modern)**:
- Face: `#001122` (dark blue)
- Markings: `#00FFAA` (cyan)
- Needle: `#00FF00` (green) or `#FFAA00` (orange)
- Bezel: `#334455` (blue-grey)

## Asset Naming Convention

```
Gauges/
├─ Speed/
│  ├─ GaugeFace_Speed.png
│  ├─ Needle_Speed.png
│  ├─ Bezel_Speed.png
│  └─ Glass_Speed.png
├─ Altitude/
│  ├─ GaugeFace_Altitude.png
│  ├─ Needle_Altitude.png
│  └─ ...
└─ Shared/
   ├─ Bezel_Generic.png
   └─ Glass_Generic.png
```

## Next Steps for Implementation

1. **Generate Sprites**:
   - Use AI prompts above with Leonardo.ai
   - Generate 4 gauge faces + needles
   - Download and process in GIMP

2. **Import to Unity**:
   - Create `Assets/Sprites/Gauges/` folder
   - Import all PNGs
   - Configure import settings

3. **Enhance CircularGauge.cs**:
   - Add needle rotation logic
   - Add sprite references
   - Test with sample sprites

4. **Update FlightHUD**:
   - Assign sprites to gauge instances
   - Configure needle angles
   - Test in-game

## Troubleshooting

### Gauge appears blurry
- Increase Max Size in import settings
- Set Compression to "None"
- Ensure source image is high resolution

### Needle pivot is off-center
- Check sprite pivot is set to "Center"
- Ensure needle image has equal margins
- Recenter in GIMP if needed

### Transparency has artifacts
- Remove and re-add alpha channel in GIMP
- Ensure PNG export has transparency enabled
- Check Unity import has Alpha Source: Input Texture Alpha

## Resources

- **Leonardo.ai Tutorial**: https://docs.leonardo.ai/docs
- **GIMP Manual**: https://docs.gimp.org
- **Unity Sprite Documentation**: https://docs.unity3d.com/Manual/Sprites.html
- **Aviation Instrument Reference**: Search "aircraft six pack instruments" for layout inspiration
