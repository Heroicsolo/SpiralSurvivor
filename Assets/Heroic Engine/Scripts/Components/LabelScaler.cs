using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace HeroicEngine.Components
{
    public class LabelScaler : MonoBehaviour
    {
        [FormerlySerializedAs("m_animTime")]
        [Min(0.1f)]
        public float _animTime = 0.3f;
        [FormerlySerializedAs("m_maxScaleFactor")]
        [Min(1f)]
        public float _maxScaleFactor = 1.1f;

        private Vector3 _initScale = Vector3.one;
        private float _timeLeft = 0f;
        private bool _animRunning = false;

        /// <summary>
        /// This method sets text string and automatically starts scale animation.
        /// </summary>
        /// <param name="_text">Text to indicate</param>
        public void SetLabelText(string text)
        {
            var textComp = GetComponent<Text>();

            if (textComp)
            {
                textComp.text = text;
            }

            var tmpComp = GetComponent<TMP_Text>();

            if (tmpComp)
            {
                tmpComp.text = text;
            }

            RunAnim();
        }

        /// <summary>
        /// This method runs scale animation.
        /// </summary>
        public void RunAnim()
        {
            _animRunning = true;
            _timeLeft = _animTime;
        }

        private void Start()
        {
            _initScale = transform.localScale;
        }

        private void Update()
        {
            if (_animRunning && _timeLeft > 0f)
            {
                _timeLeft -= Time.deltaTime;

                var percent = 1f - _timeLeft / _animTime;

                transform.localScale = _initScale * (1f + (_maxScaleFactor - 1f) * Mathf.Sin(Mathf.PI * percent * 0.5f));

                if (_timeLeft <= 0f)
                {
                    transform.localScale = _initScale;

                    _animRunning = false;
                }
            }
        }
    }
}
