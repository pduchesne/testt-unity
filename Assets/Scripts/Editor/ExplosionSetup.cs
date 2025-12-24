using UnityEngine;
using UnityEditor;

namespace GeoGame3D.Editor
{
    /// <summary>
    /// One-time setup script to configure explosion particle system
    /// Run this from: Tools > Setup Explosion Effect
    /// </summary>
    public static class ExplosionSetup
    {
        [MenuItem("Tools/Setup Explosion Effect")]
        public static void SetupExplosionEffect()
        {
            Debug.Log("Setting up explosion particle system...");

            // Load the Explosion prefab
            string prefabPath = "Assets/Prefabs/Explosion.prefab";
            GameObject explosionPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (explosionPrefab == null)
            {
                Debug.LogError($"Setup failed: Explosion prefab not found at {prefabPath}");
                return;
            }

            // Get the ParticleSystem component
            ParticleSystem ps = explosionPrefab.GetComponent<ParticleSystem>();
            if (ps == null)
            {
                Debug.LogError("Setup failed: ParticleSystem component not found on Explosion prefab");
                return;
            }

            // Configure Main module
            var main = ps.main;
            main.duration = 1.0f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(20f, 50f);
            main.startSize = new ParticleSystem.MinMaxCurve(3f, 10f);
            main.gravityModifier = 0.5f;

            // Set start color to orange-yellow gradient
            Gradient colorGradient = new Gradient();
            colorGradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(new Color(1f, 0.5f, 0f), 0f), // Orange
                    new GradientColorKey(new Color(1f, 1f, 0f), 0.5f), // Yellow
                    new GradientColorKey(new Color(0.3f, 0.3f, 0.3f), 1f)  // Gray
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(1f, 0.5f),
                    new GradientAlphaKey(0f, 1f)  // Fade out
                }
            );
            main.startColor = new ParticleSystem.MinMaxGradient(colorGradient);

            // Configure Emission module - burst of particles
            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 0; // No continuous emission
            emission.SetBursts(new ParticleSystem.Burst[] {
                new ParticleSystem.Burst(0f, 50, 100, 1, 0f) // 50-100 particles at t=0
            });

            // Configure Shape module - sphere
            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 1f;

            // Configure Color over Lifetime
            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient lifetimeGradient = new Gradient();
            lifetimeGradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(new Color(0.5f, 0.5f, 0.5f), 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(lifetimeGradient);

            // Configure Size over Lifetime - grow then shrink
            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            AnimationCurve sizeCurve = new AnimationCurve();
            sizeCurve.AddKey(0f, 0.5f);  // Start at 50%
            sizeCurve.AddKey(0.2f, 1.5f); // Grow to 150%
            sizeCurve.AddKey(1f, 0.2f);   // Shrink to 20%
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

            // Configure Renderer
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                renderer.renderMode = ParticleSystemRenderMode.Billboard;
                renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
            }

            // Mark prefab as dirty and save
            EditorUtility.SetDirty(explosionPrefab);
            AssetDatabase.SaveAssets();

            Debug.Log("✅ Explosion particle system configured successfully!");
            Debug.Log("  - Duration: 1s with 50-100 particles");
            Debug.Log("  - Colors: Orange → Yellow → Gray (fading)");
            Debug.Log("  - Size: 3-10 meters with growth/shrink animation");
            Debug.Log("  - Gravity: 0.5 (particles fall slightly)");
        }
    }
}
