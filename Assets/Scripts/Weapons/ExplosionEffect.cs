using UnityEngine;
using GeoGame3D.Utils;

namespace GeoGame3D.Weapons
{
    /// <summary>
    /// Explosion visual effect controller.
    /// Manages particle system lifecycle and auto-cleanup.
    /// </summary>
    [RequireComponent(typeof(ParticleSystem))]
    public class ExplosionEffect : MonoBehaviour
    {
        [Header("Lifetime Settings")]
        [SerializeField] private float lifetime = 2f; // seconds before cleanup

        [Header("Optional Audio")]
        [SerializeField] private AudioClip explosionSound;
        [SerializeField] private float soundVolume = 0.5f;

        private ParticleSystem particleSystem;
        private AudioSource audioSource;
        private float spawnTime;

        private void Awake()
        {
            particleSystem = GetComponent<ParticleSystem>();

            // Setup audio if sound is provided
            if (explosionSound != null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.clip = explosionSound;
                audioSource.volume = soundVolume;
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 1f; // 3D sound
                audioSource.minDistance = 10f;
                audioSource.maxDistance = 500f;
            }

            spawnTime = Time.time;
        }

        private void Start()
        {
            // Play particle system
            if (particleSystem != null)
            {
                particleSystem.Play();
                SimpleLogger.Debug("Weapons", "Explosion particle system started");
            }

            // Play explosion sound
            if (audioSource != null)
            {
                audioSource.Play();
                SimpleLogger.Debug("Weapons", "Explosion sound played");
            }
        }

        private void Update()
        {
            // Auto-cleanup after lifetime
            if (Time.time - spawnTime > lifetime)
            {
                SimpleLogger.Debug("Weapons", "Explosion effect cleaned up");
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Manually destroy the explosion effect
        /// </summary>
        public void Cleanup()
        {
            Destroy(gameObject);
        }
    }
}
