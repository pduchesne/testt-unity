# GitHub Actions Build Workflow

This directory contains the automated build pipeline for the GeoGame3D project.

## Overview

The `build.yml` workflow builds the game for Windows, macOS, and Linux platforms automatically using Unity and GitHub Actions.

## Workflow Triggers

- **Git tags**: Automatically triggers on version tags (e.g., `v1.0.0`)
- **Manual dispatch**: Can be triggered manually from GitHub Actions tab with custom version input

## Build Jobs

The workflow runs three parallel build jobs:

### 1. Windows Build (`build-windows`)
- **Runner**: `windows-latest`
- **Platform**: StandaloneWindows64
- **Output**: `FlightSim_Windows_v{VERSION}.zip`
- **Dependencies**: CMake, .NET SDK 6.0+, Visual Studio Build Tools

### 2. macOS Build (`build-mac`)
- **Runner**: `macos-latest`
- **Platform**: StandaloneOSX (Universal binary - Intel + Apple Silicon)
- **Output**: `FlightSim_Mac_v{VERSION}.dmg`
- **Dependencies**: CMake, .NET SDK, Xcode Command Line Tools

### 3. Linux Build (`build-linux`)
- **Runner**: `ubuntu-latest`
- **Platform**: StandaloneLinux64
- **Output**: `FlightSim_Linux_v{VERSION}.tar.gz`
- **Dependencies**: CMake, .NET SDK 6.0+, GCC/Clang, build-essential

## Cesium for Unity Integration

The project uses [Cesium for Unity](https://github.com/CesiumGS/cesium-unity) which requires special handling:

### Package Resolution
- Cesium is installed as a **git package** via Unity Package Manager
- Resolved to `Library/PackageCache/com.cesium.unity@{hash}/` during build
- No custom/embedded package in the repository

### Native Library Compilation
- Cesium automatically compiles C++ native libraries during Unity build via `CompileCesiumForUnityNative.cs`
- Requires platform-specific toolchains:
  - **Windows**: CMake 3.18+, Visual Studio 2022, .NET SDK 6.0+
  - **macOS**: CMake 3.18+, Xcode Command Line Tools, .NET SDK 6.0+
  - **Linux**: CMake 3.18+, GCC/Clang with C++20 support, .NET SDK 6.0+

### Linux Support Patch
- The BuildScript automatically applies a Linux support patch for Cesium
- Adds `LinuxStandalone64` to Cesium's assembly definition
- Applied automatically before Linux builds in `BuildScript.ApplyCesiumLinuxPatch()`

## Required Secrets

Configure these secrets in your GitHub repository (Settings → Secrets & Variables → Actions):

| Secret Name | Description | Required |
|------------|-------------|----------|
| `UNITY_LICENSE` | Unity license file content (Personal or Pro) | Yes |
| `UNITY_EMAIL` | Unity account email | Yes |
| `UNITY_PASSWORD` | Unity account password | Yes |
| `CESIUM_ION_TOKEN` | Cesium ion access token (optional) | No |

### How to Get Unity License

For **Unity Personal** (free):
1. Install Unity locally
2. Activate your license
3. Find the license file:
   - **Windows**: `C:\ProgramData\Unity\Unity_lic.ulf`
   - **macOS**: `/Library/Application Support/Unity/Unity_lic.ulf`
   - **Linux**: `~/.local/share/unity3d/Unity/Unity_lic.ulf`
4. Copy the entire contents and paste into `UNITY_LICENSE` secret

For **Unity Plus/Pro**:
- Use the same process, but also set `UNITY_SERIAL` secret with your serial number

## Build Process

Each build job follows these steps:

1. **Checkout** - Clone repository with LFS and full git history
2. **Install Dependencies** - Install CMake, .NET SDK, and platform-specific build tools
3. **Cache Unity Library** - Cache Unity Library folder for faster subsequent builds
4. **Setup Unity** - Install Unity 6000.3.2f1 and activate license
5. **Build** - Execute `BuildScript.PerformBuild` with platform-specific parameters
6. **Package** - Create platform-specific archive (ZIP/DMG/tar.gz)
7. **Upload Artifacts** - Upload build artifacts (retained for 30 days)

## Release Creation

When triggered by a git tag (e.g., `v1.0.0`):
- All three platform builds run in parallel
- After successful builds, a GitHub Release is created automatically
- All build artifacts are attached to the release
- Release notes are generated automatically from commit history

## Manual Build Trigger

To trigger a build manually:

1. Go to **Actions** tab in GitHub repository
2. Select **Build Multi-Platform** workflow
3. Click **Run workflow**
4. Enter version number (e.g., `1.0.0`)
5. Click **Run workflow**

## Local Build Testing

To test builds locally before pushing:

```bash
# Build all platforms
./scripts/build.sh all 0.1.0-test

# Build specific platform
./scripts/build.sh windows 0.1.0-test
./scripts/build.sh mac 0.1.0-test
./scripts/build.sh linux 0.1.0-test
```

Ensure you have the same dependencies installed locally as specified in the workflow.

## Troubleshooting

### Build Fails with "License Activation Failed"
- Check that `UNITY_LICENSE`, `UNITY_EMAIL`, and `UNITY_PASSWORD` secrets are set correctly
- Ensure license file content is complete and not corrupted

### Cesium Native Compilation Fails
- Verify all build dependencies are installed (CMake, .NET SDK, compilers)
- Check Unity build logs for specific compilation errors
- Ensure sufficient disk space (Cesium build requires ~2-3 GB)

### Package Cache Issues
- Delete `Library/PackageCache` locally and let Unity re-resolve packages
- Ensure `Packages/manifest.json` has correct Cesium git URL
- Check that `packages-lock.json` is NOT in repository (should be gitignored)

## Build Time Estimates

Approximate build times on GitHub-hosted runners:

- **First build** (no cache): 20-30 minutes per platform
- **Subsequent builds** (with cache): 10-15 minutes per platform
- **Cesium native compilation**: 5-10 minutes per platform

## Further Reading

- [Unity game-ci Documentation](https://game.ci/docs/github/getting-started)
- [Cesium for Unity Documentation](https://cesium.com/learn/unity/)
- [GitHub Actions Documentation](https://docs.github.com/en/actions)
