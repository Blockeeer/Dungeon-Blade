# Tier 4 §1 — Partial Item Authoring

**Effort:** ~30 min in Unity Editor.
**Goal:** Author the 2 minimum-required items so Boss Reward Table (GDD §6.2) can roll correctly. Defer Uncommon/Epic placeholders until needed.

When done, ping me with "items done" — I'll wire the LootTable percentages and add the "first kill only" save flag.

---

## What you're creating

| Item | Purpose | GDD reference |
|---|---|---|
| `Item_WarlordsBlade` | Legendary unique sword, 5% boss drop, **first kill only** | §6.2 |
| `Item_DungeonClearToken` | Material currency, 100% boss drop, redeemed at Bank | §6.2 + §8.4 |

The remaining Uncommon/Epic placeholders from the full GDD §6.2 table are **skipped for now**. The boss reward roll handles missing rarities gracefully — if no Epic items exist, the 30% Epic roll silently passes. You can author them later when you have time/design ideas.

---

## Item 1 — `Item_WarlordsBlade` (Legendary unique sword)

### Step 1 — Duplicate an existing Legendary sword as template

1. **Project window** → navigate to `Assets/_Project/Combat/Weapons/Items/Legend/`.
2. Find `Item_IceKatana.asset` (or any other Legend-tier weapon).
3. Right-click on it → **Duplicate** (or press Ctrl+D).
4. The copy appears as `Item_IceKatana 1.asset`. Click on the name to rename → type `Item_WarlordsBlade` → Enter.
5. Confirm: the asset now exists at `Assets/_Project/Combat/Weapons/Items/Legend/Item_WarlordsBlade.asset`.

### Step 2 — Edit the fields

1. Click `Item_WarlordsBlade.asset`.
2. Look at the Inspector. Fill each field with these exact values:

| Field | Value | Note |
|---|---|---|
| **Item Id** | `warlords_blade` | Lowercase + underscores — used as save key |
| **Display Name** | `Warlord's Blade` | What players see in tooltips |
| **Description** | `Forged from the bones of the Undead Warlord. A trophy of your first triumph.` | Flavor text |
| **Icon** | (leave whatever IceKatana had) | Placeholder until you have a custom sprite |
| **Type** | `Weapon` | Dropdown |
| **Equip Slot** | `MainHand` | Dropdown |
| **Rank** | `Legendary` | Dropdown — must be Legendary tier per GDD §6.2 |
| **Stackable** | ❌ (unchecked) | Unique weapons don't stack |
| **Max Stack** | `1` | Ignored when unstackable but set anyway |
| **Sell Value** | `600` | High — Legendary tier per GDD §8.4 |
| **Buy Value** | `0` | Drop-only per GDD §6.2 — never sold in shops |

3. Press Tab or click elsewhere to commit each field.
4. Ctrl+S to save the asset (or it auto-saves when you click elsewhere).

### Step 3 — Verify

1. Click `Item_WarlordsBlade.asset` again.
2. Confirm all fields match the table.
3. The Hierarchy/Project name should be `Item_WarlordsBlade` (no extra spaces or "_1" suffix).

---

## Item 2 — `Item_DungeonClearToken` (Material currency)

### Step 1 — Duplicate a Material/Misc item as template

1. **Project window** → navigate to `Assets/_Project/Inventory/Items/`.
2. Find `Item_BoneFragment.asset` — it's a Material-type, base Item (not WeaponItem). This is the right template.
3. Right-click → **Duplicate**.
4. Rename the copy to `Item_DungeonClearToken`.

### Step 2 — Edit the fields

1. Click `Item_DungeonClearToken.asset`.
2. Fill in:

| Field | Value | Note |
|---|---|---|
| **Item Id** | `dungeon_clear_token` | Snake-case |
| **Display Name** | `Dungeon Clear Token` | |
| **Description** | `Proof of conquest. Trade at the Bank for exclusive gear.` | Hints at Phase 2 token exchange |
| **Icon** | placeholder | |
| **Type** | `Material` | Dropdown — not Weapon, not Consumable |
| **Equip Slot** | `None` | Currency isn't equipped |
| **Rank** | `Rare` | Per GDD §6.2 — tradeable but not common loot |
| **Stackable** | ✅ checked | Currency stacks |
| **Max Stack** | `99` | Max per slot |
| **Sell Value** | `0` | Cannot sell per GDD §8.4 ("Cannot sell") |
| **Buy Value** | `0` | Earn-only |

### Step 3 — Verify

1. Re-click the asset.
2. Confirm fields match.
3. Confirm the Inspector shows **base Item fields only** — no weapon-specific stats like damage/range. If you see weapon-specific fields, you duplicated a WeaponItem by mistake. Delete and re-duplicate from `Item_BoneFragment`.

---

## Common issues

- **Duplicate didn't work** → Unity sometimes locks asset files mid-edit. Close any Inspector open on the target. If that fails, restart Unity.
- **Rename creates an additional file instead of renaming** → press F2 while the file is selected, or right-click → Rename.
- **Rank dropdown only shows Common/Rare/Legend** → enum cache stale. Save the script (`Item.cs`), wait for recompile, retry.
- **Inspector shows weapon fields on Token** → wrong template. The Token should be a plain `Item`, not a `WeaponItem`. Delete and re-duplicate from a non-weapon item like `Item_BoneFragment` or `Item_HealthPotion`.
- **Description field is single-line and weird** → it's a multi-line TextArea field — should grow as you type. If not, the field is rendered as plain `[SerializeField] string` rather than `[TextArea]`. Cosmetic only, doesn't affect functionality.

---

## When both items exist

Tell me "**items done**" — I'll execute the rest:

1. **Tune `Loot_UndeadWarlord`** LootTable per GDD §6.2:
   - Guaranteed gold roll 150-300
   - 70% chance: Rare weapon drop
   - 30% chance: Epic drop (rolls empty until Epic items exist — graceful)
   - 5% chance: `Item_WarlordsBlade` (gated by first-kill flag)
   - 100%: `Item_DungeonClearToken`
2. **Add "first kill only" save flag** to PlayerProfile:
   - `bool warlordsBladeDropped` field
   - Set to true on first boss kill that grants the blade
   - On subsequent kills: skip the 5% Legendary roll entirely
3. **Update GDD_AUDIT.md §6.2** — mark Boss Reward Table ✅
4. **Update GDD_FIX_INDEX.md** — Tier 4 §1 done

After that, Phase 1 GDD compliance is substantially complete. Only deferred items remain:
- Uncommon/Epic placeholder items (cosmetic — author when you have ideas)
- Skills system (Phase 2 per GDD §5)
- Mini-map (Phase 2)
- Dungeon clear summary screen (nice-to-have polish)

---

## Reference

- [DungeonBlade GDD v1.md](DungeonBlade%20GDD%20v1.md) §6.2 (Boss Reward Table), §8.4 (Economy)
- [ITEM_RARITY_SETUP.md](ITEM_RARITY_SETUP.md) — color/tier reference for the Rank field
- [GDD_FIX_TIER4_LOOT.md](GDD_FIX_TIER4_LOOT.md) — full §1 spec (this doc is the partial-path subset)
