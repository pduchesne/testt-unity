#!/bin/bash
# Apply Linux support patch to Cesium for Unity
# This script adds LinuxStandalone64 to the Cesium assembly definition

set -e

ASMDEF_FILE="Packages/com.cesium.unity/Source/CesiumForUnity.asmdef"
PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

cd "$PROJECT_ROOT"

# Colors
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m'

echo -e "${GREEN}Cesium Linux Support Patch${NC}"
echo "=========================================="

if [ ! -f "$ASMDEF_FILE" ]; then
    echo -e "${RED}✗ Error: Cesium assembly definition not found${NC}"
    echo "  Expected: $ASMDEF_FILE"
    echo "  Ensure Cesium for Unity package is installed"
    exit 1
fi

# Check if Linux support is already added
if grep -q "LinuxStandalone64" "$ASMDEF_FILE"; then
    echo -e "${GREEN}✓ Linux support already present${NC}"
    echo "  No changes needed"
    exit 0
fi

echo -e "${YELLOW}Adding Linux support...${NC}"

# Create backup
cp "$ASMDEF_FILE" "${ASMDEF_FILE}.backup"
echo "  Backup created: ${ASMDEF_FILE}.backup"

# Add LinuxStandalone64 after iOS in the includePlatforms list
sed -i 's/"iOS",/"iOS",\n        "LinuxStandalone64",/' "$ASMDEF_FILE"

# Verify the change was made
if grep -q "LinuxStandalone64" "$ASMDEF_FILE"; then
    echo -e "${GREEN}✓ Linux support added successfully${NC}"
    echo ""
    echo "Next steps:"
    echo "  1. Restart Unity Editor if it's currently open"
    echo "  2. Test Linux build: ./scripts/build.sh linux 0.1.0-test"
    echo ""
    echo -e "${YELLOW}Note: This change will be lost if Cesium package is updated${NC}"
    echo "      Run this script again after Cesium updates"
else
    echo -e "${RED}✗ Failed to add Linux support${NC}"
    echo "  Restoring backup..."
    mv "${ASMDEF_FILE}.backup" "$ASMDEF_FILE"
    exit 1
fi
