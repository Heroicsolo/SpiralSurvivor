using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace HeroicEngine.Utils.Editor
{
    public class SpriteFromPrefabGenerator : EditorWindow
    {
        private string _savePath = "Assets/Heroic Engine/Sprites/Icons/";

        private GameObject _prefab;
        private int _resolution = 512;
        private Color _backgroundColor = Color.clear;
        private float _shootOffsetY;
        private Texture2D _generatedTexture;

        [MenuItem("Tools/HeroicEngine/Icon From Prefab Generator")]
        private static void OpenWindow()
        {
            GetWindow<SpriteFromPrefabGenerator>("Icon From Prefab");
        }

        private void OnGUI()
        {
            GUILayout.Label("Generate Icon from Prefab", EditorStyles.boldLabel);

            _prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", _prefab, typeof(GameObject), false);
            _savePath = EditorGUILayout.TextField("Save Path", _savePath);
            _resolution = EditorGUILayout.IntField("Resolution", _resolution);
            _backgroundColor = EditorGUILayout.ColorField("Background Color", _backgroundColor);
            _shootOffsetY = EditorGUILayout.FloatField("Shoot Y Offset", _shootOffsetY);

            if (GUILayout.Button("Generate Icon"))
            {
                if (!_prefab)
                {
                    Debug.LogError("No prefab selected.");
                    return;
                }

                if (!Directory.Exists(_savePath))
                {
                    Directory.CreateDirectory(_savePath);
                }

                _generatedTexture = GenerateIcon();
            }

            if (_generatedTexture)
            {
                var previewRect = GUILayoutUtility.GetRect(128, 128, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
                EditorGUI.DrawPreviewTexture(previewRect, _generatedTexture);
            }
        }

        private Texture2D GenerateIcon()
        {
            // Create a temporary scene
            var tempScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);

            // Instantiate the prefab
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(_prefab);

            if (!instance)
            {
                Debug.LogError("Failed to instantiate prefab.");
                return null;
            }

            // Calculate bounds
            var bounds = CalculateBounds(instance);

            // Create and setup the camera
            var camera = new GameObject("Camera").AddComponent<Camera>();
            camera.backgroundColor = _backgroundColor;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.orthographic = true;

            // Calculate orthographic size to fit the object
            var maxDimension = Mathf.Max(bounds.size.x, bounds.size.y);
            camera.orthographicSize = maxDimension / 2f;

            // Position the camera to fit the object
            camera.transform.position = bounds.center + new Vector3(0, 0, -10);

            // Add directional light
            var light = new GameObject("Light").AddComponent<Light>();
            light.type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(50, -30, 0);

            // Render to texture
            var renderTexture = new RenderTexture(_resolution, _resolution, 24);
            camera.targetTexture = renderTexture;
            camera.Render();

            // Convert to Texture2D
            RenderTexture.active = renderTexture;
            var texture = new Texture2D(_resolution, _resolution, TextureFormat.RGBA32, false);
            texture.ReadPixels(new Rect(0, 0, _resolution, _resolution), 0, 0);
            texture.Apply();

            var fullPath = _savePath + $"{_prefab.name}.png";

            // Save as PNG
            var bytes = texture.EncodeToPNG();
            File.WriteAllBytes(fullPath, bytes);
            Debug.Log("Icon saved to: " + fullPath);

            // Cleanup
            RenderTexture.active = null;
            camera.targetTexture = null;

            DestroyImmediate(instance);
            DestroyImmediate(camera.gameObject);
            DestroyImmediate(light.gameObject);

            EditorSceneManager.CloseScene(tempScene, true);

            // Load and display the generated icon
            AssetDatabase.Refresh();

            Object asset = AssetDatabase.LoadAssetAtPath<Texture2D>(fullPath);
            Selection.activeObject = asset;

            return texture;
        }

        private Bounds CalculateBounds(GameObject obj)
        {
            var bounds = new Bounds(obj.transform.position, Vector3.zero);
            var renderers = obj.GetComponentsInChildren<Renderer>();

            foreach (var renderer in renderers)
            {
                bounds.Encapsulate(renderer.bounds);
            }

            return bounds;
        }
    }
}
