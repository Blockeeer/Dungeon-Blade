# Tier 4 — Unity Setup Steps

Code work is done for §2, §3, §4 (boss roar, boss cinematic, gold/checkpoint/parry SFX, parry event). This doc covers the Unity-side wiring + asset downloads you need to do.

§1 (Boss Reward Table) is deferred — see [GDD_FIX_TIER4_LOOT.md](GDD_FIX_TIER4_LOOT.md) when ready.

---

## Step 1 — Download 4 audio clips (~10 min)

Drop these into `Assets/_Project/Audio/SFX/`:

| Filename | Pixabay search term | Length |
|---|---|---|
| `BossRoar.ogg` | `"monster roar"` or `"dragon scream"` | 1-2 sec |
| `GoldPickup.ogg` | `"coin pickup chime"` | 0.3-0.5 sec |
| `CheckpointReached.ogg` | `"checkpoint save chime"` or `"level up"` | 0.5-1 sec |
| `SwordParry.ogg` | `"metal clang"` or `"sword block"` | 0.3 sec |

**Import settings (same as other SFX):**
- Load Type: `Decompress on Load`
- Compression: `Vorbis`
- Quality: `70` (default)
- Force Mono: ❌ (stereo fine)

If you don't have time to grab clips now, **skip — code works with empty fields**, sounds just don't play.

---

## Step 2 — Wire AudioBindings fields

1. Open `3_Dungeon1.unity`.
2. Hierarchy → click `[AudioBindings]` (the GameObject hosting AudioBindings).
3. Inspector → AudioBindings component → wire the **new** fields:

   | Field | Drag |
   |---|---|
   | Boss | UndeadWarlord GameObject in Hierarchy (for boss roar event) |
   | Wallet | `[Bank]` or wherever PlayerWallet lives (for gold pickup sound) |
   | Sword Parry | `SwordParry.ogg` from Project window |
   | Gold Pickup | `GoldPickup.ogg` |
   | Checkpoint Reached | `CheckpointReached.ogg` |
   | Boss Roar | `BossRoar.ogg` |

4. Save scene.

**Existing fields (already wired from Tier 1):**
- Player Stats / Player Movement / Player Combat / Inventory — leave as-is

---

## Step 3 — Wire boss death cinematic on UndeadWarlord

The boss now has a 2.5-second death cinematic delay + optional chest spawn.

1. Open `3_Dungeon1.unity`.
2. Hierarchy → click the `UndeadWarlord` GameObject.
3. Inspector → **Undead Warlord (Script)** component (or `BossBase` inherited fields).
4. Expand the new **Death Cinematic** section. Wire:

   | Field | Value |
   |---|---|
   | Death Cinematic Duration | `2.5` (GDD says 2-3 sec) |
   | Reward Chest Prefab | leave **empty** for now (no chest prefab exists yet — boss just drops loot at feet after the 2.5s delay) |
   | Chest Spawn Point | leave empty (irrelevant without prefab) |

**Without a chest prefab:** boss dies → 2.5s delay → standard loot table drops at boss feet (same as before, just with cinematic delay). Cleaner than nothing.

**With a chest prefab (later when authored):**
- Tier 4 §1 will create `BossChest.prefab` containing a Chest + LootTable reference to `Loot_BossChest`
- Wire that prefab here
- Create `ChestSpawnPoint` empty GameObject at arena center, wire as Chest Spawn Point

5. Save scene.

---

## Step 4 — Test the setup

### 4a — Boss roar test

1. Press Play → enter boss arena.
2. Damage boss to 66% HP → Console: `[Boss] UndeadWarlord transitioning → Phase2`.
3. **You should hear the boss roar** (if clip wired).
4. Damage to 33% → same roar plays again (Phase3 entry).
5. **If silent:** check AudioBindings → Boss field is wired + Boss Roar AudioClip is assigned.

### 4b — Boss death cinematic test

1. Continue damaging boss to 0 HP.
2. Boss dies → stops moving for 2.5 seconds → then loot drops.
3. **Visual:** boss stays in place during the delay (Agent.isStopped = true).
4. **If boss dies and immediately drops loot with no delay:** check Death Cinematic Duration = 2.5.

### 4c — Gold pickup test

1. Kill any enemy that drops gold.
2. Walk over the gold pile.
3. **Hear "coin chime"** when gold amount goes up.
4. Spend gold at the shop → **no sound** (only fires on gain).

### 4d — Checkpoint test

1. Walk to any checkpoint you haven't crossed yet.
2. **Hear chime + see** `[Respawn] Checkpoint reached: cp_1` in Console.
3. Cross same checkpoint again → silent (only fires on first activation per scene).

### 4e — Parry test

1. Equip sword. Stand near an enemy.
2. As an attack lands, press RMB to block (within the 0.18s parry window).
3. Console: `[Player] Parried Melee attack from SkeletonSoldier`.
4. **Hear metal clang** SFX.

---

## Step 5 — Mark Tier 4 §2-§4 done

Once tests pass:

1. Update `GDD_AUDIT.md` §12 — change rows from 🟡 to ✅ for boss roar, gold collect, checkpoint, parry sound.
2. Update `GDD_AUDIT.md` §4.2 — change "cinematic death animation" from ⏳ to ✅.

Tier 4 §1 (Boss Reward Table) remains deferred until you author the missing items (Warlord's Blade, Dungeon Clear Token, Uncommon/Epic placeholders).

---

## What's deferred to §1 (when ready)

Per [GDD_FIX_TIER4_LOOT.md §1](GDD_FIX_TIER4_LOOT.md):

- Create `Item_WarlordsBlade` (Legendary, MainHand, "first kill only")
- Create `Item_DungeonClearToken` (Material, currency)
- Create 2-3 Uncommon weapon items
- Create 2-3 Epic weapon items
- Rebuild `Loot_UndeadWarlord` LootTable to match GDD §6.2 percentages
- Add "first kill" flag to PlayerProfile + check in loot roll
- (Optional) Build BossChest prefab and wire to UndeadWarlord's Reward Chest Prefab field

Tell me when you're ready for this — it's the heaviest sub-task (~2 hours).

---

## Reference

- [DungeonBlade GDD v1.md](DungeonBlade%20GDD%20v1.md) §4.2, §12
- [GDD_AUDIT.md](GDD_AUDIT.md)
- [GDD_FIX_TIER4_LOOT.md](GDD_FIX_TIER4_LOOT.md) — the full Tier 4 doc (§1 still pending)
