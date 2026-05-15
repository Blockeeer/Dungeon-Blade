using DungeonBlade.Bank;
using DungeonBlade.Boss;
using DungeonBlade.Combat;
using DungeonBlade.Dungeon;
using DungeonBlade.Inventory;
using DungeonBlade.Player;
using UnityEngine;

namespace DungeonBlade.Core.Audio
{
    // Tier 1-4 audio event subscriber. Drop one of these into a scene, wire the
    // clip slots and Player/Boss/Wallet references in the Inspector, and the
    // matching sounds play on each event. No gameplay code is modified.
    public class AudioBindings : MonoBehaviour
    {
        [Header("Player references")]
        [SerializeField] PlayerStats playerStats;
        [SerializeField] PlayerMovement playerMovement;
        [SerializeField] PlayerCombat playerCombat;
        [SerializeField] InventoryManager inventory;
        [Tooltip("Optional. If wired, plays boss roar on phase transitions per GDD §12.")]
        [SerializeField] BossBase boss;
        [Tooltip("Optional. If wired, plays gold pickup sound on PlayerWallet.OnGoldChanged (only when gold increases).")]
        [SerializeField] PlayerWallet wallet;

        [Header("Combat SFX")]
        [SerializeField] AudioClip swordSwing;
        [SerializeField] AudioClip swordHit;
        [SerializeField] AudioClip playerDamaged;
        [SerializeField] AudioClip playerDeath;
        [Tooltip("GDD §12 — distinct sound when an attack is parried (different from a normal hit).")]
        [SerializeField] AudioClip swordParry;

        [Header("Movement SFX")]
        [SerializeField] AudioClip dashWhoosh;
        [SerializeField] AudioClip jumpGrunt;

        [Header("UI / Pickup SFX")]
        [SerializeField] AudioClip itemPickup;
        [SerializeField] AudioClip uiClick;
        [Tooltip("GDD §12 — coin chime, separate from generic item pickup.")]
        [SerializeField] AudioClip goldPickup;
        [Tooltip("GDD §12 — chime when a new checkpoint activates.")]
        [SerializeField] AudioClip checkpointReached;

        [Header("Boss SFX")]
        [Tooltip("GDD §12 — boss roar at phase transitions. Plays on Phase2 and Phase3 entry.")]
        [SerializeField] AudioClip bossRoar;

        [Header("Tuning")]
        [Tooltip("Random pitch variation to keep repeated SFX from sounding identical (0 = none, 0.1 = ±10%)")]
        [Range(0f, 0.3f)]
        [SerializeField] float pitchVariation = 0.08f;

        [Tooltip("Window after an attack press during which a hit can suppress the miss whoosh. Slightly longer than the sword's hit-frame delay.")]
        [SerializeField] float attackHitWindow = 0.30f;

        float _pendingSwingExpiresAt = -1f;

        void OnEnable()
        {
            if (playerStats != null)
            {
                playerStats.OnDamaged += OnPlayerDamaged;
                playerStats.OnDeath += OnPlayerDeath;
            }
            if (playerMovement != null)
            {
                playerMovement.DashStarted += OnDash;
                playerMovement.DodgeStarted += OnDash;
                playerMovement.Jumped += OnJump;
            }
            if (playerCombat != null)
            {
                playerCombat.AttackPerformed += OnAttackPerformed;
                playerCombat.WeaponEquipped += OnWeaponEquipped;
                // Hook into the currently-equipped weapon's hit event right away
                // (in case AudioBindings spawned after PlayerCombat.Start).
                HookWeaponHit(playerCombat.Active);
            }
            if (inventory != null)
            {
                inventory.OnInventoryChanged += OnInventoryChanged;
            }
            // Fall back to FindObjectOfType so boss roar works even if the
            // Boss field wasn't wired (one boss per scene by design).
            if (boss == null) boss = FindObjectOfType<BossBase>();
            if (boss != null) boss.OnPhaseChanged += OnBossPhaseChanged;

            // Prefer the Inspector reference, fall back to the singleton so
            // the gold sound works even if the Wallet field wasn't wired.
            if (wallet == null) wallet = PlayerWallet.Instance;
            if (wallet != null)
            {
                wallet.OnGoldChanged += OnGoldChanged;
                _lastGold = wallet.Gold;
            }
            if (playerStats != null)
            {
                // Parry event fires when an incoming attack is fully absorbed
                // during the parry window. PlayerStats.OnParry exposed below.
                playerStats.OnParry += OnParry;
            }
            // Static event — fires for any Checkpoint activation in any scene.
            Checkpoint.AnyCheckpointActivated += OnCheckpointActivated;
        }

        // Some references (Boss, PlayerWallet) may not have Awake'd yet when
        // OnEnable runs. Start() runs after all Awakes — retry late subscriptions.
        void Start()
        {
            if (wallet == null && PlayerWallet.Instance != null)
            {
                wallet = PlayerWallet.Instance;
                wallet.OnGoldChanged += OnGoldChanged;
                _lastGold = wallet.Gold;
            }
            if (boss == null)
            {
                boss = FindObjectOfType<BossBase>();
                if (boss != null) boss.OnPhaseChanged += OnBossPhaseChanged;
            }
        }

        void OnDisable()
        {
            if (playerStats != null)
            {
                playerStats.OnDamaged -= OnPlayerDamaged;
                playerStats.OnDeath -= OnPlayerDeath;
                playerStats.OnParry -= OnParry;
            }
            if (playerMovement != null)
            {
                playerMovement.DashStarted -= OnDash;
                playerMovement.DodgeStarted -= OnDash;
                playerMovement.Jumped -= OnJump;
            }
            if (playerCombat != null)
            {
                playerCombat.AttackPerformed -= OnAttackPerformed;
                playerCombat.WeaponEquipped -= OnWeaponEquipped;
                UnhookWeaponHit(playerCombat.Active);
            }
            if (inventory != null)
            {
                inventory.OnInventoryChanged -= OnInventoryChanged;
            }
            if (boss != null)
            {
                boss.OnPhaseChanged -= OnBossPhaseChanged;
            }
            if (wallet != null)
            {
                wallet.OnGoldChanged -= OnGoldChanged;
            }
            Checkpoint.AnyCheckpointActivated -= OnCheckpointActivated;
        }

        int _lastGold;

        void OnCheckpointActivated(Checkpoint cp) => SfxPool.TryPlay(checkpointReached, 0.8f);

        WeaponBase _hookedWeapon;

        void OnWeaponEquipped(WeaponBase w)
        {
            UnhookWeaponHit(_hookedWeapon);
            HookWeaponHit(w);
        }

        void HookWeaponHit(WeaponBase w)
        {
            if (w == null) return;
            _hookedWeapon = w;
            if (w is Sword sword) sword.OnHit += OnSwordHit;
            else if (w is Gun gun) gun.OnHitDamageable += OnGunHit;
        }

        void UnhookWeaponHit(WeaponBase w)
        {
            if (w == null) return;
            if (w is Sword sword) sword.OnHit -= OnSwordHit;
            else if (w is Gun gun) gun.OnHitDamageable -= OnGunHit;
            if (_hookedWeapon == w) _hookedWeapon = null;
        }

        void OnAttackPerformed()
        {
            // Don't play the whoosh immediately. Mark a pending swing — if a
            // hit fires within the window the swing is suppressed (the hit
            // sound replaces it). Otherwise Update plays the whoosh on expiry.
            _pendingSwingExpiresAt = Time.time + attackHitWindow;
        }

        void OnSwordHit(int comboIndex, float damage)
        {
            _pendingSwingExpiresAt = -1f;  // hit landed → no whoosh
            SfxPool.TryPlay(swordHit, 1f, pitchVariation);
        }

        void OnGunHit()
        {
            _pendingSwingExpiresAt = -1f;
            SfxPool.TryPlay(swordHit, 1f, pitchVariation);
        }

        void Update()
        {
            if (_pendingSwingExpiresAt > 0f && Time.time >= _pendingSwingExpiresAt)
            {
                _pendingSwingExpiresAt = -1f;
                SfxPool.TryPlay(swordSwing, 0.8f, pitchVariation);
            }
        }

        void OnPlayerDamaged(float amount) => SfxPool.TryPlay(playerDamaged, 1f, pitchVariation);
        void OnPlayerDeath() => SfxPool.TryPlay(playerDeath, 1f);
        void OnDash() => SfxPool.TryPlay(dashWhoosh, 0.7f, pitchVariation);
        void OnJump() => SfxPool.TryPlay(jumpGrunt, 0.6f, pitchVariation);
        void OnInventoryChanged() => SfxPool.TryPlay(itemPickup, 0.7f, pitchVariation);
        void OnParry() => SfxPool.TryPlay(swordParry, 1f, pitchVariation);

        void OnBossPhaseChanged(BossPhase phase)
        {
            // Roar on the two threshold transitions only — not on Phase1 entry
            // (which is silent / just the fight starting) or Dead.
            if (phase == BossPhase.Phase2 || phase == BossPhase.Phase3)
            {
                SfxPool.TryPlay(bossRoar, 1f);
            }
        }

        void OnGoldChanged(int newGold)
        {
            // Only play coin chime on gain, not on spend.
            if (newGold > _lastGold) SfxPool.TryPlay(goldPickup, 0.7f, pitchVariation);
            _lastGold = newGold;
        }

        // Public method called from Checkpoint when a new one activates.
        // Subscribing to a per-Checkpoint event would require a Checkpoint
        // event change; calling this method directly is simpler.
        public void PlayCheckpointReached() => SfxPool.TryPlay(checkpointReached, 0.8f);

        // Combat hooks — call these from PlayerCombat via UnityEvents in the
        // Inspector OR via animation events when animations are wired (M9.5).
        public void PlaySwordSwing() => SfxPool.TryPlay(swordSwing, 0.8f, pitchVariation);
        public void PlaySwordHit() => SfxPool.TryPlay(swordHit, 1f, pitchVariation);
        public void PlayUIClick() => SfxPool.TryPlay(uiClick, 0.5f);
    }
}
