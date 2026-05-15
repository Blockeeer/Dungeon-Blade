# Combat System — Dungeon Blade

GDD-aligned combat reference + setup for the new Phase 1 additions (combo HUD, mouse-wheel weapon switch, dash attack). Cross-reference: [DungeonBlade GDD v1.md §2.2](DungeonBlade%20GDD%20v1.md), [GDD_AUDIT.md](GDD_AUDIT.md).

---

## What's wired (current state)

### Melee (Sword) — GDD-aligned

| GDD spec                                            | Implementation                                                                                                                                        |
| --------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------- |
| 3-hit light combo, fast slashes, low damage per hit | `Sword.lightDamage = { 12, 14, 20 }` — 3 stages, chain via `comboChainWindow = 0.35s`                                                                 |
| Heavy attack on hold                                | `StartHeavyHold` / `TryReleaseHeavy`. Charge time `0.45s`, damage `35`, knockback `6`                                                                 |
| **Dash-attack (NEW)**                               | `Sword.StartDashAttack` — fires when Fire pressed during dash i-frames; skips charge requirement; `1.4×` damage and `1.5×` knockback multipliers      |
| Block / Parry                                       | RMB held → block (60% damage reduction). First `0.18s` of block = parry window — incoming attack returns 0 damage. Wired in `PlayerStats.ApplyDamage` |
| Sword skills (§4)                                   | Deferred to Phase 2 per GDD                                                                                                                           |

### Ranged (Gun) — GDD-aligned

| GDD spec                       | Implementation                                                        |
| ------------------------------ | --------------------------------------------------------------------- |
| Primary gun: semi-auto / burst | `Gun.fireRate = 6/sec`, `magSize = 12`, `damage = 18`                 |
| Reload on R                    | `Gun.OnReloadPressed → StartReload(1.6s)`. Auto-reloads when ammo = 0 |
| Aim-down-sights on RMB         | `Gun.SetAim(true/false)`. Spread tightens `3° → 0.4°` while ADS       |
| Recoil patterns                | Per-gun `hipSpreadDegrees` / `adsSpreadDegrees` random spread         |
| Secondary gun                  | **Not wired in Phase 1** — only one gun slot active. Defer to Phase 2 |

### Combo system (GunZ-style)

| GDD spec                          | Implementation                                                                                                                        |
| --------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------- |
| Sword + gun cancel combos         | `PlayerCombat.TryGunZCancel` cancels active Sword when Gun is fired                                                                   |
| K-style butterfly / reload-cancel | Limited — single dash-cancel + sword-cancel-on-shoot are wired. Advanced K-style mechanics deferred                                   |
| **Combo counter HUD (NEW)**       | `ComboCounterHUD` script subscribes to `ComboSystem.OnComboChanged` / `OnComboReset`. Top-center, fades 3s after last hit (GDD §11.1) |
| EXP bonus per 10-hit milestone    | Combo tracking exists in `ComboSystem`. EXP-on-milestone hookup is a separate task in [GDD_AUDIT.md §9.2](GDD_AUDIT.md)               |

### Input mapping (GDD §10.2)

| GDD action          | Binding                                | Status                                     |
| ------------------- | -------------------------------------- | ------------------------------------------ |
| Fire / Light Attack | LMB                                    | ✅                                         |
| Heavy Attack        | Hold LMB + RMB (sword equipped)        | ✅                                         |
| Block / Parry       | RMB (sword equipped)                   | ✅                                         |
| Aim Down Sights     | RMB (gun equipped — context-sensitive) | ✅                                         |
| Reload              | R                                      | ✅                                         |
| **Switch Weapon**   | **Q OR mouse wheel (NEW)**             | ✅                                         |
| Skills 1-3          | E / G / V                              | ⏳ Bindings exist, no skill code (Phase 2) |

---

## Setup steps for the new pieces

### Step 1 — Wire PlayerCombat's `Movement` field for dash-attack

The dash-attack detection requires PlayerCombat to read `PlayerMovement.IsDashing`. New optional field added.

1. Open the scene where Player exists (`3_Dungeon1.unity`, `2_Lobby.unity`).
2. Click the `Player` GameObject.
3. Inspector → **Player Combat** component → find the new **Movement** field.
4. Drag the same `Player` GameObject into the Movement slot (PlayerMovement is on the same GameObject).
5. Save scene.

Repeat for any scene where Player exists.

**If you skip this step:** dash-attacks silently won't trigger (no error, just no boosted swing). The field is optional — leaving it empty preserves the old "always heavy charge" behavior.

---

### Step 2 — Build the Combo Counter HUD

Drop a small UI element under your existing PlayerHUD canvas.

#### 2a — Create the GameObject

1. Open `3_Dungeon1.unity`.
2. Hierarchy → find your `PlayerHUD` (or whichever Canvas hosts the HUD).
3. Right-click the Canvas → **Create Empty** → name `ComboCounter`.
4. Rect Transform:
   - Anchor preset: **top-center** (top row, middle column).
   - Pivot X = `0.5`, Pivot Y = `1`.
   - Pos X = `0`, Pos Y = `-60`, Pos Z = `0`.
   - Width = `400`, Height = `140`.
5. Add Component → **Canvas Group**. Alpha = `0` (script fades it in).

#### 2b — Add the counter number TMP

1. Right-click `ComboCounter` → **UI → Text - TextMeshPro** → name `Counter`.
2. Rect Transform:
   - Anchor preset: top-stretch.
   - Left = `0`, Right = `0`, Pos Y = `-10`, Height = `90`.
3. TMP component:
   - Source Image: leave default.
   - Text = `0` (placeholder).
   - Font Size = `80`.
   - Bold ✅.
   - Alignment = center + middle.
   - Color = bright red `(255, 80, 80, 255)` or gold `(255, 200, 60, 255)` — pick what reads.

#### 2c — Add the label TMP

1. Right-click `ComboCounter` → **UI → Text - TextMeshPro** → name `Label`.
2. Rect Transform:
   - Anchor preset: bottom-stretch.
   - Left = `0`, Right = `0`, Pos Y = `15`, Height = `28`.
3. TMP component:
   - Text = `HITS` (placeholder; script writes `HITS` < 10, `COMBO!` ≥ 10).
   - Font Size = `22`.
   - Bold ✅.
   - Alignment = center + middle.
   - Color = white.

#### 2d — Add the ComboCounterHUD component

1. Click `ComboCounter` in Hierarchy.
2. Add Component → **Combo Counter HUD** (`DungeonBlade.UI.HUD.ComboCounterHUD`).
3. Wire each field by dragging:

   | Inspector field    | Drag from Hierarchy                                                              |
   | ------------------ | -------------------------------------------------------------------------------- |
   | Combo System       | the GameObject that has `ComboSystem` (likely on Player or a `[Combat]` manager) |
   | Root               | `ComboCounter` itself (it has the Canvas Group)                                  |
   | Counter Text       | `Counter`                                                                        |
   | Label Text         | `Label`                                                                          |
   | Fade After Seconds | `3` (per GDD §11.1)                                                              |
   | Fade Duration      | `0.4`                                                                            |
   | Min Combo To Show  | `2` (don't show "1 HITS" — only kicks in at 2+)                                  |
   | Pulse Scale        | `1.25`                                                                           |
   | Pulse Duration     | `0.18`                                                                           |

4. Save scene.

#### 2e — Verify hierarchy

```
Canvas (PlayerHUD)
└── ComboCounter             (Canvas Group, alpha=0)
    ├── Counter (TMP — "0")
    └── Label (TMP — "HITS")
```

#### 2f — Test

1. Press Play → enter Dungeon → kill an enemy.
2. After the 2nd hit on any target chain, ComboCounter fades in showing `2 HITS`.
3. Each hit pulses the counter (scales briefly to 1.25× then back).
4. After 10+ hits, label changes to `10 COMBO!`.
5. Stop hitting for 3 seconds → counter fades out.
6. Get hit / let combo break → fades out immediately.

---

### Step 3 — Verify mouse wheel weapon switch

The input action was extended to bind both **Q** and **Mouse Scroll Y**.

#### Test

1. Press Play.
2. Equip a sword. Press Q → switch to gun. Press Q → switch back.
3. **NEW:** scroll the mouse wheel up or down → cycles weapons same as Q.

If the scroll doesn't work:

- Confirm you have a working `PlayerInputActions.cs` (the code change is on line ~57).
- The Input System processes scroll on any direction — single press = single weapon cycle.

---

### Step 4 — Test the dash-attack

1. Press Play. Equip a sword.
2. Hold W to run forward. Press Shift (or double-tap A/D) to roll/dash.
3. **During the dash burst (0.18s window for sideways dash, 0.35s for roll)**, press Fire (LMB).
4. Sword performs an instant lunging swing — **no charge required**.
5. Damage is `35 × 1.4 = 49` instead of normal heavy `35`.
6. Knockback is boosted `1.5×`.

#### Behavior summary

- **Dash + Fire** = dash-attack (new lunge, instant, boosted)
- **Hold Fire + RMB** = charged heavy (existing — long press)
- **Just Fire** = light combo
- **Fire while gun equipped** = shoot (regardless of dashing)

The dash-attack only triggers if the active weapon is a **Sword** AND the player is currently in dash i-frames (`PlayerMovement.IsDashing == true`).

---

## GDD audit corrections

The original [GDD_AUDIT.md §2.2](GDD_AUDIT.md) had a few overly-pessimistic statuses. Corrections after reading the actual code:

| Item                      | Original status                          | Correct status                                                           |
| ------------------------- | ---------------------------------------- | ------------------------------------------------------------------------ |
| Block / Parry             | 🟡 "Block exists, parry not implemented" | ✅ Already wired (parryWindow = 0.18s, damage zeroed on parry)           |
| Aim-down-sights on RMB    | 🟡 "RMB used for block instead"          | ✅ Same RMB context-switches: Sword equipped → block, Gun equipped → ADS |
| Dash-attack               | 🟡 "Not explicit, needs verification"    | ✅ Implemented in this commit                                            |
| Combo counter HUD         | ⏳ "Not implemented"                     | ✅ Implemented in this commit (when wired per Step 2 above)              |
| Mouse wheel weapon switch | (not flagged)                            | ✅ Added in this commit                                                  |

GDD_AUDIT.md will be updated to reflect these corrections.

---

## What's intentionally NOT in Phase 1

Per GDD scope:

- **Skill system (E/G/V)** — GDD §5 explicitly defers skill-shop to Phase 2. Input bindings exist; no skill scripts.
- **Secondary gun slot** — only one gun active at a time. Adding shotgun/grenade-launcher is Phase 2 polish.
- **Advanced K-style moves** — butterfly / multi-cancel chains are mentioned in GDD but flagged as "advanced." Phase 1 ships with single-cancel only.
- **EXP bonus per 10-hit combo milestone** — combo tracking exists, but the EXP grant is wired separately. See [GDD_AUDIT.md §9.2](GDD_AUDIT.md).

---

## Tuning notes

If combat feels off, tweak in the Inspector:

| Feel                                         | What to change                                                             |
| -------------------------------------------- | -------------------------------------------------------------------------- |
| Dash-attack feels weak                       | Bump `Sword.dashAttackDamageMult` from 1.4 to 1.6+                         |
| Dash-attack window too tight                 | Increase `PlayerMovement.dashDuration` (currently 0.18s for sideways dash) |
| Heavy attack charge takes too long           | Lower `Sword.heavyChargeTime` from 0.45 to 0.30                            |
| Parry window too lenient                     | Lower `Sword.parryWindow` from 0.18 to 0.12 (Sekiro-tight)                 |
| Combo counter shows for trivial 2-hit combos | Bump `ComboCounterHUD.minComboToShow` from 2 to 3                          |
| Combo counter persists too long              | Lower `ComboCounterHUD.fadeAfterSeconds` from 3 to 2                       |

---

## Related docs

- [DungeonBlade GDD v1.md §2.2](DungeonBlade%20GDD%20v1.md) — combat spec
- [GDD_AUDIT.md](GDD_AUDIT.md) — full GDD compliance audit
- [PLAYER_HUD_SETUP.md](PLAYER_HUD_SETUP.md) — main HUD where ComboCounter lives
- [M9_TODO.md](M9_TODO.md) — remaining polish queue
