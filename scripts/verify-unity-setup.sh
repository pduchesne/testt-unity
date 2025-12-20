#!/bin/bash

# GeoGame3D Unity Setup Verification Script
# This script checks if Unity project was created successfully

echo "🔍 Verifying Unity Setup for GeoGame3D..."
echo ""

PROJECT_ROOT="/home/pduchesne/Sources/claude/hilats/geogame3d"
cd "$PROJECT_ROOT"

# Check for required directories
echo "Checking directory structure..."
REQUIRED_DIRS=("Assets" "Packages" "ProjectSettings")
MISSING_DIRS=()

for dir in "${REQUIRED_DIRS[@]}"; do
    if [ -d "$dir" ]; then
        echo "  ✅ $dir/ exists"
    else
        echo "  ❌ $dir/ missing"
        MISSING_DIRS+=("$dir")
    fi
done

# Check for Packages/manifest.json
echo ""
echo "Checking Unity package configuration..."
if [ -f "Packages/manifest.json" ]; then
    echo "  ✅ Packages/manifest.json exists"

    # Check for Cesium
    if grep -q "cesium" "Packages/manifest.json"; then
        echo "  ✅ Cesium for Unity is configured"
    else
        echo "  ⚠️  Cesium for Unity not found in manifest"
    fi

    # Check for Input System
    if grep -q "inputsystem" "Packages/manifest.json"; then
        echo "  ✅ Unity Input System is configured"
    else
        echo "  ⚠️  Unity Input System not found in manifest"
    fi
else
    echo "  ❌ Packages/manifest.json missing"
fi

# Check for ProjectSettings/ProjectVersion.txt
echo ""
echo "Checking Unity version..."
if [ -f "ProjectSettings/ProjectVersion.txt" ]; then
    echo "  ✅ Project version file exists"
    UNITY_VERSION=$(grep "m_EditorVersion:" ProjectSettings/ProjectVersion.txt | cut -d' ' -f2)
    echo "  📌 Unity version: $UNITY_VERSION"
else
    echo "  ❌ ProjectSettings/ProjectVersion.txt missing"
fi

# Summary
echo ""
echo "=========================================="
if [ ${#MISSING_DIRS[@]} -eq 0 ]; then
    echo "✅ Unity project structure verified!"
    echo ""
    echo "Next steps:"
    echo "  1. Open the project in Unity Editor"
    echo "  2. Install Cesium for Unity (if not already installed)"
    echo "  3. Configure Cesium ion token"
    echo "  4. Return here for automated script setup"
else
    echo "❌ Unity project not fully set up"
    echo ""
    echo "Missing directories: ${MISSING_DIRS[*]}"
    echo ""
    echo "Please complete Unity project creation first:"
    echo "  1. Open Unity Hub"
    echo "  2. Create new 3D project in this directory"
    echo "  3. Run this script again"
fi
echo "=========================================="
