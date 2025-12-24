# Cesium for Unity - Linux Build Fix

## Issue

By default, Cesium for Unity doesn't support Linux standalone builds. The assembly definition file `Packages/com.cesium.unity/Source/CesiumForUnity.asmdef` doesn't include `LinuxStandalone64` in its `includePlatforms` list.

This causes build errors when attempting to build for Linux:
```
error CS0246: The type or namespace name 'CesiumForUnity' could not be found
error CS0246: The type or namespace name 'CesiumGeoreference' could not be found
error CS0246: The type or namespace name 'Cesium3DTileset' could not be found
```

## Fix

Add `LinuxStandalone64` to the platforms list in the Cesium assembly definition file.

### Manual Fix

Edit `Packages/com.cesium.unity/Source/CesiumForUnity.asmdef`:

```json
{
    "name": "CesiumForUnity",
    "rootNamespace": "",
    "references": [
        "Unity.InputSystem",
        "Unity.Mathematics",
        "Unity.Splines"
    ],
    "includePlatforms": [
        "Android",
        "Editor",
        "iOS",
        "LinuxStandalone64",  // <-- Add this line
        "macOSStandalone",
        "WSA",
        "WindowsStandalone64",
        "WebGL"
    ],
    ...
}
```

### Automated Fix Script

Run this script after cloning the repository or updating the Cesium package:

```bash
#!/bin/bash
# Apply Linux support patch to Cesium for Unity

ASMDEF_FILE="Packages/com.cesium.unity/Source/CesiumForUnity.asmdef"

if [ -f "$ASMDEF_FILE" ]; then
    # Check if Linux support is already added
    if grep -q "LinuxStandalone64" "$ASMDEF_FILE"; then
        echo "✓ Linux support already present in Cesium assembly definition"
    else
        echo "Adding Linux support to Cesium assembly definition..."

        # Add LinuxStandalone64 after iOS
        sed -i 's/"iOS",/"iOS",\n        "LinuxStandalone64",/' "$ASMDEF_FILE"

        echo "✓ Linux support added successfully"
        echo "You may need to restart Unity for changes to take effect"
    fi
else
    echo "✗ Cesium assembly definition not found at: $ASMDEF_FILE"
    echo "Ensure Cesium for Unity package is installed"
    exit 1
fi
```

Save as `scripts/patch-cesium-linux.sh` and run:
```bash
chmod +x scripts/patch-cesium-linux.sh
./scripts/patch-cesium-linux.sh
```

## Why This Happens

Cesium for Unity was primarily developed for Windows, macOS, mobile, and WebGL platforms. Linux desktop support wasn't included in the initial platform list, likely because:

1. Linux desktop gaming market share is smaller
2. Cesium's primary use cases (geospatial visualization) are more common on Windows/Mac
3. The Cesium team may not have tested extensively on Linux

## Important Notes

⚠️ **This fix modifies a file in the Packages folder** (which is typically from a git repository)

**This means:**
- The change will be lost if you update the Cesium package
- You need to re-apply this fix after Cesium updates
- Consider automating this with a post-install script or patch file

**Alternatives:**
1. **Fork Cesium for Unity** and maintain your own version with Linux support
2. **Submit a PR** to the official Cesium for Unity repository to add Linux support
3. **Use a patch-package approach** to automatically apply the fix

## Reporting to Cesium

This fix should ideally be contributed back to the Cesium for Unity project. Consider:

1. **Opening an issue**: https://github.com/CesiumGS/cesium-unity/issues
2. **Creating a pull request** with Linux platform support
3. **Testing thoroughly** on Linux before submitting

## Testing

After applying the fix, verify it works:

```bash
# Test local build
export UNITY_PATH=/path/to/Unity
./scripts/build.sh linux 0.1.0-test

# Check for compilation errors
tail -100 Builds/linux_build.log | grep -i error

# Verify build output
ls -lh Builds/FlightSim_Linux_*.tar.gz
```

Expected result:
- No compilation errors
- Build completes successfully
- Archive file created (~200-400 MB)

## Related Files

- `Packages/com.cesium.unity/Source/CesiumForUnity.asmdef` - Assembly definition (needs patching)
- `scripts/build.sh` - Build script (works after fix)
- `.github/workflows/build.yml` - CI/CD workflow (will work after fix)

## Status

✅ **Fixed** in this project
⏳ **Pending** upstream contribution to Cesium for Unity

Last updated: 2024-12-24
