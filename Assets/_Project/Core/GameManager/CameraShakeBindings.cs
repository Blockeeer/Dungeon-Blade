using DungeonBlade.Boss;
using DungeonBlade.Player;
using UnityEngine;

namespace DungeonBlade.Core
{
    // Subscribes to gameplay events and triggers camera shakes.
    // Wire references in the Inspector. Missing references are skipped silently —
    // the boss field, for example, only matters in the Dungeon scene.
    public class CameraShakeBindings : MonoBehaviour
    {
        [SerializeField] PlayerStats playerStats;
        [SerializeField] PlayerMovement playerMovement;
        [SerializeField] BossBase boss;

        [Header("Shake on player damage (any source)")]
        [SerializeField] float damageStrengthBase = 0.18f;
        [SerializeField] float damageStrengthPerHP = 0.004f;
        [SerializeField] float damageDuration = 0.18f;

        [Header("Shake on player death")]
        [SerializeField] float deathStrength = 0.35f;
        [SerializeField] float deathDuration = 0.45f;

        [Header("Shake on player dash / dodge (movement burst)")]
        [SerializeField] float burstStrength = 0.06f;
        [SerializeField] float burstDuration = 0.08f;

        [Header("Shake on boss phase transitions")]
        [SerializeField] float phaseTransitionStrength = 0.25f;
        [SerializeField] float phaseTransitionDuration = 0.35f;
        [SerializeField] float enrageStrength = 0.45f;
        [SerializeField] float enrageDuration = 0.6f;

        void OnEnable()
        {
            if (playerStats != null)
            {
                playerStats.OnDamaged += OnPlayerDamaged;
                playerStats.OnDeath += OnPlayerDeath;
            }
            if (playerMovement != null)
            {
                playerMovement.DashStarted += OnBurst;
                playerMovement.DodgeStarted += OnBurst;
            }
            if (boss != null)
            {
                boss.OnPhaseChanged += OnBossPhaseChanged;
            }
        }

        void OnDisable()
        {
            if (playerStats != null)
            {
                playerStats.OnDamaged -= OnPlayerDamaged;
                playerStats.OnDeath -= OnPlayerDeath;
            }
            if (playerMovement != null)
            {
                playerMovement.DashStarted -= OnBurst;
                playerMovement.DodgeStarted -= OnBurst;
            }
            if (boss != null)
            {
                boss.OnPhaseChanged -= OnBossPhaseChanged;
            }
        }

        void OnPlayerDamaged(float amount)
        {
            float s = damageStrengthBase + amount * damageStrengthPerHP;
            CameraShake.Instance?.Shake(s, damageDuration);
        }

        void OnPlayerDeath()
        {
            CameraShake.Instance?.Shake(deathStrength, deathDuration);
        }

        void OnBurst()
        {
            CameraShake.Instance?.Shake(burstStrength, burstDuration);
        }

        void OnBossPhaseChanged(BossPhase phase)
        {
            // Phase 2 / Phase 3 entries punch the camera. Phase 3 (enrage) hits hardest.
            switch (phase)
            {
                case BossPhase.Phase2:
                    CameraShake.Instance?.Shake(phaseTransitionStrength, phaseTransitionDuration);
                    break;
                case BossPhase.Phase3:
                    CameraShake.Instance?.Shake(enrageStrength, enrageDuration);
                    break;
            }
        }
    }
}
