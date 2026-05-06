using UnityEngine;

namespace DungeonBlade.Core
{
    public class CameraShake : MonoBehaviour
    {
        public static CameraShake Instance { get; private set; }

        [Tooltip("Per-frame multiplier on shake strength. Lower = faster decay.")]
        [SerializeField] float decayRate = 0.85f;

        [Tooltip("Caps the strength of any single shake — prevents bugs from cranking the camera into orbit.")]
        [SerializeField] float maxStrength = 1.0f;

        [Header("Default presets (called via PlayShakeXxx convenience methods)")]
        [SerializeField] float lightShakeStrength = 0.08f;
        [SerializeField] float lightShakeDuration = 0.10f;
        [SerializeField] float mediumShakeStrength = 0.18f;
        [SerializeField] float mediumShakeDuration = 0.18f;
        [SerializeField] float heavyShakeStrength = 0.35f;
        [SerializeField] float heavyShakeDuration = 0.30f;

        Vector3 _baseLocal;
        float _strength;
        float _remaining;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
            _baseLocal = transform.localPosition;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void Shake(float strength, float duration)
        {
            // Don't let a weaker incoming shake override a stronger ongoing one.
            _strength = Mathf.Max(_strength, Mathf.Clamp(strength, 0f, maxStrength));
            _remaining = Mathf.Max(_remaining, duration);
        }

        public void PlayLight() => Shake(lightShakeStrength, lightShakeDuration);
        public void PlayMedium() => Shake(mediumShakeStrength, mediumShakeDuration);
        public void PlayHeavy() => Shake(heavyShakeStrength, heavyShakeDuration);

        void LateUpdate()
        {
            if (_remaining > 0f)
            {
                Vector2 r = Random.insideUnitCircle;
                Vector3 offset = new Vector3(r.x, r.y, 0f) * _strength;
                transform.localPosition = _baseLocal + offset;
                _strength *= decayRate;
                _remaining -= Time.unscaledDeltaTime;
            }
            else if (_strength != 0f || transform.localPosition != _baseLocal)
            {
                _strength = 0f;
                transform.localPosition = _baseLocal;
            }
        }
    }
}
