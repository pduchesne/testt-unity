using UnityEngine;
using GeoGame3D.Utils;

namespace GeoGame3D.Weapons
{
    /// <summary>
    /// Missile launcher weapon system for the aircraft.
    /// Manages ammo, cooldown, and missile spawning.
    /// </summary>
    public class MissileLauncher : MonoBehaviour
    {
        [Header("Missile Settings")]
        [SerializeField] private GameObject missilePrefab;
        [SerializeField] private Transform launchPoint;

        [Header("Ammo Settings")]
        [SerializeField] private int maxAmmo = 10;
        [SerializeField] private float cooldownTime = 1f; // seconds between shots

        // State
        private int currentAmmo;
        private float lastFireTime = -999f; // Allow firing immediately on start
        private Rigidbody aircraftRigidbody;

        // Public properties for HUD
        public int CurrentAmmo => currentAmmo;
        public int MaxAmmo => maxAmmo;
        public bool HasAmmo => currentAmmo > 0;

        private void Awake()
        {
            // Initialize ammo
            currentAmmo = maxAmmo;

            // Get aircraft rigidbody for velocity inheritance
            aircraftRigidbody = GetComponent<Rigidbody>();
            if (aircraftRigidbody == null)
            {
                SimpleLogger.Warning("Weapons", "MissileLauncher: No Rigidbody found on aircraft");
            }

            // Validate launch point
            if (launchPoint == null)
            {
                SimpleLogger.Error("Weapons", "MissileLauncher: Launch point not assigned!");
            }

            // Validate missile prefab
            if (missilePrefab == null)
            {
                SimpleLogger.Error("Weapons", "MissileLauncher: Missile prefab not assigned!");
            }

            SimpleLogger.Info("Weapons", $"MissileLauncher initialized: {currentAmmo}/{maxAmmo} ammo");
        }

        /// <summary>
        /// Check if the launcher can fire (has ammo and cooldown elapsed)
        /// </summary>
        public bool CanFire()
        {
            if (!HasAmmo)
            {
                return false;
            }

            float timeSinceLastFire = Time.time - lastFireTime;
            return timeSinceLastFire >= cooldownTime;
        }

        /// <summary>
        /// Fire a missile
        /// </summary>
        public void Fire()
        {
            if (!CanFire())
            {
                if (!HasAmmo)
                {
                    SimpleLogger.Warning("Weapons", "Cannot fire: Out of ammo");
                }
                else
                {
                    float timeRemaining = cooldownTime - (Time.time - lastFireTime);
                    SimpleLogger.Debug("Weapons", $"Cannot fire: Cooldown ({timeRemaining:F1}s remaining)");
                }
                return;
            }

            if (missilePrefab == null || launchPoint == null)
            {
                SimpleLogger.Error("Weapons", "Cannot fire: Missing prefab or launch point");
                return;
            }

            // Spawn missile at launch point
            GameObject missileObj = Instantiate(missilePrefab, launchPoint.position, launchPoint.rotation);

            // Get missile component and initialize it with aircraft velocity
            Missile missile = missileObj.GetComponent<Missile>();
            if (missile != null && aircraftRigidbody != null)
            {
                missile.Initialize(aircraftRigidbody.linearVelocity);
            }
            else if (missile != null)
            {
                missile.Initialize(Vector3.zero);
            }

            // Update state
            currentAmmo--;
            lastFireTime = Time.time;

            SimpleLogger.Info("Weapons", $"Missile fired! Ammo remaining: {currentAmmo}/{maxAmmo}");
        }

        /// <summary>
        /// Reload ammo to maximum (for debugging or future pickup system)
        /// </summary>
        public void Reload()
        {
            currentAmmo = maxAmmo;
            SimpleLogger.Info("Weapons", $"Ammo reloaded: {currentAmmo}/{maxAmmo}");
        }

        #region Debug

        private void OnDrawGizmos()
        {
            if (launchPoint != null)
            {
                // Draw launch point
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(launchPoint.position, 0.5f);

                // Draw launch direction
                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(launchPoint.position, launchPoint.forward * 10f);
            }
        }

        #endregion
    }
}
