# Player HUD — Unity Setup

How to wire `PlayerHUD.cs` into the Dungeon scene. Adds bottom-left HP/Stamina/EXP bars and a top-right gold readout.

---

## Layout overview

```
Top-left corner                                    Top-right corner
Lv 5                                                       [1234 G]
━━━━━━━━━━━━━━ HP   100/100
━━━━━━━━━━━━━━ STAM 80/80
━━━━━━━━━━━━━━ XP   42/200
```

All elements live under the existing Canvas in `3_Dungeon1.unity`.

---

## Step 1 — Create the HUD root

1. Open `3_Dungeon1.unity`.
2. Right-click your existing `Canvas` → **Create Empty** → name `PlayerHUD`.
3. Rect Transform: Anchor preset **stretch all** (Alt+click bottom-right preset). Left/Top/Right/Bottom = `0`.
4. This is just a container — no Image or Canvas Group needed.

---

## Step 2 — Build the top-left bars container

1. Right-click `PlayerHUD` → Create Empty → name `BarsRoot`.
2. Rect Transform:
   - Anchor preset: **top-left** (top row, left column — single corner, not stretch).
   - Pivot: X = `0`, Y = `1`.
   - Pos X = `20`, Pos Y = `-20` (negative because top-anchored — Y points down).
   - Width = `400`, Height = `120`.
3. Add Component → **Vertical Layout Group**:
   - Padding `(0, 0, 0, 0)`
   - Spacing `8`
   - Child Alignment `Upper Left`
   - Child Force Expand: Width ✅, Height ❌
   - Child Controls Size: Width ✅, Height ❌

---

## Step 3 — Add the level label

1. Right-click `BarsRoot` → **UI → Text - TextMeshPro** → name `LevelText`.
2. Add Component → **Layout Element** → Preferred Height `28`.
3. TMP component:
   - Text = `Lv 1` (placeholder — script overwrites)
   - Font Size `22`
   - Bold ✅
   - Alignment center+left
   - Color `(255, 220, 100, 255)` (gold)

---

## Step 4 — Build one bar row (template)

You'll build 3 rows: Health, Stamina, Experience. Same pattern for each. Build the first one carefully, then duplicate.

### 4a — Create `HealthRow`

1. Right-click `BarsRoot` → Create Empty → name `HealthRow`.
2. Add Component → **Layout Element** → Preferred Height `24`.

### 4b — Add the bar label

1. Right-click `HealthRow` → **UI → Text - TextMeshPro** → name `Label`.
2. Rect Transform:
   - Anchor preset: middle-left.
   - Pivot: X = `0`, Y = `0.5`.
   - Pos X = `0`, Pos Y = `0`.
   - Width = `60`, Height = `24`.
3. TMP component: Text = `HP`, Font Size `16`, Bold ✅, Alignment middle-left, Color white.

### 4c — Add the bar background

1. Right-click `HealthRow` → **UI → Image** → name `BarBackground`.
2. Rect Transform:
   - Anchor preset: middle-stretch.
   - Pivot: X = `0.5`, Y = `0.5`.
   - Left = `70`, Right = `100`.
   - Pos Y = `0`, Height = `16`.
3. Image component (set every field):
   - **Source Image** → click circle icon → pick `UISprite`.
   - **Color** = dark `(40, 40, 40, 200)`.
   - **Material** = `None` (default — leave it).
   - **Raycast Target** = ✅ (default — leave checked).
   - **Maskable** = ✅ (default — leave checked).
   - **Image Type** = `Simple`.
   - **Use Sprite Mesh** = ❌ (default — leave unchecked).
   - **Preserve Aspect** = ❌ (must be off so it stretches).

### 4d — Add the bar fill (the colored part)

1. Right-click `HealthRow` → **UI → Image** → name `BarFill`.
2. Rect Transform:
   - Anchor preset: middle-stretch (same as BarBackground).
   - Pivot: X = `0.5`, Y = `0.5`.
   - Left = `70`, Right = `100`, Pos Y = `0`, Height = `16`.
3. Image component (set every field):
   - **Source Image** → click circle icon → pick `UISprite`.
   - **Color** = red `(220, 40, 40, 255)`.
   - **Material** = `None` (default).
   - **Raycast Target** = ✅ (default).
   - **Maskable** = ✅ (default).
   - **Image Type** = `Filled` (changed from default Simple — this is the key setting that makes the bar shrink).
   - **Fill Method** = `Horizontal` (dropdown — pick from list).
   - **Fill Origin** = `Left` (dropdown).
   - **Fill Amount** = `1` (slider — drag to 1.0 or type 1).
   - **Clockwise** = ✅ (default — only matters for Radial fills, ignore for Horizontal).
   - **Preserve Aspect** = ❌.

### 4e — Add the value text

1. Right-click `HealthRow` → **UI → Text - TextMeshPro** → name `ValueText`.
2. Rect Transform:
   - Anchor preset: middle-right.
   - Pivot: X = `1`, Y = `0.5`.
   - Pos X = `0`, Pos Y = `0`.
   - Width = `100`, Height = `24`.
3. TMP component:
   - Text = `100 / 100` (placeholder)
   - Font Size `14`
   - Alignment middle-right
   - Color white.

### 4f — Verify HealthRow hierarchy

```
HealthRow                    (Layout Element, height=24)
├── Label (TMP — "HP")
├── BarBackground (Image — dark)
├── BarFill (Image — red, Filled)
└── ValueText (TMP — "100 / 100")
```

Sibling order matters — BarFill must come **after** BarBackground so it draws on top.

---

## Step 5 — Duplicate for Stamina and Experience

Two more rows, same structure as HealthRow.

### 5a — StaminaRow

1. Click `HealthRow` → Ctrl+D → rename copy to `StaminaRow`.
2. Inside StaminaRow:
   - `Label` text = `STAM`.
   - `BarFill` color = green `(80, 200, 100, 255)`.
   - `ValueText` text = `100 / 100` (placeholder).

### 5b — ExperienceRow

1. Click `HealthRow` (or StaminaRow) → Ctrl+D → rename to `ExperienceRow`.
2. Inside ExperienceRow:
   - `Label` text = `XP`.
   - `BarFill` color = cyan `(80, 200, 240, 255)`.
   - `ValueText` text = `0 / 100` (placeholder).

After this, BarsRoot looks like:

```
BarsRoot (VLG)
├── LevelText (TMP — "Lv 1")
├── HealthRow
├── StaminaRow
└── ExperienceRow
```

---

## Step 6 — Build the gold readout (top-right)

1. Right-click `PlayerHUD` → **UI → Text - TextMeshPro** → name `GoldText`.
2. Rect Transform:
   - Anchor preset: **top-right**.
   - Pivot: X = `1`, Y = `1`.
   - Pos X = `-20`, Pos Y = `-20` (margin from corner).
   - Width = `200`, Height = `40`.
3. TMP component:
   - Text = `0 G` (placeholder)
   - Font Size `28`
   - Bold ✅
   - Alignment center+right
   - Color `(255, 220, 100, 255)` (gold).

---

## Step 7 — Wire the PlayerHUD component

1. Click `PlayerHUD` in the Hierarchy.
2. Add Component → **Player HUD** (`DungeonBlade.UI.HUD.PlayerHUD`).
3. Wire fields by dragging from the Hierarchy:

   | Inspector field | Drag from Hierarchy                        |
   | --------------- | ------------------------------------------ |
   | Stats           | `Player` GameObject (PlayerStats is on it) |
   | Health Fill     | `BarsRoot/HealthRow/BarFill`               |
   | Health Text     | `BarsRoot/HealthRow/ValueText`             |
   | Stamina Fill    | `BarsRoot/StaminaRow/BarFill`              |
   | Stamina Text    | `BarsRoot/StaminaRow/ValueText`            |
   | Experience Fill | `BarsRoot/ExperienceRow/BarFill`           |
   | Level Text      | `BarsRoot/LevelText`                       |
   | Experience Text | `BarsRoot/ExperienceRow/ValueText`         |
   | Gold Text       | `GoldText`                                 |

Tip: lock the Inspector on `PlayerHUD` (padlock icon top-right) while dragging.

---

## Step 8 — Save scene + test

1. Ctrl+S.
2. Press Play.
3. Bottom-left should show:
   - `Lv 1`
   - HP bar full (100/100), red.
   - STAM bar full (100/100), green.
   - XP bar at zero or partial (depending on saved data), cyan.
4. Top-right shows current gold (e.g. `100 G` if you start with 100).
5. Take damage → HP bar shrinks live.
6. Dash/dodge/sprint actions → STAM bar drains.
7. Kill an enemy → XP bar grows. Level up → Level text updates and XP bar resets.
8. Pick up gold → top-right number increases.

---

## Common issues

- **Bars don't appear** → Stats field not wired (PlayerStats reference). Drag the Player GameObject into it.
- **Bars show but don't update** → check Console for events firing. The wiring depends on Player GameObject having PlayerStats attached.
- **Level/XP not showing** → ExperienceSystem isn't in the scene. Confirm `[Experience]` GameObject exists with the ExperienceSystem component.
- **Gold always shows 0** → PlayerWallet isn't in the scene. Confirm `[Bank]` GameObject exists with PlayerWallet (M7 setup).
- **Bars are squares not bars** → BarFill or BarBackground has Preserve Aspect on, OR Source Image is empty. Pick UISprite + uncheck Preserve Aspect.
- **Colors look wrong** → verify alpha is 255 (fully opaque) on each BarFill. Dark fills show as dim bars.
- **Bars stack vertically wrong** → BarsRoot's Vertical Layout Group might have Child Alignment as Upper instead of Lower. Set Lower Left so they hug the bottom.

---

## Optional polish (M9 add-ons)

- **HP bar pulse when low (<25%)** — add a coroutine that scales BarFill alpha sin-wave when current < max \* 0.25.
- **XP gain popup** — separate floating "+50 XP" text that fades up when XP is gained.
- **Damage flash on screen** — full-screen red vignette when HP drops, fades out.
- **Stamina depleted shake** — bar shakes briefly when player tries to dash with insufficient stamina.

These are layer-on-top features, do them after the base HUD works.

---

## Recap of final hierarchy

```
Canvas
├── (existing UI — Inventory, Pause, Settings, BossHealthBar)
└── PlayerHUD                       (root, stretch full)
    ├── BarsRoot                    (bottom-left, VLG)
    │   ├── LevelText (TMP)
    │   ├── HealthRow
    │   │   ├── Label (TMP — "HP")
    │   │   ├── BarBackground (Image)
    │   │   ├── BarFill (Image — Filled, red)
    │   │   └── ValueText (TMP)
    │   ├── StaminaRow
    │   │   ├── Label
    │   │   ├── BarBackground
    │   │   ├── BarFill (green)
    │   │   └── ValueText
    │   └── ExperienceRow
    │       ├── Label
    │       ├── BarBackground
    │       ├── BarFill (cyan)
    │       └── ValueText
    └── GoldText (TMP — top-right)
```
