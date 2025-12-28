using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Collections;
using System;
using CesiumForUnity;

namespace GeoGame3D.UI
{
    /// <summary>
    /// Handles geocoding location searches using OpenStreetMap Nominatim API
    /// Moves the aircraft to the geocoded location
    /// </summary>
    public class GeocodingController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TMP_InputField locationInputField;
        [SerializeField] private MainMenuController mainMenu;

        [Header("Game References")]
        [SerializeField] private CesiumForUnity.CesiumGeoreference georeference;
        [SerializeField] private Transform aircraftTransform;

        [Header("Settings")]
        [SerializeField] private float defaultAltitude = 500f; // Default altitude in meters
        [SerializeField] private string nominatimUrl = "https://nominatim.openstreetmap.org/search";

        private bool isSearching = false;

        private void Awake()
        {
            if (georeference == null)
            {
                georeference = FindObjectOfType<CesiumGeoreference>();
            }

            if (aircraftTransform == null)
            {
                var aircraft = FindObjectOfType<GeoGame3D.Aircraft.AircraftController>();
                if (aircraft != null)
                {
                    aircraftTransform = aircraft.transform;
                }
            }

            if (mainMenu == null)
            {
                mainMenu = FindObjectOfType<MainMenuController>();
            }
        }

        /// <summary>
        /// Search for a location and move the aircraft there
        /// Called by the "Go to Location" button
        /// </summary>
        public void SearchAndGoToLocation()
        {
            if (isSearching)
            {
                Debug.LogWarning("[Geocoding] Search already in progress");
                return;
            }

            if (locationInputField == null)
            {
                Debug.LogError("[Geocoding] Location input field not assigned");
                return;
            }

            string locationQuery = locationInputField.text?.Trim();

            if (string.IsNullOrEmpty(locationQuery))
            {
                Debug.LogWarning("[Geocoding] Please enter a location");
                return;
            }

            Debug.Log($"[Geocoding] Searching for location: {locationQuery}");
            StartCoroutine(GeocodeLocation(locationQuery));
        }

        /// <summary>
        /// Geocode a location using Nominatim API
        /// </summary>
        private IEnumerator GeocodeLocation(string query)
        {
            isSearching = true;

            // Build the API request URL
            // Using Nominatim search API with format=json
            string url = $"{nominatimUrl}?q={UnityWebRequest.EscapeURL(query)}&format=json&limit=1";

            Debug.Log($"[Geocoding] API Request: {url}");

            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                // Set User-Agent as required by Nominatim usage policy
                webRequest.SetRequestHeader("User-Agent", "GeoGame3D/1.0 (Unity Game)");

                // Send the request
                yield return webRequest.SendWebRequest();

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"[Geocoding] API Error: {webRequest.error}");
                    isSearching = false;
                    yield break;
                }

                // Parse the JSON response
                string jsonResponse = webRequest.downloadHandler.text;
                Debug.Log($"[Geocoding] API Response: {jsonResponse}");

                try
                {
                    // Parse the response
                    var results = JsonHelper.FromJson<NominatimResult>(jsonResponse);

                    if (results == null || results.Length == 0)
                    {
                        Debug.LogWarning($"[Geocoding] No results found for '{query}'");
                        isSearching = false;
                        yield break;
                    }

                    var firstResult = results[0];
                    double lat = double.Parse(firstResult.lat);
                    double lon = double.Parse(firstResult.lon);

                    Debug.Log($"[Geocoding] Found: {firstResult.display_name} at ({lat}, {lon})");

                    // Move aircraft to the location
                    MoveAircraftToLocation(lat, lon, defaultAltitude);

                    // Close the menu
                    if (mainMenu != null)
                    {
                        mainMenu.ResumeGame();
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[Geocoding] Failed to parse response: {ex.Message}");
                }
                finally
                {
                    isSearching = false;
                }
            }
        }

        /// <summary>
        /// Move the aircraft to the specified coordinates
        /// </summary>
        private void MoveAircraftToLocation(double latitude, double longitude, float altitude)
        {
            if (georeference == null)
            {
                Debug.LogError("[Geocoding] CesiumGeoreference not found");
                return;
            }

            if (aircraftTransform == null)
            {
                Debug.LogError("[Geocoding] Aircraft not found");
                return;
            }

            Debug.Log($"[Geocoding] Moving aircraft to ({latitude}, {longitude}, {altitude}m)");

            // Update CesiumGeoreference origin to center the world at this location
            georeference.latitude = latitude;
            georeference.longitude = longitude;
            georeference.height = altitude;

            // Use CesiumGlobeAnchor to position aircraft
            CesiumGlobeAnchor anchor = aircraftTransform.GetComponent<CesiumGlobeAnchor>();
            if (anchor == null)
            {
                anchor = aircraftTransform.gameObject.AddComponent<CesiumGlobeAnchor>();
            }

            // Set position (same as georeference origin for aircraft to start at world center)
            anchor.longitudeLatitudeHeight = new Unity.Mathematics.double3(
                longitude,
                latitude,
                altitude
            );

            // Reset aircraft rotation to level flight
            aircraftTransform.rotation = Quaternion.Euler(0, 0, 0);

            // Reset aircraft velocity if it has a Rigidbody
            Rigidbody rb = aircraftTransform.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            Debug.Log($"[Geocoding] Aircraft positioned at {latitude}, {longitude}, {altitude}m");
        }

        /// <summary>
        /// Nominatim API result structure
        /// </summary>
        [Serializable]
        private class NominatimResult
        {
            public string place_id;
            public string licence;
            public string osm_type;
            public string osm_id;
            public string lat;
            public string lon;
            public string display_name;
            public string @class;
            public string type;
            public float importance;
        }
    }

    /// <summary>
    /// Helper class for parsing JSON arrays
    /// Unity's JsonUtility doesn't support top-level arrays
    /// </summary>
    public static class JsonHelper
    {
        public static T[] FromJson<T>(string json)
        {
            string wrappedJson = "{\"items\":" + json + "}";
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(wrappedJson);
            return wrapper.items;
        }

        [Serializable]
        private class Wrapper<T>
        {
            public T[] items;
        }
    }
}
