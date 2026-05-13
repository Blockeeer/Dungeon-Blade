using System.Collections;
using DungeonBlade.Combat;
using TMPro;
using UnityEngine;

namespace DungeonBlade.UI.HUD
{
    // Top-center floating combo counter per Dungeon Blade GDD §11.1.
    // Subscribes to ComboSystem.OnComboChanged + OnComboReset, fades after
    // a configurable idle time. Pulses larger on each new hit.
    public class ComboCounterHUD : MonoBehaviour
    {
        [SerializeField] ComboSystem comboSystem;
        [SerializeField] CanvasGroup root;
        [SerializeField] TMP_Text counterText;
        [SerializeField] TMP_Text labelText;

        [Header("Behavior")]
        [Tooltip("Hide the counter if combo doesn't extend within this many seconds (GDD §11.1 = 3s).")]
        [SerializeField] float fadeAfterSeconds = 3f;
        [SerializeField] float fadeDuration = 0.4f;
        [Tooltip("Combo number must reach this before the counter shows. 1 = show every hit.")]
        [SerializeField] int minComboToShow = 2;

        [Header("Visual")]
        [SerializeField] float pulseScale = 1.25f;
        [SerializeField] float pulseDuration = 0.18f;

        Coroutine _fadeRoutine;
        Coroutine _pulseRoutine;
        Vector3 _baseScale;

        void Awake()
        {
            if (root != null) root.alpha = 0f;
            _baseScale = transform.localScale;
        }

        void OnEnable()
        {
            if (comboSystem != null)
            {
                comboSystem.OnComboChanged += OnComboChanged;
                comboSystem.OnComboReset += OnComboReset;
            }
        }

        void OnDisable()
        {
            if (comboSystem != null)
            {
                comboSystem.OnComboChanged -= OnComboChanged;
                comboSystem.OnComboReset -= OnComboReset;
            }
        }

        void OnComboChanged(int combo)
        {
            if (combo < minComboToShow)
            {
                if (root != null) root.alpha = 0f;
                return;
            }

            if (counterText != null) counterText.text = combo.ToString();
            if (labelText != null) labelText.text = combo >= 10 ? "COMBO!" : "HITS";

            // Snap to fully visible, then schedule the fade.
            if (root != null) root.alpha = 1f;
            if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
            _fadeRoutine = StartCoroutine(FadeAfterDelay());

            // Pulse scale on each new hit.
            if (_pulseRoutine != null) StopCoroutine(_pulseRoutine);
            _pulseRoutine = StartCoroutine(Pulse());
        }

        void OnComboReset(int finalCombo)
        {
            // Combo timed out or got broken. Fade out immediately.
            if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
            _fadeRoutine = StartCoroutine(FadeOut());
        }

        IEnumerator FadeAfterDelay()
        {
            float t = 0f;
            while (t < fadeAfterSeconds) { t += Time.unscaledDeltaTime; yield return null; }
            yield return FadeOut();
        }

        IEnumerator FadeOut()
        {
            if (root == null) yield break;
            float start = root.alpha;
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                root.alpha = Mathf.Lerp(start, 0f, t / fadeDuration);
                yield return null;
            }
            root.alpha = 0f;
        }

        IEnumerator Pulse()
        {
            float t = 0f;
            while (t < pulseDuration)
            {
                t += Time.unscaledDeltaTime;
                float k = t / pulseDuration;
                // Ease out: 0 → pulseScale at peak → back to 1.
                float s = Mathf.Lerp(pulseScale, 1f, k);
                transform.localScale = _baseScale * s;
                yield return null;
            }
            transform.localScale = _baseScale;
        }
    }
}
