using UnityEditor;
using UnityEngine;
using System.IO;
using UnityEngine.Rendering;

namespace HeroicEngine.Utils.Editor
{
    [InitializeOnLoad]
    public sealed class RainShaderHandler
    {
        private const string SHADER_FILE_PATH = "Assets/Heroic Engine/Shaders/URP_RainShader.shader";
        private const string SHADER_BACKUP_PATH = "Assets/Heroic Engine/Shaders/URP_RainShader.shader.txt";

        // Static constructor to handle when Unity starts
        static RainShaderHandler()
        {
            // Check if the pipeline is URP
            if (GraphicsSettings.defaultRenderPipeline != null && GraphicsSettings.defaultRenderPipeline.GetType().Name.Contains("Universal"))
            {
                // When URP is active, restore the shader file extension
                RestoreShaderFile();
                SwitchMaterialToURP();
            }
            else
            {
                // When not using URP, move the shader file to a hidden state
                HideShaderFile();
                SwitchMaterialToStandard();
            }

            // Register callback to handle pipeline changes
            EditorApplication.update += HandleRenderPipelineChange;
        }

        private static void HandleRenderPipelineChange()
        {
            // Check if URP is activated
            if (GraphicsSettings.defaultRenderPipeline != null && GraphicsSettings.defaultRenderPipeline.GetType().Name.Contains("Universal"))
            {
                RestoreShaderFile();
            }
            else
            {
                HideShaderFile();
            }
        }

        private static void SwitchMaterialToStandard()
        {
            // Find all materials and switch from URP Unlit to Standard
            var materialGuids = AssetDatabase.FindAssets("t:Material");

            foreach (var materialGuid in materialGuids)
            {
                var materialPath = AssetDatabase.GUIDToAssetPath(materialGuid);
                var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);

                if (material != null && material.shader != null && material.shader.name == "Custom/URP_RainShader")
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
        }

        private static void SwitchMaterialToURP()
        {
            // Find all materials and switch from Standard to URP Lit
            var materialGuids = AssetDatabase.FindAssets("t:Material");

            foreach (var materialGuid in materialGuids)
            {
                var materialPath = AssetDatabase.GUIDToAssetPath(materialGuid);
                var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);

                if (material != null && material.shader != null && material.shader.name == "Custom/RainShader")
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
        }

        private static void RestoreShaderFile()
        {
            if (File.Exists(SHADER_BACKUP_PATH))
            {
                // Restore the original file (change extension back to .shader)
                File.Move(SHADER_BACKUP_PATH, SHADER_FILE_PATH);
                AssetDatabase.Refresh(); // Refresh Asset Database to re-import the shader
            }
        }

        private static void HideShaderFile()
        {
            if (File.Exists(SHADER_FILE_PATH))
            {
                // Move the shader file to a hidden extension
                File.Move(SHADER_FILE_PATH, SHADER_BACKUP_PATH);
                AssetDatabase.Refresh(); // Refresh Asset Database to remove it from the project
            }
        }
    }
}
