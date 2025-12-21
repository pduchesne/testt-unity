using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace GeoGame3D.Editor
{
    /// <summary>
    /// Editor utility to automatically create visual gauge UI elements for the FlightHUD
    /// Creates modern/arcade style gauges with circular fills and indicators
    /// </summary>
    public class HUDGaugeSetup : EditorWindow
    {
        [MenuItem("GeoGame3D/Setup HUD Visual Gauges")]
        public static void ShowWindow()
        {
            GetWindow<HUDGaugeSetup>("HUD Gauge Setup");
        }

        private void OnGUI()
        {
            GUILayout.Label("HUD Visual Gauge Setup", EditorStyles.boldLabel);
            GUILayout.Space(10);

            GUILayout.Label("This will create visual gauge UI elements for the FlightHUD.");
            GUILayout.Label("Make sure the FlightHUD Canvas is selected in the hierarchy.");
            GUILayout.Space(10);

            if (GUILayout.Button("Create Artificial Horizon", GUILayout.Height(30)))
            {
                CreateArtificialHorizon();
            }

            if (GUILayout.Button("Create Speed Gauge", GUILayout.Height(30)))
            {
                CreateCircularGauge("SpeedGauge", "SPD", 0, 400, new Vector2(-350, -200));
            }

            if (GUILayout.Button("Create Altitude Gauge", GUILayout.Height(30)))
            {
                CreateCircularGauge("AltitudeGauge", "ALT", 0, 5000, new Vector2(350, -200));
            }

            if (GUILayout.Button("Create Heading Compass", GUILayout.Height(30)))
            {
                CreateHeadingCompass();
            }

            if (GUILayout.Button("Create Throttle Gauge", GUILayout.Height(30)))
            {
                CreateCircularGauge("ThrottleGauge", "THR", 0, 100, new Vector2(-350, 200));
            }

            GUILayout.Space(20);

            if (GUILayout.Button("Create All Gauges", GUILayout.Height(50)))
            {
                CreateAllGauges();
            }
        }

        private static void CreateArtificialHorizon()
        {
            Canvas canvas = Selection.activeGameObject?.GetComponent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("Please select the FlightHUD Canvas first!");
                return;
            }

            // Create container
            GameObject horizonContainer = new GameObject("ArtificialHorizon");
            horizonContainer.transform.SetParent(canvas.transform, false);

            RectTransform containerRT = horizonContainer.AddComponent<RectTransform>();
            containerRT.anchoredPosition = Vector2.zero;
            containerRT.sizeDelta = new Vector2(200, 200);

            // Add background
            GameObject background = new GameObject("Background");
            background.transform.SetParent(horizonContainer.transform, false);
            Image bgImage = background.AddComponent<Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.15f, 0.8f);
            RectTransform bgRT = background.GetComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;

            // Create horizon line
            GameObject horizonLine = new GameObject("HorizonLine");
            horizonLine.transform.SetParent(horizonContainer.transform, false);
            Image horizonImage = horizonLine.AddComponent<Image>();
            horizonImage.color = Color.cyan;
            RectTransform horizonRT = horizonLine.GetComponent<RectTransform>();
            horizonRT.sizeDelta = new Vector2(300, 4);

            // Create center reticle
            GameObject reticle = new GameObject("CenterReticle");
            reticle.transform.SetParent(horizonContainer.transform, false);
            Image reticleImage = reticle.AddComponent<Image>();
            reticleImage.color = Color.yellow;
            RectTransform reticleRT = reticle.GetComponent<RectTransform>();
            reticleRT.sizeDelta = new Vector2(40, 4);

            // Add ArtificialHorizon component
            GeoGame3D.UI.ArtificialHorizon horizonComponent = horizonContainer.AddComponent<GeoGame3D.UI.ArtificialHorizon>();

            // Use reflection to set private fields
            var type = typeof(GeoGame3D.UI.ArtificialHorizon);
            var horizonLineField = type.GetField("horizonLine", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var centerReticleField = type.GetField("centerReticle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (horizonLineField != null) horizonLineField.SetValue(horizonComponent, horizonRT);
            if (centerReticleField != null) centerReticleField.SetValue(horizonComponent, reticleRT);

            EditorUtility.SetDirty(horizonContainer);
            Debug.Log("Artificial Horizon created successfully!");
        }

        private static void CreateCircularGauge(string name, string label, float minValue, float maxValue, Vector2 position)
        {
            Canvas canvas = Selection.activeGameObject?.GetComponent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("Please select the FlightHUD Canvas first!");
                return;
            }

            // Create container
            GameObject gaugeContainer = new GameObject(name);
            gaugeContainer.transform.SetParent(canvas.transform, false);
            RectTransform containerRT = gaugeContainer.AddComponent<RectTransform>();
            containerRT.anchoredPosition = position;
            containerRT.sizeDelta = new Vector2(120, 120);

            // Add background circle
            GameObject background = new GameObject("Background");
            background.transform.SetParent(gaugeContainer.transform, false);
            Image bgImage = background.AddComponent<Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.15f, 0.6f);
            bgImage.type = Image.Type.Filled;
            bgImage.fillMethod = Image.FillMethod.Radial360;
            bgImage.fillAmount = 1f;
            RectTransform bgRT = background.GetComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;

            // Add fill image
            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(gaugeContainer.transform, false);
            Image fillImage = fill.AddComponent<Image>();
            fillImage.color = new Color(0, 1, 1, 0.8f); // Cyan
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Radial360;
            fillImage.fillOrigin = (int)Image.Origin360.Bottom;
            fillImage.fillAmount = 0f;
            RectTransform fillRT = fill.GetComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = Vector2.one;
            fillRT.offsetMin = new Vector2(5, 5);
            fillRT.offsetMax = new Vector2(-5, -5);

            // Add value text
            GameObject valueTextObj = new GameObject("ValueText");
            valueTextObj.transform.SetParent(gaugeContainer.transform, false);
            TextMeshProUGUI valueText = valueTextObj.AddComponent<TextMeshProUGUI>();
            valueText.text = "0";
            valueText.fontSize = 24;
            valueText.alignment = TextAlignmentOptions.Center;
            valueText.color = Color.white;
            RectTransform valueRT = valueTextObj.GetComponent<RectTransform>();
            valueRT.anchorMin = new Vector2(0.5f, 0.5f);
            valueRT.anchorMax = new Vector2(0.5f, 0.5f);
            valueRT.sizeDelta = new Vector2(100, 40);
            valueRT.anchoredPosition = Vector2.zero;

            // Add label text
            GameObject labelTextObj = new GameObject("LabelText");
            labelTextObj.transform.SetParent(gaugeContainer.transform, false);
            TextMeshProUGUI labelText = labelTextObj.AddComponent<TextMeshProUGUI>();
            labelText.text = label;
            labelText.fontSize = 16;
            labelText.alignment = TextAlignmentOptions.Center;
            labelText.color = new Color(0.8f, 0.8f, 0.8f, 1f);
            RectTransform labelRT = labelTextObj.GetComponent<RectTransform>();
            labelRT.anchorMin = new Vector2(0.5f, 0f);
            labelRT.anchorMax = new Vector2(0.5f, 0f);
            labelRT.sizeDelta = new Vector2(100, 30);
            labelRT.anchoredPosition = new Vector2(0, -70);

            // Add CircularGauge component
            GeoGame3D.UI.CircularGauge gaugeComponent = gaugeContainer.AddComponent<GeoGame3D.UI.CircularGauge>();

            // Use reflection to set private fields
            var type = typeof(GeoGame3D.UI.CircularGauge);
            var fillField = type.GetField("fillImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var valueTextField = type.GetField("valueText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var labelTextField = type.GetField("labelText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var minField = type.GetField("minValue", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var maxField = type.GetField("maxValue", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (fillField != null) fillField.SetValue(gaugeComponent, fillImage);
            if (valueTextField != null) valueTextField.SetValue(gaugeComponent, valueText);
            if (labelTextField != null) labelTextField.SetValue(gaugeComponent, labelText);
            if (minField != null) minField.SetValue(gaugeComponent, minValue);
            if (maxField != null) maxField.SetValue(gaugeComponent, maxValue);

            EditorUtility.SetDirty(gaugeContainer);
            Debug.Log($"{name} created successfully!");
        }

        private static void CreateHeadingCompass()
        {
            Canvas canvas = Selection.activeGameObject?.GetComponent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("Please select the FlightHUD Canvas first!");
                return;
            }

            // Create container
            GameObject compassContainer = new GameObject("HeadingCompass");
            compassContainer.transform.SetParent(canvas.transform, false);
            RectTransform containerRT = compassContainer.AddComponent<RectTransform>();
            containerRT.anchoredPosition = new Vector2(0, 300);
            containerRT.sizeDelta = new Vector2(200, 50);

            // Add background
            GameObject background = new GameObject("Background");
            background.transform.SetParent(compassContainer.transform, false);
            Image bgImage = background.AddComponent<Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.15f, 0.8f);
            RectTransform bgRT = background.GetComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;

            // Add heading text
            GameObject headingTextObj = new GameObject("HeadingText");
            headingTextObj.transform.SetParent(compassContainer.transform, false);
            TextMeshProUGUI headingText = headingTextObj.AddComponent<TextMeshProUGUI>();
            headingText.text = "000°";
            headingText.fontSize = 32;
            headingText.alignment = TextAlignmentOptions.Center;
            headingText.color = Color.white;
            RectTransform headingRT = headingTextObj.GetComponent<RectTransform>();
            headingRT.anchorMin = Vector2.zero;
            headingRT.anchorMax = Vector2.one;
            headingRT.offsetMin = Vector2.zero;
            headingRT.offsetMax = Vector2.zero;

            // Add HeadingCompass component
            GeoGame3D.UI.HeadingCompass compassComponent = compassContainer.AddComponent<GeoGame3D.UI.HeadingCompass>();

            // Use reflection to set private field
            var type = typeof(GeoGame3D.UI.HeadingCompass);
            var headingTextField = type.GetField("headingText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (headingTextField != null) headingTextField.SetValue(compassComponent, headingText);

            EditorUtility.SetDirty(compassContainer);
            Debug.Log("Heading Compass created successfully!");
        }

        private static void CreateAllGauges()
        {
            CreateArtificialHorizon();
            CreateCircularGauge("SpeedGauge", "SPD", 0, 400, new Vector2(-350, -200));
            CreateCircularGauge("AltitudeGauge", "ALT", 0, 5000, new Vector2(350, -200));
            CreateHeadingCompass();
            CreateCircularGauge("ThrottleGauge", "THR", 0, 100, new Vector2(-350, 200));

            Debug.Log("All primary gauges created! Check the FlightHUD Canvas.");
        }
    }
}
