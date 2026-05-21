using UnityEngine;

namespace DungeonBlade.Bank
{
    /// <summary>
    /// Subtly pulses a Light's intensity to simulate flickering firelight.
    /// Uses layered Perlin noise so the flicker reads as organic (not a sine wave).
    /// Attach to the same GameObject as the Light component (point lights on braziers,
    /// fire pits, candles, etc.).
    /// </summary>
    [RequireComponent(typeof(Light))]
    public class FireLightFlicker : MonoBehaviour
    {
        [Tooltip("Base intensity around which the flicker oscillates. Auto-captured from the Light at Awake.")]
        [SerializeField] float baseIntensity = -1f;
        [Tooltip("How much the intensity can dip below or rise above the base, as a fraction of base (0.25 = ±25%).")]
        [Range(0f, 0.6f)] [SerializeField] float flickerAmount = 0.22f;
        [Tooltip("How fast the flicker oscillates. Higher = jumpier flame.")]
        [Range(0.5f, 8f)] [SerializeField] float speed = 3.5f;
        [Tooltip("Optional subtle color warm/cool drift as the flame breathes (0 disables).")]
        [Range(0f, 0.15f)] [SerializeField] float colorDrift = 0.05f;

        Light _light;
        Color _baseColor;
        float _seed;

        void Awake()
        {
            _light = GetComponent<Light>();
            if (baseIntensity < 0f) baseIntensity = _light.intensity;
            _baseColor = _light.color;
            // Random seed so multiple flickering lights don't pulse in sync.
            _seed = Random.Range(0f, 1000f);
        }

        /// <summary>Configure flicker amount and speed at spawn time.</summary>
        public void Configure(float amount, float oscillationSpeed)
        {
            flickerAmount = Mathf.Clamp(amount, 0f, 0.6f);
            speed = Mathf.Clamp(oscillationSpeed, 0.5f, 8f);
        }

        void Update()
        {
            float t = (Time.time + _seed) * speed;
            // Two octaves of Perlin noise for natural variation.
            float n1 = Mathf.PerlinNoise(t, 0.123f);
            float n2 = Mathf.PerlinNoise(t * 2.3f, 7.77f) * 0.5f;
            float noise = (n1 + n2) / 1.5f; // ~0..1
            float offset = (noise - 0.5f) * 2f * flickerAmount * baseIntensity;
            _light.intensity = Mathf.Max(0f, baseIntensity + offset);

            if (colorDrift > 0f)
            {
                float c = (noise - 0.5f) * 2f * colorDrift;
                // Warm shift: more red/orange when bright, slightly cooler when dim.
                _light.color = new Color(
                    Mathf.Clamp01(_baseColor.r + c),
                    Mathf.Clamp01(_baseColor.g + c * 0.6f),
                    Mathf.Clamp01(_baseColor.b - c * 0.5f),
                    _baseColor.a
                );
            }
        }
    }
}
