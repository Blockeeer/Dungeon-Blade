using System.Collections;
using DungeonBlade.Boss;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonBlade.UI.HUD
{
    public class BossHealthBar : MonoBehaviour
    {
        [SerializeField] BossBase boss;
        [SerializeField] CanvasGroup root;
        [SerializeField] Image fill;
        [SerializeField] TMP_Text nameLabel;
        [SerializeField] TMP_Text phaseLabel;

        [Header("Phase markers")]
        [Tooltip("Optional vertical lines drawn over the bar at the phase 2/3 thresholds. Leave empty if you don't want them.")]
        [SerializeField] RectTransform phase2Marker;
        [SerializeField] RectTransform phase3Marker;

        [Header("Fade")]
        [SerializeField] float fadeDuration = 0.5f;

        Coroutine _fadeRoutine;

        void Awake()
        {
            if (root != null) root.alpha = 0f;
            if (boss != null)
            {
                boss.OnPhaseChanged += OnPhaseChanged;
                boss.OnBossHealthChanged += OnHealthChanged;
                boss.OnBossDefeated += OnBossDefeated;
            }
        }

        void Start()
        {
            // Stamp boss name once; phase 2/3 markers slide to their thresholds.
            if (nameLabel != null && boss != null) nameLabel.text = boss.name;

            if (phase2Marker != null) PlaceMarker(phase2Marker, 0.66f);
            if (phase3Marker != null) PlaceMarker(phase3Marker, 0.33f);
        }

        void OnDestroy()
        {
            if (boss == null) return;
            boss.OnPhaseChanged -= OnPhaseChanged;
            boss.OnBossHealthChanged -= OnHealthChanged;
            boss.OnBossDefeated -= OnBossDefeated;
        }

        void OnPhaseChanged(BossPhase phase)
        {
            if (phaseLabel != null)
            {
                phaseLabel.text = phase switch
                {
                    BossPhase.Phase1 => "Phase 1",
                    BossPhase.Transition => "Transitioning…",
                    BossPhase.Phase2 => "Phase 2",
                    BossPhase.Phase3 => "Phase 3 — Enraged",
                    _ => string.Empty,
                };
            }

            // Show on first non-Dormant phase, hide once dead.
            if (phase == BossPhase.Phase1 || phase == BossPhase.Phase2 || phase == BossPhase.Phase3 || phase == BossPhase.Transition)
            {
                Fade(1f);
            }
        }

        void OnHealthChanged(float current, float max)
        {
            if (fill == null) return;
            float pct = max > 0f ? Mathf.Clamp01(current / max) : 0f;
            fill.fillAmount = pct;
        }

        void OnBossDefeated()
        {
            if (phaseLabel != null) phaseLabel.text = "Defeated";
            Fade(0f);
        }

        void Fade(float target)
        {
            if (root == null) return;
            if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
            _fadeRoutine = StartCoroutine(FadeRoutine(target));
        }

        IEnumerator FadeRoutine(float target)
        {
            float start = root.alpha;
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                root.alpha = Mathf.Lerp(start, target, t / fadeDuration);
                yield return null;
            }
            root.alpha = target;
        }

        static void PlaceMarker(RectTransform marker, float pct)
        {
            // Anchor marker to left of bar at fractional X position.
            marker.anchorMin = new Vector2(pct, 0f);
            marker.anchorMax = new Vector2(pct, 1f);
            marker.anchoredPosition = Vector2.zero;
        }
    }
}
