# Missile System - Final Configuration Steps

The missile firing system has been implemented but requires manual configuration in Unity Editor.

## Quick Setup (2 minutes)

### 1. Configure MissileLauncher on Aircraft
1. Open `MainScene` in Unity
2. Select `Aircraft` GameObject in Hierarchy
3. Find `Missile Launcher (Script)` component in Inspector
4. Configure fields:
   - **Missile Prefab**: Drag `Assets/Prefabs/Missile.prefab` from Project window
   - **Launch Point**: Drag `MissileLaunchPoint` (child of Aircraft) from Hierarchy

### 2. Configure FlightHUD
1. Select `FlightHUD Canvas` GameObject in Hierarchy
2. Find `Flight HUD (Script)` component in Inspector
3. Configure field:
   - **Ammo Text**: Drag `Ammo Text` (child of FlightHUD Canvas) from Hierarchy

### 3. Optional: Configure FlightHUD to show MissileLauncher
1. Still in `Flight HUD (Script)` component
2. Configure field:
   - **Missile Launcher**: Drag `Aircraft` GameObject (it has the MissileLauncher component)

## Test the System

1. Press Play in Unity
2. Press **Spacebar** to fire missiles
3. Observe:
   - ✅ Missile spawns from aircraft front
   - ✅ Ballistic trajectory with gravity
   - ✅ Explosion on impact with terrain
   - ✅ Ammo count decreases (bottom-right of HUD)
   - ✅ Ammo color changes (Green → Yellow → Red)

## Optional: Enhance Explosion Effects

1. Open `Assets/Prefabs/Explosion.prefab`
2. Select the Explosion GameObject
3. Find `Particle System` component
4. Configure for better visuals:
   - **Start Color**: Set gradient (Orange → Yellow → Gray)
   - **Emission**: Set burst to 30-50 particles
   - **Start Speed**: 20-40 m/s (random)
   - **Start Size**: 2-4 meters (random)
   - **Gravity Modifier**: 0.5 (particles fall slightly)

## System Features

- **Fire**: Spacebar
- **Ammo**: 10 missiles (limited)
- **Cooldown**: 1 second between shots
- **Physics**: Ballistic trajectory with gravity
- **Collision**: Raycast-based (works with Cesium buildings)
- **Lifetime**: 10 seconds (auto-cleanup)
- **One at a time**: Previous missile must hit before next fires

## Troubleshooting

**Missiles don't fire:**
- Check Missile Prefab is assigned in MissileLauncher
- Check Launch Point is assigned in MissileLauncher
- Check Console for errors

**Ammo doesn't display:**
- Check Ammo Text is assigned in FlightHUD
- Check Ammo Text GameObject is active in hierarchy

**No explosions:**
- Explosion prefab is automatically assigned in Missile.prefab
- Check Explosion.prefab exists in Assets/Prefabs/

**Missiles pass through terrain:**
- This is expected - raycast collision is configured
- Missiles should stop and explode on impact
- If not, check collision layers
