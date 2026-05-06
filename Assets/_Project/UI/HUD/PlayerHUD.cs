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
            if (healthFill != null) healthFill.fillAmount = max > 0f ? Mathf.Clamp01(current / max) : 0f;
            if (healthText != null) healthText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
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
            Debug.Log($"[HUD] OnExperienceChanged: {current}/{needed} = {pct:F2}. experienceFill={experienceFill}, fillAmount before={experienceFill?.fillAmount}");
            if (experienceFill != null) experienceFill.fillAmount = pct;
            if (experienceText != null) experienceText.text = $"{current} / {needed} XP";
            Debug.Log($"[HUD] fillAmount after={experienceFill?.fillAmount}");
        }

        void OnGoldChanged(int gold)
        {
            if (goldText != null) goldText.text = $"{gold} G";
        }
    }
}
