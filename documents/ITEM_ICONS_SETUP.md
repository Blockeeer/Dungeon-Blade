# Item Icons Setup

Replace the placeholder "colored squares" in inventory with proper sprite icons. ~30 min total.

**No code changes needed** — the `Item.Icon` field already exists; you just need to fill it on each Item ScriptableObject.

---

## Why this works without code

Your existing systems already render `Item.Icon` if it's set:

- **Inventory grid + hotbar** — `SlotWidget` draws the sprite
- **Tooltip on hover** — already draws icon next to the name
- **ItemPickup** in the world — uses the icon for the floating label
- **Bank vault + Shop UI** — same `SlotWidget` rendering

You just need to **populate the `Icon` field** on each Item ScriptableObject in the Project window.

---

## Step 1 — Get the icons (10-15 min)

### Where to download

Three free CC-BY / CC0 sources, ranked:

**Best: game-icons.net** (game-icons.net) — best fit for fantasy weapons/potions
- 4000+ vector-style icons in a consistent art style
- License: **CC-BY 3.0** (must credit author "Lorc, Delapouite, et al." in a CREDITS file)
- Each icon has a download as PNG button at the top right
- Pick a single foreground color (e.g. white) and download — the icon comes with transparent background
- Recommended size: **128×128 PNG** (default download)

**Backup: OpenGameArt.org**
- Search "icon" or "weapon icon"
- License varies — filter by CC0 if you don't want attribution

**Optional polish: Pixabay**
- Less game-specific but works for placeholder icons

### Which icons to grab

Map these to your existing Item ScriptableObjects:

| Item asset | Search term on game-icons.net | Suggested name to save |
|---|---|---|
| `Item_HealthPotion` | `"health potion"` or `"red flask"` | `icon_potion_health.png` |
| `Item_StaminaTonic` | `"stamina"` or `"energy drink"` or `"green flask"` | `icon_potion_stamina.png` |
| `Item_BoneFragment` | `"bone"` | `icon_bone_fragment.png` |
| `Item_IronSword` | `"broadsword"` or `"longsword"` | `icon_sword_iron.png` |
| `Item_IronPistol` | `"pistol"` or `"revolver"` | `icon_pistol_iron.png` |
| `Item_Longsword` | `"longsword"` | `icon_sword_longsword.png` |
| `Item_AssaultRifle` | `"rifle"` or `"assault rifle"` | `icon_rifle_assault.png` |
| `Item_SemiAutoPistol` | `"pistol"` or `"semi auto"` | `icon_pistol_semiauto.png` |
| `Item_DualDemonSword` | `"dual sword"` or `"twin blades"` | `icon_sword_dual_demon.png` |
| `Item_FuturisticRifle` | `"futuristic rifle"` or `"laser rifle"` | `icon_rifle_futuristic.png` |
| `Item_PistolBlue` | `"pistol"` (use blue foreground) | `icon_pistol_blue.png` |
| `Item_IceKatana` | `"katana"` (use cyan foreground) | `icon_katana_ice.png` |
| `Item_FantasyGun` | `"magic gun"` or `"crystal gun"` | `icon_gun_fantasy.png` |
| `Item_FantasyPistol` | `"magic pistol"` | `icon_pistol_fantasy.png` |
| `Item_WarlordsBlade` | `"ornate sword"` or `"bone sword"` | `icon_sword_warlord.png` |
| `Item_DungeonClearToken` | `"coin"` or `"medallion"` | `icon_token_dungeon.png` |

Use **rarity colors as foreground** on game-icons.net for thematic consistency:
- Common items → white or light gray
- Uncommon → green
- Rare → blue
- Epic → purple
- Legendary → orange/gold

You can change the foreground color before downloading on the game-icons.net page (color picker at the top of the page).

---

## Step 2 — Create the Icons folder

1. Open Unity → Project window.
2. Navigate to `Assets/_Project/UI/Sprites/`.
3. If `Items` subfolder doesn't exist: right-click `Sprites` → **Create → Folder** → name `Items`.

You should end up with `Assets/_Project/UI/Sprites/Items/`.

---

## Step 3 — Import the icons

1. Drag all downloaded PNGs into `Assets/_Project/UI/Sprites/Items/` (in Unity's Project window).
2. Unity imports them as Textures by default — you need to switch them to Sprites.

### Set each icon's import settings

1. Click any icon in the Items folder.
2. Inspector → confirm or set:

   | Field | Value |
   |---|---|
   | **Texture Type** | `Sprite (2D and UI)` |
   | **Sprite Mode** | `Single` |
   | **Pixels Per Unit** | `100` (default — fine for 128×128) |
   | **Mesh Type** | `Tight` (default) |
   | **Filter Mode** | `Bilinear` (default) |
   | **Compression** | `Normal Quality` (default) |

3. Click **Apply**.

### Bulk-apply (faster)

Select all PNGs at once in the Project window → set Texture Type = Sprite (2D and UI) → Apply. Unity processes all in one go.

---

## Step 4 — Wire icons onto Item ScriptableObjects

For each item:

1. Project window → click the Item asset (e.g. `Item_HealthPotion.asset`).
2. Inspector → find the **Icon** field.
3. Drag the matching sprite from `Assets/_Project/UI/Sprites/Items/` into the Icon field.
4. The Inspector shows a small preview of the assigned sprite.
5. Press Ctrl+S to save (or click elsewhere to commit).

Repeat for every Item asset. Should take ~30 seconds per item once you have the icons ready.

### Item paths (where to find them in the Project window)

- **Consumables / materials:** `Assets/_Project/Inventory/Items/`
  - `Item_HealthPotion.asset`
  - `Item_StaminaTonic.asset`
  - `Item_BoneFragment.asset`
  - `Item_IronSword.asset`
  - `Item_IronPistol.asset`
  - `Item_DungeonClearToken.asset`

- **Common weapons:** `Assets/_Project/Combat/Weapons/Items/Common/`
  - `Item_Longsword.asset`
  - `Item_AssaultRifle.asset`
  - `Item_SemiAutoPistol.asset`

- **Rare weapons:** `Assets/_Project/Combat/Weapons/Items/Rare/`
  - `Item_DualDemonSword.asset`
  - `Item_FuturisticRifle.asset`
  - `Item_PistolBlue.asset`

- **Legendary weapons:** `Assets/_Project/Combat/Weapons/Items/Legend/`
  - `Item_IceKatana.asset`
  - `Item_FantasyGun.asset`
  - `Item_FantasyPistol.asset`
  - `Item_WarlordsBlade.asset`

---

## Step 5 — Test

1. Save scene (Ctrl+S).
2. Press Play → enter Dungeon → open Inventory (Tab or I).
3. **Expected:**
   - Health Potion shows its potion icon
   - Each weapon shows its specific sprite (sword vs gun vs pistol)
   - Tooltip on hover shows the icon next to the item name
4. **Drop or buy a weapon** → ItemPickup in the world shows the icon (if you wired the Rarity Tint Renderer earlier, the world drop's mesh tints by rarity; the icon shows in the pickup label).
5. **Open Bank or Shop** → items in those panels show icons too.

---

## Step 6 — Add CREDITS.md (legal paper trail)

If you used CC-BY icons from game-icons.net:

Create `documents/CREDITS.md`:

```markdown
# Credits

## Game Icons

Item icons from https://game-icons.net (CC-BY 3.0).

Specific authors:
- Lorc (https://lorcblog.blogspot.com/) — most weapon icons
- Delapouite (https://delapouite.com/) — potion and material icons
- [Other authors as applicable per icon's individual credit on game-icons.net]

License: CC-BY 3.0 (https://creativecommons.org/licenses/by/3.0/)
```

This satisfies CC-BY's "must credit the author" requirement. A single markdown file in the repo is fine.

---

## Common issues

| Symptom | Fix |
|---|---|
| Icon appears as a fuzzy/blurry square | Texture Type still set to `Default` instead of `Sprite (2D and UI)`. Re-import. |
| Icon appears stretched or weird aspect ratio | Source PNG isn't square. Re-crop to 128×128 or 256×256. |
| Icon imports as cyan/magenta solid color | Source PNG has no alpha channel. Re-download with transparency from game-icons.net. |
| Inspector shows "None (Sprite)" after dragging | Dragged the wrong asset. Drag the actual sprite file, not its meta. |
| Icon shows in Project window but not in inventory | The Item's Icon field is still empty. Drag the sprite into the Inspector's Icon slot. |
| Tooltip text still shows but no icon | The tooltip's image component might have Raycast Target preventing hover. Check Tooltip prefab. |

---

## When done

Tell me **"done with #1"** and I'll:

1. Verify icons render correctly (you'll send a screenshot or describe what you see).
2. Mark task #1 ✅ in [M9_REMAINING_RANKED.md](M9_REMAINING_RANKED.md).
3. Advance to next task → likely **#2 Combo EXP Milestone Bonus** (no assets needed, ~45 min code wiring).

---

## Reference

- [game-icons.net](https://game-icons.net) — primary icon source
- [ITEM_RARITY_SETUP.md](ITEM_RARITY_SETUP.md) — rarity color guide (use same colors for icon foregrounds)
- [M9_REMAINING_RANKED.md](M9_REMAINING_RANKED.md) — overall task queue
