# Boss Health Bar — Unity Setup

How to wire `BossHealthBar.cs` into the Dungeon scene.

---

## Step 1 — Open the Dungeon scene

Open `3_Dungeon1.unity`. The boss bar lives in the dungeon HUD.

## Step 2 — Build the UI hierarchy under Canvas

1. Right-click your existing `Canvas` → **Create Empty** → name `BossHealthBar`.
   - Rect Transform: anchor **top-center** (Alt+click top-center anchor preset).
   - Pos Y = `-60`. Width `900`, Height `90`.
2. Add Component → **Canvas Group** on `BossHealthBar`. Leave Alpha at `0` (script will fade it in).

### Backing panel + frame

3. Right-click `BossHealthBar` → **UI → Image** → name `Background`.
   - Stretch full (Alt+click bottom-right preset).
   - Color = `(0, 0, 0, 200)` (semi-transparent black).

### Boss name label

4. Right-click `BossHealthBar` → **UI → Text - TextMeshPro** → name `NameLabel`.
   - Anchor preset: **top-stretch** (top row, middle column in the anchor preset popup).
   - Rect Transform fields:
     - Left = `30`
     - Right = `30`
     - Pos Y = `-5` (negative = 5px below the top edge)
     - Pos Z = `0`
     - Height = `30`
   - Text = `Boss Name` (placeholder — script overwrites at runtime).
   - Font Size `24`, Bold ✅, Alignment center+middle, Color white.

### Phase label

5. Right-click `BossHealthBar` → **UI → Text - TextMeshPro** → name `PhaseLabel`.
   - Anchor preset: **bottom-stretch** (bottom row, middle column).
   - Rect Transform fields:
     - Left = `30`
     - Right = `30`
     - Pos Y = `5` (positive = 5px above the bottom edge when bottom-anchored)
     - Pos Z = `0`
     - Height = `20`
   - Text = `Phase 1` (placeholder).
   - Font Size `16`, Alignment center+middle, Color light gray (`200, 200, 200, 255`).

### HP bar (filled image)

6. Right-click `BossHealthBar` → **Create Empty** → name `BarTrack`.
   - Anchor preset: **middle-stretch** (middle row, middle column).
   - Rect Transform fields:
     - Left = `30`
     - Right = `30`
     - Pos Y = `0`
     - Pos Z = `0`
     - Height = `24`

7. Right-click `BarTrack` → **UI → Image** → name `BarBackground`.
   - Stretch full (Alt+click bottom-right anchor preset).
   - **Source Image** field → click the small circle icon → pick `UISprite` (or any built-in UI sprite). Required — without a sprite, the Image renders nothing.
   - Color = dark red `(60, 10, 10, 255)`.

8. Right-click `BarTrack` → **UI → Image** → name `BarFill`.
   - Stretch full (Alt+click bottom-right anchor preset).
   - **Source Image** field → click the small circle icon → pick `UISprite`. **Required** — the fields below (Image Type, Fill Method, etc.) only appear once a sprite is assigned.
   - Color = bright red `(220, 30, 30, 255)`.
   - **Image Type** = `Filled` (dropdown, not text — pick from the list)
   - **Fill Method** = `Horizontal`
   - **Fill Origin** = `Left`
   - **Fill Amount** = `1` (script will animate this at runtime).

### Phase markers (optional — vertical lines on the bar)

9. Right-click `BarTrack` → **UI → Image** → name `Phase2Marker`.
   - Width `2`, Height `24` (stretches to bar height).
   - Color = white `(255, 255, 255, 200)`.
   - Anchors don't matter — script auto-places it at the 66% point.

10. Repeat for `Phase3Marker` — same settings.
    - Script auto-places it at the 33% point.

---

## Step 3 — Final hierarchy check

```
Canvas
└── BossHealthBar               (Canvas Group, alpha=0)
    ├── Background (Image)
    ├── NameLabel (TMP)
    ├── PhaseLabel (TMP)
    └── BarTrack
        ├── BarBackground (Image — dark red)
        ├── BarFill (Image — Filled type, bright red)
        ├── Phase2Marker (Image — white vertical line)
        └── Phase3Marker (Image — white vertical line)
```

---

## Step 4 — Wire the BossHealthBar component

1. Select `BossHealthBar` in the Hierarchy.
2. Add Component → **Boss Health Bar** (`DungeonBlade.UI.HUD.BossHealthBar`).
3. Wire each field by dragging from the Hierarchy:

   | Inspector field | Drag |
   |---|---|
   | Boss | The boss GameObject in the scene (e.g. `UndeadWarlord`) |
   | Root | `BossHealthBar` itself (it has the Canvas Group) |
   | Fill | `BarTrack/BarFill` |
   | Name Label | `NameLabel` |
   | Phase Label | `PhaseLabel` |
   | Phase 2 Marker | `BarTrack/Phase2Marker` (optional — leave None to skip) |
   | Phase 3 Marker | `BarTrack/Phase3Marker` (optional) |
   | Fade Duration | `0.5` |

4. Save scene (Ctrl+S).

---

## Step 5 — Test

1. Press Play → enter the boss arena.
2. As the arena seals, the boss bar fades in at top of screen showing:
   - Boss name (e.g. `UndeadWarlord`)
   - Full red bar
   - Two white vertical lines at 66% and 33% (phase markers)
   - "Phase 1" label below
3. Hit the boss — bar shrinks live.
4. At 66% HP — phase 2 transition. Bar passes the first marker. Label briefly shows "Transitioning…" then "Phase 2".
5. At 33% — same for phase 3 ("Phase 3 — Enraged").
6. Boss dies — bar fades out, label shows "Defeated".
7. Die yourself and respawn outside arena — boss resets, bar hides until you re-enter and fight again.

---

## Common issues

- **Bar doesn't appear** → BossHealthBar's `Boss` field is empty, or the Canvas Group's alpha is being overridden somewhere. Check Console for null-reference errors.
- **Bar appears but doesn't update** → the boss in the scene isn't the one wired in the field. If you have multiple boss prefabs, confirm you wired the actual scene instance.
- **Bar shows from scene start** instead of fading in on activation → the script sets alpha to 0 in Awake. If it still shows, the Canvas Group might be missing — re-check it's on `BossHealthBar` itself.
- **Phase markers in wrong positions** → the markers' parent must be `BarTrack` (not BossHealthBar root), so the percentage is relative to the bar.
- **Bar doesn't fade out on death** → `OnBossDefeated` event isn't firing. Check the boss's `Die()` method runs.

---

## Optional polish later (M9)

- **Damage shake** on the bar when boss takes a hit — small RectTransform offset.
- **Lag bar** — secondary slower bar showing recent damage taken (Genshin-style).
- **Phase transition flash** — full-screen color burst when boss hits 66% / 33%.
- **Boss name typewriter effect** — letters appear one-by-one when bar fades in.

These are all optional; the basic bar above is fully functional.
