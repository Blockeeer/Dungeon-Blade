# VFX Setup — Combat Feedback

Visual effects (particle bursts, trails) for combat hits, dashes, damage, level ups, enemy deaths, boss enrage. ~2-3 hours total. Each effect is independent — ship them one at a time.

**Code is already done.** You build particle prefabs in Unity, drag them into `VfxBindings`, and they spawn at the right moments.

---

## What's wired (code)

**Script:** `Assets/_Project/Core/VFX/VfxBindings.cs`

Subscribes to existing gameplay events and instantiates particle prefabs at the right positions. All prefab slots are optional — empty slots no-op silently. You can ship Tier-1 VFX (hit sparks + muzzle flash) first, add others later.

**New events added to support this:**

- `Sword.OnHitAtPosition(Vector3)` — fires at sword hit center
- `Gun.OnFireAtPosition(Vector3)` — fires at gun muzzle on shot
- `Gun.OnHitAtPosition(Vector3)` — fires at ray-hit point
- `EnemyBase.AnyEnemyDied(Vector3)` — static event, fires when any enemy dies

Existing events reused:

- `PlayerStats.OnDamaged` → player damage flash (spawned at player position)
- `PlayerMovement.DashStarted` / `DodgeStarted` → dash trail (attached to player)
- `ExperienceSystem.OnLevelUp` → level-up burst (at player)
- `BossBase.OnPhaseChanged` → boss enrage aura (attached to boss on Phase3)

---

## Step 1 — Plan which VFX to build

8 effects in this system, ranked by impact:

| #   | VFX                     | Trigger                    | Effort to build prefab   |
| --- | ----------------------- | -------------------------- | ------------------------ |
| 1   | **Hit sparks** (sword)  | Sword hits enemy           | 15 min                   |
| 2   | **Muzzle flash** (gun)  | Gun fires                  | 10 min                   |
| 3   | **Hit sparks** (gun)    | Gun ray hits target        | 5 min (reuse hit sparks) |
| 4   | **Player damage flash** | Player takes damage        | 10 min                   |
| 5   | **Dash trail**          | DashStarted / DodgeStarted | 15 min                   |
| 6   | **Level up burst**      | OnLevelUp                  | 20 min                   |
| 7   | **Enemy death dust**    | Enemy dies                 | 15 min                   |
| 8   | **Boss enrage aura**    | Boss Phase 3               | 20 min                   |

**Recommended Tier 1 (do these first, biggest payoff):** #1, #2, #3, #4 (~40 min total). Test the loop, then build the rest.

---

## Step 2 — Create the VFX folder

1. Project window → navigate to `Assets/_Project/Core/VFX/`.
2. Right-click → **Create → Folder** → name `Prefabs`.

You'll save all VFX prefabs in `Assets/_Project/Core/VFX/Prefabs/`.

---

## Step 3 — Build prefab #1: Sword Hit Spark (template for the rest)

This is the longest-walkthrough one. The rest follow the same pattern.

### 3a — Create the GameObject

1. Hierarchy → right-click → **Create Empty** → name `VFX_SwordHitSpark`.
2. Position at (0, 0, 0) — doesn't matter, will be instantiated at hit position.

### 3b — Add a Particle System

1. With `VFX_SwordHitSpark` selected → **Add Component** → **Particle System** (search and click).
2. The Particle System inspector appears with many sections.

### 3c — Configure the main module (top section)

| Field            | Value                        | Why                                                            |
| ---------------- | ---------------------------- | -------------------------------------------------------------- |
| Duration         | `0.3`                        | Short burst                                                    |
| Looping          | ❌                           | Plays once                                                     |
| Start Lifetime   | `0.4`                        | Each spark lives 0.4 sec                                       |
| Start Speed      | `4`                          | Outward velocity                                               |
| Start Size       | `0.15`                       | Small sparks                                                   |
| Start Color      | yellow-orange (255, 200, 80) | Hot impact color                                               |
| Gravity Modifier | `0.5`                        | Sparks fall a bit                                              |
| Simulation Space | `World`                      | Sparks don't follow the moving sword                           |
| Play On Awake    | ✅ checked                   | Plays automatically when instantiated                          |
| Stop Action      | `Destroy`                    | Self-cleans (defaultLifetime in VfxBindings also handles this) |

### 3d — Configure Emission module

1. Click the **Emission** module header to enable + expand.
2. Settings:
   - **Rate over Time** = `0`
   - **Bursts** → click `+` to add a burst:
     - Time: `0`, Count: `12-20`, Cycles: `1`

This makes 12-20 particles spawn instantly at t=0, then no continuous emission. Perfect for a one-shot spark.

### 3e — Configure Shape module

1. Click **Shape** → enable + expand.
2. Settings:
   - **Shape** = `Sphere` (dropdown)
   - **Radius** = `0.1`
   - **Radius Thickness** = `0` (this is the modern equivalent of "emit from shell" — particles spawn only on the outer surface, not inside the sphere)
   - Leave Arc / Arc Mode / Position / Rotation / Scale at defaults

So sparks fly outward in all directions from a tight point.

**Note on Radius Thickness:**

- `0` = shell only (sparks burst out from a thin sphere surface — what we want)
- `1` (default) = full volume (sparks spawn anywhere inside the sphere, including its center)

You want `0` for impact sparks. Leave at `1` only if you want a fuzzy cloud rather than a directional burst.

### 3f — Configure Color over Lifetime (fade out)

1. Click **Color over Lifetime** → enable.
2. Click the white gradient bar → opens the gradient editor.
3. Add 2 keys:
   - Left (start): full opacity, color yellow-orange
   - Right (end): alpha = 0 (transparent), keep color
4. Sparks fade out smoothly over their lifetime.

### 3g — Configure Size over Lifetime (optional shrink)

1. Enable **Size over Lifetime**.
2. Click the curve → set to a decreasing curve (start at 1, end at 0). Unity has preset curves at the bottom — pick the "decreasing" one.

### 3h — Save as a prefab

1. Drag `VFX_SwordHitSpark` from Hierarchy into `Assets/_Project/Core/VFX/Prefabs/`.
2. Confirm prefab creation (Original Prefab).
3. Delete the GameObject from the Hierarchy (the prefab asset is what matters).

### 3i — Verify

1. Drag the prefab back into the scene briefly → particles should burst when scene plays.
2. Delete after testing.

---

## Step 4 — Build the remaining VFX prefabs

Each follows the Step 3 template. Variations per effect:

### #2 — Muzzle Flash (gun fires)

Very brief white-yellow burst at the gun's muzzle. Plays for ~0.08s — basically a single-frame flash.

**Sub-steps (similar to Step 3 template):**

**2.1 Create GameObject**

1. Hierarchy → Create Empty → name `VFX_MuzzleFlash`.
2. Add Component → Particle System.

**2.2 Main module**

| Field            | Value                               |
| ---------------- | ----------------------------------- |
| Duration         | `0.1`                               |
| Looping          | ❌                                  |
| Start Lifetime   | `0.08`                              |
| Start Speed      | `2`                                 |
| Start Size       | `0.25`                              |
| Start Color      | white-yellow `(255, 255, 200, 255)` |
| Gravity Modifier | `0`                                 |
| Simulation Space | `World`                             |
| Play On Awake    | ✅                                  |
| Stop Action      | `Destroy`                           |

**2.3 Emission**

- Rate over Time = `0`
- Bursts → click `+` to add one:
  - Time: `0`, Count: `6-10`, Cycles: `1`

**2.4 Shape**

| Field            | Value                                 |
| ---------------- | ------------------------------------- |
| Shape            | `Sphere`                              |
| Radius           | `0.05` (very tight — muzzle is small) |
| Radius Thickness | `0`                                   |

**2.5 Color over Lifetime (fade out)**

1. Enable Color over Lifetime.
2. Gradient → 2 keys:
   - Left: full alpha white-yellow
   - Right: alpha = 0

**2.6 Size over Lifetime (shrink)**

- Enable; curve from `1` → `0` (decreasing).

**2.7 Skip adding a Light**
Optional polish — skip per the discussion above. The particles alone work.

**2.8 Save prefab**

- Drag `VFX_MuzzleFlash` from Hierarchy → `Assets/_Project/Core/VFX/Prefabs/`.
- Choose Original Prefab.
- Delete from Hierarchy.

---

### #3 — Gun Hit Spark (ray hits target)

Cool-white version of the sword hit spark, distinguishes ranged impacts from melee.

**Sub-steps:**

**3.1 Easiest path — duplicate the sword spark and recolor**

1. Project window → click `VFX_SwordHitSpark.prefab`.
2. Ctrl+D to duplicate → `VFX_SwordHitSpark 1.prefab` appears.
3. Rename to `VFX_GunHitSpark.prefab`.
4. Double-click to open the duplicate in Prefab Mode.

**3.2 Main module — change only Start Color**

| Field       | New value                                                      |
| ----------- | -------------------------------------------------------------- |
| Start Color | cool-white `(180, 220, 255, 255)` — bluer than the sword spark |

Leave everything else (Duration, Lifetime, Speed, Size, Shape, Emission) the same as the sword spark.

**3.3 (Optional) Color over Lifetime gradient**

If you want, update the gradient's left key to also be cool-white. The right key stays alpha=0.

**3.4 Save prefab**

In Prefab Mode → click the back arrow (top-left of Hierarchy in Prefab Mode) → exits and auto-saves.

That's the entire #3 — it's #1 with a recoloring. Total time ~2-3 minutes once #1 exists.

### #4 — Player Damage Flash

A red burst spawned at the player's position when they take damage. Larger and slower than hit sparks — it should feel "the player got hit" not "the player hit something."

**Sub-steps:**

**4.1 Create GameObject**

1. Hierarchy → Create Empty → name `VFX_PlayerDamage`.
2. Add Component → Particle System.

**4.2 Main module**

| Field            | Value                    | Why                                            |
| ---------------- | ------------------------ | ---------------------------------------------- |
| Duration         | `0.5`                    | Short flash                                    |
| Looping          | ❌                       | One-shot                                       |
| Start Lifetime   | `0.5`                    | Each particle lives 0.5 sec                    |
| Start Speed      | `2`                      | Slow outward drift — heavier than spark sparks |
| Start Size       | `0.3`                    | Bigger than impact sparks                      |
| Start Color      | red `(200, 30, 30, 255)` | Damage = red                                   |
| Gravity Modifier | `-0.3`                   | Slight upward float (smoke-rising feel)        |
| Simulation Space | `World`                  | Particles don't follow the moving player       |
| Play On Awake    | ✅                       | Plays on instantiate                           |
| Stop Action      | `Destroy`                | Self-cleans                                    |

**4.3 Emission**

- Rate over Time = `0`
- Bursts → click `+` to add one:
  - Time: `0`, Count: `25-30`, Cycles: `1`

So 25-30 red particles burst out instantly when the player gets hit.

**4.4 Shape**

| Field            | Value                                                                |
| ---------------- | -------------------------------------------------------------------- |
| Shape            | `Sphere`                                                             |
| Radius           | `0.5` (larger than spark — covers the player's torso)                |
| Radius Thickness | `0` (shell — particles burst outward from the player's body surface) |

**4.5 Color over Lifetime (fade out)**

1. Enable Color over Lifetime.
2. Click the gradient bar to open the gradient editor.
3. Set 2 keys:
   - Left (start): full alpha red `(200, 30, 30, 255)`
   - Right (end): alpha = 0 (transparent), keep color red
4. Particles fade to invisible over their 0.5s lifetime.

**4.6 Size over Lifetime (slight shrink)**

- Enable Size over Lifetime.
- Curve from `1` (start) → `0.5` (end). Mild shrink — not full disappearance. They fade out before shrinking fully.

**4.7 Save prefab**

- Drag `VFX_PlayerDamage` from Hierarchy → `Assets/_Project/Core/VFX/Prefabs/`.
- Choose Original Prefab.
- Delete from Hierarchy.

**4.8 (Optional polish — defer for Phase 1)**

A full-screen red vignette pulse instead of a particle burst would be more cinematic. Requires URP Post Processing setup (Volume + Vignette effect). Skip for now — particle version works and gives the right visual cue. Revisit if you find the particle burst doesn't read clearly during play.

**4.9 What you'll see in-game**

When the player takes damage:

- A red puff erupts around the player (mostly torso-height)
- Particles drift slightly upward and outward, then fade
- Lasts about 0.5 seconds total

Visual cue says "I just took damage" without obscuring the player. Pairs well with the existing camera shake on damage.

### #5 — Dash Trail

**Different — uses Trail Renderer, not Particle System.**

1. Create Empty → name `VFX_DashTrail`.
2. Add Component → **Trail Renderer**.
3. Settings:
   - Time: `0.3`
   - Min Vertex Distance: `0.1`
   - Width: 0.3 at start, 0 at end (curve)
   - Color: cyan/blue gradient from full alpha to alpha=0
   - Material: a simple particle-additive material (or Unity's default)

The VfxBindings script instantiates this as a CHILD of the player on dash start and auto-destroys it after 0.6s.

Save as `VFX_DashTrail.prefab`.

### #6 — Level Up Burst

Gold particles rising upward from the player. Plays once when the player levels up. Should feel celebratory — slower, bigger, more particles than impact sparks.

**Sub-steps:**

**6.1 Create GameObject**

1. Hierarchy → Create Empty → name `VFX_LevelUp`.
2. Add Component → Particle System.

**6.2 Main module**

| Field            | Value                      | Why                                   |
| ---------------- | -------------------------- | ------------------------------------- |
| Duration         | `1.0`                      | Total emission window                 |
| Looping          | ❌                         | One-shot                              |
| Start Lifetime   | `1.2`                      | Particles live longer (drift upward)  |
| Start Speed      | `3`                        | Moderate upward push                  |
| Start Size       | `0.25`                     | Mid-size — visible but not blocking   |
| Start Color      | gold `(255, 220, 80, 255)` | Celebration color                     |
| Gravity Modifier | `-0.5`                     | Negative gravity = particles float UP |
| Simulation Space | `World`                    | Don't follow player as they move      |
| Play On Awake    | ✅                         | Plays on instantiate                  |
| Stop Action      | `Destroy`                  | Self-cleans                           |

**6.3 Emission**

- Rate over Time = `0`
- Bursts → click `+` to add one:
  - Time: `0`, Count: `40`, Cycles: `1`

40 particles erupt at once for a generous burst.

**6.4 Shape**

| Field            | Value                 | Why                                                                                                 |
| ---------------- | --------------------- | --------------------------------------------------------------------------------------------------- |
| Shape            | `Cone`                | Particles emit upward in a cone — visually says "rising"                                            |
| Angle            | `25`                  | Mild spread (0 = laser straight up, 90 = flat disc)                                                 |
| Radius           | `0.3`                 | Base of the cone — moderate spread at player's feet                                                 |
| Radius Thickness | `1` (default)         | Full volume — particles spawn anywhere inside the cone base                                         |
| Position         | `(0, 0, 0)` (default) | At player position                                                                                  |
| Rotation X       | `-90`                 | Tilts the cone so it points UP (default cone points along +Z; -90 X-rotation points it UP along +Y) |

Result: cone-shaped emission pattern pointing up — particles fly up and slightly outward.

**6.5 Color over Lifetime (warm fade)**

1. Enable Color over Lifetime.
2. Gradient → 3 keys for a nicer fade:
   - Left (0%): full alpha gold `(255, 220, 80, 255)`
   - Middle (60%): full alpha warmer gold `(255, 180, 60, 255)`
   - Right (100%): alpha = 0
3. Particles shift from bright gold to deeper gold as they fade out.

**6.6 Size over Lifetime (slight shrink)**

- Enable Size over Lifetime.
- Curve from `1` (start) → `0.3` (end). Slow shrink — particles get smaller as they rise.

**6.7 (Optional) Trail module — rising streaks**

For extra polish, give each particle a trailing streak:

1. Enable **Trails** module.
2. Settings:
   - Ratio: `0.4` (40% of particles have trails)
   - Lifetime: `0.4`
   - Width over Trail: 0.5 → 0
   - Color over Trail: full alpha gold → alpha 0
3. Trails make the rise feel more dynamic — like floating sparkles.

Skip this if you want simpler. Phase 1 default = no trails.

**6.8 Save prefab**

- Drag `VFX_LevelUp` from Hierarchy → `Assets/_Project/Core/VFX/Prefabs/`.
- Original Prefab.
- Delete from Hierarchy.

**6.9 What you'll see in-game**

When the player levels up (`ExperienceSystem.OnLevelUp` fires):

- 40 gold particles burst upward from the player's feet
- Drift upward over ~1.2 seconds, shrinking and fading
- Total effect lasts about 1.5 seconds
- Player feels rewarded — strong "level up" visual

---

### #7 — Enemy Death Dust

Gray dust cloud at the enemy's position when they die. Should feel "they crumbled" — heavy particles that fall, not rise.

**Sub-steps:**

**7.1 Create GameObject**

1. Hierarchy → Create Empty → name `VFX_EnemyDeath`.
2. Add Component → Particle System.

**7.2 Main module**

| Field            | Value                       | Why                                       |
| ---------------- | --------------------------- | ----------------------------------------- |
| Duration         | `0.5`                       | Short                                     |
| Looping          | ❌                          | One-shot                                  |
| Start Lifetime   | `0.8`                       | Dust lingers briefly                      |
| Start Speed      | `2`                         | Slow outward drift                        |
| Start Size       | `0.35`                      | Larger than sparks — these are dust puffs |
| Start Color      | gray `(160, 160, 160, 255)` | Bone dust color for skeletons             |
| Gravity Modifier | `1`                         | Strong gravity — dust falls               |
| Simulation Space | `World`                     | Dust doesn't follow the dying body        |
| Play On Awake    | ✅                          | Plays on instantiate                      |
| Stop Action      | `Destroy`                   | Self-cleans                               |

**7.3 Emission**

- Rate over Time = `0`
- Bursts → click `+`:
  - Time: `0`, Count: `25`, Cycles: `1`

**7.4 Shape**

| Field            | Value                                                       |
| ---------------- | ----------------------------------------------------------- |
| Shape            | `Sphere`                                                    |
| Radius           | `0.5` (envelopes the enemy's footprint)                     |
| Radius Thickness | `1` (default — particles spawn anywhere inside, then drift) |

**7.5 Color over Lifetime (dim fade)**

1. Enable Color over Lifetime.
2. Gradient → 2 keys:
   - Left: gray full alpha
   - Right: darker gray `(80, 80, 80, 0)` with alpha 0
3. Dust gets dimmer + transparent before disappearing.

**7.6 Size over Lifetime (expand then fade)**

- Enable Size over Lifetime.
- Curve from `0.5` (start small) → `1.5` (mid) → `1` (end). Dust expands as it rises before settling.

Alternatively keep it simple: linear shrink from 1 → 0.

**7.7 (Optional) Rotation over Lifetime**

Rotating particles look more like volumetric dust:

1. Enable Rotation over Lifetime.
2. Angular Velocity: `30` (degrees/sec)

Skip if you want simpler.

**7.8 Save prefab**

- Drag `VFX_EnemyDeath` → `Assets/_Project/Core/VFX/Prefabs/`.
- Original Prefab.
- Delete from Hierarchy.

**7.9 Variant per enemy type (optional)**

If you want green slime drips for slime enemies, blood spray for human enemies, etc:

- Duplicate `VFX_EnemyDeath` and recolor
- Save as `VFX_EnemyDeath_Slime.prefab`, `VFX_EnemyDeath_Human.prefab`, etc.
- Wire the right prefab per enemy type via a per-enemy override (Phase 2 task — defer)

Phase 1 ships with one gray-bone-dust effect for all enemies. Skeletons dominate the dungeon anyway.

**7.10 What you'll see in-game**

When any enemy dies:

- Gray dust cloud erupts at their last position
- Particles drift outward briefly, then fall back down
- Cloud fades over ~0.8 seconds
- Total effect ~1 second

---

### #8 — Boss Enrage Aura

Continuous red aura around the boss when they enter Phase 3 (33% HP). Unlike the other VFX, this **loops** — it stays active until the boss dies.

**Sub-steps:**

**8.1 Create GameObject**

1. Hierarchy → Create Empty → name `VFX_BossEnrage`.
2. Add Component → Particle System.

**8.2 Main module**

| Field            | Value                    | Why                                                                               |
| ---------------- | ------------------------ | --------------------------------------------------------------------------------- |
| Duration         | `5`                      | Internal cycle length (looping anyway, value doesn't matter much above 1)         |
| Looping          | ✅ **CHECKED**           | Stays active continuously                                                         |
| Start Lifetime   | `1.5`                    | Each particle visible for 1.5s                                                    |
| Start Speed      | `1`                      | Slow drift — atmospheric                                                          |
| Start Size       | `0.3`                    | Visible but not overwhelming                                                      |
| Start Color      | red `(200, 30, 30, 255)` | Danger / enrage signal                                                            |
| Gravity Modifier | `-0.2`                   | Slight upward (looks like rising rage)                                            |
| Simulation Space | `Local`                  | Aura moves WITH the boss — `Local` keeps particles relative to the boss transform |
| Play On Awake    | ✅                       | Plays on instantiate                                                              |
| Stop Action      | `None`                   | Don't auto-destroy — VfxBindings handles cleanup on boss death                    |

**Note on Simulation Space:** Local follows the boss's position rotation. The other effects use World because they're spawned once and the object doesn't move. For a persistent aura on a moving boss, Local is the right call.

**8.3 Emission**

- Rate over Time = `30-50` (continuous emission — pick based on how dense you want the aura)
- Bursts → leave empty (no burst — pure continuous emission)

If 50/sec is too dense, drop to 25. If too sparse, raise to 60.

**8.4 Shape**

| Field            | Value       | Why                                                                      |
| ---------------- | ----------- | ------------------------------------------------------------------------ |
| Shape            | `Sphere`    | Aura surrounds the boss                                                  |
| Radius           | `1`         | Roughly boss-body radius                                                 |
| Radius Thickness | `0`         | Shell only — particles spawn on the surface of the sphere, drift outward |
| Position         | `(0, 1, 0)` | Offset upward so the sphere is centered at the boss's chest, not feet    |

**8.5 Color over Lifetime (pulse)**

1. Enable Color over Lifetime.
2. Gradient → 3 keys for a pulsing visual:
   - Left (0%): alpha `0`, color red
   - Middle (50%): alpha `200`, color brighter red `(255, 60, 60, 255)`
   - Right (100%): alpha `0`, color red
3. Each particle fades in → peaks → fades out.

**8.6 Size over Lifetime (grow then shrink)**

- Enable Size over Lifetime.
- Curve: 0 (start) → 1 (mid) → 0 (end). Particles grow into view then shrink before disappearing.

**8.7 (Optional) Noise module — chaotic feel**

For more visual chaos:

1. Enable **Noise** module.
2. Strength: `0.5`
3. Frequency: `1`
4. Particles wobble unpredictably — feels more menacing.

Skip if you want simpler.

**8.8 Save prefab**

- Drag `VFX_BossEnrage` → `Assets/_Project/Core/VFX/Prefabs/`.
- Original Prefab.
- Delete from Hierarchy.

**8.9 How VfxBindings handles this differently**

Unlike one-shot VFX, the enrage aura is:

- Instantiated as a CHILD of the boss when Phase 3 begins
- NOT auto-destroyed by the `defaultLifetime` timer (because we want it persistent)
- Explicitly destroyed by VfxBindings when boss enters `BossPhase.Dead`

You don't need to do anything special on the prefab — the script handles parenting and cleanup automatically.

**8.10 What you'll see in-game**

When the boss drops to 33% HP (Phase 3 entry):

- Red particles continuously bubble outward from the boss's chest area
- Aura follows the boss as it moves and attacks
- Pulsing effect (each particle fades in/peaks/fades out)
- Continues for the entire Phase 3
- Disappears when boss dies (VfxBindings destroys the spawned aura GameObject)

Visual signal: "boss is dangerous now."

---

## Step 5 — Wire prefabs onto VfxBindings

1. Open `3_Dungeon1.unity`.
2. Hierarchy → Create Empty → name `[VfxBindings]` (root level).
3. Add Component → **Vfx Bindings** (`DungeonBlade.Core.VFX.VfxBindings`).
4. **Reference fields** (let auto-find work or wire manually):

   | Field           | Drag                                          |
   | --------------- | --------------------------------------------- |
   | Player Stats    | leave empty (auto-finds via FindObjectOfType) |
   | Player Movement | leave empty                                   |
   | Player Combat   | leave empty                                   |
   | Boss            | leave empty (auto-finds the boss in scene)    |

   You can wire these manually for explicit performance, but auto-find works fine for one-scene single-player.

5. **VFX prefab fields** — drag each prefab from `Assets/_Project/Core/VFX/Prefabs/` into its matching slot:

   | Inspector field          | Prefab              |
   | ------------------------ | ------------------- |
   | Sword Hit Spark Prefab   | `VFX_SwordHitSpark` |
   | Gun Hit Spark Prefab     | `VFX_GunHitSpark`   |
   | Muzzle Flash Prefab      | `VFX_MuzzleFlash`   |
   | Player Damage Vfx Prefab | `VFX_PlayerDamage`  |
   | Dash Trail Prefab        | `VFX_DashTrail`     |
   | Level Up Vfx Prefab      | `VFX_LevelUp`       |
   | Enemy Death Dust Prefab  | `VFX_EnemyDeath`    |
   | Boss Enrage Aura Prefab  | `VFX_BossEnrage`    |

6. **Default Lifetime** = `2` (auto-destroy one-shot VFX after 2 seconds; doesn't affect looping enrage aura because it's parented to boss and cleaned up explicitly).

7. Save scene.

---

## Step 6 — Test

| Action                          | Expected VFX                                              |
| ------------------------------- | --------------------------------------------------------- |
| Equip sword, hit enemy          | Yellow-orange spark at hit point                          |
| Equip gun, fire                 | White flash at gun muzzle + cool-white spark at hit point |
| Take damage                     | Red burst at player position                              |
| Shift + W (roll)                | Cyan trail behind player for 0.6s                         |
| Double-tap A/D (dash)           | Cyan trail                                                |
| Gain enough EXP to level up     | Gold particles rising from player                         |
| Kill an enemy                   | Gray dust cloud at enemy position                         |
| Damage boss to 33% HP (Phase 3) | Red aura particles around boss, continuous                |
| Boss dies                       | Red aura disappears                                       |

---

## Common issues

| Symptom                                      | Fix                                                                                                                                                  |
| -------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------- |
| No VFX at all                                | VfxBindings not in scene, OR all prefab slots empty                                                                                                  |
| Sword hit spark spawns but at wrong position | Sword's `hitOrigin` Transform is offset; particles spawn at `hitOrigin + forward * hitForwardOffset` which is correct, but visually may need tuning  |
| Muzzle flash spawns at world origin          | Gun's `muzzle` field is empty. Click the Gun GameObject (under WeaponHolder) → wire its `muzzle` Transform to a child point at the gun's barrel tip. |
| Particles invisible / black squares          | Material on the Particle System Renderer is broken. Click the particle system → Renderer → Material → set to `Default-ParticleSystem`                |
| Dash trail attaches but stays forever        | `Destroy(go, 0.6f)` should clean it up. Check the script isn't disabled.                                                                             |
| Boss enrage aura doesn't disappear           | Phase event doesn't fire `Dead` after death. Verify `BossBase.Die()` calls `Phase = BossPhase.Dead` (it does, but verify in your scene).             |

---

## Tier-1 minimum (the fastest path to a noticeable upgrade)

If 2-3 hours is too much, just do these 3:

1. `VFX_SwordHitSpark` — combat hits feel impactful
2. `VFX_MuzzleFlash` — gun feels real
3. `VFX_PlayerDamage` — damage taken has weight

Skip dash trail, level up, enemy death, boss enrage. Total time: ~30-40 minutes for the Tier-1 set.

---

## When done

Tell me **"done with #3"** (or "done with Tier-1 VFX" if partial) and I'll:

1. Mark task ✅ in [M9_REMAINING_RANKED.md](M9_REMAINING_RANKED.md).
2. Advance to next task (#4 Item Rarity Glow on Equipped Weapon, or whatever you pick).

---

## Reference

- [M9_REMAINING_RANKED.md](M9_REMAINING_RANKED.md) — task queue
- [Unity Particle System docs](https://docs.unity3d.com/Manual/PartSysMainModule.html) — for tuning beyond what this doc covers
