using HeroicEngine.Utils.Pooling;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace HeroicEngine.Components
{
    public class FlyUpText : PooledObject
    {
        [FormerlySerializedAs("m_lifetime")]
        [SerializeField] private float _lifetime = 1f;
        [FormerlySerializedAs("m_floatingSpeed")]
        [SerializeField] private float _floatingSpeed = 1f;
        [FormerlySerializedAs("m_curved")]
        [SerializeField] private bool _curved = false;
        [FormerlySerializedAs("m_flyDirection")]
        [SerializeField] private Vector3 _flyDirection = Vector3.up;
        [FormerlySerializedAs("m_curveDirection")]
        [SerializeField] private Vector3 _curveDirection = Vector3.right;

        private float _timeToDeath = 1f;
        private TextMesh _textMesh;
        private TMP_Text _textMeshUI;
        private Text _text;
        private Vector3 _initPos;
        private float _side = 0f;

        private void Awake()
        {
            _textMesh = GetComponentInChildren<TextMesh>();
            _textMeshUI = GetComponentInChildren<TMP_Text>();
            _text = GetComponentInChildren<Text>();
            _initPos = transform.position;
        }

        /// <summary>
        /// This method sets Curved parameter value. If set to true, object will fly up by curved trajectory, otherwise it will just move along Fly Direction.
        /// </summary>
        /// <param name="value">Is it curved?</param>
        public void SetCurved(bool value)
        {
            _curved = value;
            _side = Mathf.Sign(2f * Random.value - 1f);
        }

        public void SetColor(Color color)
        {
            if (_textMesh)
                _textMesh.color = color;

            if (_text)
                _text.color = color;

            if (_textMeshUI)
                _textMeshUI.color = color;
        }

        public void SetText(string text)
        {
            if (_textMesh)
                _textMesh.text = text;

            if (_text)
                _text.text = text;

            if (_textMeshUI)
                _textMeshUI.text = text;
        }

        private void Start()
        {
            _timeToDeath = _lifetime;
        }

        private void OnEnable()
        {
            _timeToDeath = _lifetime;
        }

        private void Update()
        {
            if (_timeToDeath > 0f)
            {
                _timeToDeath -= Time.deltaTime;

                var percent = _timeToDeath / _lifetime;

                transform.Translate((_flyDirection + _curveDirection * (_side * percent)) * (_floatingSpeed * Time.deltaTime), Space.World);

                if (_textMesh)
                {
                    var color = _textMesh.color;
                    color.a = Mathf.Min(1f, 2f * percent);
                    _textMesh.color = color;
                }
                else if (_text)
                {
                    var color = _text.color;
                    color.a = Mathf.Min(1f, 2f * percent);
                    _text.color = color;
                }
                else if (_textMeshUI)
                {
                    var color = _textMeshUI.color;
                    color.a = Mathf.Min(1f, 2f * percent);
                    _textMeshUI.color = color;
                }

                if (_timeToDeath <= 0f)
                {
                    transform.position = _initPos;

                    if (_textMesh)
                    {
                        var color = _textMesh.color;
                        color.a = 1f;
                        _textMesh.color = color;
                    }
                    else if (_text)
                    {
                        var color = _text.color;
                        color.a = 1f;
                        _text.color = color;
                    }
                    else if (_textMeshUI)
                    {
                        var color = _textMeshUI.color;
                        color.a = 1f;
                        _textMeshUI.color = color;
                    }

                    PoolSystem.ReturnToPool(this);
                }
            }
        }
    }
}
