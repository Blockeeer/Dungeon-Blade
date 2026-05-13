# GDD Fix Tier 4 — Boss Reward Table + Late-Stage Features

**Effort:** ~3-4 hours total
**Goal:** Bring loot drops, boss death cinematic, and small audio details in line with `DungeonBlade GDD v1.md` §6.2, §4.2, §12.

This tier covers the polish on boss kill rewards and the smaller GDD spec items that didn't fit earlier tiers.

---

## §1 — Boss Reward Table per GDD §6.2

**GDD spec:**

| Roll | Reward | Quantity |
|---|---|---|
| Guaranteed | Gold (150-300) | 1 roll |
| Guaranteed | Uncommon or Rare item (weapon or armor) | 1 item |
| 70% chance | Rare item (weapon or armor) | 1 item |
| 30% chance | Epic item | 1 item |
| 5% chance | Legendary: Warlord's Blade (unique sword) — **first kill only** | 1 item |
| 100% | Dungeon Clear Token | 1 token |

### 1a — Verify your current boss loot table

1. Find the LootTable ScriptableObject for the boss.
   - Project window → search `t:LootTable` → find one named `LootTable_UndeadWarlord` or similar.
2. Click it. Inspector shows the current drops.

If the boss doesn't have a dedicated LootTable, create one:
- Project window → right-click → **Create → DungeonBlade → Reward → Loot Table** → name `LootTable_UndeadWarlord`.
- Wire it on the UndeadWarlord boss's `EnemyBase.lootTable` field.

### 1b — Update loot rolls to match GDD

You'll need to set up the loot table's weighted rolls. The exact UI depends on how `LootTable.cs` is structured. Open the file and check:

1. Open `Assets/_Project/Rewards/Scripts/LootTable.cs`.
2. Read the public fields — what does the table store?

**Tell me what's in LootTable.cs** and I'll write the exact Inspector wiring for the GDD §6.2 percentages. The general approach:

- **Gold entry:** Item = none (gold), Quantity = random(150, 300), Drop chance = 100%
- **Guaranteed Uncommon/Rare entry:** weighted between specific Uncommon items (60%) and Rare items (40%)
- **70% Rare entry:** Drop chance = 70%, picks from Rare pool
- **30% Epic entry:** Drop chance = 30%, picks from Epic pool
- **5% Legendary entry:** Drop chance = 5%, picks from `Item_WarlordsBlade` (NEW item)
- **100% Token entry:** Drop chance = 100%, gives `Item_DungeonClearToken` (NEW item)

### 1c — Create missing items

GDD references items that may not exist yet in your project:

- [ ] **Item_WarlordsBlade** — unique Legendary sword (5% boss drop)
- [ ] **Item_DungeonClearToken** — token currency (100% boss drop)
- [ ] **Uncommon items** — at least 2-3 (the Boss Reward table needs Uncommon weapons/armor to roll)
- [ ] **Epic items** — at least 2-3 (for the 30% Epic roll)

Without these, you can't fully implement the loot table. Phase 1 minimum: create placeholder ScriptableObjects with names/icons. Real stats can be tuned later.

**Per-item checklist for creation:**

1. Project window → `Assets/_Project/Inventory/Items/`.
2. Right-click → **Create → DungeonBlade → Item → Weapon Item** (or similar — depends on your menu name).
3. Name: `Item_WarlordsBlade`.
4. Inspector → set:
   - Item Id = `warlords_blade`
   - Display Name = `Warlord's Blade`
   - Description = `Unique sword forged from the bones of the Undead Warlord. (Legendary)`
   - Type = Weapon
   - Equip Slot = MainHand
   - **Rank = Legendary** (per GDD §6.1 — legendary tier from the rarity system you just set up)
   - Stackable = ❌
   - Buy Value = 0 (drop-only)
   - Sell Value = 600 (high — Legendary tier)
5. Repeat for Item_DungeonClearToken (Type = Material, Stackable, no equip slot, MaxStack = 99).
6. Repeat for any Uncommon/Epic placeholders you want.

### 1d — Implement "first kill only" for Warlord's Blade

The 5% Legendary drop has a flag: "first kill only." Need a persistent boolean: "has the player ever defeated this boss before?"

This requires a save flag. Options:

- **Option A** — extend `PlayerProfile.cs` with `bool warlordsBladeDropped`.
- **Option B** — track in `SaveSystem` separately.

**Recommended Option A**. Tell me when ready and I'll add the field + wire it into the loot roll logic.

### 1e — Implement Dungeon Clear Token

This is a new currency type the GDD references in §6.2 and §8.2. It's used at the Bank's Token Exchange (deferred to Phase 2 per GDD).

For Phase 1 minimum:
- Token exists as an Item asset (created in §1c).
- Token drops 100% on boss kill (set in §1b).
- Token shows up in inventory like any other item.
- **Token Exchange UI in the Bank is deferred** to Phase 2 — GDD §8.2 lists "Dungeon Token Exchange" but doesn't specify Phase 1 UI for it.

---

## §2 — Boss death cinematic (GDD §4.2)

**GDD spec:** "Cinematic death animation (2-3 sec), then reward chest spawns in center of arena."

**Current:** Boss `Die()` runs immediately → loot drops at boss feet (no cinematic).

### 2a — Add a 2-3 sec death delay

Open `Assets/_Project/Boss/Scripts/BossBase.cs`. Find the `Die()` method (it should be `protected override void Die()` overriding EnemyBase):

```csharp
protected override void Die()
{
    Phase = BossPhase.Dead;
    OnBossDefeated?.Invoke();
    base.Die();
}
```

**Tell me when ready** and I'll add a coroutine that:
- Plays the death animation (2.5s default)
- Disables boss collisions/AI during the animation
- Spawns the reward chest at arena center after the delay
- Camera optionally pans to the chest

### 2b — Spawn reward chest at arena center

Currently loot drops at the boss's feet. GDD says "reward chest spawns in center of arena."

This needs:
- A reference to the arena center point (Transform in scene)
- Instantiate a chest prefab at that point on boss death
- Chest contains the loot table rolls from §1

Add a `[SerializeField] Transform chestSpawnPoint;` to BossArenaTrigger or BossBase, wire it in Inspector.

### 2c — Optional polish

- **Slow-mo on death:** `Time.timeScale = 0.3f` for 1 second after boss dies
- **Camera shake** on death (already wired via CameraShake — extend to also fire on boss death)
- **Screen flash** white briefly when boss HP hits 0

These are optional cinematic flourishes. The minimum GDD spec is just the 2-3 second delay.

---

## §3 — Boss roar SFX at phase transitions (GDD §12)

**GDD spec:** "Boss roar: plays at each phase transition. Distinct and impactful."

**Current:** Camera shake fires on phase change, but no audio cue.

### 3a — Get a boss roar audio clip

1. Pixabay/Freesound search: `"monster roar"`, `"demon scream"`, `"dragon roar"`.
2. Save as `BossRoar.ogg` in `Assets/_Project/Audio/SFX/`.
3. Import settings: Vorbis, Decompress on Load, Preserve Sample Rate.

### 3b — Wire to phase transitions

Open `Assets/_Project/Boss/Scripts/BossBase.cs`. The `EnterPhase` method fires `OnPhaseChanged?.Invoke(next)`.

Subscribers can listen to this and play the roar. Easiest path: extend `AudioBindings.cs` to subscribe to boss phase changes.

**Tell me when ready** and I'll add:
- A `BossBase boss` field on `AudioBindings`
- Subscribe to `boss.OnPhaseChanged` → play roar on Phase2 and Phase3 entry
- An AudioClip slot for `bossRoar` in the Inspector

### 3c — Wire in Unity

After the code change:
1. Open `3_Dungeon1.unity`.
2. Click `[AudioBindings]` GameObject.
3. Inspector → drag UndeadWarlord into the new `Boss` field.
4. Drag `BossRoar.ogg` into the new `Boss Roar` AudioClip field.

---

## §4 — Additional GDD §12 audio (gold pickup, checkpoint, parry)

Minor SFX gaps:

### 4a — Gold pickup sound

Already partially wired (item pickup sound plays for inventory changes). GDD calls out a distinct **gold collect** sound separate from item pickup.

- Get a "coin pickup" clip (Pixabay: `coin chime`).
- Add `goldPickup` AudioClip slot to AudioBindings.
- Subscribe to `PlayerWallet.OnGoldChanged` (only fires on increase, not decrease).

### 4b — Checkpoint reached sound

GDD §11.1 mentions a "checkpoint indicator: brief top-center notification." Add an audio cue too.

- Get a "level up chime" or "checkpoint" clip.
- Subscribe to `RespawnManager.OnCheckpointReached` event (may need to add this event to RespawnManager).
- Or fire from `Checkpoint.OnTriggerEnter` when newly activated.

### 4c — Parry sound

GDD §12: "different sound for hit vs. parry vs. miss."

- Currently swing miss and hit sounds work (after Tier 0 dash-attack fix).
- Add a parry-specific clip ("metal clang") that plays when `Sword.IsParryActive` returns true and an attack is parried in `PlayerStats.ApplyDamage`.
- Need a small code change in PlayerStats to fire an event on parry, then AudioBindings subscribes.

---

## Action plan summary

Three big sub-tasks + smaller polish:

| Task | Effort | Priority |
|---|---|---|
| §1 Boss Reward Table | ~2 hr (incl. creating missing items) | High — GDD-explicit |
| §2 Boss death cinematic | ~30 min | Medium — feels-good polish |
| §3 Boss roar SFX | ~15 min | Low effort, high impact |
| §4a Gold pickup sound | ~10 min | Low priority |
| §4b Checkpoint sound | ~15 min | Low priority |
| §4c Parry sound | ~20 min | Low priority |

### Recommended sequence

1. **§3 first** (boss roar) — quickest, biggest "wow" payoff at phase transitions.
2. **§2** (death cinematic) — small code change, feels great.
3. **§1** (boss reward table) — largest task, do last so you're not blocked by missing items.
4. **§4** (small audio polish) — interleave as time allows.

---

## When ready

Tell me which sub-task you want to start, and I'll handle the code changes. Examples:

- **"§3 boss roar"** — I'll extend AudioBindings.
- **"§2 boss cinematic"** — I'll add the coroutine to BossBase.
- **"§1 boss reward table — show me LootTable.cs first"** — I'll read it and tell you the exact Inspector wiring.

---

## When done — mark Tier 4 complete

1. Update `GDD_AUDIT.md` §4.2, §6.2, §12 — relevant items from 🟡/⏳ to ✅.
2. **Phase 1 GDD compliance is now substantially complete.** Remaining gaps are out-of-scope (skills, mini-map, dungeon clear screen).
3. Phase 1 ready for QA per [DungeonBlade GDD v1.md §15 Approval Criteria].

---

## Reference

- [DungeonBlade GDD v1.md](DungeonBlade%20GDD%20v1.md) §4.2, §6.2, §12
- [GDD_AUDIT.md](GDD_AUDIT.md)
- [ITEM_RARITY_SETUP.md](ITEM_RARITY_SETUP.md) — needed for §1 (creating Legendary/Epic items)
- [AUDIO_SETUP.md](AUDIO_SETUP.md) — needed for §3 and §4 (SFX wiring)
