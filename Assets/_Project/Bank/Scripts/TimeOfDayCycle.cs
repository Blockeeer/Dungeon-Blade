using UnityEngine;

namespace DungeonBlade.Bank
{
    /// <summary>
    /// Drives a slow day/night cycle on a Directional Light, plus optional fog and
    /// ambient tinting in sync. Attach to your Directional Light (or any GameObject —
    /// it auto-finds the scene's directional light if not set).
    ///
    /// Normalized time runs 0..1 where:
    ///   0.00 = midnight (sun pointing straight down beneath horizon)
    ///   0.25 = sunrise (sun at east horizon)
    ///   0.50 = noon (sun overhead)
    ///   0.75 = sunset (sun at west horizon)
    /// </summary>
    [DisallowMultipleComponent]
    public class TimeOfDayCycle : MonoBehaviour
    {
        [Header("Target")]
        [Tooltip("The directional light driven by the cycle. Auto-found at Awake if left empty.")]
        [SerializeField] Light directionalLight;

        [Header("Timing")]
        [Tooltip("Seconds for one full day/night cycle. 300 = 5 minutes per cycle.")]
        [SerializeField] float cycleDurationSeconds = 300f;
        [Tooltip("Where the cycle starts. 0.7 ≈ early sunset, 0.5 = noon, 0.25 = sunrise.")]
        [Range(0f, 1f)] [SerializeField] float startTime = 0.7f;
        [Tooltip("Pause the cycle so the time stays fixed at startTime.")]
        [SerializeField] bool paused = false;

        [Header("Sun arc")]
        [Tooltip("Azimuth (Y rotation) of the sun's arc — controls which compass direction the sun rises/sets from.")]
        [SerializeField] float sunYawDegrees = 30f;

        [Header("Sun color + intensity")]
        [SerializeField] Color dayColor    = new Color(1f, 0.95f, 0.85f);
        [SerializeField] Color sunsetColor = new Color(1f, 0.65f, 0.40f);
        [SerializeField] Color nightColor  = new Color(0.30f, 0.42f, 0.62f);
        [SerializeField] float dayIntensity   = 2.5f;
        [SerializeField] float nightIntensity = 0.20f;

        [Header("Environment sync")]
        [Tooltip("If true, RenderSettings.fogColor is tinted toward the current sun color.")]
        [SerializeField] bool updateFogColor = true;
        [SerializeField, Range(0f, 1f)] float fogTintStrength = 0.5f;
        [Tooltip("If true, RenderSettings.ambientLight is tinted toward the current sun color.")]
        [SerializeField] bool updateAmbientLight = true;
        [SerializeField, Range(0f, 1f)] float ambientTintStrength = 0.3f;

        float _t;
        Color _baseFog;
        Color _baseAmbient;

        void Awake()
        {
            if (directionalLight == null)
            {
                foreach (var l in FindObjectsOfType<Light>())
                {
                    if (l.type == LightType.Directional) { directionalLight = l; break; }
                }
            }
            _t = startTime;
            _baseFog = RenderSettings.fogColor;
            _baseAmbient = RenderSettings.ambientLight;
            Apply(_t);
        }

        void Update()
        {
            if (paused || cycleDurationSeconds <= 0.01f) { Apply(_t); return; }
            _t += Time.deltaTime / cycleDurationSeconds;
            if (_t >= 1f) _t -= 1f;
            Apply(_t);
        }

        void Apply(float t)
        {
            // Sun rotation: rotate around X so t=0.25 has the sun at the horizon (X=0),
            // t=0.5 directly overhead (X=90 → light pointing straight down).
            float sunPitch = (t - 0.25f) * 360f;
            if (directionalLight != null)
            {
                directionalLight.transform.rotation = Quaternion.Euler(sunPitch, sunYawDegrees, 0f);
                directionalLight.color = SunColor(t);
                directionalLight.intensity = SunIntensity(t);
            }

            if (updateFogColor)
            {
                RenderSettings.fogColor = Color.Lerp(_baseFog, SunColor(t), fogTintStrength * SunIntensityNormalized(t));
            }
            if (updateAmbientLight)
            {
                RenderSettings.ambientLight = Color.Lerp(_baseAmbient, SunColor(t), ambientTintStrength * SunIntensityNormalized(t));
            }
        }

        Color SunColor(float t)
        {
            // Quarter segments: midnight → sunrise → noon → sunset → midnight
            if (t < 0.25f) return Color.Lerp(nightColor, sunsetColor, t / 0.25f);
            if (t < 0.50f) return Color.Lerp(sunsetColor, dayColor, (t - 0.25f) / 0.25f);
            if (t < 0.75f) return Color.Lerp(dayColor, sunsetColor, (t - 0.50f) / 0.25f);
            return Color.Lerp(sunsetColor, nightColor, (t - 0.75f) / 0.25f);
        }

        // 0..1 where 0 = sun below horizon, 1 = sun overhead.
        float SunIntensityNormalized(float t) => Mathf.Clamp01(Mathf.Sin(t * Mathf.PI));

        float SunIntensity(float t)
        {
            return Mathf.Lerp(nightIntensity, dayIntensity, SunIntensityNormalized(t));
        }

#if UNITY_EDITOR
        // Reflect inspector changes immediately in edit mode.
        void OnValidate()
        {
            if (Application.isPlaying) Apply(_t);
        }
#endif
    }
}
