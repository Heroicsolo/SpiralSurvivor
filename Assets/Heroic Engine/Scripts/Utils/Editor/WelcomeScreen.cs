using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace HeroicEngine.Utils.Editor
{
    public sealed class WelcomeScreen : EditorWindow
    {
        private const string FIRST_LAUNCH_PREFS_KEY = "FirstLaunch";
        private const string WEATHER_INSTALLED_KEY = "WeatherInstalled";
        private const string IGNORE_WEATHER_KEY = "IgnoreWeather";
        private const string RAIN_PACKAGE_URL = "https://assetstore.unity.com/packages/vfx/particles/environment/rain-maker-2d-and-3d-rain-particle-system-for-unity-34938";
        private const string RAIN_MAKER_PACKAGE_NAME = "DigitalRuby.RainMaker";
        private const string RAIN_MAKER_INSTALL_HEADER = "If you want to use weather system, you need to install Rain Maker package";

        // List of scene paths to add to Build Settings
        private readonly List<string> _scenePaths = new()
        {
            "Assets/Heroic Engine/Example/Scenes/InitialScene.unity", "Assets/Heroic Engine/Example/Scenes/MainMenuScene.unity",
            "Assets/Heroic Engine/Example/Scenes/SampleSceneDuel.unity", "Assets/Heroic Engine/Example/Scenes/SampleSceneTopDown.unity",
            "Assets/Heroic Engine/Example/Scenes/TicTacToeSample.unity"
        };

        private GUIStyle _headerStyle;
        private GUIStyle _headerStyleSmall;

        private Texture _texture;
        private bool _weatherEnabled;

        [MenuItem("Tools/HeroicEngine/Information and links", false, 101)]
        public static void ShowWindow()
        {
            GetWindow<WelcomeScreen>("Information");
            PlayerPrefs.SetInt(FIRST_LAUNCH_PREFS_KEY, 0);
        }

        public static void TryShowWindow()
        {
            if (PlayerPrefs.GetInt(FIRST_LAUNCH_PREFS_KEY, 1) == 1 || PlayerPrefs.GetInt(WEATHER_INSTALLED_KEY, 0) == 0
                && PlayerPrefs.GetInt(IGNORE_WEATHER_KEY, 0) == 0)
            {
                GetWindow<WelcomeScreen>("Welcome");
                PlayerPrefs.SetInt(FIRST_LAUNCH_PREFS_KEY, 0);
            }
        }

        private static bool DoesNamespaceExist(string targetNamespace)
        {
            // Get all loaded assemblies in the AppDomain
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                try
                {
                    // Get all types defined in the assembly
                    var types = assembly.GetTypes();

                    // Check if any type belongs to the specified namespace
                    if (types.Any(t => t.Namespace == targetNamespace))
                    {
                        return true;
                    }
                }
                catch (ReflectionTypeLoadException)
                {
                    // Handle cases where types in the assembly can't be loaded (e.g., due to missing references)
                    Debug.LogWarning($"Unable to load types from assembly: {assembly.FullName}");
                }
            }

            return false;
        }

        private static void EnableWeather()
        {
            // Get the current build target group (e.g., Standalone, iOS, Android, etc.)
            var buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;

            // Get the existing scripting define symbols
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);

            // Check if the symbol is already added
            const string defineSymbol = "WEATHER_PACKAGE";
            if (!defines.Contains(defineSymbol))
            {
                // Add the symbol if not present
                defines = string.IsNullOrEmpty(defines) ? defineSymbol : $"{defines};{defineSymbol}";
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, defines);

                Debug.Log($"Scripting define symbol '{defineSymbol}' has been added for {buildTargetGroup}.");
            }
        }

        private bool AreDemoScenesSetUp()
        {
            if (EditorBuildSettings.scenes.Length == 0)
            {
                return false;
            }

            List<EditorBuildSettingsScene> buildScenes = new(EditorBuildSettings.scenes);

            foreach (var scenePath in _scenePaths)
            {
                if (buildScenes.FindIndex(bs => bs.path == scenePath) < 0)
                {
                    return false;
                }
            }

            return true;
        }

        private void SetUpDemoScenes()
        {
            // Create a list of EditorBuildSettingsScene
            var editorBuildSettingsScenes = new EditorBuildSettingsScene[_scenePaths.Count];

            for (var i = 0; i < _scenePaths.Count; i++)
            {
                // Ensure the scene exists at the specified path
                if (File.Exists(_scenePaths[i]))
                {
                    editorBuildSettingsScenes[i] = new EditorBuildSettingsScene(_scenePaths[i], true);
                }
                else
                {
                    Debug.LogWarning($"Scene not found at path: {_scenePaths[i]}");
                }
            }

            // Assign the scenes to the Build Settings
            EditorBuildSettings.scenes = editorBuildSettingsScenes;
            EditorSceneManager.OpenScene(_scenePaths[0]);
        }

        private static void InstallPackage(string url)
        {
            // Parse the package name from the URL (adjust based on URL structure)
            var packageName = ParsePackageNameFromUrl(url);
            if (string.IsNullOrEmpty(packageName))
            {
                Debug.LogError("Invalid URL or unable to parse package name.");
                return;
            }

            var manifestPath = Path.Combine(Application.dataPath, "../Packages/manifest.json");
            if (!File.Exists(manifestPath))
            {
                Debug.LogError("manifest.json not found.");
                return;
            }

            var manifestJson = File.ReadAllText(manifestPath);
            var manifest = JObject.Parse(manifestJson);

            var dependencies = (JObject)manifest["dependencies"];
            if (dependencies != null && dependencies.ContainsKey(packageName))
            {
                Debug.Log($"Package '{packageName}' is already installed.");
                return;
            }

            // Add package to dependencies
            if (dependencies != null)
            {
                dependencies[packageName] = "latest"; // Replace "latest" with a specific version if needed.
            }

            // Write back to manifest.json
            File.WriteAllText(manifestPath, manifest.ToString());
            Debug.Log($"Package '{packageName}' has been added. Unity will now refresh.");

            // Refresh assets
            AssetDatabase.Refresh();
        }

        private static string ParsePackageNameFromUrl(string url)
        {
            // Extract package name or ID from the URL
            // Example URL: https://assetstore.unity.com/packages/tools/ai/some-package-id
            // Return "com.unity.some-package-id" or similar if applicable
            // Adjust based on how your packages are named or provided.

            // Placeholder logic: parse based on your URL pattern
            var parts = url.Split('/');
            if (parts.Length > 0)
            {
                return parts[^1]; // Extract last part of the URL as package name
            }

            return null;
        }

        private void OnGUI()
        {
            _headerStyle ??=  new GUIStyle
            {
                fontStyle = FontStyle.Bold, fontSize = 20,
                wordWrap = false, richText = true
            };
            
            _headerStyleSmall ??= new GUIStyle
            {
                fontStyle = FontStyle.Bold, fontSize = 14,
                wordWrap = true, richText = true
            };
            
            EditorGUILayout.LabelField("Welcome to Heroic Engine!".ToColorizedString(Color.white), _headerStyle);

            _texture ??= AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Heroic Engine/Sprites/HeroicEngineBanner.png");
            GUILayout.Label(_texture);

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            if (PlayerPrefs.GetInt(IGNORE_WEATHER_KEY, 0) == 0)
            {
                if (!DoesNamespaceExist(RAIN_MAKER_PACKAGE_NAME))
                {
                    EditorGUILayout.LabelField(RAIN_MAKER_INSTALL_HEADER.ToColorizedString(Color.yellow), _headerStyleSmall);
                    if (GUILayout.Button("Install Rain Maker"))
                    {
                        Application.OpenURL(RAIN_PACKAGE_URL);
                    }
                    if (GUILayout.Button("Skip"))
                    {
                        PlayerPrefs.SetInt(IGNORE_WEATHER_KEY, 1);
                    }
                    return;
                }
                else if (!_weatherEnabled)
                {
                    EnableWeather();
                    _weatherEnabled = true;
                    PlayerPrefs.SetInt(WEATHER_INSTALLED_KEY, 1);
                }
            }

            if (EditorSettings.defaultBehaviorMode == EditorBehaviorMode.Mode3D)
            {
                if (!DoesNamespaceExist(RAIN_MAKER_PACKAGE_NAME))
                {
                    EditorGUILayout.LabelField(RAIN_MAKER_INSTALL_HEADER.ToColorizedString(Color.yellow), _headerStyleSmall);
                    if (GUILayout.Button("Install Rain Maker"))
                    {
                        Application.OpenURL(RAIN_PACKAGE_URL);
                    }
                }
                else if (!_weatherEnabled)
                {
                    EnableWeather();
                    _weatherEnabled = true;
                    PlayerPrefs.SetInt(WEATHER_INSTALLED_KEY, 1);
                }
            }

            if (!AreDemoScenesSetUp())
            {
                EditorGUILayout.LabelField("Click button below, if you want to see demo scenes");
                EditorGUILayout.Space();

                if (GUILayout.Button("Set Up Demo Scenes"))
                {
                    SetUpDemoScenes();
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            if (GUILayout.Button("Read Documentation"))
            {
                Application.OpenURL("https://heroicsolo.gitbook.io/heroic-engine");
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Go to our Discord"))
            {
                Application.OpenURL("https://discord.gg/gTbzY4vhvD");
            }
        }
    }

    [InitializeOnLoad]
    public static class OpenEditorWindowOnLoad
    {
        static OpenEditorWindowOnLoad()
        {
            EditorApplication.delayCall += () =>
            {
                // Check if the window is already open to avoid duplicates
                if (EditorWindow.HasOpenInstances<WelcomeScreen>() == false)
                {
                    WelcomeScreen.TryShowWindow();
                }
            };
        }
    }
}
