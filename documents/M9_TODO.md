# M9 Polish — Remaining Tasks

Ranked by recommended priority. Check items off as completed.

---

## Done so far

- [x] Boss HP bar HUD
- [x] Player HUD (HP / Stamina / EXP / Gold)
- [x] Camera shake (damage, dash, boss phases, death)
- [x] Audio system foundation (SfxPool, MusicPlayer, AudioBindings, UIClickListener)
- [x] AudioMixer routing (Master / Music / SFX with PlayerPrefs persistence)
- [x] Tier 1 SFX wired (sword swing/hit, dash whoosh, jump grunt, damage, death, item pickup, UI click)
- [x] Boss retry fix (BossArenaTrigger + BossBase.ResetForRetry)
- [x] Zone gates (block progression until zone enemies cleared)
- [x] MenuState auto-reset on scene load (fixes pause-menu input freeze)
- [x] Animations cherry-picked into project (44 FBXes + Animator controller)
- [x] Landing page polish cherry-picked (logo, flash burst, slash sweep, halo)
- [x] Lobby pause menu + settings
- [x] Game Over screen on RespawnManager.OnRunFailed
- [x] Boss music swap (persists during retry walk-back)

---

## Remaining tasks (ranked)

### 1. Item rarity tiers (data + tooltip colors) ✅ CODE DONE

- [x] `ItemRank` enum in `Item.cs` (Common / Rare / Legend — already existed)
- [x] `ItemRankColors` static helper for color lookups
- [x] Color-code tooltip name text per rarity (off-white / blue / gold)
- [x] Color-code drop-pickup labels via TMP rich-text `<color>` tags
- [x] Optional Rarity Tint Renderer field on ItemPickup for emissive glow
- [ ] **Action remaining**: tag each Item ScriptableObject in Unity with its Rank value (see [ITEM_RARITY_SETUP.md](ITEM_RARITY_SETUP.md) Step 1)
- [ ] Optional: filter items by rarity in Bank/Shop UIs (defer)

**Effort:** ~5 min remaining (tag items)
**Impact:** High visibility, low risk
**Dependencies:** None
**Doc:** [ITEM_RARITY_SETUP.md](ITEM_RARITY_SETUP.md)

---

### 2. Music wiring (3 tracks → 3 scenes)

- [ ] Download 3 tracks from Pixabay (Lobby calm, Dungeon rock, Boss epic)
- [ ] Drop into `Assets/_Project/Audio/Music/`
- [ ] Set import settings: Streaming + Vorbis + Preserve Sample Rate
- [ ] Add `[MusicPlayer]` to `0_LandingScene.unity` (DontDestroyOnLoad persists across scenes)
- [ ] Wire `Music Group` field → drag `MainMixer/Music` from Audio Mixer
- [ ] Add `[Music]` GameObject + MusicTrigger to `2_Lobby.unity` (lobby track)
- [ ] Add `[Music]` GameObject + MusicTrigger to `3_Dungeon1.unity` (dungeon track + arena track for boss)
- [ ] Wire `BossArenaTrigger.musicTrigger` field → drag the Dungeon's `[Music]` GameObject
- [ ] Test: walk Lobby → Dungeon → Boss arena, confirm crossfades

**Effort:** ~30 min (after Pixabay run)
**Impact:** Huge atmospheric upgrade
**Dependencies:** Need to download music files first

---

### 3. Tier 2 audio — Land thud + footsteps (animation-dependent)

- [ ] Add `Land Thud` SFX clip to AudioBindings
- [ ] Hook `Land Thud` to PlayerMovement landing event (after airtime > 0.3s)
- [ ] Defer footsteps until animations are wired (uses animation events on contact frames)

**Effort:** ~30 min for Land thud alone
**Impact:** Medium
**Dependencies:** Footsteps need animations wired first

---

### 4. VFX — Hit particles + dash trail

- [ ] Sword hit → spawn red particle burst at hit point
- [ ] Gun hit → spawn small spark / muzzle flash
- [ ] Player dash → trail VFX (Trail Renderer or particle stream behind player)
- [ ] Level-up → upward gold particle flash on Player
- [ ] Boss phase transition → arena pulse VFX

**Effort:** ~2 hrs
**Impact:** Medium
**Dependencies:** None (Unity built-in particles work)

---

### 5. Lighting + atmosphere polish

- [ ] Dungeon torch flicker (Light component + simple flicker script)
- [ ] Volumetric fog in dungeon corridors (URP Volume profile)
- [ ] Boss arena dramatic lighting (red tint + low ambient)
- [ ] Lobby softer warm lighting (contrast with dungeon)

**Effort:** ~1-2 hrs
**Impact:** Medium
**Dependencies:** None

---

### 6. Real enemy models (replace cubes)

- [ ] Find / commission skeleton model
- [ ] Find / commission knight model
- [ ] Find / commission archer model
- [ ] Find / commission Undead Warlord boss model
- [ ] Replace cube-based enemies in scenes
- [ ] Re-bake NavMesh after geometry changes

**Effort:** ~3-5 hrs (depends on model source — Mixamo / asset store / custom)
**Impact:** High visual transformation
**Dependencies:** None for setup; depends on finding models

---

### 7. Animations (player character) — DEFERRED

- [ ] Add Animator component to player character (Aurelia / Kaelen / etc.)
- [ ] Wire `Player_Animator.controller` to the Animator
- [ ] Set Avatar from rigged character model
- [ ] Apply Root Motion = ❌ (PlayerMovement drives position via CharacterController)
- [ ] Create `PlayerAnimatorBridge.cs` — translates PlayerMovement events/state into Animator parameters
- [ ] Wire animation events on attack clips for hit-frame-synced damage
- [ ] Test: idle, walk, run, jump, dash, attack, death all play correctly

**Effort:** ~4-6 hrs
**Impact:** Highest visual transformation
**Dependencies:** Animator controller (already cherry-picked), bridge script (need to write)
**Note:** Skipped for now — large scope. Pick up when ready.

---

### 8. Item rarity (full aura / glow visual)

- [ ] Once weapon models exist (task 6 partially done): add per-rarity glow shader
- [ ] Apply outline / emission per rarity color
- [ ] Drop-pickup glow trail in world
- [ ] Equipped weapon retains rarity glow

**Effort:** ~2 hrs
**Impact:** Polish on top of item rarity
**Dependencies:** Real weapon models (task 6)

---

## Suggested next 2-3 tasks

If working in priority order:

1. **Item rarity (data + colors)** — quick 30-min win.
2. **Music wiring** — biggest atmospheric jump, requires Pixabay run first.
3. **Land thud SFX + light VFX** — small polish layers.

Then circle back to **animations** when ready for the bigger task.

---

## Phase 1 completion criteria

Phase 1 is "done" when:
- [x] Full game loop works (Landing → MainMenu → Lobby → Dungeon → Boss → Death/Victory → return)
- [x] Save persistence across sessions
- [x] HUD displays player state
- [x] Combat has audio + visual feedback (camera shake)
- [ ] Music plays appropriately per scene
- [ ] Items have rarity tiers
- [ ] Player has animations (defer if needed)
- [ ] Real models replace placeholder cubes (defer if needed)

5 / 8 criteria met. The remaining 3 are polish that can ship in any order.
