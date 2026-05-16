# M9 Polish — Remaining Tasks Ranked

Ranked by **effort-per-impact** with animations paused. Tackle top-to-bottom. Tell me "done with #X" and I'll proceed to the next implementation.

**Last updated:** after Tier 4 partial + VFX docs/code + WeaponDisplayHUD ammo-format unification.

---

## Inline polish landed during this session

- ✅ **WeaponDisplayHUD ammo unified** — gun ammo shows `12 / 12` format from equip onward (was inconsistent: `Ammo: 12` on equip, `11 / 12` after first shot)
- ✅ **Gun.SyncAmmoUI()** added — public method to re-fire ammo event for late subscribers
- ✅ **VfxBindings code shipped** — full event dispatcher for 8 VFX types, even though prefabs are still in progress

---

## 🥇 #1 — Item Icons

- [ ] Status: **Setup doc written ([ITEM_ICONS_SETUP.md](ITEM_ICONS_SETUP.md)), awaiting Unity work**
- [ ] Effort: ~30 min
- [ ] Risk: zero
- [ ] Assets needed: 5-10 sprite icons (free CC-BY from game-icons.net)

### Why first

- Fastest visible win remaining
- Touches the rarity system shipped in Tier 0 (Common longsword vs Legendary Warlord's Blade becomes visually distinct)
- Inventory / Bank / Shop UIs go from "colored squares" to "looks like a real game"
- Animation-independent — UI-only work, no Varion or Animator dependency
- Zero code risk — just dragging sprites into Inspector slots

### What you'll do

1. Download 5-10 icons from https://game-icons.net (filter: CC-BY)
2. Drop them into `Assets/_Project/UI/Sprites/Items/` (folder to be created)
3. Set their Texture Type to `Sprite (2D and UI)` in the Inspector
4. Click each `Item_*.asset` in the Project window
5. Drag the matching sprite into the `Icon` field
6. Press Play → inventory now shows proper icons

### When you say "done with #1"

I'll: validate icons render correctly in tooltip + inventory grid + pickup label colors.

### Doc when starting

Tell me "start #1" and I'll write `ITEM_ICONS_SETUP.md` with icon source list + recommended icons per item.

---

## 🥈 #2 — Combo EXP Milestone Bonus

- [x] **Code shipped:** `ComboExpBonus.cs` subscribes to ComboSystem milestones, grants +25 EXP at every 10-hit mark.
- [x] **Setup doc:** [COMBO_EXP_BONUS_SETUP.md](COMBO_EXP_BONUS_SETUP.md)
- [ ] Status: **Awaiting Unity wiring + playtest verification**
- [ ] Effort: ~5 min remaining (Add Component on the ComboSystem GameObject)
- [ ] GDD reference: §9.2 ("Combo bonus +25 EXP per 10-hit milestone")

### Why second

- Closes one explicit GDD compliance gap → Phase 1 audit hits ~98% complete
- Zero asset hunting — pure code wiring
- Uses systems you already have: `ComboSystem.OnComboChanged` + `ExperienceSystem.GrantExperience`
- Independent of all other M9 work

### What you'll get

- Hit 10 enemies in a chain → +25 EXP popup
- Hit 20 in a chain → another +25 EXP (at every 10-hit milestone)
- Combo breaks reset to 0 milestone count

### What you'll do in Unity

- Verify the `ComboSystem` GameObject exists in scene (it should from Tier 0 wiring)
- Confirm `ExperienceSystem` exists (should — used by enemy kills)
- That's it — pure code change on my end

### When you say "start #2"

I'll: write the code change in `ComboSystem.cs` or add a small `ComboExpBonus.cs` script that subscribes to milestones.

---

## 🥉 #3 — VFX (Combat Feedback)

- [x] **Code shipped:** `VfxBindings.cs` dispatcher + new events on `Sword.OnHitAtPosition`, `Gun.OnFireAtPosition`, `Gun.OnHitAtPosition`, `EnemyBase.AnyEnemyDied`.
- [x] **Setup doc:** [VFX_SETUP.md](VFX_SETUP.md) with detailed sub-steps for all 8 prefabs (#1-#8) at the same detail level.
- [ ] Status: **Building prefabs in Unity (in progress)** — Tier-1 minimum (#1, #2, #4) = ~40 min, full set = ~2-3 hr
- [ ] Risk: low (each prefab is independent; empty slots no-op)
- [ ] Assets needed: none (Unity built-in particle system)

### Why third

- Big combat-feel upgrade
- After #1 and #2 are done, the game looks/feels significantly better with these
- Independent of animations — VFX can spawn at any position (e.g. enemy's hit point) regardless of player animation state

### Effects ranked by impact

| # | VFX | Trigger | Effort |
|---|---|---|---|
| 1 | **Hit sparks** | Sword/gun hits enemy | 20 min |
| 2 | **Muzzle flash** | Gun fires | 10 min |
| 3 | **Player damage flash** | Player takes damage (red vignette pulse) | 15 min |
| 4 | **Dash trail** | DashStarted / DodgeStarted (TrailRenderer) | 15 min |
| 5 | **Level up burst** | OnLevelUp (gold particles upward) | 20 min |
| 6 | **Boss enrage aura** | Boss Phase 3 entry | 20 min |
| 7 | **Death dust** | Enemy dies | 15 min |
| 8 | **Pickup sparkle** | Item drop continuous shimmer | 10 min |

Could split into a "Tier 1 VFX (top 4)" session vs full set.

### When you say "start #3"

I'll: write `VFX_SETUP.md` with prefab build steps + code hooks. You'd build the particle prefabs in Unity (drag-and-drop) and wire references.

---

## #4 — Item Rarity Glow on Equipped Weapon

- [ ] Status: **Not started**
- [ ] Effort: ~1 hr
- [ ] Risk: low
- [ ] Assets needed: none (uses existing emissive shader)

### Why fourth

- Polishes the rarity system you already shipped (Tier 0)
- Drops a rarity-colored emission on the equipped weapon's mesh
- Makes Legendary weapons visually pop ("ooh, that's the Warlord's Blade")
- Trivial code — apply MaterialPropertyBlock when weapon equips

### When you say "start #4"

I'll: extend `ItemPickup.ApplyRarityTint` to also apply on `WeaponBase.OnEquip`, write the wiring doc.

---

## #5 — Boss Arena Atmosphere

- [ ] Status: **Not started**
- [ ] Effort: ~1-2 hr
- [ ] Risk: low
- [ ] Assets needed: torch flame texture (free) OR none if you use just lights

### Why fifth

- Boss room currently has flat lighting — same vibe as Zone 1
- GDD §3.1 calls for "dark, gothic, intense" atmosphere
- Easy wins: dimmer ambient light + red point lights at pillars + flickering torch effect

### When you say "start #5"

I'll: write `BOSS_ARENA_ATMOSPHERE.md` with lighting placement + flicker script.

---

## #6 — Dungeon Clear Summary Screen

- [ ] Status: **Not started**
- [ ] Effort: ~1-2 hr
- [ ] Risk: medium (new UI panel + scene transitions)
- [ ] Assets needed: none
- [ ] GDD reference: §11.2

### Why sixth

- GDD requires it
- Player gets a victory payoff (time, kills, combo best, loot, EXP)
- Bridges the gap between boss defeat and return-to-lobby

### When you say "start #6"

I'll: write `DUNGEON_CLEAR_SCREEN.md` + the new `DungeonClearController.cs` script that aggregates kill counts, combos, and EXP.

---

## Skipped / Deferred

- **Animations** — paused, you'll come back when ready
- **Real enemy models** — same animation dependency; defer until animations are figured out
- **Skills system (E/G/V)** — Phase 2 per GDD §5
- **Mini-map** — Phase 2 per GDD §11.1

---

## How to use this doc

1. **Tell me "start #1"** (or whichever # you want first).
2. I write the implementation doc + any code changes.
3. You work through the Unity-side steps.
4. **Tell me "done with #1"** when complete.
5. I verify, mark the task ✅ here, and we proceed to the next.
6. Repeat.

---

## Quick math — what completing this list gets you

- All 6 tasks done: ~6-8 hours of focused work
- After: Phase 1 polish substantially shipped except animations
- Animations resume later as a focused session when ready

The order is designed so each task can be tackled in a single sitting. Don't feel pressure to chain them.

---

## Reference

- [DungeonBlade GDD v1.md](DungeonBlade%20GDD%20v1.md)
- [GDD_AUDIT.md](GDD_AUDIT.md) — current Phase 1 compliance status
- [GDD_FIX_INDEX.md](GDD_FIX_INDEX.md) — tier-by-tier compliance progress
- [M9_TODO.md](M9_TODO.md) — older general M9 todo (now superseded by this doc for ranked priorities)
