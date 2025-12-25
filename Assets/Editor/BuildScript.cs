using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Build script for automated multi-platform builds.
/// Called by build.sh script or CI/CD pipeline.
/// </summary>
public static class BuildScript
{
    private static readonly string[] DefaultScenes = FindEnabledEditorScenes();

    /// <summary>
    /// Main build method called from command line.
    /// Reads build configuration from command line arguments.
    /// </summary>
    public static void PerformBuild()
    {
        try
        {
            // Parse command line arguments
            string buildPath = GetCommandLineArg("-buildPath");
            string buildVersion = GetCommandLineArg("-buildVersion", PlayerSettings.bundleVersion);
            string buildTarget = GetCommandLineArg("-buildTarget", EditorUserBuildSettings.activeBuildTarget.ToString());

            // Update version
            PlayerSettings.bundleVersion = buildVersion;
            Debug.Log($"Building version: {buildVersion}");

            // Determine build target
            BuildTarget target = ParseBuildTarget(buildTarget);
            Debug.Log($"Building for target: {target}");

            // Apply Cesium Linux patch if building for Linux
            if (target == BuildTarget.StandaloneLinux64)
            {
                ApplyCesiumLinuxPatch();
            }

            // Configure build options
            BuildPlayerOptions buildOptions = new BuildPlayerOptions
            {
                scenes = DefaultScenes,
                locationPathName = buildPath,
                target = target,
                options = BuildOptions.None // Release build (no Development Build)
            };

            // Set compression to LZ4 for faster load times
            ConfigureBuildSettings(target);

            // Perform the build
            Debug.Log($"Starting build to: {buildPath}");
            BuildReport report = BuildPipeline.BuildPlayer(buildOptions);

            // Check result
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"Build succeeded! Size: {FormatBytes(summary.totalSize)}, Time: {summary.totalTime}");

                // Copy README.txt to build directory
                CopyReadmeToBuilddirectory(buildPath, buildVersion);

                EditorApplication.Exit(0);
            }
            else
            {
                Debug.LogError($"Build failed! Result: {summary.result}");

                // Log build errors
                foreach (BuildStep step in report.steps)
                {
                    foreach (BuildStepMessage message in step.messages)
                    {
                        if (message.type == LogType.Error || message.type == LogType.Exception)
                        {
                            Debug.LogError($"{message.type}: {message.content}");
                        }
                    }
                }

                EditorApplication.Exit(1);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Build exception: {e}");
            EditorApplication.Exit(1);
        }
    }

    /// <summary>
    /// Apply Linux support patch to Cesium for Unity package.
    /// Adds LinuxStandalone64 to the assembly definition if not already present.
    /// </summary>
    private static void ApplyCesiumLinuxPatch()
    {
        try
        {
            // Find Cesium package in PackageCache
            string packageCachePath = Path.Combine(Application.dataPath, "..", "Library", "PackageCache");

            if (!Directory.Exists(packageCachePath))
            {
                Debug.LogWarning("PackageCache directory not found - Cesium may not be resolved yet");
                return;
            }

            // Find com.cesium.unity directory (it will have a hash suffix)
            string[] cesiumDirs = Directory.GetDirectories(packageCachePath, "com.cesium.unity@*");

            if (cesiumDirs.Length == 0)
            {
                Debug.LogWarning("Cesium package not found in PackageCache");
                return;
            }

            string cesiumPath = cesiumDirs[0];
            string asmdefPath = Path.Combine(cesiumPath, "Source", "CesiumForUnity.asmdef");

            if (!File.Exists(asmdefPath))
            {
                Debug.LogWarning($"Cesium assembly definition not found at: {asmdefPath}");
                return;
            }

            // Read and check if Linux support is already present
            string asmdefContent = File.ReadAllText(asmdefPath);

            if (asmdefContent.Contains("LinuxStandalone64"))
            {
                Debug.Log("Cesium Linux support already present");
                return;
            }

            // Add Linux support after iOS in the includePlatforms list
            string modifiedContent = asmdefContent.Replace(
                "\"iOS\",",
                "\"iOS\",\n        \"LinuxStandalone64\","
            );

            // Write back the modified content
            File.WriteAllText(asmdefPath, modifiedContent);

            Debug.Log($"Applied Cesium Linux patch to: {asmdefPath}");

            // Refresh AssetDatabase to pick up changes
            AssetDatabase.Refresh();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to apply Cesium Linux patch: {e.Message}");
        }
    }

    /// <summary>
    /// Configure platform-specific build settings.
    /// </summary>
    private static void ConfigureBuildSettings(BuildTarget target)
    {
        // Set compression to LZ4 for all platforms
        EditorUserBuildSettings.SetPlatformSettings(
            target.ToString(),
            "Compression",
            "Lz4"
        );

        // Platform-specific settings
        switch (target)
        {
            case BuildTarget.StandaloneWindows64:
                // Windows-specific settings
                PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.IL2CPP);
                PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
                break;

            case BuildTarget.StandaloneOSX:
                // macOS-specific settings
                PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.IL2CPP);
                PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
                // Create universal binary (Intel + Apple Silicon)
                PlayerSettings.SetArchitecture(BuildTargetGroup.Standalone, 2); // Universal
                break;

            case BuildTarget.StandaloneLinux64:
                // Linux-specific settings
                PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.IL2CPP);
                PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
                break;
        }

        Debug.Log($"Configured build settings for {target}");
    }

    /// <summary>
    /// Find all enabled scenes in the build settings.
    /// </summary>
    private static string[] FindEnabledEditorScenes()
    {
        return EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();
    }

    /// <summary>
    /// Parse build target from string.
    /// </summary>
    private static BuildTarget ParseBuildTarget(string targetString)
    {
        switch (targetString.ToLower())
        {
            case "win64":
            case "windows":
            case "standalonewindows64":
                return BuildTarget.StandaloneWindows64;

            case "osx":
            case "mac":
            case "macos":
            case "osxuniversal":
            case "standaloneosx":
                return BuildTarget.StandaloneOSX;

            case "linux":
            case "linux64":
            case "standalonelinux64":
                return BuildTarget.StandaloneLinux64;

            default:
                Debug.LogWarning($"Unknown build target: {targetString}, using default");
                return EditorUserBuildSettings.activeBuildTarget;
        }
    }

    /// <summary>
    /// Get command line argument value.
    /// </summary>
    private static string GetCommandLineArg(string name, string defaultValue = "")
    {
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == name && i + 1 < args.Length)
            {
                return args[i + 1];
            }
        }
        return defaultValue;
    }

    /// <summary>
    /// Format bytes to human-readable string.
    /// </summary>
    private static string FormatBytes(ulong bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }

    /// <summary>
    /// Copy README.txt to the build directory and replace version placeholder.
    /// </summary>
    private static void CopyReadmeToBuilddirectory(string buildPath, string version)
    {
        try
        {
            // Find the README.txt source
            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            string readmeSource = Path.Combine(projectRoot, "Distribution", "README.txt");

            if (!File.Exists(readmeSource))
            {
                Debug.LogWarning($"README.txt not found at: {readmeSource}");
                return;
            }

            // Determine destination directory (parent of executable)
            string buildDir = Path.GetDirectoryName(buildPath);

            // For macOS .app bundles, copy to Contents folder
            if (buildPath.EndsWith(".app"))
            {
                buildDir = Path.Combine(buildPath, "Contents");
            }

            string readmeDest = Path.Combine(buildDir, "README.txt");

            // Read, replace version placeholder, and write
            string readmeContent = File.ReadAllText(readmeSource);
            readmeContent = readmeContent.Replace("{{VERSION}}", version);
            File.WriteAllText(readmeDest, readmeContent);

            Debug.Log($"Copied README.txt to: {readmeDest}");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to copy README.txt: {e.Message}");
        }
    }
}
