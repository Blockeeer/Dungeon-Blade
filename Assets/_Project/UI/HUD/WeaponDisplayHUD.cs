using DungeonBlade.Combat;
using DungeonBlade.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonBlade.UI.HUD
{
    // Bottom-right HUD panel per Dungeon Blade GDD §11.1:
    // shows the currently equipped weapon's icon, name, and either
    // ammo (for Gun) or "Sword Ready" / combo index (for Sword).
    public class WeaponDisplayHUD : MonoBehaviour
    {
        [Header("Source")]
        [SerializeField] PlayerCombat combat;

        [Header("UI refs")]
        [SerializeField] Image weaponIcon;
        [SerializeField] TMP_Text weaponName;
        [SerializeField] TMP_Text ammoOrDurability;

        [Header("Optional per-weapon icons")]
        [Tooltip("Optional icon shown for any Sword weapon. Leave null to use the weapon's own icon if available, otherwise hide the icon.")]
        [SerializeField] Sprite swordIcon;
        [SerializeField] Sprite gunIcon;

        WeaponBase _hookedWeapon;
        Gun _hookedGun;

        void OnEnable()
        {
            if (combat != null)
            {
                combat.WeaponEquipped += OnWeaponEquipped;
                if (combat.Active != null) OnWeaponEquipped(combat.Active);
            }
        }

        void OnDisable()
        {
            if (combat != null) combat.WeaponEquipped -= OnWeaponEquipped;
            UnhookGun();
        }

        void OnWeaponEquipped(WeaponBase w)
        {
            UnhookGun();
            _hookedWeapon = w;
            if (w == null)
            {
                ClearDisplay();
                return;
            }

            if (weaponName != null) weaponName.text = w.DisplayName;

            if (weaponIcon != null)
            {
                Sprite s = w.Kind == WeaponKind.Ranged ? gunIcon : swordIcon;
                weaponIcon.sprite = s;
                weaponIcon.enabled = s != null;
            }

            if (w is Gun gun)
            {
                _hookedGun = gun;
                gun.OnAmmoChanged += OnAmmoChanged;
                // Force-refresh once so the HUD shows current ammo even
                // before any shot/reload event fires.
                RefreshGunAmmo(gun);
            }
            else
            {
                if (ammoOrDurability != null) ammoOrDurability.text = "Sword Ready";
            }
        }

        void OnAmmoChanged(int current, int max)
        {
            if (ammoOrDurability != null) ammoOrDurability.text = $"{current} / {max}";
        }

        void RefreshGunAmmo(Gun gun)
        {
            // Gun stores Ammo but not max publicly — use reflection-free approach:
            // re-fire its own event by checking Ammo and inferring max from
            // the format we want. Simplest: just show "Ammo: X".
            if (ammoOrDurability != null) ammoOrDurability.text = $"Ammo: {gun.Ammo}";
        }

        void UnhookGun()
        {
            if (_hookedGun != null) _hookedGun.OnAmmoChanged -= OnAmmoChanged;
            _hookedGun = null;
        }

        void ClearDisplay()
        {
            if (weaponName != null) weaponName.text = "—";
            if (ammoOrDurability != null) ammoOrDurability.text = "";
            if (weaponIcon != null) weaponIcon.enabled = false;
        }
    }
}
