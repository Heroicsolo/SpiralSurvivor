using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace HeroicEngine.Utils.Editor
{
    public sealed class AutoMaterialSwitcher : EditorWindow
    {
        private static RenderPipelineAsset _lastCheckedPipeline;

        [MenuItem("Tools/HeroicEngine/Fix Materials for Render Pipeline")]
        private static void AutoSwitchMaterials()
        {
            // Check the current render pipeline asset
            var currentPipeline = GraphicsSettings.defaultRenderPipeline;

            // If the render pipeline has changed, switch materials accordingly
            if (_lastCheckedPipeline != currentPipeline)
            {
                _lastCheckedPipeline = currentPipeline;

                if (currentPipeline == null)
                {
                    // Built-in render pipeline (Standard)
                    SwitchMaterialsToStandard();
                }
                else if (IsURP(currentPipeline))
                {
                    // URP render pipeline
                    SwitchMaterialsToURPLit();
                }
                else
                {
                    Debug.Log("Unknown or custom render pipeline detected.");
                }
            }
        }

        private static bool IsURP(RenderPipelineAsset pipelineAsset)
        {
            // Check if the pipeline is URP by checking for URP specific shader presence
            return Shader.Find("Universal Render Pipeline/Unlit") != null;
        }

        private static void SwitchMaterialsToStandard()
        {
            // Find all materials and switch from URP Unlit to Standard
            var materialGuids = AssetDatabase.FindAssets("t:Material");

            foreach (var materialGuid in materialGuids)
            {
                var materialPath = AssetDatabase.GUIDToAssetPath(materialGuid);
                var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);

                if (materialPath.Contains("Heroic Engine") && material != null && material.shader != null && material.shader.name == "Universal Render Pipeline/Lit")
                {
                    var col = material.color;
                    var standardShader = Shader.Find("Standard");
                    if (standardShader != null)
                    {
                        material.shader = standardShader;
                        material.color = col;
                        EditorUtility.SetDirty(material);
                    }
                }
                else if (material != null && material.shader != null && material.shader.name == "Custom/URP_RainShader")
                {
                    var col = material.color;
                    var standardRainShader = Shader.Find("Custom/RainShader");
                    if (standardRainShader != null)
                    {
                        material.shader = standardRainShader;
                        material.color = col;
                        EditorUtility.SetDirty(material);
                    }
                }
            }

            // Refresh the asset database to apply the changes
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("Switched sample URP materials to Standard.");
        }

        private static void SwitchMaterialsToURPLit()
        {
            // Find all materials and switch from Standard to URP Lit
            var materialGuids = AssetDatabase.FindAssets("t:Material");

            foreach (var materialGuid in materialGuids)
            {
                var materialPath = AssetDatabase.GUIDToAssetPath(materialGuid);
                var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);

                if (materialPath.Contains("Heroic Engine") && material != null && material.shader != null && material.shader.name == "Standard")
                {
                    var col = material.color;
                    var urpUnlitShader = Shader.Find("Universal Render Pipeline/Lit");
                    if (urpUnlitShader != null)
                    {
                        material.shader = urpUnlitShader;
                        material.color = col;
                        EditorUtility.SetDirty(material);
                    }
                }
                else if (material != null && material.shader != null && material.shader.name == "Custom/RainShader")
                {
                    var col = material.color;
                    var urpRainShader = Shader.Find("Custom/URP_RainShader");
                    if (urpRainShader != null)
                    {
                        material.shader = urpRainShader;
                        material.color = col;
                        EditorUtility.SetDirty(material);
                    }
                }
            }

            // Refresh the asset database to apply the changes
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("Switched sample Standard materials to URP.");
        }

        // You can also use this to track the render pipeline changes in the editor
        [InitializeOnLoadMethod]
        private static void RegisterRenderPipelineChangeListener()
        {
            // Check for render pipeline change when entering play mode or switching the pipeline
            EditorApplication.playModeStateChanged += (PlayModeStateChange state) =>
            {
                AutoSwitchMaterials();
            };
        }
    }
}
