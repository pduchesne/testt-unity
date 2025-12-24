using UnityEngine;
using GeoGame3D.Utils;

namespace GeoGame3D.Weapons
{
    /// <summary>
    /// Missile projectile with ballistic physics and raycast-based collision detection.
    /// Designed to work with Cesium buildings that have no colliders.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class Missile : MonoBehaviour
    {
        [Header("Flight Settings")]
        [SerializeField] private float launchSpeed = 100f; // m/s additional speed beyond aircraft velocity
        [SerializeField] private float lifetime = 10f; // seconds before self-destruct

        [Header("Collision Detection")]
        [SerializeField] private float collisionCheckDistance = 10f; // raycast distance per frame
        [SerializeField] private LayerMask collisionLayers = -1; // all layers by default

        [Header("Effects")]
        [SerializeField] private GameObject explosionPrefab;

        // State
        private Rigidbody rb;
        private float spawnTime;
        private bool hasExploded = false;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.useGravity = true;
            rb.linearDamping = 0f; // No air resistance for simplicity

            spawnTime = Time.time;
        }

        /// <summary>
        /// Initialize missile with aircraft velocity
        /// </summary>
        /// <param name="aircraftVelocity">Current velocity of the aircraft</param>
        public void Initialize(Vector3 aircraftVelocity)
        {
            if (rb == null)
            {
                rb = GetComponent<Rigidbody>();
            }

            // Set initial velocity: aircraft velocity + forward thrust
            Vector3 launchVelocity = aircraftVelocity + transform.forward * launchSpeed;
            rb.linearVelocity = launchVelocity;

            SimpleLogger.Info("Weapons", $"Missile initialized: velocity={launchVelocity.magnitude:F1} m/s");
        }

        private void FixedUpdate()
        {
            // Check lifetime
            if (Time.time - spawnTime > lifetime)
            {
                SimpleLogger.Debug("Weapons", "Missile lifetime expired");
                SelfDestruct();
                return;
            }

            // Raycast-based collision detection
            CheckCollision();
        }

        private void CheckCollision()
        {
            if (hasExploded)
            {
                return;
            }

            // Calculate distance to check based on current velocity
            Vector3 velocity = rb.linearVelocity;
            if (velocity.magnitude < 0.1f)
            {
                return; // Not moving, skip check
            }

            float checkDistance = velocity.magnitude * Time.fixedDeltaTime + collisionCheckDistance;
            Vector3 direction = velocity.normalized;

            // Raycast forward to detect collisions
            RaycastHit hit;
            if (Physics.Raycast(transform.position, direction, out hit, checkDistance, collisionLayers))
            {
                SimpleLogger.Info("Weapons", $"Missile hit: {hit.collider.gameObject.name} at distance {hit.distance:F1}m");
                Explode(hit.point);
            }
        }

        private void Explode(Vector3 position)
        {
            if (hasExploded)
            {
                return;
            }

            hasExploded = true;

            // Spawn explosion effect
            if (explosionPrefab != null)
            {
                Instantiate(explosionPrefab, position, Quaternion.identity);
                SimpleLogger.Debug("Weapons", $"Explosion spawned at {position}");
            }
            else
            {
                SimpleLogger.Warning("Weapons", "No explosion prefab assigned to missile");
            }

            // Destroy missile
            Destroy(gameObject);
        }

        private void SelfDestruct()
        {
            // Destroy without explosion
            SimpleLogger.Debug("Weapons", "Missile self-destructed (no explosion)");
            Destroy(gameObject);
        }

        /// <summary>
        /// Handle physics-based collision (backup for objects with colliders)
        /// </summary>
        private void OnCollisionEnter(Collision collision)
        {
            if (!hasExploded)
            {
                SimpleLogger.Info("Weapons", $"Missile collision: {collision.gameObject.name}");
                Explode(collision.contacts[0].point);
            }
        }

        #region Debug

        private void OnDrawGizmos()
        {
            if (Application.isPlaying && rb != null && !hasExploded)
            {
                // Draw velocity vector
                Gizmos.color = Color.red;
                Gizmos.DrawRay(transform.position, rb.linearVelocity);

                // Draw collision check ray
                Vector3 velocity = rb.linearVelocity;
                if (velocity.magnitude > 0.1f)
                {
                    float checkDistance = velocity.magnitude * Time.fixedDeltaTime + collisionCheckDistance;
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawRay(transform.position, velocity.normalized * checkDistance);
                }
            }
        }

        #endregion
    }
}
