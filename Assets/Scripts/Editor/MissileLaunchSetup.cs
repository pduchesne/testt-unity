using UnityEngine;
using UnityEditor;
using TMPro;
using GeoGame3D.UI;

namespace GeoGame3D.Editor
{
    /// <summary>
    /// One-time setup script to configure missile system references
    /// Run this from: Tools > Setup Missile System
    /// </summary>
    public static class MissileLaunchSetup
    {
        [MenuItem("Tools/Setup Missile System")]
        public static void SetupMissileSystem()
        {
            Debug.Log("Setting up missile system references...");

            // Find FlightHUD Canvas
            GameObject hudCanvas = GameObject.Find("FlightHUD Canvas");
            if (hudCanvas == null)
            {
                Debug.LogError("Setup failed: FlightHUD Canvas not found in scene");
                return;
            }

            // Get FlightHUD component
            FlightHUD flightHUD = hudCanvas.GetComponent<FlightHUD>();
            if (flightHUD == null)
            {
                Debug.LogError("Setup failed: FlightHUD component not found on FlightHUD Canvas");
                return;
            }

            // Find Ammo Text child
            Transform ammoTextTransform = hudCanvas.transform.Find("Ammo Text");
            if (ammoTextTransform == null)
            {
                Debug.LogError("Setup failed: Ammo Text not found as child of FlightHUD Canvas");
                return;
            }

            // Get TextMeshProUGUI component
            TextMeshProUGUI ammoText = ammoTextTransform.GetComponent<TextMeshProUGUI>();
            if (ammoText == null)
            {
                Debug.LogError("Setup failed: TextMeshProUGUI component not found on Ammo Text");
                return;
            }

            // Use SerializedObject to set the reference (works in Editor)
            SerializedObject serializedHUD = new SerializedObject(flightHUD);
            SerializedProperty ammoTextProperty = serializedHUD.FindProperty("ammoText");

            if (ammoTextProperty != null)
            {
                ammoTextProperty.objectReferenceValue = ammoText;
                serializedHUD.ApplyModifiedProperties();
                Debug.Log("✅ Successfully configured FlightHUD.ammoText reference");
            }
            else
            {
                Debug.LogError("Setup failed: Could not find ammoText property on FlightHUD");
                return;
            }

            // Mark scene as dirty so Unity knows to save changes
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene()
            );

            Debug.Log("✅ Missile system setup complete! Don't forget to save the scene.");
        }
    }
}
