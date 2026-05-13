# GDD Fix Tiers — Index

Sequential GDD-compliance work, broken into 4 self-contained tiers. Do them in order — each one builds on the previous. Total estimated time: **~6-8 hours** spread across multiple sessions.

**Strict GDD alignment goal:** every Phase 1 item flagged 🔴 or 🟡 in [GDD_AUDIT.md](GDD_AUDIT.md) gets resolved (either fixed to match or explicitly documented as a deviation).

---

## Tier overview

| Tier | Focus | Effort | Status |
|---|---|---|---|
| **Tier 1** | Numeric data alignment (HP, EXP, levels, checkpoints) | ~15 min | ✅ **Done** |
| **Tier 2** | HUD layout rebuild per §11.1 | ~2-3 hr | ✅ **Done** |
| **Tier 3** | Structural deviations (equipment slots, hotbar, inventory key) | ~3-4 hr | ✅ **Done** (lightweight path — deviations documented) |
| **Tier 4** | Boss reward table + late-stage features | ~3-4 hr | 🟡 **Partial** — §2/§3/§4 code done; §1 (Boss Reward Table) deferred |

---

## How to use

1. **Open the tier doc** for the tier you're on.
2. Each doc is self-contained — Unity Inspector steps + setup checklist + verification.
3. When all checks pass, mark the tier complete here in this index (change ⬜ to ✅).
4. Update [GDD_AUDIT.md](GDD_AUDIT.md) to mark the corresponding GDD sections from 🔴/🟡 to ✅.
5. Move to next tier.

---

## Tier docs

### 🥇 [Tier 1 — Numeric Data Alignment](GDD_FIX_TIER1_DATA.md)

**Quick win.** 4 sections of Inspector-only changes. ~15 minutes total.

- Experience System: level cap → 20, +5 HP / +2 stamina per level
- Enemy HP: Soldier 30, Archer 20, Knight 80, Boss 400
- Enemy EXP rewards: Soldier 15, Archer 15, Knight 60, Boss 300
- Checkpoint heal: +20 HP + restore stamina

**Why first:** zero risk (no code changes), fastest GDD wins, foundation for testing tiers above.

---

### 🥈 [Tier 2 — HUD Layout per §11.1](GDD_FIX_TIER2_HUD.md)

**The biggest visual change.** Rearrange Player HUD from top-left to GDD-mandated layout.

- HP bar **bottom-left**, red, flashes red when <25%
- Stamina bar below HP, **cyan/blue** (currently green)
- Equipped weapons **bottom-right** with ammo/durability
- Hotbar **bottom-center**

**Decision point first:** GDD wants bottom-left. You previously preferred top-left. The doc handles both options.

---

### 🥉 [Tier 3 — Structural Deviations](GDD_FIX_TIER3_STRUCTURE.md)

**The refactor-heavy tier.** Inventory + equipment + input bindings.

- Equipment slots: add Legs / Boots / Ring×2 / Amulet (currently missing). Rename Body → Chest.
- Hotbar: trim from 6 to 4 slots (or document deviation)
- Open Inventory: bind `I` key (currently Tab-only)

**Per-section decisions** — you can defer the equipment slot expansion until you actually have armor items.

---

### Tier 4 [Tier 4 — Boss Reward Table + Polish](GDD_FIX_TIER4_LOOT.md)

**Phase 1 finisher.** Final cinematic + loot polish.

- Boss reward table per §6.2 (70% Rare / 30% Epic / 5% Legendary "first kill only")
- Boss death cinematic (2-3 sec animation + chest spawn at arena center)
- Boss roar SFX at phase transitions
- Gold pickup sound, checkpoint sound, parry sound

**Requires:** ITEM_RARITY_SETUP done (uses 5-tier rank system), AUDIO_SETUP done (uses SfxPool).

---

## After all tiers done

Phase 1 should pass GDD §15 approval criteria. Remaining work is:

- Skills system (E/G/V) — explicitly Phase 2 per GDD §5
- Mini-map — explicitly Phase 2
- Dungeon clear screen — nice-to-have polish
- Crafting / multiplayer / PvP — Phase 2/3 per GDD §14

Total Phase 1 completion at that point: ~95%+ GDD compliance with documented deviations for the rest.

---

## Reference

- [DungeonBlade GDD v1.md](DungeonBlade%20GDD%20v1.md) — the source of truth
- [GDD_AUDIT.md](GDD_AUDIT.md) — full audit, current status of every GDD section
- [M9_TODO.md](M9_TODO.md) — broader polish queue including non-GDD items
- [COMBAT_SYSTEM.md](COMBAT_SYSTEM.md) — Tier 0 (combat) already complete
- [ITEM_RARITY_SETUP.md](ITEM_RARITY_SETUP.md) — Tier 0 (rarity) already complete
