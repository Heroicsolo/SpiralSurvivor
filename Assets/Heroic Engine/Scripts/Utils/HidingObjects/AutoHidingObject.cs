using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace HeroicEngine.Utils.HidingObjects
{
    //Add this script to object which must be hidden if will appear near the camera (that object has to contain MeshRenderer component)
    //If that object doesn't have MeshRenderer component, this script will search all MeshRenderer components in his children
    //You can also switch "proccessChildren" flag to "true", to proccess all children MeshRenderer components by default
    //Place needed shader which contains "_Color" property and Fade/Transparency mode into the "fadeShader" field!
    //For example, "Legacy Shaders/Transparent/Diffuse" will be fine
    public class AutoHidingObject : MonoBehaviour
    {
        private static readonly int _colorPropertyKey = Shader.PropertyToID("_Color");
        
        [FormerlySerializedAs("m_fadeShader")]
        [SerializeField]
        private Shader _fadeShader;
        [FormerlySerializedAs("m_proccessChildren")]
        [SerializeField]
        private bool _proccessChildren = false;

        private List<MeshRenderer> _renderers;
        private bool _hidden;
        private Dictionary<Material, Shader> _initShaders;

        public bool IsHidden => _hidden;

        private void Awake()
        {
            _renderers = new List<MeshRenderer>();

            var rootRenderer = GetComponent<MeshRenderer>();

            if (!rootRenderer)
            {
                _proccessChildren = true;
            }

            if (_proccessChildren)
            {
                _renderers.AddRange(GetComponentsInChildren<MeshRenderer>());
            }
            else
            {
                _renderers.Add(rootRenderer);
            }

            _initShaders = new Dictionary<Material, Shader>();

            foreach (var rend in _renderers)
            {
                foreach (var mat in rend.materials)
                {
                    _initShaders.Add(mat, rend.material.shader);
                }
            }

            _hidden = false;
        }

        public void Hide()
        {
            if (_hidden)
            {
                return;
            }

            _hidden = true;

            foreach (var rend in _renderers)
            {
                foreach (var mat in rend.materials)
                {
                    mat.shader = _fadeShader;
                    mat.ToFadeMode();
                    var col = mat.color;
                    col.a = 0.15f;
                    mat.color = col;
                }
            }
        }

        public void ShowChild(Transform child)
        {
            for (var i = 0; i < _renderers.Count; i++)
            {
                if (_renderers[i].transform == child)
                {
                    foreach (var mat in _renderers[i].materials)
                    {
                        mat.shader = _initShaders[mat];

                        if (mat.HasProperty(_colorPropertyKey))
                        {
                            var col = mat.color;
                            col.a = 1f;
                            mat.color = col;
                        }
                    }
                }
            }
        }

        public void Show()
        {
            if (!_hidden)
            {
                return;
            }

            _hidden = false;

            if (_renderers == null)
            {
                return;
            }

            for (var i = 0; i < _renderers.Count; i++)
            {
                foreach (var mat in _renderers[i].materials)
                {
                    mat.shader = _initShaders[mat];
                    mat.ToOpaqueMode();

                    if (mat.HasProperty(_colorPropertyKey))
                    {
                        var col = mat.color;
                        col.a = 1f;
                        mat.color = col;
                    }
                }
            }
        }
    }
}
