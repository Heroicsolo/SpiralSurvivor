using UnityEngine;

namespace HeroicEngine.Components
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(ParticleSystem))]
    public class ColorizedParticles : MonoBehaviour
    {
        [SerializeField] private Color color = Color.white;

        private ParticleSystem _particleSystem;
        private Color _prevColor;

        /// <summary>
        /// This method sets needed particles color. It changes both Start color of Main Module and Lifetime gradient color.
        /// </summary>
        /// <param name="newColor">Needed color of particles</param>
        public void SetColor(Color newColor)
        {
            color = newColor;
        }

        private void Awake()
        {
            _particleSystem = GetComponent<ParticleSystem>();
            _prevColor = _particleSystem.main.startColor.color;
        }

        private void UpdateColor()
        {
            if (color != _prevColor)
            {
                var mainModule = _particleSystem.main;
                var startColor = mainModule.startColor;

                startColor.color = color;
                mainModule.startColor = startColor;
                _prevColor = color;

                if (_particleSystem.colorOverLifetime.enabled)
                {
                    var lifetimeColor = _particleSystem.colorOverLifetime;

                    var gradient = new Gradient();
                    gradient.SetKeys(
                        new GradientColorKey[]
                        {
                            new(color, 0f), new(color, 0.5f), new(color, 1f)
                        },
                        lifetimeColor.color.gradient.alphaKeys
                    );

                    lifetimeColor.color = new ParticleSystem.MinMaxGradient(gradient);
                }
            }
        }

        private void Update()
        {
            UpdateColor();
        }

        private void OnGUI()
        {
            UpdateColor();
        }
    }
}
