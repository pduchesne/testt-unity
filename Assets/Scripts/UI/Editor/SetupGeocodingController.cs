using UnityEngine;
using UnityEditor;
using UnityEngine.Events;
using TMPro;
using GeoGame3D.UI;
using CesiumForUnity;

namespace GeoGame3D.UI.Editor
{
    public static class SetupGeocodingController
    {
        [MenuItem("Tools/Setup Geocoding Controller")]
        public static void Setup()
        {
            // Find MainMenuCanvas
            var canvas = GameObject.Find("MainMenuCanvas");
            if (canvas == null)
            {
                Debug.LogError("[Setup] MainMenuCanvas not found");
                return;
            }

            // Add GeocodingController if it doesn't exist
            var geocodingController = canvas.GetComponent<GeocodingController>();
            if (geocodingController == null)
            {
                geocodingController = canvas.AddComponent<GeocodingController>();
                Debug.Log("[Setup] Added GeocodingController to MainMenuCanvas");
            }

            // Find UI elements
            var locationInputField = GameObject.Find("LocationInputField")?.GetComponent<TMP_InputField>();
            var goButton = GameObject.Find("GoToLocationButton")?.GetComponent<UnityEngine.UI.Button>();
            var mainMenuController = canvas.GetComponent<MainMenuController>();
            var georeference = Object.FindFirstObjectByType<CesiumGeoreference>();
            var aircraft = Object.FindFirstObjectByType<GeoGame3D.Aircraft.AircraftController>();
            var aircraftTransform = aircraft?.transform;

            // Wire up references using SerializedObject
            var serializedController = new SerializedObject(geocodingController);

            if (locationInputField != null)
            {
                var inputFieldProp = serializedController.FindProperty("locationInputField");
                if (inputFieldProp != null)
                {
                    inputFieldProp.objectReferenceValue = locationInputField;
                    Debug.Log("[Setup] Wired locationInputField");
                }
            }
            else
            {
                Debug.LogWarning("[Setup] LocationInputField not found");
            }

            if (mainMenuController != null)
            {
                var mainMenuProp = serializedController.FindProperty("mainMenu");
                if (mainMenuProp != null)
                {
                    mainMenuProp.objectReferenceValue = mainMenuController;
                    Debug.Log("[Setup] Wired mainMenu");
                }
            }

            if (georeference != null)
            {
                var georeferenceProp = serializedController.FindProperty("georeference");
                if (georeferenceProp != null)
                {
                    georeferenceProp.objectReferenceValue = georeference;
                    Debug.Log("[Setup] Wired georeference");
                }
            }
            else
            {
                Debug.LogWarning("[Setup] CesiumGeoreference not found in scene");
            }

            if (aircraftTransform != null)
            {
                var aircraftProp = serializedController.FindProperty("aircraftTransform");
                if (aircraftProp != null)
                {
                    aircraftProp.objectReferenceValue = aircraftTransform;
                    Debug.Log("[Setup] Wired aircraftTransform");
                }
            }
            else
            {
                Debug.LogWarning("[Setup] Aircraft not found in scene");
            }

            serializedController.ApplyModifiedProperties();

            // Wire up the button onClick event
            if (goButton != null)
            {
                // Remove existing listeners
                goButton.onClick.RemoveAllListeners();

                // Add persistent listener
                UnityAction action = new UnityAction(geocodingController.SearchAndGoToLocation);
                UnityEditor.Events.UnityEventTools.AddPersistentListener(goButton.onClick, action);

                EditorUtility.SetDirty(goButton);
                Debug.Log("[Setup] Wired GoToLocationButton onClick event");
            }
            else
            {
                Debug.LogWarning("[Setup] GoToLocationButton not found");
            }

            // Mark scene as dirty to save changes
            EditorUtility.SetDirty(geocodingController);
            EditorUtility.SetDirty(canvas);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
            );

            Debug.Log("[Setup] Geocoding Controller setup complete!");
        }
    }
}
