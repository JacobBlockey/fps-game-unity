using UnityEngine;

namespace ZONE.World
{
    /// <summary>
    /// Dynamic weather system. Controls weather state transitions, particle effects,
    /// fog density, and wind. Notifies AI and ecology managers of weather changes.
    /// </summary>
    public class WeatherSystem : MonoBehaviour
    {
        public enum WeatherType
        {
            Clear,
            Cloudy,
            Fog,
            Rain,
            Storm,
            AnomalyStorm
        }

        [Header("Current State")]
        [SerializeField] private WeatherType currentWeather = WeatherType.Clear;

        [Header("Transition")]
        [SerializeField] private float weatherCheckInterval = 120f; // Seconds between weather rolls
        [SerializeField] private float transitionDuration = 10f; // Seconds to transition between weather states
        [SerializeField] private float baseWeatherChangeChance = 0.3f;

        [Header("Fog")]
        [SerializeField] private float clearFogDensity = 0.01f;
        [SerializeField] private float fogFogDensity = 0.08f;
        [SerializeField] private float stormFogDensity = 0.05f;

        [Header("Wind")]
        [SerializeField] private float baseWindStrength = 0.5f;
        [SerializeField] private float stormWindStrength = 3f;

        [Header("Effects")]
        [SerializeField] private ParticleSystem rainParticles;
        [SerializeField] private ParticleSystem stormParticles;
        [SerializeField] private Light directionalLight;

        private float _weatherTimer;
        private float _currentFogDensity;
        private float _targetFogDensity;
        private float _currentWindStrength;
        private float _targetWindStrength;

        public WeatherType CurrentWeather => currentWeather;
        public float WindStrength => _currentWindStrength;
        public bool IsFoggy => currentWeather == WeatherType.Fog;
        public bool IsStormy => currentWeather == WeatherType.Storm || currentWeather == WeatherType.AnomalyStorm;

        public delegate void WeatherChangedHandler(WeatherType newWeather);
        public event WeatherChangedHandler OnWeatherChanged;

        private void Awake()
        {
            _currentFogDensity = clearFogDensity;
            _targetFogDensity = clearFogDensity;
            _currentWindStrength = baseWindStrength;
            _targetWindStrength = baseWindStrength;
        }

        private void Update()
        {
            _weatherTimer += Time.deltaTime;

            if (_weatherTimer >= weatherCheckInterval)
            {
                _weatherTimer = 0f;
                RollWeatherChange();
            }

            // Smooth transitions
            _currentFogDensity = Mathf.Lerp(_currentFogDensity, _targetFogDensity, transitionDuration > 0 ? Time.deltaTime / transitionDuration : 1f);
            _currentWindStrength = Mathf.Lerp(_currentWindStrength, _targetWindStrength, transitionDuration > 0 ? Time.deltaTime / transitionDuration : 1f);

            // Apply fog
            RenderSettings.fog = _currentFogDensity > 0.005f;
            RenderSettings.fogDensity = _currentFogDensity;
        }

        private void RollWeatherChange()
        {
            if (Random.value > baseWeatherChangeChance) return;

            WeatherType newWeather = ChooseWeather();
            if (newWeather != currentWeather)
            {
                SetWeather(newWeather);
            }
        }

        private WeatherType ChooseWeather()
        {
            // Weighted random selection
            float roll = Random.value;
            return roll switch
            {
                < 0.35f => WeatherType.Clear,
                < 0.55f => WeatherType.Cloudy,
                < 0.70f => WeatherType.Fog,
                < 0.85f => WeatherType.Rain,
                < 0.95f => WeatherType.Storm,
                _       => WeatherType.AnomalyStorm
            };
        }

        public void SetWeather(WeatherType weather)
        {
            currentWeather = weather;
            OnWeatherChanged?.Invoke(weather);

            switch (weather)
            {
                case WeatherType.Clear:
                    _targetFogDensity = clearFogDensity;
                    _targetWindStrength = baseWindStrength;
                    SetParticles(false, false);
                    break;
                case WeatherType.Cloudy:
                    _targetFogDensity = clearFogDensity * 1.5f;
                    _targetWindStrength = baseWindStrength * 1.5f;
                    SetParticles(false, false);
                    break;
                case WeatherType.Fog:
                    _targetFogDensity = fogFogDensity;
                    _targetWindStrength = baseWindStrength * 0.5f;
                    SetParticles(false, false);
                    break;
                case WeatherType.Rain:
                    _targetFogDensity = stormFogDensity;
                    _targetWindStrength = baseWindStrength * 2f;
                    SetParticles(true, false);
                    break;
                case WeatherType.Storm:
                    _targetFogDensity = stormFogDensity;
                    _targetWindStrength = stormWindStrength;
                    SetParticles(true, true);
                    break;
                case WeatherType.AnomalyStorm:
                    _targetFogDensity = stormFogDensity * 1.5f;
                    _targetWindStrength = stormWindStrength * 1.5f;
                    SetParticles(true, true);
                    // Anomaly storms trigger special events via WorldManager
                    break;
            }
        }

        private void SetParticles(bool rain, bool storm)
        {
            if (rainParticles) { if (rain) rainParticles.Play(); else rainParticles.Stop(); }
            if (stormParticles) { if (storm) stormParticles.Play(); else stormParticles.Stop(); }
        }

        /// <summary>
        /// Get the sound masking factor (0 = silent, 1 = full mask).
        /// Rain/storm mask player movement sounds.
        /// </summary>
        public float GetSoundMaskFactor()
        {
            return currentWeather switch
            {
                WeatherType.Rain  => 0.4f,
                WeatherType.Storm => 0.7f,
                WeatherType.AnomalyStorm => 0.9f,
                _ => 0f
            };
        }
    }
}
