# Build System - Quick Reference

## Local Builds

### Prerequisites
```bash
export UNITY_PATH="/path/to/Unity"
export CESIUM_ION_TOKEN="your_token"  # Optional

# Required for Linux builds
./scripts/patch-cesium-linux.sh
```

### Build Commands

```bash
# Single platform
./scripts/build.sh windows 1.0.0
./scripts/build.sh mac 1.0.0
./scripts/build.sh linux 1.0.0

# All platforms
./scripts/build.sh all 1.0.0

# Help
./scripts/build.sh --help
```

### Output Location
```
Builds/
├── FlightSim_Windows_v1.0.0.zip
├── FlightSim_Mac_v1.0.0.dmg
└── FlightSim_Linux_v1.0.0.tar.gz
```

## CI/CD Builds

### Automatic (Tagged Release)
```bash
git tag v1.0.0
git push origin v1.0.0
```
→ Builds all platforms + creates GitHub Release

### Manual Dispatch
1. Go to Actions → Build Multi-Platform
2. Click "Run workflow"
3. Enter version number
4. Run

→ Builds all platforms (no release)

## GitHub Secrets

Required in Settings → Secrets:
- `UNITY_LICENSE` - Unity license file
- `UNITY_EMAIL` - Unity account email
- `UNITY_PASSWORD` - Unity account password
- `CESIUM_ION_TOKEN` - Optional, Cesium token

## Version Management

### Unity Editor
- Menu: Build → Update Version from Git
- Menu: Build → Show Current Version

### Automatic
- Version auto-updates from git tags
- Format: `v1.0.0` → `1.0.0` in builds

## Build Settings

| Setting | Value |
|---------|-------|
| Development Build | OFF |
| Script Debugging | OFF |
| Compression | LZ4 |
| Scripting Backend | IL2CPP |
| macOS Architecture | Universal |

## Platform Requirements

| Platform | Tools |
|----------|-------|
| Windows  | CMake 3.18+, VS 2022, .NET 6+ |
| macOS    | CMake 3.18+, Xcode CLI, .NET 6+ |
| Linux    | CMake 3.18+, GCC C++20, .NET 6+ |

## Build Times

| Platform | First Build | Cached |
|----------|-------------|--------|
| Windows  | ~10-15 min  | ~5-8 min |
| macOS    | ~10-15 min  | ~5-8 min |
| Linux    | ~10-15 min  | ~5-8 min |
| **Total (parallel)** | **~15 min** | **~8 min** |

## Troubleshooting

### Build fails locally
```bash
# Check Unity path
echo $UNITY_PATH

# Check dependencies
cmake --version   # Should be 3.18+
dotnet --version  # Should be 6.0+

# View logs
cat Builds/windows_build.log
```

### Build fails in CI
1. Check GitHub Actions logs
2. Verify secrets are set correctly
3. Check Unity license hasn't expired

### Cesium compilation fails
- Ensure CMake 3.18+ installed
- Verify C++ compiler available
- Check .NET SDK 6.0+ installed

## Common Commands

```bash
# Update version from git
# (In Unity: Build → Update Version from Git)

# Show current version
# (In Unity: Build → Show Current Version)

# Generate build info
npm run build-info

# Clean builds
rm -rf Builds/

# Test build locally
./scripts/build.sh linux 0.1.0-test
```

## Distribution Checklist

- [ ] Update version in git tag
- [ ] Run local build test
- [ ] Push tag to trigger CI/CD
- [ ] Wait for builds to complete (~15 min)
- [ ] Download artifacts from GitHub Release
- [ ] Test each platform build
- [ ] Update release notes if needed
- [ ] Announce release

## File Locations

| File | Purpose |
|------|---------|
| `scripts/build.sh` | Local build script |
| `.github/workflows/build.yml` | CI/CD workflow |
| `Assets/Editor/BuildScript.cs` | Unity build logic |
| `Assets/Editor/VersionManager.cs` | Version management |
| `Distribution/README.txt` | User documentation |
| `docs/BUILD_SYSTEM.md` | Full documentation |

## Support

- Full docs: `docs/BUILD_SYSTEM.md`
- Issues: GitHub Issues
- Unity version: 6000.3.2f1
