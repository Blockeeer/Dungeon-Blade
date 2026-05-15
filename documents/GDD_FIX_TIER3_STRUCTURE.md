# GDD Fix Tier 3 — Structural Deviations

**Effort:** ~3-4 hours total
**Goal:** Bring inventory/equipment structure in line with `DungeonBlade GDD v1.md` §7.2.

This tier has 3 sections of structural deviations that are too big to be "Inspector tweaks" — they involve adding new equipment slots, trimming inventory dimensions, and fixing input bindings.

---

## §1 — Equipment slots completion (GDD §7.2)

**GDD spec:** 9 equipment slots total:
- Head
- Chest
- Legs
- Boots
- MainHand
- OffHand
- Ring × 2 (two ring slots)
- Amulet

**Current:** 4 slots only:
- Head
- Body
- MainHand
- OffHand

**Missing:** Legs, Boots, Ring1, Ring2, Amulet (5 missing slots). Also `Body` should be renamed `Chest` for GDD alignment.

### 1a — Update the EquipmentSlot enum

Open `Assets/_Project/Inventory/Scripts/Item.cs`.

Current:
```csharp
public enum EquipmentSlot { None, Head, Body, MainHand, OffHand }
```

**Tell me when you're ready and I'll change this to:**
```csharp
public enum EquipmentSlot
{
    None,
    Head,
    Chest,    // renamed from "Body"
    Legs,     // new
    Boots,    // new
    MainHand,
    OffHand,
    Ring1,    // new
    Ring2,    // new
    Amulet    // new
}
```

**Caveat — enum int values shift.** Currently `Body = 2, MainHand = 3, OffHand = 4`. After the change, `Chest = 2` (same as Body, name only), `MainHand = 5, OffHand = 6`. Any existing `EquipmentBinder` ScriptableObject that stored MainHand will silently become `Legs` because Unity serializes enums as ints.

**Mitigation:** before changing the enum, audit your existing equipment-related assets:
1. Project window → search **`t:WeaponItem`** (returns only weapon-class items; alternatively `t:Item` returns all items including consumables — both work, `t:WeaponItem` is more precise).
2. Click each result → check the `Equip Slot` field in the Inspector.
3. Note which slot each weapon is in (probably MainHand for most weapons — int value `3` in the asset YAML).
4. After the enum change, re-set those values if they shifted.

**Note on filter syntax:** Unity's `t:` filter matches by C# class name, **including subclasses**. So `t:Item` matches `Item`, `WeaponItem`, and `ConsumableItem` together (because the latter two inherit from `Item`). `t:WeaponItem` matches only the weapons.

### 1b — Add UI slots to InventoryUI

The Inventory panel has equipment slot widgets visible on the left side. Adding 5 new slots requires:

1. Open `1_MainMenu.unity` or wherever Inventory UI lives (likely `3_Dungeon1.unity`).
2. Find the equipment slots container under `InventoryPanel/EquipmentSlots/`.
3. Duplicate an existing slot widget (e.g. the MainHand slot) 5 times.
4. Rename each new copy: `Slot_Legs`, `Slot_Boots`, `Slot_Ring1`, `Slot_Ring2`, `Slot_Amulet`.
5. Position each in the equipment panel UI (re-arrange the layout for 9 slots — paper-doll style works).
6. Each `SlotWidget` component has a field that defines which slot it represents — set per the new EquipmentSlot enum value.

This is a heavy UI rebuild. **Recommend doing this only if you actually have armor items to equip** (currently no Legs/Boots/Ring/Amulet items exist as Item ScriptableObjects).

### 1c — Or defer until armor items exist

GDD §6.3 lists armor as an item category but no specific armor pieces are mentioned for Phase 1. The equipment slots are spec but **without armor items to fill them, they're empty UI**.

**Recommended Phase 1 path:**
1. Skip adding the 5 missing slots for now.
2. Rename `Body → Chest` only (minor consistency win).
3. Add a note in GDD_AUDIT.md: "Equipment slots incomplete (Head/Chest/MainHand/OffHand only). Legs/Boots/Ring×2/Amulet deferred until armor items are added."
4. Implement the full slot set when armor items become a Phase 1 priority (or push to Phase 2).

**If you want the full set anyway**, tell me — I'll do the enum change and write a per-slot UI rebuild guide.

---

## §2 — Hotbar 6 → 4 slots (GDD §7.2 + §10.2)

**GDD spec:** exactly 4 hotbar slots, keys 1-4.
**Current:** 6 slots, keys 1-6.

### 2a — Decision

- ✅ **Strict GDD:** trim to 4 slots.
- ✏️ **Keep 6:** more flexibility for QoL. Document deviation.

Most action-RPG games default to 4-8 hotbar slots. The GDD's 4 is on the conservative side.

### 2b — If trimming to 4

Hotbar slots are **generated at runtime** by `InventoryUI.cs` (a `for` loop that runs `InventoryManager.HotbarSize` times). So you cannot delete slot 5/6 in the Hierarchy at edit-time — they don't exist until Play mode.

To trim to 4 slots, only **3 code changes** are needed (no scene editing):

**Step 1 — Update the hotbar size constant:**

In `Assets/_Project/Inventory/Scripts/InventoryManager.cs`:

```csharp
public const int HotbarSize = 6;   // change to 4
```

This drives:
- How many slot widgets get instantiated on Play
- How many slots the save system serializes

**Step 2 — Remove input actions:**

In `Assets/_Project/Core/Input/PlayerInputActions.cs`, remove these field declarations:

```csharp
public readonly InputAction Hotbar5;
public readonly InputAction Hotbar6;
```

And their constructor assignments:
```csharp
Hotbar5 = Map.AddAction("Hotbar5", InputActionType.Button, "<Keyboard>/5");
Hotbar6 = Map.AddAction("Hotbar6", InputActionType.Button, "<Keyboard>/6");
```

**Step 3 — Remove their handlers in HotbarBinder.cs:**

In `Assets/_Project/Inventory/Scripts/HotbarBinder.cs`, remove:

```csharp
if (_input.Hotbar5.WasPressedThisFrame()) UseHotbar(4);
if (_input.Hotbar6.WasPressedThisFrame()) UseHotbar(5);
```

**Tell me when ready and I'll do all three code changes** in one pass. Optional: add save-migration so existing player saves with items in slot 5/6 don't lose those items (drop them into the grid instead). I'll add ~10 lines if you want that.

After the changes, next time you press Play the hotbar will spawn only 4 slots automatically. **No scene editing required.**

### 2c — If keeping 6

Open `GDD_AUDIT.md` and add to the §7.2 row:
- "Hotbar has 6 slots (keys 1-6) instead of GDD's 4 — kept for QoL convenience."

---

## §3 — OpenInventory: bind `I` key (GDD §10.2)

**GDD spec:** "Open Inventory — I or Tab"
**Current:** Tab only.

### Fix

Open `Assets/_Project/Core/Input/PlayerInputActions.cs`.

Find:
```csharp
OpenInventory = Map.AddAction("OpenInventory", InputActionType.Button, "<Keyboard>/tab");
```

Add a second binding right after:
```csharp
OpenInventory.AddBinding("<Keyboard>/i");
```

**Tell me when ready and I'll do this code change** (it's a 1-line addition).

### Verify

1. Press Play → enter game.
2. Press Tab → inventory opens ✅
3. Press I → inventory opens ✅

---

## Action plan summary

Three sub-tasks, all with code changes needed:

| Task | Decision | Effort |
|---|---|---|
| §1 Equipment slots | Defer (recommended) OR add 5 slots | 30 min (defer) / 2-3 hr (full) |
| §2 Hotbar trim | Trim (strict GDD) OR keep 6 (deviation) | 30 min (trim) / 1 min (keep + document) |
| §3 Open inventory on I | Always do | 1 min (code) |

### Recommended sequence (lightest path)

1. **§3 first** — 1-line code fix, no decisions. Always do.
2. **§2 — decide trim vs keep.** If keep, just document. If trim, ~30 min.
3. **§1 — likely defer to Phase 2** unless you're adding armor items now.

---

## When ready

Tell me which sub-tasks you want to do, and I'll handle the code changes. For example:

- **"Do §3 only"** — I'll add the `I` binding.
- **"Do §3 + trim §2 to 4 slots"** — I'll do both code changes + identify scripts needing patches.
- **"Defer §1, trim §2, do §3"** — full Tier 3 lightweight path.
- **"Full strict §1 + §2 + §3"** — heavy refactor but maximum GDD compliance.

---

## When done — mark Tier 3 complete

1. Update `GDD_AUDIT.md` §7.2 / §10.2 — change items from 🔴/🟡 to ✅ (or document deviation).
2. Move to **Tier 4: Boss Reward Table + Loot Polish** → `GDD_FIX_TIER4_LOOT.md`.

---

## Reference

- [DungeonBlade GDD v1.md](DungeonBlade%20GDD%20v1.md) §7.2, §10.2
- [GDD_AUDIT.md](GDD_AUDIT.md)
