# Build System Documentation

This document describes the automated multi-platform build system for the 3D Flight Simulator project.

## Overview

The build system supports creating distributable executables for:
- **Windows** (x86_64 standalone)
- **macOS** (Universal binary - Intel + Apple Silicon)
- **Linux** (x86_64 standalone)

Builds can be triggered:
1. **Locally** - Using the `build.sh` script
2. **CI/CD** - Via GitHub Actions on git tags or manual dispatch

## Architecture

### Components

1. **BuildScript.cs** (`Assets/Editor/BuildScript.cs`)
   - Unity Editor script that performs the actual build
   - Configures platform-specific settings
   - Sets compression to LZ4
   - Handles IL2CPP compilation

2. **VersionManager.cs** (`Assets/Editor/VersionManager.cs`)
   - Manages version numbers from git tags or build-info.json
   - Auto-updates PlayerSettings.bundleVersion
   - Provides Unity menu items for version management

3. **build.sh** (`scripts/build.sh`)
   - Shell script for local builds
   - Supports all three platforms
   - Creates compressed distribution archives
   - Configurable via environment variables

4. **GitHub Actions Workflow** (`.github/workflows/build.yml`)
   - Automated CI/CD pipeline
   - Parallel builds on platform-specific runners
   - Automatic artifact upload and release creation

## Local Building

### Prerequisites

1. **Unity 6000.3.2f1** installed
2. **Platform-specific build toolchains**:
   - Windows: CMake 3.18+, Visual Studio 2022, .NET SDK 6.0+
   - macOS: CMake 3.18+, Xcode Command Line Tools, .NET SDK 6.0+
   - Linux: CMake 3.18+, GCC/Clang with C++20, .NET SDK 6.0+

3. **Environment variables**:
   ```bash
   export UNITY_PATH="/path/to/Unity"
   export CESIUM_ION_TOKEN="your_cesium_token"  # Optional but recommended
   ```

### Building for a Single Platform

```bash
# Windows
./scripts/build.sh windows 1.0.0

# macOS
./scripts/build.sh mac 1.0.0

# Linux
./scripts/build.sh linux 1.0.0
```

### Building All Platforms

```bash
./scripts/build.sh all 1.0.0
```

### Output

Builds are created in the `Builds/` directory:
```
Builds/
├── Windows/
│   ├── FlightSim.exe
│   └── FlightSim_Data/
├── Mac/
│   └── FlightSim.app
├── Linux/
│   ├── FlightSim.x86_64
│   └── FlightSim_Data/
├── FlightSim_Windows_v1.0.0.zip
├── FlightSim_Mac_v1.0.0.dmg
└── FlightSim_Linux_v1.0.0.tar.gz
```

## CI/CD Setup

### GitHub Actions Configuration

The workflow is configured in `.github/workflows/build.yml`.

#### Required Secrets

Configure these in GitHub Settings → Secrets and variables → Actions:

1. **UNITY_LICENSE**
   - Your Unity license file contents
   - Get from Unity Hub or Unity website
   - Required for all builds

2. **UNITY_EMAIL**
   - Email associated with your Unity account
   - Required for license activation

3. **UNITY_PASSWORD**
   - Password for your Unity account
   - Required for license activation

4. **CESIUM_ION_TOKEN** (Optional)
   - Your Cesium ion access token
   - If not set, builds will use the token configured in Unity

### Triggering Builds

#### 1. Automatic Builds on Tags

Create and push a git tag:
```bash
git tag v1.0.0
git push origin v1.0.0
```

This will:
- Trigger builds for all three platforms in parallel
- Create GitHub release with all artifacts
- Generate release notes automatically

#### 2. Manual Builds

1. Go to Actions tab in GitHub
2. Select "Build Multi-Platform" workflow
3. Click "Run workflow"
4. Enter version number (e.g., 1.0.0)
5. Click "Run workflow"

This creates artifacts but does NOT create a GitHub release.

### Build Process

Each platform builds in parallel on its native runner:

1. **Checkout code** with Git LFS support
2. **Install dependencies** (CMake, .NET SDK, build tools)
3. **Cache Unity Library** for faster subsequent builds
4. **Run Unity Build** using game-ci/unity-builder action
5. **Package build** into platform-specific archive
6. **Upload artifact** to GitHub

Build logs are available in the Actions tab.

### Build Times

Expected build times (first build):
- Windows: ~10-15 minutes
- macOS: ~10-15 minutes
- Linux: ~10-15 minutes

Subsequent builds with cache: ~5-8 minutes per platform

Total pipeline time: ~15 minutes (parallel execution)

## Cesium for Unity Considerations

### Native Compilation

Cesium for Unity includes native C++ libraries that must be compiled during the build process:

- **Build Hook**: `CompileCesiumForUnityNative.cs` automatically triggers compilation
- **Platform-Specific**: Each platform compiles its own native binaries
- **Cross-Compilation**: NOT supported - must build on native OS
- **First Build**: Takes 5-10 minutes for C++ compilation
- **Cached Builds**: Much faster as natives are cached

### Required Toolchains

| Platform | Required Tools |
|----------|----------------|
| Windows  | CMake 3.18+, Visual Studio 2022, .NET SDK 6.0+ |
| macOS    | CMake 3.18+, Xcode CLI Tools, .NET SDK 6.0+ |
| Linux    | CMake 3.18+, GCC/Clang C++20, .NET SDK 6.0+ |

### Cesium Ion Token

The token can be configured in two ways:

1. **Unity Editor** (development):
   - Configure in Cesium window
   - Stored in project settings
   - Used for local builds

2. **Environment Variable** (CI/CD):
   - Set `CESIUM_ION_TOKEN` secret in GitHub
   - Injected during build process
   - Recommended for security

## Version Management

### Automatic Version Updates

The `VersionManager.cs` script automatically updates the version:

1. **From git tags**: When you create a tag like `v1.0.0`
2. **From build-info.json**: Generated by `git-build-info` package
3. **Manual**: Use Unity menu "Build → Update Version from Git"

### Version Format

- Git tags: `v1.0.0`, `v2.1.0-beta`, etc.
- `v` prefix is automatically removed
- Supports semantic versioning

### Checking Current Version

In Unity Editor:
- Menu: Build → Show Current Version
- Logs version, product name, and company name

## Build Configuration

### Player Settings

Configure in Unity Editor (Edit → Project Settings → Player):

- **Product Name**: "3D GeoGame"
- **Company Name**: "DefaultCompany"
- **Bundle Version**: Auto-updated from git
- **Default Screen Width**: 1024
- **Default Screen Height**: 768

### Build Settings

Configured in `BuildScript.cs`:

- **Development Build**: OFF (release mode)
- **Script Debugging**: OFF
- **Compression**: LZ4 (faster load times)
- **Scripting Backend**: IL2CPP (better performance)
- **Architecture** (macOS): Universal (Intel + Apple Silicon)

### Quality Settings

Configure in Unity Editor (Edit → Project Settings → Quality):
- Optimize for each platform as needed
- Current settings are suitable for mid-range hardware

## Packaging

### Windows (.zip)

```
FlightSim_Windows_v1.0.0.zip
├── FlightSim.exe
├── FlightSim_Data/
├── UnityPlayer.dll
├── UnityCrashHandler64.exe
└── README.txt
```

### macOS (.dmg)

```
FlightSim_Mac_v1.0.0.dmg
└── FlightSim.app/
    └── Contents/
        ├── MacOS/
        ├── Resources/
        └── Frameworks/
```

### Linux (.tar.gz)

```
FlightSim_Linux_v1.0.0.tar.gz
├── FlightSim.x86_64
├── FlightSim_Data/
└── UnityPlayer.so
```

## Distribution

### Including README in Builds

The `Distribution/README.txt` file should be copied into each build:

1. Add a post-build step in `BuildScript.cs`
2. Or manually copy before packaging
3. Update `{{VERSION}}` placeholder with actual version

### Recommended Distribution Channels

1. **GitHub Releases** (automated)
   - Automatic for tagged builds
   - Includes all three platforms
   - Generates release notes

2. **itch.io** (optional)
   - Can add automatic deployment
   - Modify workflow to use butler

3. **Steam** (requires Steamworks SDK)
   - Not currently configured
   - Can be added if needed

## Troubleshooting

### Build Fails Locally

1. **Check Unity Path**:
   ```bash
   echo $UNITY_PATH
   which unity  # macOS/Linux
   ```

2. **Check Dependencies**:
   ```bash
   cmake --version  # Should be 3.18+
   dotnet --version  # Should be 6.0+
   ```

3. **Check Logs**:
   ```bash
   cat Builds/windows_build.log
   cat Builds/mac_build.log
   cat Builds/linux_build.log
   ```

### GitHub Actions Build Fails

1. **Check Secrets**: Verify UNITY_LICENSE, UNITY_EMAIL, UNITY_PASSWORD
2. **Check Logs**: Go to Actions tab → failed workflow → view logs
3. **Check Dependencies**: Ensure all dependency installation steps succeed
4. **License Issues**: Unity license may have expired

### Common Errors

**"Unity not found"**
- Set UNITY_PATH environment variable
- Verify Unity is installed at that path

**"Cesium native compilation failed"**
- Check CMake installation
- Verify C++ compiler is available
- Ensure .NET SDK 6.0+ is installed

**"License activation failed"** (CI/CD)
- Verify UNITY_LICENSE secret is correct
- Check UNITY_EMAIL and UNITY_PASSWORD
- License may need to be refreshed

**"Out of memory"**
- Increase runner memory
- Enable build caching
- Consider building platforms separately

## Performance Optimization

### Caching Strategy

GitHub Actions caches:
- Unity Library folder (biggest impact)
- Key: Based on Assets, Packages, ProjectSettings
- Restore: Uses partial key matching

### Parallel Builds

All three platform builds run simultaneously:
- Windows on windows-latest runner
- macOS on macos-latest runner
- Linux on ubuntu-latest runner

### Build Time Reduction

1. **Enable caching** (already configured)
2. **Use build artifacts** from previous builds
3. **Incremental builds** (Unity Library cache)
4. **Parallel execution** (already configured)

## Future Enhancements

### Planned Features

- [ ] Automated smoke tests post-build
- [ ] itch.io automatic deployment
- [ ] Steam deployment pipeline
- [ ] Build notifications (Discord, Slack)
- [ ] Artifact retention policies
- [ ] Build badges in README

### Optional Additions

- Unit tests before build
- Performance profiling
- Asset bundle optimization
- Build size tracking
- Automated changelogs

## References

- [Unity Build Automation](https://docs.unity3d.com/Manual/CommandLineArguments.html)
- [GameCI Unity Builder](https://game.ci/docs/github/builder)
- [Cesium for Unity Documentation](https://cesium.com/docs/tutorials/cesium-for-unity/)
- [GitHub Actions Documentation](https://docs.github.com/en/actions)

## Support

For issues or questions about the build system:
1. Check this documentation
2. Review build logs
3. Create GitHub issue with:
   - Platform being built
   - Build log output
   - Steps to reproduce
