using UnityEngine;
using System.Collections.Generic;

namespace GeoGame3D.Utils
{
    /// <summary>
    /// MonoBehaviour component to configure SimpleLogger at runtime.
    /// Attach to a GameObject in the scene to control logging levels.
    /// </summary>
    public class LoggerConfig : MonoBehaviour
    {
        [System.Serializable]
        public class DomainLogLevel
        {
            public string domain;
            public SimpleLogger.LogLevel logLevel;
        }

        [Header("Global Settings")]
        [Tooltip("Global minimum log level (applies to all domains unless overridden)")]
        public SimpleLogger.LogLevel globalMinimumLogLevel = SimpleLogger.LogLevel.Info;

        [Header("Domain-Specific Settings")]
        [Tooltip("Override log levels for specific domains")]
        public List<DomainLogLevel> domainLogLevels = new List<DomainLogLevel>();

        private void Awake()
        {
            ApplyConfiguration();
        }

        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                ApplyConfiguration();
            }
        }

        private void ApplyConfiguration()
        {
            // Set global minimum
            SimpleLogger.MinimumLogLevel = globalMinimumLogLevel;

            // Set domain-specific levels
            foreach (var domainLevel in domainLogLevels)
            {
                if (!string.IsNullOrEmpty(domainLevel.domain))
                {
                    SimpleLogger.SetDomainLogLevel(domainLevel.domain, domainLevel.logLevel);
                }
            }
        }

        [ContextMenu("Reset to Defaults")]
        private void ResetToDefaults()
        {
            globalMinimumLogLevel = SimpleLogger.LogLevel.Info;
            domainLogLevels.Clear();
            ApplyConfiguration();
        }

        [ContextMenu("Enable Minimap Debug")]
        private void EnableMinimapDebug()
        {
            if (!domainLogLevels.Exists(d => d.domain == "Minimap"))
            {
                domainLogLevels.Add(new DomainLogLevel
                {
                    domain = "Minimap",
                    logLevel = SimpleLogger.LogLevel.Debug
                });
            }
            ApplyConfiguration();
        }
    }
}
