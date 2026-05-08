using DungeonBlade.Combat;
using DungeonBlade.Inventory;
using DungeonBlade.Player;
using UnityEngine;

namespace DungeonBlade.Core.Audio
{
    // Tier 1 audio event subscriber. Drop one of these into a scene, wire the
    // clip slots and Player references in the Inspector, and the matching
    // sounds play on each event. No gameplay code is modified.
    public class AudioBindings : MonoBehaviour
    {
        [Header("Player references")]
        [SerializeField] PlayerStats playerStats;
        [SerializeField] PlayerMovement playerMovement;
        [SerializeField] PlayerCombat playerCombat;
        [SerializeField] InventoryManager inventory;

        [Header("Combat SFX")]
        [SerializeField] AudioClip swordSwing;
        [SerializeField] AudioClip swordHit;
        [SerializeField] AudioClip playerDamaged;
        [SerializeField] AudioClip playerDeath;

        [Header("Movement SFX")]
        [SerializeField] AudioClip dashWhoosh;
        [SerializeField] AudioClip jumpGrunt;

        [Header("UI / Pickup SFX")]
        [SerializeField] AudioClip itemPickup;
        [SerializeField] AudioClip uiClick;

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
        }

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

        // Combat hooks — call these from PlayerCombat via UnityEvents in the
        // Inspector OR via animation events when animations are wired (M9.5).
        public void PlaySwordSwing() => SfxPool.TryPlay(swordSwing, 0.8f, pitchVariation);
        public void PlaySwordHit() => SfxPool.TryPlay(swordHit, 1f, pitchVariation);
        public void PlayUIClick() => SfxPool.TryPlay(uiClick, 0.5f);
    }
}
