# Inventory Panel Layout — What You Should See

A clear reference for the Inventory UI: what slots exist, where they live, and how to verify each one is wired correctly. Aligned to `DungeonBlade GDD v1.md §7.2`.

---

## The complete inventory at a glance

When the player presses **I** or **Tab**, the inventory opens. You should see **three distinct slot groups** + a tooltip:

```
┌──────────────────────────────────────────────────────────────────┐
│                       INVENTORY PANEL                            │
│                                                                  │
│  ┌─ Equipment ─┐    ┌── Grid (6 × 8 = 48 slots) ──────────┐    │
│  │ ┌─┐         │    │ ┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐                  │    │
│  │ │H│ Head    │    │ └─┘└─┘└─┘└─┘└─┘└─┘                  │    │
│  │ └─┘         │    │ ┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐                  │    │
│  │ ┌─┐         │    │ └─┘└─┘└─┘└─┘└─┘└─┘                  │    │
│  │ │C│ Chest   │    │ (...48 slots total, 6 wide × 8 tall)│    │
│  │ └─┘         │    │                                      │    │
│  │ ┌─┐ ┌─┐     │    │                                      │    │
│  │ │M│ │O│     │    │                                      │    │
│  │ └─┘ └─┘     │    │                                      │    │
│  │ Main Off    │    └──────────────────────────────────────┘    │
│  └─────────────┘                                                 │
│                                                                  │
└──────────────────────────────────────────────────────────────────┘

(Outside the inventory panel, always visible while playing:)

┌──────────────────────────────────────────────────────────────────┐
│                                                                  │
│                                                                  │
│                                                                  │
│                                                                  │
│                                                                  │
│                                                                  │
│                                                                  │
│  ┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐                                              │
│  │1││2││3││4││5││6│      ← HOTBAR (bottom-center, 6 slots)        │
│  └─┘└─┘└─┘└─┘└─┘└─┘                                              │
└──────────────────────────────────────────────────────────────────┘
```

---

## The three slot groups

### 1. Hotbar (bottom-center of screen, always visible)

| Detail | Value |
|---|---|
| **Position** | Bottom-center of the screen, visible during gameplay (not just when inventory is open) |
| **Purpose** | Quick-use consumables (health potions, stamina tonics, grenades) |
| **Slots** | **6** (keys 1, 2, 3, 4, 5, 6) — Note: GDD spec says 4, your project kept 6 as a QoL deviation |
| **How they're created** | Runtime spawn by `InventoryUI.cs` — they only appear when you Play |
| **What goes here** | Only consumables (potions, food, grenades). NOT weapons or armor. |
| **Hotkey behavior** | Press 1-6 in-game → triggers `ConsumableItem.OnUse()` on that slot's item |

### 2. Grid (large center area inside inventory panel, only visible when inventory is open)

| Detail | Value |
|---|---|
| **Position** | Center/right of the Inventory panel, opens when you press I or Tab |
| **Purpose** | Main storage — every item you pick up goes here |
| **Slots** | **48** (6 columns × 8 rows) — matches GDD §7.2 exactly |
| **How they're created** | Runtime spawn by `InventoryUI.cs` |
| **What goes here** | Any item: weapons, armor, consumables, materials, key items |
| **Hotkey behavior** | None — drag-and-drop only |

### 3. Equipment (left side of inventory panel, only visible when inventory is open)

| Detail | Value |
|---|---|
| **Position** | Left column of the Inventory panel |
| **Purpose** | What the player is currently wearing/wielding |
| **Slots** | **4** (Head, Chest, MainHand, OffHand) — GDD §7.2 specifies 9; the other 5 are reserved for future armor items |
| **How they're created** | Runtime spawn by `InventoryUI.cs` |
| **What goes here** | Only the matching type. MainHand accepts Sword/Gun, etc. |

#### The 4 active equipment slots in detail

| Slot # | Name | What it holds | Tag in Hierarchy at Play |
|---|---|---|---|
| 1 | Head | Helmets (no armor items yet — slot will be empty) | `EquipSlot_Head` |
| 2 | Chest | Chestplates (no armor items yet — empty) | `EquipSlot_Chest` |
| 3 | MainHand | Active weapon (sword OR gun) | `EquipSlot_MainHand` |
| 4 | OffHand | Secondary item (shield/second weapon) | `EquipSlot_OffHand` |

#### Slots reserved but not generated

GDD §7.2 specifies 9 slots total. The remaining 5 are reserved in the `EquipmentSlot` enum (values 5-9) for future armor items but **no UI widgets exist** yet:

| Slot # | Name | Status |
|---|---|---|
| 5 | Legs | Reserved — no UI widget |
| 6 | Boots | Reserved — no UI widget |
| 7 | Ring1 | Reserved — no UI widget |
| 8 | Ring2 | Reserved — no UI widget |
| 9 | Amulet | Reserved — no UI widget |

These will become visible when armor items are added (Phase 1 polish or Phase 2).

---

## What you should see, step-by-step

### Setup (one-time check, at edit-time / before pressing Play)

1. Open `3_Dungeon1.unity`.
2. Find the InventoryUI GameObject in the Hierarchy. Usually `Canvas/InventoryPanel` with an `InventoryUI` script component.
3. Inspector → confirm these three fields are wired:

   | Field | Should point to |
   |---|---|
   | Slot Prefab | `SlotWidget.prefab` from Project window |
   | Grid Parent | An empty GameObject configured as a 6-column Grid Layout Group |
   | Hotbar Parent | An empty GameObject with a Horizontal Layout Group (for 6 slots side-by-side) |
   | Equipment Parent | An empty GameObject with a Vertical Layout Group (for 4 stacked slots) |

4. At edit-time, these three "parent" GameObjects are **empty containers**. Their children (the actual slot widgets) only appear at Play time.

If any of these fields is empty (`None`), the corresponding slot group won't generate. That's the most common cause of "no inventory slots showing up."

### In Play mode

1. Press Play.
2. Open Inventory (Tab or I).
3. **You should see all THREE groups appear at once:**
   - Hotbar — 6 slots at the bottom
   - Grid — 48 slots in the center (6×8)
   - Equipment — 4 slots on the left

If you only see ONE group (e.g. only 4 slots on the left), the other two **container fields aren't wired** on the InventoryUI component.

### Test interactions

| Action | Expected |
|---|---|
| Press **I** (in Play) | Inventory panel opens |
| Press **Tab** | Same — inventory opens |
| Hover any slot | Tooltip appears showing item info (if slot has item) |
| Drag a Health Potion from Grid → Hotbar | Potion lands in hotbar; original grid slot empties |
| Press `1` on keyboard | If slot 1 has a consumable, it's used |
| Drag a Sword from Grid → MainHand equipment slot | Sword equips; player visual changes |
| Drag a Health Potion → MainHand | Rejected — not a weapon |
| Pick up a new item from the world | New item appears in the next empty grid slot |
| Drop an item by dragging it OUT of any slot | Item dropped on the ground at player's feet |

---

## "I only see 4 slots" — diagnostic

Based on your earlier message, you saw 4 slots when opening the inventory. Here's what those 4 likely are and what's possibly missing:

### Case A — You saw 4 slots on the LEFT side of the inventory

✅ Those are the **Equipment slots** (Head, Chest, MainHand, OffHand). 4 is correct.

**What might be missing:**
- The Grid (center) — should be 48 slots, big 6×8 panel
- The Hotbar (bottom-center) — should be 6 slots in a row

If you don't see the Grid or Hotbar, the `Grid Parent` or `Hotbar Parent` Inspector fields on InventoryUI are empty.

### Case B — You saw 4 slots at the BOTTOM of the screen

That'd be wrong — Hotbar should be 6 slots. But you DID see something… so either:

- The bottom slots are actually the Equipment slots (UI layout is bottom-aligned by accident)
- Or the hotbar parent's Layout Group only has 4 visible cells due to width constraints
- Or `HotbarSize = 4` somehow (it should be 6 — check `InventoryManager.cs` line 14)

### Case C — You saw 4 slots somewhere else

Tell me where exactly:
- Are they laid out vertically or horizontally?
- Are they on the left, right, top, bottom?
- Do they show icons or are they empty boxes?

---

## Fixing missing slot groups

If a slot group isn't appearing (e.g. no Grid):

1. Open `3_Dungeon1.unity`.
2. Hierarchy → find `InventoryPanel` or wherever the InventoryUI component lives.
3. Inspector → InventoryUI component → check which parent field is empty:
   - **Grid Parent empty** → no 48-slot grid renders
   - **Hotbar Parent empty** → no hotbar
   - **Equipment Parent empty** → no equipment column
4. To fix: each parent should be an empty child GameObject under the InventoryPanel with the right Layout Group component:

| Parent | Layout Group needed | Settings |
|---|---|---|
| Grid Parent | **Grid Layout Group** | Cell Size = (64, 64), Constraint = Fixed Column Count, Constraint Count = 6 |
| Hotbar Parent | **Horizontal Layout Group** | Spacing = 8, Child Force Expand off |
| Equipment Parent | **Vertical Layout Group** | Spacing = 8 |

5. If any parent GameObject doesn't exist, create one: right-click `InventoryPanel` → Create Empty → name appropriately → Add Component → relevant Layout Group → drag into the matching InventoryUI Inspector field.

---

## Visual mock — full inventory open

```
┌─────────────────────────────────────────────────────────┐
│                                                         │
│  [HEAD]   ┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐                            │
│  ┌────┐   └─┘└─┘└─┘└─┘└─┘└─┘                            │
│  │    │   ┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐                            │
│  └────┘   └─┘└─┘└─┘└─┘└─┘└─┘                            │
│                                                         │
│  [CHEST]  ┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐                            │
│  ┌────┐   └─┘└─┘└─┘└─┘└─┘└─┘                            │
│  │    │   ┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐                            │
│  └────┘   └─┘└─┘└─┘└─┘└─┘└─┘                            │
│                                                         │
│  [HAND]   ┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐                            │
│  ┌────┐   └─┘└─┘└─┘└─┘└─┘└─┘  ← Grid: 6 × 8 = 48 slots │
│  │ SWD│   ┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐                            │
│  └────┘   └─┘└─┘└─┘└─┘└─┘└─┘                            │
│                                                         │
│  [OFF]    ┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐                            │
│  ┌────┐   └─┘└─┘└─┘└─┘└─┘└─┘                            │
│  │    │   ┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐                            │
│  └────┘   └─┘└─┘└─┘└─┘└─┘└─┘                            │
│   ↑                                                     │
│   4 Equipment slots                                     │
│                                                         │
└─────────────────────────────────────────────────────────┘

(Outside inventory, always visible:)

           ┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐
           │1││2││3││4││5││6│   ← Hotbar: 6 slots, bottom-center
           └─┘└─┘└─┘└─┘└─┘└─┘
```

---

## Summary

**You should see:**
- 4 Equipment slots on the LEFT of the inventory panel (Head / Chest / MainHand / OffHand)
- 48 Grid slots in the CENTER (6 × 8)
- 6 Hotbar slots at the BOTTOM-CENTER of the screen (always visible)

**Tier 3 work is complete.** The number of slots is intentional:
- 4 equipment = current Phase 1 (5 more reserved for future armor)
- 48 grid = exact GDD spec
- 6 hotbar = QoL deviation from GDD's 4

**If anything is missing or different from this layout, tell me which group and where — I'll help you fix the wiring.**
