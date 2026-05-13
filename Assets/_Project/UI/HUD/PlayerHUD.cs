using System.Collections;
using DungeonBlade.Bank;
using DungeonBlade.Player;
using DungeonBlade.Rewards;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonBlade.UI.HUD
{
    public class PlayerHUD : MonoBehaviour
    {
        [Header("Source")]
        [SerializeField] PlayerStats stats;

        [Header("Health")]
        [SerializeField] Image healthFill;
        [SerializeField] TMP_Text healthText;
        [Tooltip("GDD §11.1 — HP bar flashes red below this threshold (0.25 = 25%).")]
        [Range(0f, 1f)]
        [SerializeField] float lowHealthThreshold = 0.25f;
        [SerializeField] Color healthBaseColor = new Color(0.86f, 0.16f, 0.16f, 1f);
        [SerializeField] Color healthFlashColor = new Color(1f, 0.60f, 0.60f, 1f);
        [SerializeField] float lowHealthFlashPeriod = 0.5f;

        [Header("Stamina")]
        [SerializeField] Image staminaFill;
        [SerializeField] TMP_Text staminaText;

        [Header("Experience")]
        [SerializeField] Image experienceFill;
        [SerializeField] TMP_Text levelText;
        [SerializeField] TMP_Text experienceText;

        [Header("Gold")]
        [SerializeField] TMP_Text goldText;

        ExperienceSystem _exp;
        PlayerWallet _wallet;
        Coroutine _lowHealthFlash;

        void OnEnable()
        {
            // Subscribe only — initial values are read in Start() so that all
            // PlayerStats / ExperienceSystem / PlayerWallet Awakes have run first.
            if (stats != null)
            {
                stats.OnHealthChanged += OnHealthChanged;
                stats.OnStaminaChanged += OnStaminaChanged;
            }

            _exp = ExperienceSystem.Instance;
            if (_exp != null)
            {
                _exp.OnLevelUp += OnLevelUp;
                _exp.OnExperienceChanged += OnExperienceChanged;
            }

            _wallet = PlayerWallet.Instance;
            if (_wallet != null)
            {
                _wallet.OnGoldChanged += OnGoldChanged;
            }
        }

        void OnDisable()
        {
            if (stats != null)
            {
                stats.OnHealthChanged -= OnHealthChanged;
                stats.OnStaminaChanged -= OnStaminaChanged;
            }
            if (_exp != null)
            {
                _exp.OnLevelUp -= OnLevelUp;
                _exp.OnExperienceChanged -= OnExperienceChanged;
            }
            if (_wallet != null)
            {
                _wallet.OnGoldChanged -= OnGoldChanged;
            }
        }

        void Start()
        {
            // Late-bind in case singletons spawned after OnEnable.
            if (_exp == null && ExperienceSystem.Instance != null)
            {
                _exp = ExperienceSystem.Instance;
                _exp.OnLevelUp += OnLevelUp;
                _exp.OnExperienceChanged += OnExperienceChanged;
            }
            if (_wallet == null && PlayerWallet.Instance != null)
            {
                _wallet = PlayerWallet.Instance;
                _wallet.OnGoldChanged += OnGoldChanged;
            }

            // All Awakes have run by Start — read initial values now so the
            // HUD shows the right numbers even though no event has fired yet.
            if (stats != null)
            {
                OnHealthChanged(stats.Health, stats.MaxHealth);
                OnStaminaChanged(stats.Stamina, stats.MaxStamina);
            }
            if (_exp != null)
            {
                OnLevelUp(_exp.Level);
                OnExperienceChanged(_exp.Experience, _exp.ExperienceForNextLevel);
            }
            if (_wallet != null)
            {
                OnGoldChanged(_wallet.Gold);
            }
        }

        void OnHealthChanged(float current, float max)
        {
            float pct = max > 0f ? Mathf.Clamp01(current / max) : 0f;
            if (healthFill != null) healthFill.fillAmount = pct;
            if (healthText != null) healthText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
            UpdateLowHealthFlash(pct);
        }

        // GDD §11.1 — flash the HP bar fill between base + flash colors while
        // HP is below the low-health threshold. Stops when HP rises above the
        // threshold OR reaches zero (dead — no point flashing a death bar).
        void UpdateLowHealthFlash(float pct)
        {
            bool shouldFlash = pct > 0f && pct < lowHealthThreshold;
            if (shouldFlash)
            {
                if (_lowHealthFlash == null && gameObject.activeInHierarchy)
                    _lowHealthFlash = StartCoroutine(LowHealthFlashRoutine());
            }
            else
            {
                if (_lowHealthFlash != null)
                {
                    StopCoroutine(_lowHealthFlash);
                    _lowHealthFlash = null;
                }
                if (healthFill != null) healthFill.color = healthBaseColor;
            }
        }

        IEnumerator LowHealthFlashRoutine()
        {
            if (healthFill == null) yield break;
            float t = 0f;
            while (true)
            {
                t += Time.unscaledDeltaTime;
                // Sin-wave alpha between base color and flash color, full period = lowHealthFlashPeriod.
                float k = 0.5f * (1f + Mathf.Sin(t * Mathf.PI * 2f / lowHealthFlashPeriod));
                healthFill.color = Color.Lerp(healthBaseColor, healthFlashColor, k);
                yield return null;
            }
        }

        void OnStaminaChanged(float current, float max)
        {
            if (staminaFill != null) staminaFill.fillAmount = max > 0f ? Mathf.Clamp01(current / max) : 0f;
            if (staminaText != null) staminaText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
        }

        void OnLevelUp(int level)
        {
            if (levelText != null) levelText.text = $"Lv {level}";
            if (_exp != null) OnExperienceChanged(_exp.Experience, _exp.ExperienceForNextLevel);
        }

        void OnExperienceChanged(int current, int needed)
        {
            float pct = needed > 0 ? Mathf.Clamp01((float)current / needed) : 0f;
            if (experienceFill != null) experienceFill.fillAmount = pct;
            if (experienceText != null) experienceText.text = $"{current} / {needed} XP";
        }

        void OnGoldChanged(int gold)
        {
            if (goldText != null) goldText.text = $"{gold} G";
        }
    }
}
