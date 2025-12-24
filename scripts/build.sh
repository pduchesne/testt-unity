#!/bin/bash

# Unity Multi-Platform Build Script
# Builds the project for Windows, macOS, and Linux platforms
# Usage: ./build.sh [windows|mac|linux|all] [version]

set -e

# Configuration
UNITY_PATH="${UNITY_PATH:-/opt/unity/Editor/Unity}"
PROJECT_PATH="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
BUILD_OUTPUT_DIR="${PROJECT_PATH}/Builds"
BUILD_METHOD="BuildScript.PerformBuild"
DEFAULT_VERSION="0.1.0"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Functions
print_usage() {
    echo "Usage: $0 [windows|mac|linux|all] [version]"
    echo ""
    echo "Arguments:"
    echo "  platform  - Target platform (windows, mac, linux, or all)"
    echo "  version   - Build version (default: ${DEFAULT_VERSION})"
    echo ""
    echo "Environment Variables:"
    echo "  UNITY_PATH           - Path to Unity executable"
    echo "  CESIUM_ION_TOKEN     - Cesium ion access token"
    echo "  BUILD_OUTPUT_DIR     - Output directory for builds"
    echo ""
    echo "Examples:"
    echo "  $0 windows 1.0.0"
    echo "  $0 all"
    echo "  UNITY_PATH=/Applications/Unity/Hub/Editor/6000.3.2f1/Unity.app/Contents/MacOS/Unity $0 mac"
}

log_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

log_warn() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

check_unity() {
    if [ ! -f "${UNITY_PATH}" ]; then
        log_error "Unity not found at: ${UNITY_PATH}"
        log_info "Please set UNITY_PATH environment variable"
        exit 1
    fi
    log_info "Using Unity at: ${UNITY_PATH}"
}

check_cesium_token() {
    if [ -z "${CESIUM_ION_TOKEN}" ]; then
        log_warn "CESIUM_ION_TOKEN not set - build may fail if token is not configured in Unity"
    else
        log_info "Cesium ion token configured"
    fi
}

build_platform() {
    local platform=$1
    local version=$2
    local build_target=""
    local output_path=""

    case "${platform}" in
        windows)
            build_target="Win64"
            output_path="${BUILD_OUTPUT_DIR}/Windows/FlightSim.exe"
            ;;
        mac)
            build_target="OSXUniversal"
            output_path="${BUILD_OUTPUT_DIR}/Mac/FlightSim.app"
            ;;
        linux)
            build_target="Linux64"
            output_path="${BUILD_OUTPUT_DIR}/Linux/FlightSim.x86_64"
            ;;
        *)
            log_error "Unknown platform: ${platform}"
            return 1
            ;;
    esac

    log_info "Building for ${platform} (${build_target})..."
    log_info "Output: ${output_path}"

    # Create build directory
    mkdir -p "$(dirname "${output_path}")"

    # Build command
    "${UNITY_PATH}" \
        -quit \
        -batchmode \
        -nographics \
        -projectPath "${PROJECT_PATH}" \
        -buildTarget "${build_target}" \
        -executeMethod "${BUILD_METHOD}" \
        -logFile "${BUILD_OUTPUT_DIR}/${platform}_build.log" \
        -buildPath "${output_path}" \
        -buildVersion "${version}"

    local exit_code=$?

    if [ $exit_code -eq 0 ]; then
        log_info "✓ ${platform} build completed successfully"

        # Create archive
        package_build "${platform}" "${version}"
    else
        log_error "✗ ${platform} build failed (exit code: ${exit_code})"
        log_info "Check log: ${BUILD_OUTPUT_DIR}/${platform}_build.log"
        return $exit_code
    fi
}

package_build() {
    local platform=$1
    local version=$2
    local archive_name=""
    local source_dir=""

    case "${platform}" in
        windows)
            archive_name="FlightSim_Windows_v${version}.zip"
            source_dir="${BUILD_OUTPUT_DIR}/Windows"
            cd "${source_dir}"
            zip -r "../${archive_name}" . > /dev/null
            ;;
        mac)
            archive_name="FlightSim_Mac_v${version}.dmg"
            source_dir="${BUILD_OUTPUT_DIR}/Mac"
            # For DMG creation, we need hdiutil (macOS only)
            if command -v hdiutil &> /dev/null; then
                hdiutil create -volname "FlightSim" -srcfolder "${source_dir}" -ov -format UDZO "${BUILD_OUTPUT_DIR}/${archive_name}"
            else
                log_warn "hdiutil not found - creating tar.gz instead of DMG"
                archive_name="FlightSim_Mac_v${version}.tar.gz"
                cd "${source_dir}"
                tar czf "../${archive_name}" . > /dev/null
            fi
            ;;
        linux)
            archive_name="FlightSim_Linux_v${version}.tar.gz"
            source_dir="${BUILD_OUTPUT_DIR}/Linux"
            cd "${source_dir}"
            tar czf "../${archive_name}" . > /dev/null
            ;;
    esac

    cd "${PROJECT_PATH}"
    log_info "Created archive: ${archive_name}"
}

# Main script
main() {
    local platform="${1:-all}"
    local version="${2:-${DEFAULT_VERSION}}"

    log_info "=== Unity Multi-Platform Build Script ==="
    log_info "Project: ${PROJECT_PATH}"
    log_info "Platform: ${platform}"
    log_info "Version: ${version}"
    log_info ""

    check_unity
    check_cesium_token

    # Create build output directory
    mkdir -p "${BUILD_OUTPUT_DIR}"

    case "${platform}" in
        windows|mac|linux)
            build_platform "${platform}" "${version}"
            ;;
        all)
            log_info "Building all platforms..."
            build_platform "windows" "${version}" || log_error "Windows build failed"
            build_platform "mac" "${version}" || log_error "Mac build failed"
            build_platform "linux" "${version}" || log_error "Linux build failed"
            ;;
        help|--help|-h)
            print_usage
            exit 0
            ;;
        *)
            log_error "Invalid platform: ${platform}"
            print_usage
            exit 1
            ;;
    esac

    log_info ""
    log_info "=== Build Complete ==="
    log_info "Builds available in: ${BUILD_OUTPUT_DIR}"
}

# Run main function
main "$@"
