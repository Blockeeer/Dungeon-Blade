# Today's Queue — Finish In-Flight Work

Ranked by **effort-per-impact**, focused on **closing what's already started** instead of opening new fronts. Tell me "start #X" or "done with #X" and I'll move accordingly.

**Total estimated time:** ~2 hours to clear all 4 items.
**Phase 1 impact:** ~85% → ~93% completion.

---

## The principle

> Closing 4 in-flight items takes Phase 1 from ~85% to ~93%. Starting new work just adds more in-flight items.

You don't get credit for things until they're done. Right now you have a lot of "almost done." Push them across the finish line before opening new tasks.

---

## 🥇 #1 — Finish Enemy Models (Skeleton Soldier foot alignment + Archer + Knight)

- [ ] Status: **Skeleton Soldier placed but floating; Archer + Knight not started**
- [ ] Effort: ~30-45 min
- [ ] Risk: low — visual swap only, no code change
- [ ] Doc: [ENEMY_MODELS_SETUP.md](ENEMY_MODELS_SETUP.md)

### Why first

- Highest visual impact of anything remaining
- You're **already in Prefab Edit mode for Skeleton_Soldier** — finishing the foot alignment is 1 field change
- Replicate exact same steps for Archer + Knight — pure repetition
- Pushes Phase 1 to look like a real game instead of a prototype with capsules
- All 3 enemy FBXes already in your project — no Mixamo download required

### What you'll do

1. Skeleton_Soldier — adjust FBX child Position Y until feet touch the floor (try -0.9, -1.0, etc.)
2. Save prefab, verify in `3_Dungeon1.unity`
3. Open `Skeleton_Archer.prefab` → repeat (drag `Skeleton-Archer-T-Pose.fbx`, align feet)
4. Open `Armored_Knight.prefab` → repeat (drag `Armored-Knight-T-Pose.fbx`, align feet)
5. Test in dungeon — all 3 enemies should patrol, take damage, die with proper visuals

### When you say "done with #1"

I'll verify the FBX integration looks correct in scene + propose the next step (probably wiring a basic enemy Animator).

---

## 🥈 #2 — Combo EXP Bonus (Unity wiring)

- [ ] Status: **Code shipped, awaiting "Add Component" in Dungeon scene**
- [ ] Effort: ~5 min
- [ ] Risk: zero
- [ ] Doc: [COMBO_EXP_BONUS_SETUP.md](COMBO_EXP_BONUS_SETUP.md)

### Why second

- Literally just "Add Component" on the ComboSystem GameObject
- Free completion — closes one of the last GDD §9.2 gaps
- Phase 1 audit moves from ~98% to nearly complete on EXP sources
- Zero asset work, zero code change

### What you'll do

1. Open `3_Dungeon1.unity`
2. Hierarchy → search bar (top of Hierarchy panel) → type `t:ComboSystem` → press Enter
   - The Player GameObject (which has ComboSystem) highlights
   - **Confirmed:** ComboSystem already exists in both `2_Lobby.unity` and `3_Dungeon1.unity` — it lives on the Player GameObject per [PHASE1_SETUP.md §81](PHASE1_SETUP.md)
3. With that GameObject selected, Inspector → Add Component → `Combo Exp Bonus`
4. Verify it auto-finds ComboSystem + ExperienceSystem (defaults: `milestoneEvery=10`, `expPerMilestone=25`)
5. Save scene (Ctrl+S)
6. Press Play → attack enemies in a chain for 10 hits → Console should show `[Combo] 10-hit milestone! +25 EXP` and EXP bar should increase

**Full instructions:** see [COMBO_EXP_BONUS_SETUP.md](COMBO_EXP_BONUS_SETUP.md) for step-by-step Inspector wiring + troubleshooting.

### When you say "done with #2"

I'll confirm the GDD §9.2 milestone bonus is fully closed.

---

## 🥉 #3 — Item Icons

- [ ] Status: **Setup doc written, awaiting Unity work**
- [ ] Effort: ~30 min
- [ ] Risk: zero
- [ ] Doc: [ITEM_ICONS_SETUP.md](ITEM_ICONS_SETUP.md)

### Why third

- Inventory / Bank / Shop UIs go from "colored squares" to "looks like a real game"
- Touches the rarity system shipped in Tier 0 (Common Longsword vs Legendary Warlord's Blade becomes visually distinct)
- Animation-independent, no code risk — just dragging sprites into Inspector slots
- Pairs well with enemy models for the "this feels like a real game now" jump

### What you'll do

1. Download 5-16 icons from https://game-icons.net (filter: CC-BY)
2. Drop into `Assets/_Project/UI/Sprites/Items/` (create folder)
3. Set Texture Type to `Sprite (2D and UI)` per icon
4. Click each `Item_*.asset` in Project window
5. Drag the matching sprite into the Icon field
6. Press Play → inventory shows proper icons

### When you say "done with #3"

I'll validate icons render in tooltip + inventory grid + pickup labels with rarity colors.

---

## 🏅 #4 — VFX Tier 1 (Hit Sparks + Muzzle Flash + Dash Trail)

- [ ] Status: **Code shipped, doc written, building prefabs in Unity**
- [ ] Effort: ~40 min for Tier 1 only (~2 hr for full set)
- [ ] Risk: low — empty slots no-op
- [ ] Doc: [VFX_SETUP.md](VFX_SETUP.md)

### Why fourth

- Massive combat-feel upgrade once enemies look like characters
- After #1-#3 land, combat juice becomes the most-noticeable gap
- Independent of animations — VFX spawn at any position
- Stop after Tier 1 (~40 min) — leave Tier 2/3 effects for a future session

### Tier 1 prefabs to build

| # | VFX | Trigger | Time |
|---|---|---|---|
| 1 | Hit Sparks | Sword/gun hits enemy | 20 min |
| 2 | Muzzle Flash | Gun fires | 10 min |
| 3 | Dash Trail | Dash / Dodge / Roll | 15 min |

### When you say "done with #4"

I'll verify VfxBindings dispatches correctly + suggest moving to Tier 2 (player damage flash, level up burst, enemy death dust) or wrapping for the day.

---

## ⏸ What I would NOT do today

| Task | Why skip |
|---|---|
| Player animation rebuild | 30-clip Mixamo session = nothing else gets done; you paused this for a reason |
| Dungeon clear screen | Needs design decisions + fresh code + new UI — big task, low daily-feel impact |
| Boss model | Bigger task than the 3 regular enemies; do it after them so you can compare |
| Map redesign / "real maps" | Multi-day task — needs Phase 2 zone list locked first |
| Anything without a ready doc | You have 5+ ready-to-execute docs already; use them before opening new fronts |

---

## Suggested execution order

1. **Now:** Finish Skeleton_Soldier foot alignment (5 min — the very first part of #1)
2. **Then:** Combo EXP wire (5 min — #2 is free)
3. **Then:** Archer + Knight foot alignment (20 min — rest of #1)
4. **PAUSE + PLAYTEST** — see how the dungeon feels with real enemies
5. If energy left: Item icons (#3, 30 min)
6. If still energy: VFX Tier 1 (#4, 40 min)

Step 4 is the important one. After 3 enemies look like enemies, you'll have a much better sense of which visual gap is most painful, and that tells you what to do next better than I can predict from outside.

---

## How to use this doc

1. Tell me **"start #X"** to begin or resume an item.
2. Tell me **"done with #X"** when complete; I'll verify and propose the next step.
3. After step 4 (playtest pause), come back and decide whether to continue with #3/#4 or pivot.

---

## Reference

- [M9_REMAINING_RANKED.md](M9_REMAINING_RANKED.md) — full M9 polish queue
- [GDD_AUDIT.md](GDD_AUDIT.md) — Phase 1 compliance status (currently ~85%)
- [DungeonBlade GDD v1.md](DungeonBlade%20GDD%20v1.md) — design source of truth
