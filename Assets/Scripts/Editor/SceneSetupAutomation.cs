using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace GeoGame3D.Editor
{
    /// <summary>
    /// Automated scene setup for GeoGame3D
    /// Creates the main flight scene with all required components
    /// </summary>
    public class SceneSetupAutomation : EditorWindow
    {
        private float startLatitude = 50.85f;
        private float startLongitude = 4.35f;
        private float startHeight = 1000f;

        [MenuItem("GeoGame3D/Setup Main Scene")]
        public static void ShowWindow()
        {
            GetWindow<SceneSetupAutomation>("Scene Setup");
        }

        private void OnGUI()
        {
            GUILayout.Label("GeoGame3D Scene Setup", EditorStyles.boldLabel);
            GUILayout.Space(10);

            GUILayout.Label("Starting Location:", EditorStyles.label);
            startLatitude = EditorGUILayout.FloatField("Latitude", startLatitude);
            startLongitude = EditorGUILayout.FloatField("Longitude", startLongitude);
            startHeight = EditorGUILayout.FloatField("Height (m)", startHeight);

            GUILayout.Space(20);

            if (GUILayout.Button("Create Complete Scene", GUILayout.Height(40)))
            {
                CreateCompleteScene();
            }

            GUILayout.Space(10);
            GUILayout.Label("This will create:", EditorStyles.helpBox);
            GUILayout.Label("• Cesium Georeference & World Terrain");
            GUILayout.Label("• Aircraft with controller");
            GUILayout.Label("• Camera with follow rig");
            GUILayout.Label("• Flight HUD");
            GUILayout.Label("• Input System configuration");
        }

        private void CreateCompleteScene()
        {
            if (!EditorUtility.DisplayDialog("Create Scene",
                "This will modify the current scene. Continue?",
                "Yes", "Cancel"))
            {
                return;
            }

            // Create scene elements
            CreateCesiumComponents();
            GameObject aircraft = CreateAircraft();
            CreateCamera(aircraft);
            CreateHUD(aircraft);

            // Mark scene as dirty
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            EditorUtility.DisplayDialog("Scene Setup Complete!",
                "Main scene has been created successfully!\n\n" +
                "Next steps:\n" +
                "1. Save the scene (Ctrl+S)\n" +
                "2. Configure Input System (see console)\n" +
                "3. Press Play to test!",
                "OK");

            Debug.Log("<color=green><b>Scene setup complete!</b></color>");
            Debug.Log("Don't forget to configure Input System - see SCENE_SETUP_GUIDE.md Step 6");
        }

        private void CreateCesiumComponents()
        {
            Debug.Log("Creating Cesium components...");

            // Find or create Georeference
            GameObject georeference = GameObject.Find("Georeference");
            if (georeference == null)
            {
                georeference = new GameObject("CesiumGeoreference");

                // Try to add Cesium Georeference component
                var georefType = System.Type.GetType("CesiumForUnity.CesiumGeoreference, CesiumForUnity");
                if (georefType != null)
                {
                    var georefComponent = georeference.AddComponent(georefType);

                    // Set origin using reflection (since we don't have compile-time reference)
                    var originLatProperty = georefType.GetProperty("latitude");
                    var originLonProperty = georefType.GetProperty("longitude");
                    var originHeightProperty = georefType.GetProperty("height");

                    if (originLatProperty != null) originLatProperty.SetValue(georefComponent, startLatitude);
                    if (originLonProperty != null) originLonProperty.SetValue(georefComponent, startLongitude);
                    if (originHeightProperty != null) originHeightProperty.SetValue(georefComponent, startHeight);

                    Debug.Log($"Cesium Georeference created at ({startLatitude}, {startLongitude}, {startHeight}m)");
                }
                else
                {
                    Debug.LogWarning("Cesium Georeference component not found. Make sure Cesium for Unity is installed.");
                    Debug.LogWarning("You'll need to add Cesium World Terrain manually via the Cesium tab.");
                }
            }
            else
            {
                Debug.Log("Georeference already exists, using existing one");
            }

            // Note: Terrain creation is handled by Cesium tab - user should add it there
            Debug.Log("If you haven't already, add Cesium World Terrain via the Cesium tab");
        }

        private GameObject CreateAircraft()
        {
            Debug.Log("Creating Aircraft...");

            // Create aircraft GameObject
            GameObject aircraft = new GameObject("Aircraft");
            aircraft.transform.position = Vector3.zero;

            // Add Rigidbody
            Rigidbody rb = aircraft.AddComponent<Rigidbody>();
            rb.mass = 1000f;
            rb.linearDamping = 0.3f;
            rb.angularDamping = 0.5f;
            rb.useGravity = false;

            // Add Aircraft Controller
            var controller = aircraft.AddComponent<GeoGame3D.Aircraft.AircraftController>();

            // Create visual model (cube)
            GameObject model = GameObject.CreatePrimitive(PrimitiveType.Cube);
            model.name = "Model";
            model.transform.SetParent(aircraft.transform);
            model.transform.localPosition = Vector3.zero;
            model.transform.localScale = new Vector3(2f, 0.5f, 4f);

            // Create and apply material
            Material aircraftMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            aircraftMaterial.color = new Color(0.2f, 0.5f, 1f); // Blue
            model.GetComponent<Renderer>().material = aircraftMaterial;

            Debug.Log("Aircraft created successfully");
            return aircraft;
        }

        private void CreateCamera(GameObject target)
        {
            Debug.Log("Setting up Camera...");

            // Find main camera
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                // Create camera if it doesn't exist
                GameObject cameraObj = new GameObject("Main Camera");
                mainCamera = cameraObj.AddComponent<Camera>();
                cameraObj.tag = "MainCamera";
                cameraObj.AddComponent<AudioListener>();
            }

            // Add Camera Rig component
            var cameraRig = mainCamera.gameObject.AddComponent<GeoGame3D.Camera.CameraRig>();

            // Use reflection to set the target (it's private)
            var targetField = typeof(GeoGame3D.Camera.CameraRig).GetField("target",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (targetField != null)
            {
                targetField.SetValue(cameraRig, target.transform);
                Debug.Log("Camera rig configured to follow Aircraft");
            }
            else
            {
                Debug.LogWarning("Could not auto-set camera target. Please set it manually in Inspector.");
            }
        }

        private void CreateHUD(GameObject aircraft)
        {
            Debug.Log("Creating HUD...");

            // Create Canvas
            GameObject canvasObj = new GameObject("FlightHUD");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var canvasScaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasScaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);

            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // Add FlightHUD component
            var hudScript = canvasObj.AddComponent<GeoGame3D.UI.FlightHUD>();

            // Create text elements
            CreateHUDText(canvasObj.transform, "SpeedText", new Vector2(20, -20), TextAnchor.UpperLeft);
            CreateHUDText(canvasObj.transform, "AltitudeText", new Vector2(20, -60), TextAnchor.UpperLeft);
            CreateHUDText(canvasObj.transform, "HeadingText", new Vector2(-20, -20), TextAnchor.UpperRight);
            CreateHUDText(canvasObj.transform, "ThrottleText", new Vector2(-20, -60), TextAnchor.UpperRight);

            // Set aircraft reference using reflection
            var aircraftField = typeof(GeoGame3D.UI.FlightHUD).GetField("aircraft",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (aircraftField != null)
            {
                aircraftField.SetValue(hudScript, aircraft.GetComponent<GeoGame3D.Aircraft.AircraftController>());
            }

            Debug.Log("HUD created successfully");
            Debug.LogWarning("HUD text elements created but need TextMeshPro assignment - see SCENE_SETUP_GUIDE.md Step 5");
        }

        private void CreateHUDText(Transform parent, string name, Vector2 position, TextAnchor anchor)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent);

            RectTransform rectTransform = textObj.AddComponent<RectTransform>();
            rectTransform.anchorMin = anchor == TextAnchor.UpperLeft ? new Vector2(0, 1) : new Vector2(1, 1);
            rectTransform.anchorMax = rectTransform.anchorMin;
            rectTransform.pivot = rectTransform.anchorMin;
            rectTransform.anchoredPosition = position;
            rectTransform.sizeDelta = new Vector2(200, 30);

            // Try to add TextMeshPro component
            var tmpType = System.Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro");
            if (tmpType != null)
            {
                var textComponent = textObj.AddComponent(tmpType);

                // Set text properties using reflection
                var textProperty = tmpType.GetProperty("text");
                var fontSizeProperty = tmpType.GetProperty("fontSize");
                var colorProperty = tmpType.GetProperty("color");
                var alignmentProperty = tmpType.GetProperty("alignment");

                if (textProperty != null) textProperty.SetValue(textComponent, name.Replace("Text", ": 000"));
                if (fontSizeProperty != null) fontSizeProperty.SetValue(textComponent, 24f);
                if (colorProperty != null) colorProperty.SetValue(textComponent, Color.white);
            }
            else
            {
                Debug.LogWarning($"TextMeshPro not found for {name}. You'll need to add text components manually.");
            }
        }
    }
}
