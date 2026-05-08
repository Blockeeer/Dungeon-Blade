# Zone Gates Setup — Dungeon Blade

Stops players from rushing past mobs to the boss. Each zone's enemies must die before the gate to the next zone opens.

**Final flow:**
- Player spawns → free in Zone 1.
- Kill all Zone 1 mobs → Gate 1 opens → walk to Zone 2.
- Kill Zone 2 mobs → Gate 2 opens.
- Repeat through Zone 4.
- Kill Zone 4 mobs → Boss arena gate opens → walk into boss arena trigger.

---

## Step 1 — Wire enemies to each Zone

The Zone script needs to know which enemies belong to it.

### 1a — Find your zone GameObjects

The PHASE1_SETUP convention is to name zone containers `Zone_1_GateHall`, `Zone_2_Barracks`, etc. The actual **Zone component + BoxCollider trigger** is on a child GameObject (typically named `ZoneTrigger`) inside each container.

So the structure looks like:
```
Zone_1_GateHall                    ← parent container (no Zone component)
├── ZoneTrigger                    ← has Zone (Script) + BoxCollider ← this is what we wire
├── (enemies, props, checkpoints, etc.)
```

**Quickest way to find the actual Zone-bearing GameObjects:**

1. Open `3_Dungeon1.unity`.
2. In the Hierarchy window, click the search bar at the top.
3. Type `t:Zone` and press Enter.
4. The Hierarchy filters to ONLY GameObjects that have a Zone component — these are your `ZoneTrigger` GameObjects (one per zone).
5. You should see 4-5 entries.
6. **Clear the search** (click the X) when done — you'll need the full Hierarchy to find enemies in Step 1b.

If `t:Zone` shows nothing → no zone has the Zone component yet. See Step 1c.
If `t:Zone` shows fewer entries than zones → some `ZoneTrigger` GameObjects are missing the component. See Step 1c.

### 1b — Wire enemies (per zone)

Once you've identified the ZoneTrigger GameObjects from Step 1a:

1. Click the `ZoneTrigger` inside `Zone_1_GateHall` (the one with the Zone component on it).
2. Inspector → find **Zone (Script)** component (below Transform and Box Collider).
3. Find the **Enemies** list field. Initially empty.
4. Click the lock icon on the Inspector (top-right padlock) to keep it focused on this ZoneTrigger.
5. **Clear the Hierarchy search** if you still have `t:Zone` filter active — you need to browse the full Hierarchy now.
6. Find every enemy GameObject inside Zone 1. They're typically children of `Zone_1_GateHall` OR siblings — depends on how the dungeon was structured.
7. Multi-select the enemies (Ctrl+click each), then **drag** them onto the Enemies list field in the locked Inspector.
8. The list expands showing the entries.
9. Unlock the Inspector (click the padlock again).

Repeat for the ZoneTrigger inside `Zone_2_Barracks`, `Zone_3_*`, `Zone_4_Armory`, etc.

### 1c — If a zone is missing the Zone component

For any zone GameObject that doesn't have a Zone component:

1. Click the GameObject (e.g. `Zone_3_*`).
2. Inspector → confirm it has:
   - Transform
   - Box Collider (with Is Trigger ✅)
3. If Zone component is missing → **Add Component** → search `Zone` → click `DungeonBlade.Dungeon.Zone`.
4. Inspector → set **Zone Id** = `zone_3` and **Zone Name** = `(zone name like "Throne Approach")`.
5. Then continue Step 1b for that zone.

If the GameObject doesn't even have a Box Collider, you'll need to add one (right size to cover the zone area, Is Trigger ✅) before adding the Zone component.

**Tip:** for clean Hierarchy layout, parent each zone's enemies under the Zone GameObject. Then you can quickly select all children with Shift-click on first + last child.

### What if a zone has no enemies?

Zones with empty Enemies list are **considered cleared from the start**. Their gate (if any) opens immediately. Useful for a "safe room" zone or the spawn zone.

---

## Step 2 — Build a gate between two zones

A gate is a physical blocker (wall/door) at the boundary between zones. Player can't pass until it opens.

### 2a — Create the blocker GameObject

1. Hierarchy → right-click → **3D Object → Cube** → name `Gate_Zone1_to_Zone2`.
2. Position it in the doorway/corridor between Zone 1 and Zone 2.
3. Scale to fit the doorway (e.g. `(4, 3, 0.5)` for a wall slab).
4. Add Component → **Box Collider** (default — leave Is Trigger ❌, the player must physically bump into it).
5. (Optional) Tint the material red or add a glow effect to signal "locked."

### 2b — Add the ZoneGate script

The gate script lives on a parent GameObject (so the wall can be a child that gets disabled).

1. Hierarchy → right-click → Create Empty → name `Gate_Zone1_to_Zone2_Manager`.
2. **Drag** the `Gate_Zone1_to_Zone2` cube **inside** as a child.

Hierarchy structure:
```
Gate_Zone1_to_Zone2_Manager   ← has ZoneGate component
└── Gate_Zone1_to_Zone2       ← the visible blocker, enabled/disabled by ZoneGate
```

3. Click `Gate_Zone1_to_Zone2_Manager` → Add Component → **Zone Gate**.
4. Inspector wiring:

   | Field | Value |
   |---|---|
   | Required Zone | drag `Zone_1` from Hierarchy |
   | Blockers | drag the cube `Gate_Zone1_to_Zone2` into element 0 |
   | Open Visual | (optional) leave empty for now |
   | Open Delay | `0.5` |

5. Save scene.

### 2c — Test this single gate

1. Press Play → spawn in Zone 1.
2. Walk past the gate — should be **blocked** (you collide with the cube).
3. Kill all Zone 1 enemies. Console: `[Zone] Zone 1: enemy down. X remaining.` then `[Zone] Zone 1 CLEARED.` then `[ZoneGate] Gate_Zone1_to_Zone2_Manager opened — Zone 1 cleared.`
4. Walk through — gate is now passable.

If working, repeat for Zones 2→3, 3→4, and 4→Boss.

---

## Step 3 — Build the gate to the boss arena

Same pattern. The "required zone" is Zone 4 (the last zone before the boss).

1. Build a `Gate_Zone4_to_Boss_Manager` GameObject (with a child blocker cube) at the boss arena entrance.
2. ZoneGate component:

   | Field | Value |
   |---|---|
   | Required Zone | drag `Zone_4` |
   | Blockers | drag the boss-gate cube |
   | Open Delay | `1.5` (slightly longer for dramatic effect — last gate before boss) |

3. Save scene.

**Important:** the existing `BossArenaTrigger` (the trigger that seals the player in and starts the boss fight) stays separate — that's a different mechanism (triggers when player walks INTO the arena, not when they unlock the entrance).

So the boss flow is now:
1. Clear Zone 4 → Boss gate opens.
2. Walk through → cross the BossArenaTrigger → arena seals + boss activates + boss music starts.

---

## Step 4 — Optional polish

### 4a — Add visual feedback when a gate opens

In the Inspector field **Open Visual** on each ZoneGate:
1. Create an empty GameObject as a child (e.g. a particle effect or a "door opens" animation).
2. Drag it into the Open Visual slot.
3. The script auto-enables it when the gate opens.

### 4b — Multiple blockers per gate

If your gate is multiple GameObjects (e.g. left door + right door + frame), drag them all into the Blockers array:
1. Click `Gate_Manager`.
2. Inspector → ZoneGate → Blockers array → click `+` to add slots.
3. Drag each piece into its own slot.

### 4c — Add "X enemies remaining" UI text

If you want HUD feedback like "Zone 1: 3/3 enemies remaining," subscribe to `Zone.EnemyCountChanged` event from a HUD script. Skip for now — Phase 1 polish, not critical.

---

## Step 5 — Verify everything

1. Press Play → enter Dungeon.
2. Console output as you progress:

| Action | Expected |
|---|---|
| Walk to Gate 1 | Blocked physically |
| Kill all Zone 1 enemies | `[Zone] Zone 1: enemy down. 2 remaining.` (then 1, then 0) → `[Zone] Zone 1 CLEARED.` → `[ZoneGate] Gate_Zone1_to_Zone2 opened` |
| Walk through Gate 1 | Passes through (gate disabled) |
| Kill Zone 2-4 enemies | Each gate opens after its zone clears |
| Reach boss arena gate | Opens after Zone 4 cleared |
| Walk into BossArenaTrigger | Arena seals, boss activates, boss music plays |

---

## Common issues

- **Gate never opens** → Required Zone field is empty, or the zone has no enemies wired in. Check Step 1 + 2b.
- **Gate opens but player walks through anyway** → Blockers array is empty. Drag the cube into the Blockers slot.
- **Gate opens immediately on Play** → the zone has no enemies in its list. Either wire them, or accept it's an "auto-open" gate.
- **Gate stays closed even after killing everything** → some enemies aren't in the Zone's Enemies list, so the count never hits zero. Double-check the list.
- **Player gets stuck on top of the gate** → the cube collider has Is Trigger ❌ (correct), but the cube might be too tall or angled. Reduce height or move out of the way.
- **Console shows zone clear but gate doesn't open** → Required Zone field on ZoneGate points to a different Zone (zone_1 vs Zone_1 case-sensitivity, or wrong instance). Re-wire.

---

## Recommended layout per zone

For Phase 1 dungeon balance:

| Zone | Enemies | Gate after |
|---|---|---|
| Zone 1 (entrance) | 3 SkeletonSoldiers | Gate 1→2 |
| Zone 2 | 2 SkeletonSoldiers + 1 SkeletonArcher | Gate 2→3 |
| Zone 3 | 1 ArmoredKnight + 2 SkeletonSoldiers | Gate 3→4 |
| Zone 4 (pre-boss) | 1 ArmoredKnight + 1 SkeletonArcher | Gate 4→Boss |
| Boss arena | UndeadWarlord (no list — boss is separate) | (no exit gate) |

Adjust enemy counts to taste. ~2-4 enemies per zone is a good fight-length target.

---

## What's next after gates work

1. **Test the full dungeon flow** — start to finish, kill mobs, beat boss.
2. **Music: drop your boss track in** + wire per [MUSIC_SETUP.md §4](MUSIC_SETUP.md).
3. **Animations** — wire the cherry-picked PlayerAnimator.controller to the Player.
