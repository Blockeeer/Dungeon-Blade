# GDD Compliance Audit — Phase 1

Comparison of current implementation against `DungeonBlade GDD v1.md`. Lists every mismatch found during the audit, prioritized.

**Legend:**
- ✅ Aligned with GDD
- 🟡 Minor deviation, easy fix
- 🔴 Significant deviation, refactor required
- ⏳ Not yet implemented (deferred per Phase 1 scope)

---

## §2.1 Movement System

| GDD spec | Current | Status |
|---|---|---|
| WASD movement, mouse aim | ✅ | ✅ |
| Single jump + double jump | ✅ (maxAirJumps = 1) | ✅ |
| Dash via Shift OR double-tap A/D | ✅ (Shift = roll, double-tap A/D = sideways dash) | ✅ |
| Wall running 1-2 sec | ✅ (wallRunMaxDuration = 1.5) | ✅ |
| Bunny Hop momentum chain | ✅ (bhop window + gain) | ✅ |
| Slide on crouch+run | ✅ | ✅ |

**Verdict:** ✅ Movement is GDD-aligned.

---

## §2.2 Combat System

After deeper code audit + Phase 1 polish pass, status revised:

| GDD spec | Current | Status |
|---|---|---|
| 3-hit light combo | ✅ Sword.lightDamage[3] with chain window | ✅ |
| Heavy attack on hold | ✅ StartHeavyHold / TryReleaseHeavy | ✅ |
| Dash-attack (dash + heavy) | ✅ Sword.StartDashAttack — Fire during dash i-frames triggers boosted lunge | ✅ |
| Block / Parry | ✅ RMB block + 0.18s parry window in PlayerStats.ApplyDamage | ✅ |
| Sword skills (Section 4) | ⏳ Skills system deferred (GDD says Phase 2) | ⏳ |
| Primary gun (semi-auto) | ✅ | ✅ |
| Secondary gun (heavy) | ⏳ Only one gun slot — second slot deferred | ⏳ |
| Reload on R | ✅ | ✅ |
| Aim-down-sights on RMB | ✅ RMB context-switches: sword=block, gun=ADS (PlayerCombat dispatches per active weapon) | ✅ |
| Sword + gun cancel combos | ✅ TryGunZCancel | ✅ |
| Combo counter HUD | ✅ ComboCounterHUD (top-center, fades 3s, pulses on hit) | ✅ |
| Mouse wheel weapon switch | ✅ Bound to Mouse/scroll/y in addition to Q | ✅ |

**Verdict:** ✅ Combat is fully Phase-1 aligned with GDD. See [COMBAT_SYSTEM.md](COMBAT_SYSTEM.md) for setup/tuning. Skills + secondary gun deferred per GDD scope.

---

## §2.3 Health, Stamina & Defense

| GDD spec | Current | Status |
|---|---|---|
| Base 100 HP | ✅ | ✅ |
| HP regenerates out of combat | ✅ | ✅ |
| Stamina for dashes/wall runs/heavy | ✅ | ✅ |
| Stamina regens quickly | ✅ | ✅ |
| Death → respawn at last checkpoint with 50% HP | ✅ (respawnHealthPercent = 0.5) | ✅ |
| Limited respawns (3 per dungeon run) | ✅ (maxRespawnsPerRun = 3) | ✅ |
| Armor reduces incoming damage | ⏳ Armor system not yet implemented | ⏳ |

**Verdict:** ✅ Core stats aligned; armor is a Phase 1 gap.

---

## §3.2 Dungeon Structure (5 Zones)

| GDD Zone | GDD Enemies | Current | Status |
|---|---|---|---|
| Zone 1: Gate Hall | Skeleton Soldiers x4 | Implemented (Zone_1_GateHall) | ✅ |
| Zone 2: Barracks | Skeleton Archer x3, Soldier x4 | Implemented (Zone_2_Barracks) | ✅ |
| Zone 3: The Bridge | Elite Soldier x2, Archer x2 | Implemented (zone exists) | 🟡 (need to verify enemy mix) |
| Zone 4: Armory | Armored Knight (Elite) x1 | Implemented | 🟡 (verify enemies) |
| Zone 5: Throne Room | Boss: Undead Warlord | Implemented | ✅ |

**Verdict:** 🟡 Zone count + names match. Enemy distribution per zone needs verification.

---

## §3.3 Checkpoints

| GDD spec | Current | Status |
|---|---|---|
| Checkpoint 1: After Zone 1 cleared | ✅ | ✅ |
| Checkpoint 2: After Zone 3 cleared (auto-saves) | 🟡 Checkpoint exists; auto-save trigger needs verification | 🟡 |
| Restore stamina + grant +20 HP on touch | HealAmount = 20 set per Tier 1; stamina restore in Checkpoint code | ✅ |
| Implementation has 3+ checkpoints, GDD specifies 2 | 🟡 Extra checkpoints kept for testing convenience — documented deviation | 🟡 |

**Verdict:** Healing values aligned. Extra checkpoints retained as a deliberate playtest convenience.

---

## §3.4 Environmental Hazards

| GDD spec | Current | Status |
|---|---|---|
| Spike traps | ✅ (SpikeTrap.cs) | ✅ |
| Collapsing floors | ✅ (CollapsingFloor.cs) | ✅ |
| Arrow walls | ✅ (ArrowWall.cs) | ✅ |
| Pit damage 20 HP + respawn at last stable ground | ✅ (KillVolume.cs + RespawnManager.RecoverFromFall) | ✅ |

**Verdict:** ✅ All hazards present.

---

## §4.1 Regular Enemies

| GDD Enemy | GDD HP | Current HP | Drop | Status |
|---|---|---|---|---|
| Skeleton Soldier | 30 | 30 | ✅ | ✅ |
| Skeleton Archer | 20 | 20 | ✅ | ✅ |
| Armored Knight | 80 | 80 | ✅ | ✅ |

**Verdict:** ✅ Enemy HP aligned per Tier 1 fix.

---

## §4.2 Boss — The Undead Warlord

| GDD spec | Current | Status |
|---|---|---|
| 3 phases (66% / 33% thresholds) | ✅ (BossBase phase2Threshold = 0.66, phase3Threshold = 0.33) | ✅ |
| Phase 1: sword combo + slow stomp AoE | 🟡 verify attack list | 🟡 |
| Phase 2: summons 2 Skeleton Soldiers + bone throw (15 dmg) | 🟡 verify | 🟡 |
| Phase 3: enrages (red glow, +30% attack speed, ground shockwaves) | 🟡 verify | 🟡 |
| Boss HP: 400 | ✅ Set in Tier 1 | ✅ |
| Cinematic death anim 2-3 sec | ✅ 2.5s delay in DeathCinematicRoutine (Tier 4 §2) | ✅ |
| Reward chest spawns on death | 🟡 Hooks ready (chestSpawnPoint + rewardChestPrefab fields); chest prefab pending Tier 4 §1 | 🟡 |

**Action:** verify boss HP = 400 and attack list matches GDD.

---

## §6.1 Item Rarity ✅ FIXED IN THIS COMMIT

| GDD spec | Previous | Now |
|---|---|---|
| 5 tiers: Common / Uncommon / Rare / Epic / Legendary | 3 tiers (Common / Rare / Legend) | ✅ 5 tiers per GDD |
| Specific colors per tier | Diablo conventions (subset) | ✅ Matches GDD §6.1 |

See [ITEM_RARITY_SETUP.md](ITEM_RARITY_SETUP.md).

---

## §6.2 Boss Reward Table

| GDD Roll | Reward | Current | Status |
|---|---|---|---|
| Guaranteed | Gold 150-300 | Loot_UndeadWarlord minGold=150, maxGold=300 | ✅ |
| Guaranteed | Uncommon or Rare item | Item_DualDemonSword (Rare) at 100% — Uncommon tier deferred | 🟡 |
| 70% chance | Rare item | Item_FuturisticRifle + Item_PistolBlue, each at 70% | ✅ |
| 30% chance | Epic item | 🟡 No Epic items exist yet — Epic roll silently skipped | 🟡 |
| 5% chance | Legendary: Warlord's Blade (first kill only) | Item_WarlordsBlade at 5% with LootRollGate.WarlordsBladeFirstKill | ✅ |
| 100% | Dungeon Clear Token | Item_DungeonClearToken at 100% | ✅ |

**Verdict:** Boss reward table substantially GDD-aligned. Remaining gaps:
- Uncommon items (no items at this tier yet — defer)
- Epic items (no items at this tier yet — defer)

PlayerProfile gained `warlordsBladeDropped` flag. LootTable.RollItems checks the profile's flag via `LootRollGate.WarlordsBladeFirstKill` — first successful roll sets the flag + auto-saves, blocking future Legendary drops per GDD §6.2.

Token Exchange UI at the Bank is explicitly Phase 2 per GDD §8.2.

---

## §6.3 Item Categories

| GDD category | Current | Status |
|---|---|---|
| Weapons (Swords, Guns) | ✅ | ✅ |
| Armor (Helmet, Chest, Legs, Boots) | ⏳ Equipment slots defined but no armor items | ⏳ |
| Accessories (Rings, Amulets) | ⏳ Not implemented | ⏳ |
| Consumables (Health, Stamina, Grenades) | 🟡 Health Potion + Stamina Tonic exist; no grenade | 🟡 |
| Materials | 🟡 BoneFragment exists | 🟡 |

---

## §7 Inventory System

| GDD spec | Current | Status |
|---|---|---|
| 6 columns x 8 rows = 48 slots | 🟡 Verify InventoryManager grid size | 🟡 |
| Equipment slots: Head, Chest, Legs, Boots, MainHand, OffHand, Ring x2, Amulet | 🟡 4 active slots (Head/Chest/MainHand/OffHand); Legs/Boots/Ring×2/Amulet reserved in enum but no UI yet — deferred until armor items exist | 🟡 |
| Hotbar: 4 slots (1, 2, 3, 4) | 🟡 6 slots (1-6) — kept for QoL convenience; documented deviation from GDD | 🟡 |
| Tooltip on hover | ✅ | ✅ |
| Right-click context menu | 🟡 Only drag-and-drop currently | 🟡 |

**Verdict:**
- **Equipment slots:** `Body` renamed to `Chest` in Tier 3. Other 5 GDD slots (Legs, Boots, Ring1, Ring2, Amulet) reserved in `EquipmentSlot` enum at values 5-9, but no UI widgets generated. Defer Phase 1 — no armor items exist to fill them.
- **Hotbar:** intentional deviation. Keeping 6 slots for QoL; documented here per GDD-deviation policy.

---

## §8 Banking & Economy

| GDD spec | Current | Status |
|---|---|---|
| Bank vault: 120 slots (6 cols × 20 rows) | 🟡 Verify BankManager grid size | 🟡 |
| Sell price = 30% of item base value | 🟡 Verify Item.SellValue ratios | 🟡 |
| Buy from NPC shop | ✅ (ShopManager) | ✅ |
| Dungeon Token Exchange | ⏳ Not implemented | ⏳ |
| Bank persistent across sessions | ✅ (JSON save) | ✅ |
| Inventory NOT lost on death | ✅ (RespawnManager doesn't clear inventory) | ✅ |
| Gold-in-inventory LOST on dungeon failure (optional rule) | ⏳ Not implemented | ⏳ |

---

## §8.4 Economy Pricing

| Item | GDD Buy / Sell | Current | Status |
|---|---|---|---|
| Small Health Potion | 25 / 7 | 🟡 Verify | 🟡 |
| Large Health Potion | 75 / 22 | ⏳ "Large" variant doesn't exist | ⏳ |
| Stamina Potion | 30 / 9 | 🟡 Verify | 🟡 |
| Grenade | 50 / 15 | ⏳ No grenade item | ⏳ |
| Common Weapon | 100-250 / 30-75 | 🟡 Verify | 🟡 |
| Uncommon Weapon | 400-800 / 120-240 | ⏳ No Uncommon items yet | ⏳ |

---

## §9.1 Experience & Leveling

| GDD spec | Current | Status |
|---|---|---|
| Level cap: 20 | ExperienceSystem.LevelCap = 20 | ✅ |
| +5 max HP per level | maxHpPerLevel = 5 | ✅ |
| +2 max stamina per level | maxStaminaPerLevel = 2 | ✅ |
| +1 Skill Point per level | ⏳ Skill system Phase 2 | ⏳ |

**Verdict:** ✅ Aligned to GDD §9.1.

---

## §9.2 EXP Sources

| GDD source | GDD EXP | Current | Status |
|---|---|---|---|
| Skeleton Soldier kill | 15 | 15 (Loot_SkeletonSoldier) | ✅ |
| Skeleton Archer kill | 15 | 15 (Loot_SkeletonArcher) | ✅ |
| Armored Knight kill | 60 | 60 (Loot_ArmoredKnight) | ✅ |
| Boss kill | 300 | 300 (Loot_UndeadWarlord) | ✅ |
| Dungeon clear bonus | 200 | ⏳ Bonus award not implemented | ⏳ |
| First clear bonus | +100 | ⏳ Not implemented | ⏳ |
| Combo bonus (10+ hits) | +25 per milestone | ⏳ Hookup pending (combo HUD done, EXP grant pending) | ⏳ |

---

## §10.2 Input Mapping

| GDD action | GDD key | Current binding | Status |
|---|---|---|---|
| Move | WASD | ✅ | ✅ |
| Jump / Double Jump | Space x2 | ✅ | ✅ |
| Dash / Dodge | Shift OR double-tap A/D | ✅ | ✅ |
| Light Attack | LMB | ✅ | ✅ |
| Heavy Attack | Hold LMB | ✅ | ✅ |
| Block / Parry | RMB (melee) | ✅ | ✅ |
| Aim Down Sights | RMB (gun) | 🟡 Same RMB used for block — context-sensitive switch | 🟡 |
| Fire Gun | LMB | ✅ | ✅ |
| Reload | R | ✅ | ✅ |
| Switch Weapon | Q or Mouse Wheel | 🟡 Verify mouse wheel binding | 🟡 |
| Open Inventory | I or Tab | Both I and Tab bound (Tier 3 fix) | ✅ |
| Hotbar 1-4 | Keys 1-4 | 🟡 Hotbar 5-6 also bound — kept for QoL, documented deviation | 🟡 |
| **Skill 1-3** | **E, G, V** | 🔴 Skill system not implemented | 🔴 (Phase 2) |
| Interact | F | ✅ | ✅ |
| Pause | Escape | ✅ | ✅ |

---

## §11.1 In-Dungeon HUD

| GDD element | Current | Status |
|---|---|---|
| HP bar bottom-left | ✅ Rebuilt in Tier 2 | ✅ |
| HP flashes red below 25% | ✅ LowHealthFlashRoutine in PlayerHUD | ✅ |
| Stamina bar below HP, blue/cyan | ✅ Re-positioned + recolored to cyan in Tier 2 | ✅ |
| Equipped weapons bottom-right | ✅ WeaponDisplayHUD (icon + name + ammo/sword state) | ✅ |
| Hotbar bottom-center | 🟡 Hotbar position may still need verification | 🟡 |
| Active skill icons above hotbar | ⏳ Skills deferred (Phase 2 per GDD §5) | ⏳ |
| Combo counter top-center | ✅ ComboCounterHUD (done in Tier 0) | ✅ |
| Mini-map top-right | ⏳ Phase 2 | ⏳ |
| Checkpoint reached notification | ⏳ Not implemented | ⏳ |

**Verdict:** ✅ HUD layout substantially GDD-aligned. Mini-map and skill icons deferred to Phase 2 per GDD scope.

---

## §11.2 Menu Screens

| GDD screen | Current | Status |
|---|---|---|
| Main Menu (Play, Settings, Quit) | ✅ | ✅ |
| Character Select / Profile | 🟡 CharacterSelectController exists, profile view incomplete | 🟡 |
| Lobby / Hub | ✅ | ✅ |
| Inventory screen | ✅ | ✅ |
| Bank screen | ✅ | ✅ |
| Dungeon clear screen (summary) | ⏳ Not implemented | ⏳ |
| Game over screen | ✅ | ✅ |

---

## §12 Audio Design

| GDD spec | Current | Status |
|---|---|---|
| Sword hit: metallic clang, distinct hit/parry/miss | ✅ SwordHit + SwordSwing (miss) + SwordParry (Tier 4 §4c) | ✅ |
| Gunshots punchy and distinct per weapon | 🟡 Single gun firing sound — no per-gun variety | 🟡 |
| Enemy death: shatter for skeletons | ⏳ Not implemented | ⏳ |
| Boss roar at phase transitions | ✅ AudioBindings subscribes to BossBase.OnPhaseChanged (Tier 4 §3) | ✅ |
| Music: dungeon ambient → combat transition | 🟡 Music system supports it; combat-aggro transition not wired | 🟡 |
| Boss fight: dedicated epic track | ✅ MusicTrigger.SwitchToArena hooks to BossArenaTrigger | ✅ |
| UI sounds: open/close, pickup, gold, checkpoint | ✅ Item pickup + gold + checkpoint wired (Tier 4 §4) | ✅ |

---

## §13 Development Milestones

| GDD milestone | Status |
|---|---|
| M1 — Movement Prototype | ✅ Done |
| M2 — Combat Prototype | ✅ Done |
| M3 — Dungeon 1 Layout | ✅ Done |
| M4 — Enemy AI | ✅ Done |
| M5 — Boss Fight | ✅ Done |
| M6 — Inventory System | ✅ Done |
| M7 — Bank & Economy | ✅ Done |
| M8 — Reward System | ✅ Done |
| **M9 — Polish & HUD** | 🟡 In progress |
| M10 — QA & Approval | ⏳ Pending |

---

## §15 Approval Criteria

| Criteria | Status |
|---|---|
| Movement responsive (jump, dash, wall run) | ✅ |
| Sword and gun combat impactful with VFX/audio | 🟡 Audio mostly done, VFX missing |
| Dungeon 1 completable start-to-finish | ✅ |
| Boss fight with all 3 phases | ✅ |
| Reward chest with correct loot table | 🟡 Loot table needs §6.2 percentages |
| Inventory fully functional | 🟡 Equipment slot list incomplete (no Legs/Boots/Rings/Amulet) |
| Bank persistent across sessions | ✅ |
| No progress loss bugs | ✅ |

---

## Priority fix list (suggested order)

### High priority (data corrections, ~1 hour total)

1. **Item rarity tiers (5)** ✅ DONE
2. **Enemy HP per GDD** — Soldier 30, Archer 20, Knight 80, Boss 400
3. **EXP sources per GDD §9.2** — set on each EnemyStats asset
4. **Level cap 20, +5 HP / +2 stamina per level** — `ExperienceSystem` constants
5. **Checkpoint heal +20 HP** — set on each Checkpoint asset

### Medium priority (UI alignment, ~2-3 hours)

6. **Equipment slots: add Legs, Boots, Ring x2, Amulet** to `EquipmentSlot` enum + UI
7. **Reduce hotbar from 6 → 4 slots** OR document deviation
8. **HUD layout** — move HP/Stamina to bottom-left, blue/cyan stamina, add weapon panel bottom-right
9. **HP < 25% red flash** — small Update tick on PlayerHUD

### Lower priority (Phase 1 or P2)

10. **Boss Reward Table** — match §6.2 percentages, add Dungeon Token loot
11. **Combo counter HUD** — top-center fade-after-3s
12. **Mini-map** — top-right
13. **Dungeon clear screen** — summary on victory
14. **Boss roar SFX** — at phase transitions
15. **Cinematic boss death** — 2-3 sec animation lock

### Out of Phase 1 scope

- Skills system (E/G/V keys)
- Armor stat reduction
- Crafting / materials use
- Multiplayer / co-op
- Mini-map

---

## How to use this doc

Treat each row as a discrete fix. Cross items off as you complete them. Reference [M9_TODO.md](M9_TODO.md) for the active polish queue — it pulls items off this list as they get prioritized.

When the High-priority list is fully ✅, you're at the GDD's "data layer correct" milestone. Medium-priority gets you to the HUD/UX target. Lower-priority polish + scope cuts complete Phase 1.
