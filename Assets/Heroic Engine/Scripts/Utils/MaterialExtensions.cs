using UnityEngine;

namespace HeroicEngine.Utils
{
    public static class MaterialExtensions
    {
        private const string ALPHATEST_ON = "_ALPHATEST_ON";
        private const string ALPHABLEND_ON = "_ALPHABLEND_ON";
        private const string ALPHAPREMULTIPLY_ON = "_ALPHAPREMULTIPLY_ON";
        
        private static readonly int _srcBlend = Shader.PropertyToID("_SrcBlend");
        private static readonly int _dstBlend = Shader.PropertyToID("_DstBlend");
        private static readonly int _zWrite = Shader.PropertyToID("_ZWrite");
        /// <summary>
        /// This extension method tries to switch given material to opaque mode, if possible.
        /// </summary>
        /// <param name="material">Given material</param>
        public static void ToOpaqueMode(this Material material)
        {
            material.SetOverrideTag("RenderType", "");
            material.SetInt(_srcBlend, (int)UnityEngine.Rendering.BlendMode.One);
            material.SetInt(_dstBlend, (int)UnityEngine.Rendering.BlendMode.Zero);
            material.SetInt(_zWrite, 1);
            material.DisableKeyword(ALPHATEST_ON);
            material.DisableKeyword(ALPHABLEND_ON);
            material.DisableKeyword(ALPHAPREMULTIPLY_ON);
            material.renderQueue = -1;
        }

        /// <summary>
        /// This extension method tries to switch given material to transparent mode, if possible.
        /// </summary>
        /// <param name="material">Given material</param>
        public static void ToFadeMode(this Material material)
        {
            material.SetOverrideTag("RenderType", "Transparent");
            material.SetInt(_srcBlend, (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt(_dstBlend, (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt(_zWrite, 0);
            material.DisableKeyword(ALPHATEST_ON);
            material.EnableKeyword(ALPHABLEND_ON);
            material.DisableKeyword(ALPHAPREMULTIPLY_ON);
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }
    }
}