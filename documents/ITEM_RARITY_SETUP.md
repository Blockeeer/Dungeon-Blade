# Item Rarity Setup — Dungeon Blade

How to use the rarity color system. Aligned to **Dungeon Blade GDD v1 §6.1**.

**What's wired in code:**
- `Item.Rank` field — uses the `ItemRank` enum
- 5 rarity tiers per GDD: `Common / Uncommon / Rare / Epic / Legendary`
- `ItemRankColors` static helper for color lookup
- `ItemTooltip` colors the item name per rarity + appends `[Rarity]` to the type line
- `ItemPickup` builds a rich-text prompt: "Press [F] to pick up `<rarity-colored>` Item Name"
- `Interactable.PromptText` is `virtual` so subclasses can override it

---

## Rarity tiers (per GDD §6.1)

| Rank | Color | Description (GDD) |
|---|---|---|
| **Common** | White / Gray `(217, 217, 217)` | Basic gear. No special stats. Vendor trash or salvage material. |
| **Uncommon** | Green `(77, 217, 77)` | Minor stat boosts (+ATK, +DEF). Found in regular enemy drops. |
| **Rare** | Blue `(77, 140, 255)` | Notable stat bonuses. Found in chests and elite enemies. |
| **Epic** | Purple `(179, 89, 255)` | Significant bonuses + 1 special property (e.g., lifesteal, piercing shots). |
| **Legendary** | Orange / Gold `(255, 140, 26)` | Unique items with powerful effects. Boss-exclusive drops only. |

If you want different colors, edit the static fields in `Item.cs` → `ItemRankColors`.

---

## Step 1 — Tag your existing items with rarity

### Consumables (typically all Common per GDD §6.3)

- `Item_HealthPotion` → **Common**
- `Item_StaminaTonic` → **Common**
- `Item_BoneFragment` → **Common** (material)

### Weapons (per GDD economy §8.4 + intent)

The GDD groups weapons by purchasable price tiers. Suggested mapping:

| Weapon | Rank | Reasoning |
|---|---|---|
| `Item_Longsword` | **Common** | Buyable common gear (100-250g) |
| `Item_AssaultRifle` | **Common** | Buyable common gear |
| `Item_SemiAutoPistol` | **Common** | Buyable common gear |
| `Item_IronSword` | **Common** | Starter weapon |
| `Item_IronPistol` | **Common** | Starter weapon |
| `Item_DualDemonSword` | **Rare** | Drop-only (per existing folder structure) |
| `Item_PistolBlue` | **Rare** | Drop-only |
| `Item_FuturisticRifle` | **Rare** | Drop-only |
| `Item_IceKatana` | **Legendary** | Boss-exclusive intent, GDD §6.2 — Warlord's Blade analog |
| `Item_FantasyGun` | **Legendary** | Boss reward tier |
| `Item_FantasyPistol` | **Legendary** | Boss reward tier |

**Uncommon and Epic tiers are currently empty in your item list.** That's fine for Phase 1 — fill them in as you add new items per the GDD's Boss Reward Table (§6.2):
- Uncommon → guaranteed boss-chest drop alongside Rare
- Epic → 30% chance boss-chest drop

### How to set the rank

1. Project window → `Assets/_Project/Inventory/Items/`.
2. Click an Item asset.
3. Inspector → find the **Rank** dropdown (now shows 5 options).
4. Pick the right tier.
5. Save (Ctrl+S).

**Tip:** select multiple items at once → set their Rank in one Inspector edit.

---

## Step 2 — Test in Play mode

### 2a — Inventory tooltip

1. Press Play → enter Lobby or Dungeon → open Inventory.
2. Hover over a Common item → name shows in white/gray.
3. Hover over an Uncommon item → name shows in green, type line shows `Weapon • MainHand  [Uncommon]`.
4. Hover over a Rare item → name shows in blue, `[Rare]` appended.
5. Hover over an Epic item → name shows in purple, `[Epic]` appended.
6. Hover over a Legendary item → name shows in orange-gold, `[Legendary]` appended.

### 2b — World pickup prompt

1. In the Dungeon, find an `ItemPickup` (chest drop, enemy drop, etc).
2. Walk near it.
3. The "Press [F] to pick up" prompt now shows the item name **colored** by rarity.
4. Quantities > 1 also show: `Press [F] to pick up Health Potion x3`.

If the colors don't appear (text shows `<color=#XXXXXX>` literally), the prompt's TMP component has Rich Text disabled. Fix:
- Click the prompt label GameObject in the scene.
- Inspector → TextMeshPro - Text → check **Rich Text** ✅ (default is on).

---

## Step 3 — (Optional) Add rarity glow to dropped items

`ItemPickup` has a **Rarity Tint Renderer** field. Wire it for an emissive glow that matches the item's rarity.

### Per-item-pickup setup

1. In your scene (or in the ItemPickup prefab), find the visual mesh that represents the item.
2. Click `ItemPickup` GameObject.
3. Inspector → ItemPickup component → **Rarity Tint Renderer** field.
4. Drag the visual mesh's MeshRenderer into the slot.
5. Save scene.

At runtime, the script applies a property block to that renderer with the rarity color on `_BaseColor` (URP), `_Color` (legacy), and a dimmed `_EmissionColor`. This works without making per-instance material instances.

**Material requirement:** the assigned renderer's material must be **emissive-capable** for the glow to show:
- URP Lit shader: enable **Emission** in the material. Set Emission color to white (script overwrites with the rarity color at runtime).
- Built-in Standard shader: same — enable Emission.

If the material doesn't support Emission, only the base color tint applies — still readable, just no glow.

---

## Step 4 — Boss Reward Table per GDD §6.2

Once rarity tags are set, the boss loot table needs updating. Per GDD §6.2:

| Roll | Reward | Quantity |
|---|---|---|
| Guaranteed | Gold (150-300) | 1 roll |
| Guaranteed | Uncommon or Rare item (weapon or armor) | 1 item |
| 70% chance | Rare item (weapon or armor) | 1 item |
| 30% chance | Epic item | 1 item |
| 5% chance | Legendary: Warlord's Blade (unique sword) | 1 item (first kill only) |
| 100% | Dungeon Clear Token | 1 token |

**Implementation note:** this is on `LootTable.cs` → boss-specific table. The current loot table system supports weighted rolls. Wiring the boss-specific table to match GDD percentages is a follow-up task in [M9_TODO.md](M9_TODO.md) — not in scope for this rarity setup doc.

---

## Step 5 — (Optional) Sort or filter Bank/Shop UI by rarity

`ItemRank` is now ordered by power (Common=0 → Legendary=4), so sort works directly:

```csharp
itemList.Sort((a, b) => b.Rank.CompareTo(a.Rank));   // Legendary → Common
```

Plug into the existing Bank/Shop UI sorting if you want.

---

## Common issues

- **Tooltip name color not changing** → the tooltip's TMP component has `Color` overridden by another script. The new code sets `nameText.color = ItemRankColors.For(item.Rank)` — confirm nothing else is writing to `nameText.color` after `Show()`.
- **Pickup prompt shows raw `<color=#...>` text** → Rich Text is disabled on the prompt TMP. Enable it in the Inspector.
- **All items show as Common** → existing Item ScriptableObjects haven't been tagged yet. Click each → set Rank in the Inspector.
- **Custom rarity color not applying** → you edited the constants in `ItemRankColors`, but Unity may cache. Save Item.cs, wait for recompile, restart Play mode.
- **Items previously tagged "Legend" now show as Rare** → in the old enum, `Legend = 2`. The new enum shifts that slot to `Rare`. If you tagged items before this change, retag them. (Probably none — this enum was new for most items.)

---

## Where this slots into Phase 1

Data layer is now GDD-aligned (5 tiers). Visual layer beyond the tooltip + pickup label (e.g. equipped weapon glow per rarity) plugs in later when real weapon models are in place.

[M9_TODO.md](M9_TODO.md) — task #1 (rarity data + colors) is now done; aura visual and Boss Reward Table remain.
