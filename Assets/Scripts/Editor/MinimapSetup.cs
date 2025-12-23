using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using GeoGame3D.UI;

namespace GeoGame3D.Editor
{
    /// <summary>
    /// Editor utility to set up the minimap hierarchy correctly
    /// </summary>
    public class MinimapSetup : EditorWindow
    {
        [MenuItem("GeoGame3D/Setup Minimap")]
        public static void SetupMinimapHierarchy()
        {
            // Find the Minimap GameObject
            GameObject minimapRoot = GameObject.Find("Minimap");
            if (minimapRoot == null)
            {
                Debug.LogError("MinimapSetup: Could not find 'Minimap' GameObject in scene");
                return;
            }

            MinimapController controller = minimapRoot.GetComponent<MinimapController>();
            if (controller == null)
            {
                Debug.LogError("MinimapSetup: Minimap GameObject does not have MinimapController component");
                return;
            }

            Debug.Log("MinimapSetup: Configuring minimap hierarchy...");

            // Create or find MapDisplay child
            Transform mapDisplayTransform = minimapRoot.transform.Find("MapDisplay");
            GameObject mapDisplayObj;

            if (mapDisplayTransform == null)
            {
                mapDisplayObj = new GameObject("MapDisplay");
                mapDisplayObj.transform.SetParent(minimapRoot.transform, false);
            }
            else
            {
                mapDisplayObj = mapDisplayTransform.gameObject;
            }

            // Add RectTransform if not present
            RectTransform mapDisplayRect = mapDisplayObj.GetComponent<RectTransform>();
            if (mapDisplayRect == null)
            {
                mapDisplayRect = mapDisplayObj.AddComponent<RectTransform>();
            }

            // Configure MapDisplay RectTransform
            mapDisplayRect.anchorMin = new Vector2(0, 0);
            mapDisplayRect.anchorMax = new Vector2(0, 0);
            mapDisplayRect.pivot = new Vector2(0.5f, 0.5f);
            mapDisplayRect.anchoredPosition = new Vector2(100, 100); // Bottom-left corner
            mapDisplayRect.sizeDelta = new Vector2(200, 200); // Size of minimap

            // Add RawImage for map rendering
            RawImage mapRawImage = mapDisplayObj.GetComponent<RawImage>();
            if (mapRawImage == null)
            {
                mapRawImage = mapDisplayObj.AddComponent<RawImage>();
            }

            // Add OSMMapDisplay component
            OSMMapDisplay osmDisplay = mapDisplayObj.GetComponent<OSMMapDisplay>();
            if (osmDisplay == null)
            {
                osmDisplay = mapDisplayObj.AddComponent<OSMMapDisplay>();
            }

            // Use reflection to set the mapImage field (it's private SerializeField)
            SerializedObject so = new SerializedObject(osmDisplay);
            so.FindProperty("mapImage").objectReferenceValue = mapRawImage;
            so.ApplyModifiedProperties();

            // Create or find MinimapContainer child of MapDisplay
            Transform containerTransform = mapDisplayObj.transform.Find("MinimapContainer");
            GameObject containerObj;

            if (containerTransform == null)
            {
                containerObj = new GameObject("MinimapContainer");
                containerObj.transform.SetParent(mapDisplayObj.transform, false);
            }
            else
            {
                containerObj = containerTransform.gameObject;
            }

            // Add RectTransform to container
            RectTransform containerRect = containerObj.GetComponent<RectTransform>();
            if (containerRect == null)
            {
                containerRect = containerObj.AddComponent<RectTransform>();
            }

            // Configure container - fills parent and applies rotation
            containerRect.anchorMin = new Vector2(0.5f, 0.5f);
            containerRect.anchorMax = new Vector2(0.5f, 0.5f);
            containerRect.pivot = new Vector2(0.5f, 0.5f);
            containerRect.anchoredPosition = Vector2.zero;
            containerRect.sizeDelta = new Vector2(200, 200);

            // Add RectMask2D for clipping
            RectMask2D mask = containerObj.GetComponent<RectMask2D>();
            if (mask == null)
            {
                mask = containerObj.AddComponent<RectMask2D>();
            }

            // Create or find Aircraft Icon
            Transform iconTransform = containerObj.transform.Find("AircraftIcon");
            GameObject iconObj;

            if (iconTransform == null)
            {
                iconObj = new GameObject("AircraftIcon");
                iconObj.transform.SetParent(containerObj.transform, false);
            }
            else
            {
                iconObj = iconTransform.gameObject;
            }

            // Add RectTransform to icon
            RectTransform iconRect = iconObj.GetComponent<RectTransform>();
            if (iconRect == null)
            {
                iconRect = iconObj.AddComponent<RectTransform>();
            }

            // Configure icon - centered, small size
            iconRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.anchoredPosition = Vector2.zero;
            iconRect.sizeDelta = new Vector2(20, 20);

            // Add Image component for aircraft icon
            Image iconImage = iconObj.GetComponent<Image>();
            if (iconImage == null)
            {
                iconImage = iconObj.AddComponent<Image>();
            }
            iconImage.color = Color.red;

            // Try to load a default sprite (triangle for aircraft)
            // You may want to create/assign a custom sprite
            iconImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

            // Wire up references in MinimapController
            SerializedObject controllerSO = new SerializedObject(controller);
            controllerSO.FindProperty("mapDisplay").objectReferenceValue = osmDisplay;
            controllerSO.FindProperty("minimapContainer").objectReferenceValue = containerRect;
            controllerSO.FindProperty("aircraftIcon").objectReferenceValue = iconRect;
            controllerSO.ApplyModifiedProperties();

            Debug.Log("MinimapSetup: Minimap hierarchy configured successfully!");
            Debug.Log("Hierarchy: Minimap -> MapDisplay (OSMMapDisplay) -> MinimapContainer (RectMask2D) -> AircraftIcon");

            EditorUtility.SetDirty(minimapRoot);
            EditorUtility.SetDirty(mapDisplayObj);
            EditorUtility.SetDirty(containerObj);
            EditorUtility.SetDirty(iconObj);
        }
    }
}
