using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using GeoGame3D.UI;

namespace GeoGame3D.Editor
{
    /// <summary>
    /// Automatically sets up minimap on editor load
    /// </summary>
    [InitializeOnLoad]
    public static class MinimapAutoSetup
    {
        static MinimapAutoSetup()
        {
            // Run setup after editor finishes loading
            EditorApplication.delayCall += () =>
            {
                if (!EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    CheckAndSetupMinimap();
                }
            };
        }

        private static void CheckAndSetupMinimap()
        {
            GameObject minimapRoot = GameObject.Find("Minimap");
            if (minimapRoot == null) return;

            MinimapController controller = minimapRoot.GetComponent<MinimapController>();
            if (controller == null) return;

            // Check if already set up
            SerializedObject controllerSO = new SerializedObject(controller);
            if (controllerSO.FindProperty("mapDisplay").objectReferenceValue != null)
            {
                // Already configured
                return;
            }

            Debug.Log("[MinimapAutoSetup] Minimap not configured. Run 'GeoGame3D > Setup Minimap' from menu.");
        }
    }
}
