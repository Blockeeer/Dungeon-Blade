# GDD Fix Tier 1 — Numeric Data Alignment

**Effort:** ~15 minutes total (Inspector-only changes, no code)
**Goal:** Bring runtime values in line with `DungeonBlade GDD v1.md` exact numbers.

This tier has 4 sections. Do them in order — each one is independent.

---

## §1 — Experience System (GDD §9.1)

**GDD spec:**
- Level cap = **20** (currently 10)
- **+5 max HP** per level (currently +10)
- **+2 max stamina** per level (currently +5)

### Where to find the component

ExperienceSystem is a singleton — it lives on a GameObject in the **Dungeon scene** (where leveling matters).

1. Open `3_Dungeon1.unity`.
2. In the Hierarchy search bar, type `t:ExperienceSystem` → press Enter.
3. The matching GameObject is highlighted (likely named `[Experience]` or similar).
4. Click it.

If `t:ExperienceSystem` returns nothing → ExperienceSystem isn't in the scene. Add it: Hierarchy → Create Empty → name `[Experience]` → Add Component → **Experience System**.

### Fields to change

In the Inspector → **Experience System (Script)** component:

| Field | Current | Set to | GDD reference |
|---|---|---|---|
| Level Cap | 10 (constant in code) | **20** | §9.1 |
| Max Hp Per Level | 10 | **5** | §9.1 |
| Max Stamina Per Level | 5 | **2** | §9.1 |

**Wait — `LevelCap` is a `const int` in code, not a serialized field.** You can't edit it in Inspector. We need a code change.

Skip the LevelCap change for now — I'll handle that in a code update after you do the other 2 fields. The other 2 (`maxHpPerLevel`, `maxStaminaPerLevel`) ARE Inspector fields.

### Save

After changing the 2 Inspector fields → Ctrl+S to save the scene.

---

## §2 — Enemy HP per GDD §4.1

**GDD spec:**

| Enemy | GDD HP |
|---|---|
| Skeleton Soldier | **30** |
| Skeleton Archer | **20** |
| Armored Knight | **80** |
| Undead Warlord (Boss) | **400** |

### Where HP is stored

Enemy stats are **inlined on each prefab** as a serialized `EnemyStats` field on the `EnemyBase` component (not in a separate ScriptableObject asset). So you edit the prefabs directly.

### File paths

The enemy prefabs are at:

- `Assets/_Project/Enemies/Prefabs/Skeleton_Soldier.prefab`
- `Assets/_Project/Enemies/Prefabs/Skeleton_Archer.prefab`
- `Assets/_Project/Enemies/Prefabs/Armored_Knight.prefab`

The boss prefab is in the scene directly (no standalone prefab) — find it in the Dungeon scene hierarchy.

### Fix each enemy prefab

For each of the 3 regular enemy prefabs:

1. In the Project window, navigate to `Assets/_Project/Enemies/Prefabs/`.
2. Click the prefab (e.g. `Skeleton_Soldier`).
3. Inspector → find the enemy script component (e.g. `Skeleton Soldier (Script)`).
4. The script has an inlined **Stats** section (foldout, may need to expand by clicking the ▶ triangle).
5. Inside Stats, find the **Health** section → **Max Health** field.
6. Set the value per the table above.
7. Press Enter to save.

**Per-prefab checklist:**

- [ ] `Skeleton_Soldier.prefab` → Stats → Max Health = `30`
- [ ] `Skeleton_Archer.prefab` → Stats → Max Health = `20`
- [ ] `Armored_Knight.prefab` → Stats → Max Health = `80`

### Fix the boss (in-scene, no standalone prefab)

1. Open `3_Dungeon1.unity`.
2. Hierarchy → find `UndeadWarlord` GameObject (or search `t:UndeadWarlord` if you renamed it).
3. Click it.
4. Inspector → **Undead Warlord (Script)** component → Stats foldout → Max Health = `400`.
5. Save the scene (Ctrl+S).

### Verify

After setting all 4:
1. Press Play in `3_Dungeon1.unity`.
2. Find an enemy → attack it.
3. Console should log damage: `[SkeletonSoldier] -12 (Melee) HP: 18/30` (the `/30` confirms the new max HP).
4. If you see `/100` or some other number, the prefab wasn't saved or you edited a different instance.

---

## §3 — Enemy EXP rewards per GDD §9.2

**GDD spec:**

| Enemy | GDD EXP |
|---|---|
| Skeleton Soldier kill | **15** |
| Skeleton Archer kill | **15** |
| Armored Knight kill | **60** |
| Undead Warlord (Boss) kill | **300** |

### Where EXP is stored

EXP rewards live on the **LootTable ScriptableObject** for each enemy. Each enemy has a `lootTable` field on its `EnemyBase` script that points to a specific LootTable asset.

### File paths

The 4 loot table assets are at:

- `Assets/_Project/Rewards/Loot_SkeletonSoldier.asset`
- `Assets/_Project/Rewards/Loot_SkeletonArcher.asset`
- `Assets/_Project/Rewards/Loot_ArmoredKnight.asset`
- `Assets/_Project/Rewards/Loot_UndeadWarlord.asset`

### Fix each LootTable asset

For each of the 4:

1. In the Project window, navigate to `Assets/_Project/Rewards/`.
2. Click the LootTable asset (e.g. `Loot_SkeletonSoldier`).
3. Inspector → find the **Experience** field (might be labeled `experience` or `Exp Reward`).
4. Set the value per the table.
5. Press Enter to save.

**Per-asset checklist:**

- [ ] `Loot_SkeletonSoldier.asset` → Experience = `15`
- [ ] `Loot_SkeletonArcher.asset` → Experience = `15`
- [ ] `Loot_ArmoredKnight.asset` → Experience = `60`
- [ ] `Loot_UndeadWarlord.asset` → Experience = `300`

### Special case — bonus EXP

GDD §9.2 also specifies:
- Dungeon clear bonus: +200 EXP (not yet implemented — Tier 4 work)
- First clear bonus: +100 EXP (not yet implemented)
- Combo bonus: +25 EXP per 10-hit milestone (not yet implemented)

**For Tier 1, ignore those — they're separate features.** Tier 1 only covers per-kill EXP.

### Verify

1. Press Play → kill a Skeleton Soldier.
2. Console should log `[EXP] +15 EXP. (15/100)`.
3. If you see `+10` or another value, the asset wasn't saved.

---

## §4 — Checkpoint Heal Value per GDD §3.3

**GDD spec:**
- Checkpoints **restore stamina** (full)
- Checkpoints grant **+20 HP** on touch

### Where to find checkpoints

Each checkpoint is a GameObject in `3_Dungeon1.unity` with the `Checkpoint` component.

1. Open `3_Dungeon1.unity`.
2. Hierarchy search bar → type `t:Checkpoint` → Enter.
3. All Checkpoint GameObjects highlight. GDD says you need 2 (after Zone 1, after Zone 3).

### Fix each one

For each Checkpoint:

1. Click it in the Hierarchy.
2. Inspector → **Checkpoint (Script)** component.
3. Find the **Heal On Reach** field.
4. Set to **`20`** per GDD.

**Per-checkpoint checklist (GDD §3.3 lists 2):**

- [ ] Checkpoint after Zone 1 (Gate Hall) → Heal On Reach = `20`
- [ ] Checkpoint after Zone 3 (The Bridge) → Heal On Reach = `20`

**If you have more than 2 checkpoints in your scene:** the GDD only specifies 2. You can either:
- **Option A** — delete the extra checkpoints (strict GDD compliance, harsher difficulty)
- **Option B** — keep them (easier playtesting, document deviation)

For Tier 1, recommend leaving extras in place. Just set their HealAmount to 20 too. Document the deviation as "extra checkpoints retained for testing convenience" in GDD_AUDIT.md.

### Stamina restoration verification

The existing Checkpoint script should already call `_stats.RestoreStamina(max)` on touch — but let me confirm. If you press Play, walk to a checkpoint, and your stamina jumps to full → ✅ working. If stamina stays where it was → flag it and tell me; we'll patch the Checkpoint script.

### Save

Ctrl+S after editing all checkpoints.

---

## Code change needed for §1 (LevelCap)

Since `LevelCap` is a `const int` in code (not Inspector-editable), I'll change it after you finish the Inspector-only items above. **Don't touch the code yourself** — let me handle it when you're ready.

---

## When done — verification

Press Play in `3_Dungeon1.unity`:

| Action | Expected (after Tier 1 fixes) |
|---|---|
| Hit a Skeleton Soldier | Console: `HP: X/30` (not /100 or /50) |
| Kill the Skeleton Soldier | Console: `[EXP] +15 EXP. (15/100)` |
| Hit an Armored Knight | `HP: X/80` |
| Kill the Knight | `[EXP] +60 EXP.` |
| Hit the Boss | `HP: X/400` |
| Cross a checkpoint when wounded | HP increases by 20 + stamina fills to max |
| Gain 100 EXP | Level up logs |
| At level 2 | Max HP went up by 5 (not 10); Max stamina up by 2 (not 5) |

---

## Mark this tier done

When all 4 sections pass verification:

1. Update `GDD_AUDIT.md` — mark §4.1 (enemy HP), §9.1 (level scaling), §9.2 (EXP per kill), §3.3 (checkpoint heal) all from 🟡 to ✅.
2. Tell me — I'll do the `LevelCap` code change (small, ~30 sec).
3. Then move to **Tier 2: HUD GDD Compliance** → `GDD_FIX_TIER2_HUD.md`.

---

## Reference

- [DungeonBlade GDD v1.md](DungeonBlade%20GDD%20v1.md) §3.3, §4.1, §9.1, §9.2
- [GDD_AUDIT.md](GDD_AUDIT.md) — full audit context
