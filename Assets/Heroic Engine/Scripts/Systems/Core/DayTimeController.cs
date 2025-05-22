using HeroicEngine.Systems.DI;
using UnityEngine;

namespace HeroicEngine.Systems
{
    public sealed class DayTimeController : SystemBase, IDayTimeController
    {
        [SerializeField] private Material skyMaterial;
        [SerializeField] private Light sun;
        [SerializeField][Min(0f)] private float dayLength = 60f;
        [SerializeField][Min(0f)] private float nightLength = 60f;
        [SerializeField][Range(0f, 1f)] private float timeOfDay = 0f;

        private Transform _sunTransform;
        private float _fullDayLength;
        private float _currentTimeOfDay;
        private float _sunMoveSpeed;
        private float _xAngle;

        /// <summary>
        /// Set current time of day
        /// </summary>
        /// <param name="timeOfDay">Time of day (from 0 to 1, where 0 is sunrise, 0.5 is sunset, 1 is sunrise again)</param>
        public void SetTimeOfDay(float timeOfDay)
        {
            this.timeOfDay = Mathf.Clamp01(timeOfDay);
            _xAngle = Mathf.Lerp(0f, 360f, this.timeOfDay);
            _sunTransform.rotation = Quaternion.Euler(_xAngle, 0f, 0f);
        }

        protected override void Start()
        {
            base.Start();
            _sunTransform = sun.transform;
            _fullDayLength = dayLength + nightLength;
            if (skyMaterial != null)
            {
                RenderSettings.skybox = skyMaterial;
            }
            RenderSettings.sun = sun;
            SetTimeOfDay(timeOfDay);
        }

        private void Update()
        {
            _fullDayLength = dayLength + nightLength;

            if (_xAngle < 180f)
            {
                _sunMoveSpeed = _fullDayLength / (2f * dayLength);
            }
            else
            {
                _sunMoveSpeed = _fullDayLength / (2f * nightLength);
            }

            _currentTimeOfDay += _sunMoveSpeed * Time.deltaTime;

            if (_currentTimeOfDay > _fullDayLength)
            {
                _currentTimeOfDay = 0f;
            }

            SetTimeOfDay(_currentTimeOfDay / _fullDayLength);
        }
    }
}