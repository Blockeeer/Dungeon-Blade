# Combo EXP Bonus Setup

Wires the GDD §9.2 "+25 EXP per 10-hit milestone" mechanic. Code is done — you just need to drop one component in the scene.

**Time:** ~5 min in Unity.
**GDD reference:** §9.2 ("Combo bonus (10+ hits): +25 EXP per 10-hit milestone").

---

## What's wired

**Code (already shipped):** `Assets/_Project/Combat/ComboExpBonus.cs`

This component:

- Subscribes to `ComboSystem.OnComboChanged` (fires each hit registered into the combo counter)
- Tracks the highest milestone awarded for the current combo (combo of 23 = milestone 2 at 20 hits)
- When the combo crosses a new multiple of `milestoneEvery` (default 10), calls `ExperienceSystem.GrantExperience(expPerMilestone)` (default 25)
- Resets the tracker when the combo breaks (`OnComboReset`)
- Logs `[Combo] X-hit milestone! +Y bonus EXP.` for each bonus granted

**Inspector tunables:**
- `Milestone Every` = `10` (GDD spec)
- `Exp Per Milestone` = `25` (GDD spec)

You can change those if you want a different curve later. Defaults match GDD.

---

## Step 1 — Find the ComboSystem GameObject

The ComboExpBonus component must live on the **same GameObject** as ComboSystem (it's `[RequireComponent(typeof(ComboSystem))]`).

1. Open `3_Dungeon1.unity`.
2. Hierarchy → search bar → type `t:ComboSystem` → press Enter.
3. The GameObject containing ComboSystem highlights. Most likely it's on:
   - `Player` (root) OR
   - `[Combat]` manager GameObject OR
   - some other gameplay manager

4. Click the GameObject that shows up.

If `t:ComboSystem` returns nothing, ComboSystem isn't in the scene → see "ComboSystem missing" below.

---

## Step 2 — Add ComboExpBonus component

1. With the ComboSystem GameObject selected, scroll the Inspector to the bottom.
2. **Add Component** → search `Combo Exp Bonus`.
3. Click `DungeonBlade.Combat.ComboExpBonus`.
dwwwwwwwwwa
The component appears with two fields. Leave them at defaults:

| Field | Value | Source |
|---|---|---|
| Milestone Every | `10` | GDD §9.2 spec |
| Exp Per Milestone | `25` | GDD §9.2 spec |

That's it for wiring. No other references needed — the component finds ComboSystem via `GetComponent` and ExperienceSystem via its singleton.

3. Save scene (Ctrl+S).

---

## Step 3 — Repeat for Lobby (optional)

If you want combo EXP bonuses to also work in the Lobby (unlikely since no enemies there, but for consistency):

1. Open `2_Lobby.unity`.
2. Repeat Step 1 + 2.

Skip if Lobby has no ComboSystem — bonuses only make sense in dungeons.

---

## Step 4 — Test

### 4a — Verify the bonus fires

1. Press Play → enter the Dungeon.
2. Find a zone with multiple enemies (Zone 2 Barracks works — 4 Skeleton Soldiers + 3 Archers).
3. Attack enemies in a chain (each successful hit registers into ComboSystem).
4. Open the Console.
5. **At 10 hits**, you should see:
   ```
   [Combo] 10-hit milestone! +25 bonus EXP.
   [EXP] +25 EXP. (X/100)
   ```
6. **At 20 hits**, same log fires again with `+25`.
7. **At 30 hits**, same.

### 4b — Verify the reset on combo break

1. Get to a combo of 12+.
2. Stop attacking and wait for the combo timeout (3 seconds default).
3. Console: `[Combo Counter HUD] OnComboReset` (or the combo counter fades).
4. Start a new combo — bonuses re-fire at 10 hits (not at hit 1 even though `_lastMilestoneAwarded` was 1 during the previous combo).

### 4c — Verify the HUD also shows it

The combo counter HUD you built in Tier 0 should also visibly count up. The EXP bonus is granted silently in the background, but you'll notice:
- EXP bar advances faster than usual during long combos
- Console logs `[Combo] X-hit milestone!`

---

## Common issues

| Symptom | Fix |
|---|---|
| `[Combo]` log never appears | ComboExpBonus component not added, OR ComboSystem is on a different GameObject |
| Console shows `[Combo] 10-hit milestone!` but EXP doesn't increase | ExperienceSystem.Instance is null. Confirm `[Experience]` GameObject exists in scene. |
| Bonus fires at hit 1 instead of 10 | `_lastMilestoneAwarded` not properly reset. Check the script wasn't modified. |
| Bonus fires repeatedly at every hit after 10 | Same as above. |
| Combo reaches 9 and stops | Combo timeout too short. Increase `ComboSystem.comboTimeoutSeconds` in Inspector. |

### ComboSystem missing entirely

If `t:ComboSystem` returns nothing in the scene:

1. Find your Player GameObject (search `t:PlayerStats`).
2. Inspector → Add Component → **Combo System**.
3. The Add Component for ComboExpBonus is now safe to do.

If you have a `[Combat]` manager pattern instead, add ComboSystem + ComboExpBonus there.

### EXP granted but no Tier 0 combo counter HUD update

The HUD subscribes to `ComboSystem.OnComboChanged` — independent of ComboExpBonus. If counter doesn't update but EXP bonuses fire, ComboCounterHUD's "Combo System" field is empty. Re-wire it.

---

## When done

Tell me **"done with #2"** and I'll:

1. Mark task #2 ✅ in [M9_REMAINING_RANKED.md](M9_REMAINING_RANKED.md).
2. Update [GDD_AUDIT.md §9.2](GDD_AUDIT.md) → "Combo bonus (10+ hits)" from ⏳ to ✅.
3. Advance to the next task (#3 VFX, or whatever you pick next).

---

## Reference

- [DungeonBlade GDD v1.md](DungeonBlade%20GDD%20v1.md) §9.2 — EXP sources
- [GDD_AUDIT.md](GDD_AUDIT.md) — current compliance status
- [M9_REMAINING_RANKED.md](M9_REMAINING_RANKED.md) — ranked task queue
- [COMBAT_SYSTEM.md](COMBAT_SYSTEM.md) — combo system overview
