using UnityEngine;

namespace ZONE.World
{
    /// <summary>
    /// Day/night cycle system. Controls sun/moon position, skybox, lighting,
    /// and ambient color transitions throughout a 24-hour cycle.
    /// </summary>
    public class DayNightCycle : MonoBehaviour
    {
        [Header("Celestial Bodies")]
        [SerializeField] private Light sunLight;
        [SerializeField] private Light moonLight;
        [SerializeField] private Transform sunTransform;
        [SerializeField] private Transform moonTransform;

        [Header("Lighting Presets")]
        [SerializeField] private DayNightPreset dawnPreset;
        [SerializeField] private DayNightPreset dayPreset;
        [SerializeField] private DayNightPreset duskPreset;
        [SerializeField] private DayNightPreset nightPreset;

        [Header("Ambient")]
        [SerializeField] private Material skyboxMaterial;
        [SerializeField] private float sunIntensityMax = 1.2f;
        [SerializeField] private float moonIntensityMax = 0.3f;

        [Header("Settings")]
        [SerializeField] private float transitionSmoothness = 2f;

        private float _currentHours;

        /// <summary>
        /// Called by WorldManager every frame with the current time in hours (0–24).
        /// </summary>
        public void UpdateTime(float hours)
        {
            _currentHours = hours;

            // Sun angle: 0h = midnight (below horizon), 6h = sunrise (horizon east),
            // 12h = noon (overhead), 18h = sunset (horizon west)
            float sunAngle = (hours / 24f) * 360f - 90f; // -90 at midnight, 0 at 6am, 90 at noon, 180 at 6pm
            if (sunTransform) sunTransform.localRotation = Quaternion.Euler(sunAngle, 0f, 0f);

            // Moon is opposite the sun
            float moonAngle = sunAngle + 180f;
            if (moonTransform) moonTransform.localRotation = Quaternion.Euler(moonAngle, 0f, 0f);

            // Determine active preset and blend
            DayNightPreset activePreset = GetCurrentPreset(out float blendFactor);
            ApplyPreset(activePreset, blendFactor);
        }

        private DayNightPreset GetCurrentPreset(out float blendFactor)
        {
            float h = _currentHours;

            if (h >= 5f && h < 7f)
            {
                blendFactor = Mathf.Clamp01((h - 5f) / 2f);
                return BlendPreset(nightPreset, dawnPreset, blendFactor);
            }
            else if (h >= 7f && h < 17f)
            {
                if (h < 8f)
                {
                    blendFactor = Mathf.Clamp01((h - 7f) / 1f);
                    return BlendPreset(dawnPreset, dayPreset, blendFactor);
                }
                if (h >= 16f)
                {
                    blendFactor = Mathf.Clamp01((h - 16f) / 1f);
                    return BlendPreset(dayPreset, duskPreset, blendFactor);
                }
                blendFactor = 0f;
                return dayPreset;
            }
            else if (h >= 17f && h < 19f)
            {
                blendFactor = Mathf.Clamp01((h - 17f) / 2f);
                return BlendPreset(duskPreset, nightPreset, blendFactor);
            }
            else
            {
                blendFactor = 0f;
                return nightPreset;
            }
        }

        private DayNightPreset BlendPreset(DayNightPreset from, DayNightPreset to, float t)
        {
            return new DayNightPreset
            {
                ambientColor = Color.Lerp(from.ambientColor, to.ambientColor, t),
                sunColor = Color.Lerp(from.sunColor, to.sunColor, t),
                fogColor = Color.Lerp(from.fogColor, to.fogColor, t),
                fogDensity = Mathf.Lerp(from.fogDensity, to.fogDensity, t),
                sunIntensity = Mathf.Lerp(from.sunIntensity, to.sunIntensity, t),
                moonIntensity = Mathf.Lerp(from.moonIntensity, to.moonIntensity, t)
            };
        }

        private void ApplyPreset(DayNightPreset preset, float _)
        {
            if (sunLight)
            {
                sunLight.color = preset.sunColor;
                sunLight.intensity = Mathf.Max(0f, preset.sunIntensity) * sunIntensityMax;
            }

            if (moonLight)
            {
                moonLight.intensity = Mathf.Max(0f, preset.moonIntensity) * moonIntensityMax;
            }

            RenderSettings.ambientLight = preset.ambientColor;

            if (preset.fogDensity > 0f)
            {
                RenderSettings.fog = true;
                RenderSettings.fogColor = preset.fogColor;
                RenderSettings.fogDensity = preset.fogDensity;
            }
            else
            {
                RenderSettings.fog = false;
            }

            if (skyboxMaterial)
            {
                // Adjust skybox based on time (implementation depends on skybox shader)
                skyboxMaterial.SetColor("_Tint", preset.ambientColor);
            }
        }
    }

    [System.Serializable]
    public struct DayNightPreset
    {
        public Color ambientColor;
        public Color sunColor;
        public Color fogColor;
        public float fogDensity;
        public float sunIntensity;
        public float moonIntensity;
    }
}
