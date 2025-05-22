#if WEATHER_PACKAGE
using System;
using DigitalRuby.RainMaker;
using HeroicEngine.Systems.DI;
using System.Collections;
using UnityEngine;

namespace HeroicEngine.Systems.Weather
{
    public enum WeatherState
    {
        None = 0,
        RainingLight = 1,
        RainingMedium = 2,
        RainingHeavy = 3
    }

    public enum WindState
    {
        None = 0,
        Light = 1,
        Medium = 2,
        Heavy = 3
    }

    public sealed class WeatherController : SystemBase, IWeatherController
    {
        [SerializeField] private BaseRainScript rainParticles;
        [SerializeField] private BaseRainScript rainParticles2D;
        [SerializeField] private WeatherState initialWeather;
        [SerializeField] private WindState initialWindState;

        private BaseRainScript _rainScript;

        /// <summary>
        /// This method allows to set current weather state. It can be None, RainingLight, RainingMedium or RainingHeavy.
        /// You can also set certain weather change time by fadeDuration parameter.
        /// </summary>
        /// <param name="weatherState">Needed weather state</param>
        /// <param name="fadeDuration">Time of weather change</param>
        public void SetWeatherState(WeatherState weatherState, float fadeDuration = 3f)
        {
            switch (weatherState)
            {
                case WeatherState.None:
                    StartCoroutine(RainIntensityChanger(0f, fadeDuration));
                    break;
                case WeatherState.RainingLight:
                    StartCoroutine(RainIntensityChanger(0.25f, fadeDuration));
                    break;
                case WeatherState.RainingMedium:
                    StartCoroutine(RainIntensityChanger(0.5f, fadeDuration));
                    break;
                case WeatherState.RainingHeavy:
                    StartCoroutine(RainIntensityChanger(1f, fadeDuration));
                    break;
            }
        }

        /// <summary>
        /// This method allows to set current wind power. It can be None, Light, Medium or Heavy.
        /// </summary>
        /// <param name="windState">Needed wind power</param>
        public void SetWindState(WindState windState)
        {
            switch (windState)
            {
                case WindState.None:
                    _rainScript.EnableWind = false;
                    break;
                case WindState.Light:
                    _rainScript.EnableWind = true;
                    _rainScript.WindSpeedRange.x = 0.5f;
                    _rainScript.WindSpeedRange.y = 2.5f;
                    break;
                case WindState.Medium:
                    _rainScript.EnableWind = true;
                    _rainScript.WindSpeedRange.x = 2.5f;
                    _rainScript.WindSpeedRange.y = 5f;
                    break;
                case WindState.Heavy:
                    _rainScript.EnableWind = true;
                    _rainScript.WindSpeedRange.x = 5f;
                    _rainScript.WindSpeedRange.y = 10f;
                    break;
            }
        }

        private IEnumerator RainIntensityChanger(float targetIntensity, float fadeDuration)
        {
            var startIntensity = _rainScript.RainIntensity;

            var t = 0f;

            do
            {
                t += Time.deltaTime;

                _rainScript.RainIntensity = Mathf.Lerp(startIntensity, targetIntensity, t / fadeDuration);

                yield return null;
            } while (t < fadeDuration);
        }

        private bool Is2DMode()
        {
            // Check if the main camera is orthographic (common in 2D games)
            var isCameraOrthographic = Camera.main != null && Camera.main.orthographic;

            // Check if there are any Rigidbody2D or Collider2D components (strong 2D indicator)
            var has2DPhysics = FindObjectOfType<Rigidbody2D>() != null || FindObjectOfType<Collider2D>() != null;

            // Check if Sorting Layers exist (common in 2D but also used in some 3D games)
            var hasSortingLayers = SortingLayer.layers.Length > 0;

            // Check if there are 3D physics objects (if true, it's likely a 3D game)
            var has3DPhysics = FindObjectOfType<Rigidbody>() != null || FindObjectOfType<Collider>() != null;

            // Final Decision:
            // - A true 2D game should have an orthographic camera AND either 2D physics or sorting layers.
            // - If 3D physics objects are found, it's definitely a 3D game.
            return isCameraOrthographic && (has2DPhysics || hasSortingLayers) && !has3DPhysics;
        }

        protected override void Start()
        {
            base.Start();
            
            if (!Is2DMode())
            {
                _rainScript = rainParticles;
                rainParticles.gameObject.SetActive(true);
                rainParticles2D.gameObject.SetActive(false);
            }
            else
            {
                _rainScript = rainParticles2D;
                rainParticles.gameObject.SetActive(false);
                rainParticles2D.gameObject.SetActive(true);
            }

            SetWeatherState(initialWeather);
            SetWindState(initialWindState);
        }
    }
}
#endif
