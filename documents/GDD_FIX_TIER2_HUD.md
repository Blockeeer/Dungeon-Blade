# GDD Fix Tier 2 — HUD Layout Per §11.1

**Effort:** ~2-3 hours
**Goal:** Rebuild Player HUD positioning to match `DungeonBlade GDD v1.md §11.1` exactly.

This tier rearranges the Player HUD from the current **top-left** layout into the GDD-mandated layout.

---

## Decision point (read first)

The current HUD has HP/Stamina/EXP in top-left. The GDD says:

- HP bar **bottom-left**
- Stamina bar **below HP**, color **blue/cyan**
- Equipped weapons **bottom-right**
- Hotbar **bottom-center**

**You previously preferred top-left.** Before doing this tier, decide:

- ✅ **"Follow GDD strictly"** → proceed with this doc, rebuild HUD layout.
- ✏️ **"Keep top-left, document deviation"** → skip this tier entirely. Just add a note in `GDD_AUDIT.md` §11.1: "HUD position deviates from GDD by design choice — top-left preferred over bottom-left."

The rest of this doc assumes you chose to follow GDD.

---

## GDD §11.1 reference

| Element                 | Position                                 | Color                                                | Notes                               |
| ----------------------- | ---------------------------------------- | ---------------------------------------------------- | ----------------------------------- |
| HP Bar                  | bottom-left                              | red, **flash red when <25% HP**                      | numeric value shown (e.g. "75/100") |
| Stamina Bar             | below HP (bottom-left, stacked under HP) | **blue/cyan**                                        | depletes on dash/wall run/heavy     |
| Equipped Weapons        | bottom-right                             | (sprite + ammo count for guns, durability for sword) | shows main weapon                   |
| Hotbar                  | bottom-center                            | (4 slots with item icons + keybind labels)           | 4 slots only                        |
| Active Skills           | above hotbar                             | (icons with cooldown overlays)                       | DEFERRED — skills are Phase 2       |
| Combo Counter           | top-center                               | (fades after 3s)                                     | ✅ already done in Tier 0           |
| Mini-map                | top-right                                | —                                                    | DEFERRED — Phase 2                  |
| Checkpoint Notification | top-center brief banner                  | —                                                    | optional polish                     |

---

## Step 1 — Plan the rearrangement

Current `PlayerHUD` GameObject is anchored top-left and contains:

- `BarsRoot` (Lv label, HealthRow, StaminaRow, ExperienceRow)
- `GoldText` (top-right — keep this!)

After Tier 2:

- HP Row → moves to **bottom-left**, red color
- Stamina Row → **below HP**, blue/cyan
- EXP Row → optional, can stay top-left OR move under Lv label somewhere
- Lv label → stays top-left or move beside HP
- GoldText → stays top-right (unchanged)
- **NEW:** Weapon display → bottom-right with ammo/durability
- **NEW:** Hotbar → bottom-center (if not already there)

---

## Step 2 — Move BarsRoot to bottom-left

### 2a — Re-anchor

1. Open `3_Dungeon1.unity`.
2. In the Hierarchy, find `BarsRoot` (under `Canvas → PlayerHUD`).
3. Click it.
4. Inspector → Rect Transform:
   - **Anchor preset:** click the anchor preset square (top-left of Rect Transform), hold **Shift+Alt**, click the **bottom-left preset** (bottom row, left column — single corner, not stretch).
   - This sets anchors AND pivot in one click.
5. Set the position values:

   | Field       | Value                           |
   | ----------- | ------------------------------- |
   | Pos X       | `20` (20px right of left edge)  |
   | Pos Y       | `20` (20px up from bottom edge) |
   | Pos Z       | `0`                             |
   | Width       | `400`                           |
   | Height      | `120`                           |
   | Pivot X     | `0`                             |
   | Pivot Y     | `0`                             |
   | Anchors Min | `(0, 0)`                        |
   | Anchors Max | `(0, 0)`                        |

6. Update the **Vertical Layout Group** Child Alignment:
   - Click `BarsRoot` → Vertical Layout Group component.
   - **Child Alignment** = `Lower Left` (so bars stack from the bottom upward).

### 2b — Reorder children

Bars stack top-down in the Hierarchy. For bottom-anchored layout you want this visual order (top to bottom on screen):

```
Lv 1
HP   [████████] 100/100   ← red
STAM [████████] 100/100   ← blue/cyan (NEW color)
XP   [████    ] 50/100    ← cyan/gold
```

If the Lv label and bars look wrong after the re-anchor, drag children in the Hierarchy to reorder. The Vertical Layout Group respects sibling order.

---

## Step 3 — Recolor Stamina to blue/cyan

GDD says stamina bar is **blue/cyan**, currently green.

1. Hierarchy → expand `BarsRoot → StaminaRow → BarFill`.
2. Click `BarFill`.
3. Inspector → Image component → **Color** swatch.
4. Set RGB to **cyan**: `(80, 200, 240, 255)` — same color the EXP bar currently uses.
5. **EXP bar color change:** since stamina is now cyan, change EXP to a different color so they don't blend.
   - Hierarchy → `BarsRoot/ExperienceRow/BarFill` → Color = **gold/yellow** `(240, 200, 60, 255)`.

Updated palette:

- HP = red `(220, 40, 40, 255)` (unchanged)
- Stamina = cyan `(80, 200, 240, 255)` (was green)
- EXP = gold `(240, 200, 60, 255)` (was cyan)

---

## Step 4 — Add HP low-health flash (below 25%)

GDD says HP bar **flashes red when below 25%**.

### 4a — Extend PlayerHUD.cs to detect low health

Open `Assets/_Project/UI/HUD/PlayerHUD.cs`. Find the `OnHealthChanged` method:

```csharp
void OnHealthChanged(float current, float max)
{
    if (healthFill != null) healthFill.fillAmount = max > 0f ? Mathf.Clamp01(current / max) : 0f;
    if (healthText != null) healthText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
}
```

Tell me when ready and I'll add the low-health flash logic. It needs:

- A pulse coroutine that runs while `current/max < 0.25`
- Flashes the BarFill color between red and bright-red over ~0.5s loop
- Stops when HP rises above 25% or hits 0

### 4b — Or skip code change (acceptable)

Phase 1 can ship without the flash effect. HP bar is still visible; players notice the number directly. Mark this as a "nice-to-have" in GDD_AUDIT.

---

## Step 5 — Add bottom-right Weapon Display

GDD says equipped weapon (icon + ammo for guns, durability for swords) goes bottom-right.

### 5a — Create the GameObject

1. Hierarchy → right-click `PlayerHUD` → Create Empty → name `WeaponDisplay`.
2. Rect Transform:
   - Anchor preset: **bottom-right** (Shift+Alt + click bottom-right preset).
   - Pos X = `-20`, Pos Y = `20`.
   - Pivot X = `1`, Pivot Y = `0`.
   - Width = `240`, Height = `80`.

### 5b — Build the children

**Target layout** (parent is 240 wide × 80 tall):

```
WeaponDisplay (240 × 80)
┌──────────────────────────────────┐
│ ┌────┐                           │
│ │ICON│  Iron Sword               │
│ │60×60│  Ammo: 12 / 12           │
│ └────┘                           │
└──────────────────────────────────┘
  ↑    ↑                          ↑
  10px 10px gap                   right edge
```

The icon sits left, texts stack on the right side. They must not overlap.

#### 5b.1 — Add the icon

1. Right-click `WeaponDisplay` → **UI → Image** → name `WeaponIcon`.
2. Rect Transform — set every field explicitly:

   | Field         | Value                                     | Why                                     |
   | ------------- | ----------------------------------------- | --------------------------------------- |
   | Anchor preset | **middle-left** (middle row, left column) | Anchored to parent's left edge          |
   | Pivot X       | `0`                                       | Icon's own left edge = the anchor point |
   | Pivot Y       | `0.5`                                     | Vertically centered                     |
   | Pos X         | `10`                                      | 10px padding from parent's left edge    |
   | Pos Y         | `0`                                       | Centered vertically                     |
   | Width         | `60`                                      | Icon size                               |
   | Height        | `60`                                      | Icon size                               |

3. Image component:
   - **Source Image** → click circle icon → pick `UISprite` (placeholder; replace with real weapon icon later)
   - **Color** = white `(255, 255, 255, 255)`
   - **Image Type** = `Simple`
   - **Preserve Aspect** = ✅ (so swapping rectangular icons later doesn't squash)

#### 5b.2 — Add the weapon name (top row, right of icon)

1. Right-click `WeaponDisplay` → **UI → Text - TextMeshPro** → name `WeaponName`.
2. Rect Transform — these values place the text to the right of the icon, near the top:

   | Field         | Value                                 | Why                                      |
   | ------------- | ------------------------------------- | ---------------------------------------- |
   | Anchor preset | **top-right** (top row, right column) | Anchored to parent's top-right corner    |
   | Pivot X       | `1`                                   | Text's own right edge = the anchor point |
   | Pivot Y       | `1`                                   | Text's own top edge = the anchor point   |
   | Pos X         | `-10`                                 | 10px in from parent's right edge         |
   | Pos Y         | `-5`                                  | 5px down from parent's top edge          |
   | Width         | `160`                                 | Leaves ~80px on the left for icon + gap  |
   | Height        | `28`                                  | Font + tiny padding                      |

3. TextMeshPro - Text:
   - **Text** = `Iron Sword` (placeholder; script overwrites at runtime)
   - **Font Size** = `20`
   - **Font Style** = Bold ✅
   - **Alignment** = right + middle (click the right-align icon, then middle-vertical)
   - **Color** = white `(255, 255, 255, 255)`
   - **Raycast Target** = ❌ (HUD elements don't need to block UI clicks)

#### 5b.3 — Add the ammo / durability text (bottom row, right of icon)

1. Right-click `WeaponDisplay` → **UI → Text - TextMeshPro** → name `AmmoOrDurability`.
2. Rect Transform:

   | Field         | Value                                       | Why                                      |
   | ------------- | ------------------------------------------- | ---------------------------------------- |
   | Anchor preset | **bottom-right** (bottom row, right column) | Anchored to parent's bottom-right corner |
   | Pivot X       | `1`                                         | Right-aligned                            |
   | Pivot Y       | `0`                                         | Text's own bottom = anchor point         |
   | Pos X         | `-10`                                       | 10px in from parent's right edge         |
   | Pos Y         | `5`                                         | 5px up from parent's bottom edge         |
   | Width         | `160`                                       | Same width as WeaponName so they align   |
   | Height        | `28`                                        | Font + padding                           |

3. TextMeshPro - Text:
   - **Text** = `Ammo: 12 / 12` (placeholder)
   - **Font Size** = `18`
   - **Alignment** = right + middle
   - **Color** = gold `(255, 220, 100, 255)`
   - **Raycast Target** = ❌

#### 5b.4 — Verify hierarchy

The Inspector should show this structure with no overlap in the Scene view:

```
WeaponDisplay  (240 × 80, bottom-right of Canvas)
├── WeaponIcon         (60 × 60, hugs left, centered vertically)
├── WeaponName         (160 × 28, top-right, right-aligned)
└── AmmoOrDurability   (160 × 28, bottom-right, right-aligned)
```

**Math check:** WeaponName/AmmoOrDurability sit at x = -10 to -170 in parent space (right-anchored, 160 wide). WeaponIcon spans x = 10 to 70 in parent space. Gap between icon's right edge (70) and text's left edge (240 - 170 = 70) = **0px clean** — they sit edge-to-edge with no overlap.

If you want a visible gap between icon and text, lower the text Width to `150` (gives 10px clear space).

#### 5b.5 — Switch to Game view to see the result

1. Top of the Editor → click the **Game** tab.
2. The WeaponDisplay should render bottom-right with the icon on the left and two text lines on the right.
3. If it still looks wrong, the most likely cause is one of:
   - **Pivot wasn't set to match anchor side** (e.g. anchor=top-right but pivot=(0.5, 0.5) → text rect sticks out)
   - **Width is wider than 160** → overlaps the icon
   - **Pos X is 0 instead of -10** for right-anchored texts → texts clip off the right edge

### Common layout pitfalls

| Symptom                               | Fix                                                                                                           |
| ------------------------------------- | ------------------------------------------------------------------------------------------------------------- |
| Icon and text overlap                 | WeaponName/AmmoOrDurability Width is too wide. Drop from 170 → 160 (or 150 for a visible gap)                 |
| Icon falls outside parent on the left | WeaponIcon Pivot X = `0` (not `0.5`) and Pos X = `10` (not `0`)                                               |
| Text wraps to two lines               | Text Width too narrow. Bump Width to 180 and reduce icon Pos X to `5`                                         |
| Text right-edge clipped off screen    | Pos X is `0` instead of `-10` for right-anchored elements (negative = inward from anchor)                     |
| Whole panel is in wrong corner        | Re-check parent `WeaponDisplay` anchor preset is bottom-right with Pivot (1, 0) and Pos (-20, 20) per Step 5a |

### 5c — Create the script

Drop this script at `Assets/_Project/UI/HUD/WeaponDisplayHUD.cs`:

**Tell me when ready and I'll write the script.** It needs to:

- Subscribe to `PlayerCombat.WeaponEquipped` event
- Read current weapon's icon, name, ammo (for Gun) or durability (for Sword)
- Update the 3 UI elements live as you fire/reload/swap weapons

For now, just build the visuals (steps 5a/5b). Wiring comes after the script.

---

## Step 6 — Hotbar to bottom-center

If your hotbar is already bottom-center, **skip this step**. Verify first:

1. Find your hotbar GameObject (likely under Canvas, named `Hotbar` or `HotbarParent`).
2. Confirm its anchor is bottom-center. If yes → ✅ done. If no → re-anchor below.

### Re-anchor to bottom-center (if needed)

1. Click the Hotbar GameObject.
2. Anchor preset: **bottom-center** (bottom row, middle column — Shift+Alt+click).
3. Pos X = `0`, Pos Y = `20`.
4. Pivot X = `0.5`, Pivot Y = `0`.
5. Width matches your hotbar size, Height matches.

---

## Step 7 — Trim Hotbar from 6 to 4 slots (GDD §7.2 + §10.2)

GDD specifies exactly **4 hotbar slots** (keys 1-4). Your current hotbar has 6.

**Decision:**

- ✅ Trim to 4 slots (strict GDD compliance)
- ✏️ Keep 6 slots (more user-friendly, document deviation)

### To trim to 4

1. Hierarchy → expand the hotbar parent.
2. Delete slot widgets for slots 5 and 6 (`Slot_5`, `Slot_6`, or similar names).
3. Open `Assets/_Project/Core/Input/PlayerInputActions.cs`.
4. Comment out or remove the lines:
   ```csharp
   Hotbar5 = Map.AddAction("Hotbar5", ...);
   Hotbar6 = Map.AddAction("Hotbar6", ...);
   ```
5. Open any script that reads Hotbar5/Hotbar6 (likely `HotbarController` or `InventoryUI`). Tell me if you go this route — I'll find and patch the references.

### To keep 6

Document the deviation in `GDD_AUDIT.md` §7.2: "Hotbar has 6 slots instead of GDD's 4 — extended for testing/QoL convenience."

---

## Step 8 — Verify hierarchy after rebuild

Expected final state:

```
Canvas
└── PlayerHUD                    (stretch full)
    ├── BarsRoot                 (bottom-left, anchored)
    │   ├── LevelText
    │   ├── HealthRow            (red bar)
    │   ├── StaminaRow           (cyan bar — NEW color)
    │   └── ExperienceRow        (gold bar — NEW color)
    ├── GoldText                 (top-right, unchanged)
    ├── WeaponDisplay            (bottom-right, NEW)
    │   ├── WeaponIcon
    │   ├── WeaponName
    │   └── AmmoOrDurability
    └── (Hotbar separate or here, bottom-center)

Canvas
└── ComboCounter                 (top-center, fades — from Tier 0)
```

---

## Step 9 — Save and test

1. Ctrl+S to save the scene.
2. Press Play.
3. **Verify visually:**
   - HP bar bottom-left, red, with `100/100` text
   - Stamina cyan, below HP
   - EXP gold, below stamina
   - Gold readout top-right (unchanged)
   - Weapon display bottom-right (placeholder icon + name + ammo)
4. **Test interactions:**
   - Take damage → HP bar shrinks
   - Dash → stamina bar shrinks
   - Switch weapon (Q or scroll) → weapon name should update (after Step 5c script is wired)

---

## Common issues

- **Bars look stretched or in the middle of screen** → BarsRoot's anchors aren't bottom-left. Re-do Step 2a.
- **Bars stack upward off-screen** → Vertical Layout Group Child Alignment isn't `Lower Left`. Set it.
- **HP and Stamina overlap** → BarsRoot is too short. Increase Height to 120.
- **Stamina still green** → BarFill on StaminaRow wasn't recolored. Check the right child.

---

## When done — mark Tier 2 complete

After all checks pass:

1. Update `GDD_AUDIT.md` §11.1 — change HP/Stamina position rows from 🔴 to ✅.
2. Tell me — we proceed to **Tier 3: Structural Deviations** → `GDD_FIX_TIER3_STRUCTURE.md`.

---

## Reference

- [DungeonBlade GDD v1.md](DungeonBlade%20GDD%20v1.md) §11.1
- [GDD_AUDIT.md](GDD_AUDIT.md) §11.1
- [PLAYER_HUD_SETUP.md](PLAYER_HUD_SETUP.md) — original HUD setup doc (now outdated for position; colors still valid)
