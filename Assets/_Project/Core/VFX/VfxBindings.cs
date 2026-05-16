using DungeonBlade.Boss;
using DungeonBlade.Combat;
using DungeonBlade.Enemies;
using DungeonBlade.Player;
using DungeonBlade.Rewards;
using UnityEngine;

namespace DungeonBlade.Core.VFX
{
    // Centralized VFX event dispatcher. Subscribes to combat / movement /
    // enemy / boss events and spawns particle prefabs at the right positions.
    // Drop one of these into the Dungeon scene, wire references + prefabs.
    //
    // All prefab fields are optional — empty slots silently no-op, so you can
    // ship Tier-1 VFX (hit sparks + muzzle flash) first and add the rest later.
    public class VfxBindings : MonoBehaviour
    {
        [Header("References (optional — auto-finds via singleton/FindObjectOfType)")]
        [SerializeField] PlayerStats playerStats;
        [SerializeField] PlayerMovement playerMovement;
        [SerializeField] PlayerCombat playerCombat;
        [SerializeField] BossBase boss;

        [Header("Combat VFX prefabs")]
        [Tooltip("Spawns at hit position when sword swing connects.")]
        [SerializeField] GameObject swordHitSparkPrefab;
        [Tooltip("Spawns at hit point when gun shot connects.")]
        [SerializeField] GameObject gunHitSparkPrefab;
        [Tooltip("Spawns at muzzle when gun fires (regardless of hit/miss).")]
        [SerializeField] GameObject muzzleFlashPrefab;

        [Header("Player VFX prefabs")]
        [Tooltip("Spawns at player position when taking damage.")]
        [SerializeField] GameObject playerDamageVfxPrefab;
        [Tooltip("Trail prefab — attached to player on dash/dodge start, auto-detaches after burst.")]
        [SerializeField] GameObject dashTrailPrefab;
        [Tooltip("Spawns on player on level up.")]
        [SerializeField] GameObject levelUpVfxPrefab;

        [Header("Enemy VFX prefabs")]
        [Tooltip("Spawns at enemy position when they die.")]
        [SerializeField] GameObject enemyDeathDustPrefab;

        [Header("Boss VFX prefabs")]
        [Tooltip("Spawns/attaches on the boss when it enters Phase 3 (enrage).")]
        [SerializeField] GameObject bossEnrageAuraPrefab;

        [Header("Tuning")]
        [Tooltip("Default lifetime in seconds for spawned one-shot VFX (auto-destroyed). Set to 0 to disable auto-destroy.")]
        [SerializeField] float defaultLifetime = 2f;

        Sword _hookedSword;
        Gun _hookedGun;
        GameObject _activeDashTrail;
        GameObject _activeEnrageAura;

        void OnEnable()
        {
            ResolveReferences();
            HookAll();
        }

        void OnDisable()
        {
            UnhookAll();
        }

        void ResolveReferences()
        {
            if (playerStats == null) playerStats = FindObjectOfType<PlayerStats>();
            if (playerMovement == null) playerMovement = FindObjectOfType<PlayerMovement>();
            if (playerCombat == null) playerCombat = FindObjectOfType<PlayerCombat>();
            if (boss == null) boss = FindObjectOfType<BossBase>();
        }

        void HookAll()
        {
            if (playerStats != null)
            {
                playerStats.OnDamaged += OnPlayerDamaged;
            }
            if (playerMovement != null)
            {
                playerMovement.DashStarted += OnDashOrDodgeStarted;
                playerMovement.DodgeStarted += OnDashOrDodgeStarted;
            }
            if (playerCombat != null)
            {
                playerCombat.WeaponEquipped += OnWeaponEquipped;
                HookWeaponEvents(playerCombat.Active);
            }
            if (boss != null)
            {
                boss.OnPhaseChanged += OnBossPhaseChanged;
            }
            EnemyBase.AnyEnemyDied += OnEnemyDied;
            if (ExperienceSystem.Instance != null)
            {
                ExperienceSystem.Instance.OnLevelUp += OnLevelUp;
            }
        }

        void UnhookAll()
        {
            if (playerStats != null)
            {
                playerStats.OnDamaged -= OnPlayerDamaged;
            }
            if (playerMovement != null)
            {
                playerMovement.DashStarted -= OnDashOrDodgeStarted;
                playerMovement.DodgeStarted -= OnDashOrDodgeStarted;
            }
            if (playerCombat != null)
            {
                playerCombat.WeaponEquipped -= OnWeaponEquipped;
                UnhookWeaponEvents(_hookedSword, _hookedGun);
            }
            if (boss != null)
            {
                boss.OnPhaseChanged -= OnBossPhaseChanged;
            }
            EnemyBase.AnyEnemyDied -= OnEnemyDied;
            if (ExperienceSystem.Instance != null)
            {
                ExperienceSystem.Instance.OnLevelUp -= OnLevelUp;
            }
        }

        void OnWeaponEquipped(WeaponBase w)
        {
            UnhookWeaponEvents(_hookedSword, _hookedGun);
            HookWeaponEvents(w);
        }

        void HookWeaponEvents(WeaponBase w)
        {
            if (w == null) return;
            if (w is Sword sword)
            {
                _hookedSword = sword;
                sword.OnHitAtPosition += OnSwordHitAtPosition;
            }
            else if (w is Gun gun)
            {
                _hookedGun = gun;
                gun.OnFireAtPosition += OnGunFiredAtPosition;
                gun.OnHitAtPosition += OnGunHitAtPosition;
            }
        }

        void UnhookWeaponEvents(Sword sword, Gun gun)
        {
            if (sword != null) sword.OnHitAtPosition -= OnSwordHitAtPosition;
            if (gun != null)
            {
                gun.OnFireAtPosition -= OnGunFiredAtPosition;
                gun.OnHitAtPosition -= OnGunHitAtPosition;
            }
            _hookedSword = null;
            _hookedGun = null;
        }

        // ------------ Handlers ------------

        void OnSwordHitAtPosition(Vector3 pos) => Spawn(swordHitSparkPrefab, pos);
        void OnGunFiredAtPosition(Vector3 pos) => Spawn(muzzleFlashPrefab, pos);
        void OnGunHitAtPosition(Vector3 pos)   => Spawn(gunHitSparkPrefab, pos);
        void OnEnemyDied(Vector3 pos)          => Spawn(enemyDeathDustPrefab, pos);

        void OnPlayerDamaged(float amount)
        {
            if (playerDamageVfxPrefab == null || playerStats == null) return;
            Spawn(playerDamageVfxPrefab, playerStats.transform.position);
        }

        void OnLevelUp(int newLevel)
        {
            if (levelUpVfxPrefab == null || playerStats == null) return;
            Spawn(levelUpVfxPrefab, playerStats.transform.position);
        }

        void OnDashOrDodgeStarted()
        {
            if (dashTrailPrefab == null || playerStats == null) return;
            // Detach any leftover trail (rapid double-tap) before spawning a new one.
            if (_activeDashTrail != null) Destroy(_activeDashTrail);
            _activeDashTrail = Instantiate(dashTrailPrefab, playerStats.transform.position, Quaternion.identity, playerStats.transform);
            // Auto-destroy after a short window (dash duration + a tail).
            Destroy(_activeDashTrail, 0.6f);
        }

        void OnBossPhaseChanged(BossPhase phase)
        {
            if (phase == BossPhase.Phase3 && bossEnrageAuraPrefab != null && boss != null)
            {
                if (_activeEnrageAura == null)
                {
                    _activeEnrageAura = Instantiate(bossEnrageAuraPrefab, boss.transform.position, Quaternion.identity, boss.transform);
                }
            }
            if (phase == BossPhase.Dead && _activeEnrageAura != null)
            {
                Destroy(_activeEnrageAura);
            }
        }

        // ------------ Helper ------------

        void Spawn(GameObject prefab, Vector3 pos)
        {
            if (prefab == null) return;
            var go = Instantiate(prefab, pos, Quaternion.identity);
            if (defaultLifetime > 0f) Destroy(go, defaultLifetime);
        }
    }
}
