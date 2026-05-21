# Animation Shopping List — GunZ-style Player

Complete list of animations to source (Mixamo) or author (Blender) for a GunZ: The Duel inspired player. Designed to slot into a fresh Animator controller with the existing parameters: `Speed`, `MoveX`, `MoveZ`, `Grounded`, `Jump`, `BigJump`, `Dash`, `Dodge`, `Roll`, `Slide`, `Tired`, `Attack`, `HeavyAttack`, `Combo`, `Block`, `Parry`, `Reload`, `Equip`, `WeaponType`, `Hit`, `BigHit`, `Die`.

**Goal:** clean replacement for the current Mixamo set that doesn't fit the game's pace.

---

## How to use this doc

1. **Mixamo column** = direct search term on https://www.mixamo.com (filter: In Place where applicable).
2. **Priority column** — P0 = blocking, P1 = important, P2 = nice-to-have.
3. **Param column** = the Animator parameter / state name the clip drives.
4. As you download a clip, tick its checkbox and drop the FBX into the matching folder.
5. When all P0 clips are gathered, we wire them into a fresh controller.

**Folder convention:**

```
Assets/_Project/Player/Animations/
├── Idle/           — idle variants per weapon
├── Locomotion/     — walk/run/strafe/sprint
├── Jump/           — jump/fall/land variants
├── Burst/          — dash/dodge/roll/slide (action-game-specific)
├── Combat/         — attacks/blocks/reloads
├── Reactions/      — hits/stagger
└── Death/          — death + revive
```

---

## §1 — Idle (P0)

Looped, smooth, weapon-aware. One per weapon class so the body posture differs (sword down vs gun raised vs unarmed relaxed).

| State | Param trigger | Mixamo search | Priority | Loop? |
|---|---|---|---|---|
| `Idle_Unarmed` | WeaponType = 0, Speed < deadzone | "Breathing Idle" or "Standing Idle 01" | P0 | ✅ |
| `Idle_Sword` | WeaponType = 1 | "Sword And Shield Idle" or "Standing Sword Idle" | P0 | ✅ |
| `Idle_GreatSword` | WeaponType = 2 | "Great Sword Idle" or "Idle (Holding Sword Outward)" | P1 | ✅ |
| `Idle_Pistol` | WeaponType = 3 | "Pistol Idle" or "Gun Idle" | P0 | ✅ |
| `Idle_Rifle` | WeaponType = 4 | "Rifle Aim Idle" or "Standing Aim Idle 01" | P0 | ✅ |

**FBX settings (all):** Loop Time ✅, Loop Pose ✅, Bake Rotation/Y/XZ ✅.

---

## §2 — Locomotion Blend Tree (P0)

Drives the 2D Freeform Cartesian Blend Tree on `MoveX` (strafe) and `MoveZ` (forward/back). One full set per weapon class — minimum: unarmed, sword, gun.

### §2a — Unarmed locomotion (P0)

| Position (MoveX, MoveZ) | Mixamo search | Loop? |
|---|---|---|
| (0, 0) — center | "Breathing Idle" (reused from §1) | ✅ |
| (0, 1) — W forward | "Running" or "Run Forward" In-Place | ✅ |
| (0, -1) — S backward | "Walking Backwards" or "Run Backward" | ✅ |
| (-1, 0) — A strafe left | "Left Strafe Walk" or "Strafe Left" | ✅ |
| (1, 0) — D strafe right | "Right Strafe Walk" or "Strafe Right" | ✅ |
| (0, 1.5) — sprint forward (optional) | "Fast Run" or "Sprint" | ✅ |

### §2b — Sword locomotion (P1)

Same 5 positions but with sword-held posture. Mixamo:
- `Sword And Shield Idle` (center)
- `Sword And Shield Run` (W)
- `Sword Walking Backward` (S)
- `Strafe Left Sword` (A)
- `Strafe Right Sword` (D)

### §2c — Gun locomotion (P1)

Same 5 positions, aim-pose:
- `Rifle Aim Idle` (center)
- `Rifle Walk Forward` (W) — keep gun raised
- `Rifle Walk Backward` (S)
- `Rifle Strafe Left` (A)
- `Rifle Strafe Right` (D)

**FBX settings:** Loop Time ✅, Loop Pose ✅, Bake Rotation/Y/XZ ✅ (CharacterController drives all world translation).

---

## §3 — Jumps + Air (P0)

Triggered states. No blending — single clips played start-to-finish.

| State | Trigger | Mixamo search | Loop? | Bake settings |
|---|---|---|---|---|
| `Jump_Start` | Jump trigger, Grounded=true → false | "Jump" or "Standing Jump" | ❌ | Rotation/Y/XZ all baked |
| `Jump_Loop` (fall) | Grounded = false after Jump_Start | "Falling Idle" or "Jump Loop" | ✅ | Rotation/XZ baked, Y unbaked |
| `Land` | Grounded false → true | "Hard Landing" (mild) or "Standing Land" | ❌ | All baked |
| `HardLand` | BigJump=true at land | "Heavy Landing" or "Hard Landing" | ❌ | All baked |
| `WallRun_R` (optional) | wallRun + right wall | "Run To Wall" or "Wall Run Right" | ✅ | All baked |
| `WallRun_L` (optional) | wallRun + left wall | "Wall Run Left" | ✅ | All baked |
| `WallJump` (optional) | wallRun + Jump trigger | "Wall Jump" | ❌ | All baked |

**Note for GunZ feel:** GunZ wall-running was very dynamic; if you replicate this fully, source clips with a leaned posture so it reads as wall-running not just running.

---

## §4 — Burst movement (P0) — the GunZ signature

This is the *most important* category for GunZ-feel. Bursts must be ultra-snappy with minimal wind-up.

| State | Trigger | Mixamo search | Notes |
|---|---|---|---|
| `Roll_Forward` | Roll trigger, MoveZ > 0 | "Roll Forward" or "Diving Forward Roll" | The flagship GunZ dive |
| `Roll_Back` | Roll trigger, MoveZ < 0 | "Roll Backward" or "Standing Dodge Backward" | |
| `Roll_Left` (optional) | Roll trigger, MoveX < 0 | "Roll Left" or "Dodge Left" | |
| `Roll_Right` (optional) | Roll trigger, MoveX > 0 | "Roll Right" or "Dodge Right" | |
| `Slide` | Slide bool true | "Sliding" or "Running Slide" | GunZ butt-slide |
| ~~`Dash_Side`~~ | — | **Skip** — sideways double-tap A/D is physics-only by your design |

**Critical FBX setting:** ALL three Root Transform sections (Rotation, Position Y, Position XZ) → Bake Into Pose ✅. The script drives motion, animation is visual-only.

**Clip speed note:** if Mixamo's "Roll Forward" feels too slow, set the state's `Speed` to 1.5-2.0 in the Animator.

---

## §5 — Combat — Sword (P0/P1)

Three-hit melee combo + heavy attack + block. Combo index handled via `Combo` int parameter (0/1/2 = swing 1/2/3).

| State | Trigger condition | Mixamo search | Loop? |
|---|---|---|---|
| `Attack_Sword_1` | Attack, Combo=0, WeaponType=1 | "Sword Slash" or "Sword And Shield Slash" | ❌ |
| `Attack_Sword_2` | Attack, Combo=1, WeaponType=1 | "Stable Sword Outward Slash" or "Sword Combo Attack 2" | ❌ |
| `Attack_Sword_3` | Attack, Combo=2, WeaponType=1 | "Sword Heavy Slash Finisher" or "Two-Handed Sword Slash" | ❌ |
| `Attack_GreatSword_1` | Attack, WeaponType=2 | "Great Sword Slash" or "Two Handed Slash" | ❌ |
| `Attack_GreatSword_Heavy` | HeavyAttack, WeaponType=2 | "Great Sword Heavy Strike" | ❌ |
| `Block_Sword` | Block bool true | "Sword Block Idle" or "Standing Block" | ✅ |
| `BlockImpact` | BlockBroken trigger (if added) | "Sword Block React" or "Block Impact" | ❌ |
| `Parry` | Parry trigger | "Sword Counter" or "Parry" | ❌ |

**FBX settings:** Loop Time ❌ except Block_Sword. All Bake Pose options ✅ to prevent rotation drift during attack spam.

---

## §6 — Combat — Guns (P0/P1)

GunZ-style hip-fire (no ADS lock) is fine, but provide aim-down poses for the rifle.

| State | Trigger | Mixamo search | Loop? |
|---|---|---|---|
| `Fire_Pistol` | Attack, WeaponType=3 | "Firing Pistol" or "Pistol Shoot" | ❌ |
| `Fire_Rifle` | Attack, WeaponType=4 | "Firing Rifle" or "Rifle Shoot" | ❌ |
| `Aim_Pistol` | (handled via Idle_Pistol pose, RMB held) | "Pistol Aim Idle" | ✅ |
| `Aim_Rifle` | (handled via Idle_Rifle pose, RMB held) | "Rifle Aim Idle" | ✅ |
| `Reload_Pistol` | Reload, WeaponType=3 | "Reloading" or "Pistol Reload" | ❌ |
| `Reload_Rifle` | Reload, WeaponType=4 | "Reloading-Rifle" or "Rifle Reload" | ❌ |
| `Melee_Pistol` | HeavyAttack with gun (pistol whip) | "Pistol Whip" | ❌ |
| `Melee_Rifle` | HeavyAttack with rifle (butt-stroke) | "Rifle Hit" or "Butt Stroke" | ❌ |

---

## §7 — Combat — Unarmed (P1)

Light punch combo for when nothing is equipped.

| State | Trigger | Mixamo search |
|---|---|---|
| `Attack_Unarmed_1` | Attack, WeaponType=0, Combo=0 | "Punching" or "Boxing Cross Punch" |
| `Attack_Unarmed_2` | Attack, WeaponType=0, Combo=1 | "Boxing Right Cross" |
| `Attack_Unarmed_Kick` | HeavyAttack, WeaponType=0 | "Kicking" or "Front Kick" |

---

## §8 — Reactions (P0/P1)

Hit reactions sell impact. Don't skip these — combat without reactions feels weightless.

| State | Trigger | Mixamo search |
|---|---|---|
| `Hit_Front` | Hit trigger, damage from front | "Hit Reaction" or "Standing React Light From Front" |
| `Hit_Back` | Hit trigger, damage from back | "Standing React Small From Back" |
| `Hit_Left` | Hit trigger, damage from left (optional) | "Standing React Light From Left" |
| `Hit_Right` | Hit trigger, damage from right (optional) | "Standing React Light From Right" |
| `BigHit` | BigHit trigger (damage >= 25) | "Big Hit" or "Standing React Large" |
| `Stagger` | optional flinch w/ tiny knockback | "Stagger" or "Hit React Stagger" |
| `Stunned` (optional) | combo'd into stun | "Standing React Confused" |

**FBX settings:** Bake Pose ✅ all axes (don't let hit anim translate the character).

---

## §9 — Death + Revive (P0)

| State | Trigger | Mixamo search | Loop? |
|---|---|---|---|
| `Death_Front` | Die trigger, killing blow from front | "Falling Back Death" or "Dying" | ❌ |
| `Death_Back` | Die trigger, killing blow from back | "Falling Forward Death" | ❌ |
| `Death_Headshot` (optional) | Die trigger, headshot tag | "Death From Headshot" | ❌ |
| `Respawn` (Phase 2) | OnRespawn | "Stand Up" or "Getting Up" | ❌ |

**FBX settings:** Loop Time ❌, Bake all axes ✅ (don't slide post-death).

---

## §10 — Equip / Weapon swap (P0)

Two seconds of "putting weapon away / drawing new one." Used between weapon switches.

| State | Trigger | Mixamo search |
|---|---|---|
| `Equip_Sword` | Equip trigger, WeaponType→1 | "Draw Sword" or "Sword Equip" |
| `Equip_GreatSword` | Equip trigger, WeaponType→2 | "Two Hand Sword Equip" |
| `Equip_Pistol` | Equip trigger, WeaponType→3 | "Draw Pistol" or "Pistol Equip" |
| `Equip_Rifle` | Equip trigger, WeaponType→4 | "Grab Rifle From Back" or "Rifle Equip" |
| `Sheathe_Generic` (optional) | between Equip triggers | "Sheathe Sword" |

---

## §11 — GunZ-specific signature moves (P1/P2)

These are what make GunZ feel like GunZ. Not strict requirements but highly recommended.

| State | Trigger | Mixamo search / Blender |
|---|---|---|
| `WallRun_R` / `WallRun_L` | wallRun system in PlayerMovement | "Wall Run Right" / "Wall Run Left" — if missing, Blender custom |
| `WallJump` | wallRun + Jump | "Wall Jump" |
| `DashAttack` (slash while moving fast) | Attack during dash window | Blender custom — combine "Sword Slash" upper body + dash lower body |
| `AirSwing` (slash mid-air) | Attack while !Grounded with sword | "Air Sword Attack" or "Jump Slash" |
| `AirShot` (fire mid-air) | Attack while !Grounded with gun | Mixamo "Firing Rifle" works if Y baked |
| `KStyle` flick reload (GunZ-specific reload cancel) | rapid Attack→Reload→Attack chain | Code-side, not animation |

---

## §12 — Tired / Low Stamina (P2)

When stamina drops below threshold, layered breathing animation.

| State | Trigger | Mixamo search |
|---|---|---|
| `Idle_Tired` (overrides Idle_Unarmed if Tired=true) | Tired bool true | "Running Tired" or "Tired Idle" |
| `Run_Tired` | Tired + locomotion | "Running Tired" |

**Implementation:** put as an Override layer with `Tired` bool weight, instead of branching from each Idle.

---

## §13 — Total clip count

| Category | P0 count | P1 count | P2 count |
|---|---|---|---|
| Idle | 4 | 1 | 0 |
| Locomotion | 5 | 10 (sword + gun sets) | 1 (sprint) |
| Jumps + Air | 4 | 2 (wallrun) | 1 (walljump) |
| Burst (rolls + slide) | 3 | 2 (side rolls) | 0 |
| Combat Sword | 4 | 4 | 0 |
| Combat Guns | 6 | 2 | 0 |
| Combat Unarmed | 0 | 3 | 0 |
| Reactions | 2 | 4 | 0 |
| Death | 1 | 2 | 1 (revive) |
| Equip | 2 | 3 | 0 |
| GunZ signature | 0 | 4 | 2 |
| Tired | 0 | 0 | 2 |
| **Total** | **31** | **37** | **7** |

**Minimum viable** = 31 P0 clips. Two evenings of Mixamo browsing.

---

## §14 — Bake Into Pose default policy

Apply on EVERY downloaded clip unless flagged otherwise:

| Section | Bake Into Pose | Based Upon | Notes |
|---|---|---|---|
| Root Transform Rotation | ✅ | Original | Stops drift on attack spam |
| Root Transform Position (Y) | ✅ | Original | Stops sinking, except Jump_Loop (keep Y unbaked for fall pose) |
| Root Transform Position (XZ) | ✅ | Center of Mass | All world XZ from CharacterController, not animation |

Apply by selecting all FBXes in a folder at once, then setting these on the multi-edit Inspector.

---

## §15 — Authoring in Blender (when Mixamo fails)

If a clip doesn't exist on Mixamo (e.g. `DashAttack`, `AirSwing`, custom GunZ-flick):

1. Import Mixamo "base T-pose" of your character into Blender.
2. Use Mixamo Auto-Rigger output as the armature.
3. Build the action as a 0.4-0.8s pose-to-pose, focus on the **upper body** since lower body is often replaced by scripted motion.
4. Export as FBX with Bake All Bones ✅, Apply Modifiers ✅, **without** the mesh (just the armature animation).
5. In Unity: drag the FBX, set Rig=Humanoid + Copy Avatar from Other Model (Varion's Avatar).
6. The clip retargets to all 6 characters automatically.

**Tip:** for hybrid clips like `AirSwing`, author only frames where the upper body matters and let Mecanim blend the lower body from the previous state.

---

## §16 — Suggested fetch order

Do them in waves so you can test as you go, not all-at-once.

### Wave 1 (P0 minimum, ~30 min)
1. `Idle_Unarmed` — base Idle
2. 5 unarmed locomotion clips — fill the Blend Tree
3. `Jump_Start`, `Jump_Loop`, `Land`, `HardLand` — air system
4. `Roll_Forward`, `Roll_Back`, `Slide` — burst movement
5. `Hit_Front`, `BigHit` — reactions
6. `Death_Front` — gameover

**After Wave 1:** wire fresh controller, test movement → combat is still empty but should feel right.

### Wave 2 (P0 sword combat, ~20 min)
7. `Idle_Sword`, `Attack_Sword_1`, `Attack_Sword_2`, `Attack_Sword_3`, `Block_Sword`, `Equip_Sword`

**After Wave 2:** sword combo + block + draw works.

### Wave 3 (P0 gun combat, ~20 min)
8. `Idle_Pistol`, `Idle_Rifle`, `Fire_Pistol`, `Fire_Rifle`, `Reload_Pistol`, `Reload_Rifle`, `Equip_Pistol`, `Equip_Rifle`

**After Wave 3:** all P0 done.

### Wave 4+ (P1/P2 polish)
Sword/gun locomotion variants, hit reactions per direction, wallrun, dash attacks. Iterate based on what feels missing in playtest.

---

## §17 — How this plugs into a fresh controller

Once Wave 1 is downloaded, controller structure (using sub-state machines for organization):

```
Base Layer
├── Locomotion (Blend Tree)      ← default state, MoveX/MoveZ driven
├── Air (sub-state machine)
│   ├── Jump_Start
│   ├── Jump_Loop
│   ├── Land
│   ├── HardLand
│   └── WallRun (future)
├── Burst (sub-state machine)
│   ├── Roll_Forward
│   ├── Roll_Back
│   └── Slide
├── Combat_Sword (sub-state machine)
│   ├── Attack_Sword_1
│   ├── Attack_Sword_2
│   ├── Attack_Sword_3
│   ├── Block_Sword
│   ├── Parry
│   └── Equip_Sword
├── Combat_Gun (sub-state machine)
│   ├── Fire_Pistol
│   ├── Fire_Rifle
│   ├── Reload_Pistol
│   ├── Reload_Rifle
│   ├── Melee_Pistol
│   └── Equip_Pistol / Equip_Rifle
├── Combat_Unarmed (sub-state machine)
│   ├── Attack_Unarmed_1
│   ├── Attack_Unarmed_2
│   └── Attack_Unarmed_Kick
├── Reactions (sub-state machine)
│   ├── Hit_Front / Hit_Back / Hit_Left / Hit_Right
│   ├── BigHit
│   └── Stagger
└── Death (sub-state machine)
    ├── Death_Front
    └── Death_Back
```

**8 top-level groups instead of 20+ scattered states.** Empty state for "between things"? Not needed at this level — Locomotion is the default; everything else fires from Any State.

---

## §18 — When clips are gathered

Tell me **"Wave 1 done"** when you've placed the first 14 clips, and I'll:
1. Write `FRESH_CONTROLLER_SETUP.md` — step-by-step Unity wiring guide
2. Optionally rebuild the `PlayerAnimator.controller` YAML from scratch (high risk — only if you really want it scripted)

Most likely path: you'll build the new controller in Unity yourself using the guide. Way safer.

---

## §19 — What this replaces

This shopping list is intended to **replace** the current set of Mixamo clips that don't fit:
- Generic Mixamo running cycle → snappier in-place run
- Wide stable stance → readier action stance
- Slow dive-roll → punchy GunZ-style roll
- Mismatched aim poses → consistent hipfire-ready

The **existing FBXes** in `Assets/_Project/Player/Animations/` can be kept as fallback or deleted when their replacements are in. Recommend keeping until everything plays correctly in scene.

---

## Reference

- [ANIMATION_SETUP.md](ANIMATION_SETUP.md) — original animation wiring guide
- [ANIMATION_FIX_ROTATION_DRIFT.md](ANIMATION_FIX_ROTATION_DRIFT.md) — Bake Into Pose deep dive
- [DungeonBlade GDD v1.md](DungeonBlade%20GDD%20v1.md) — game design context
- [Mixamo browser](https://www.mixamo.com) — primary source (free with Adobe account)
- [GunZ: The Duel movement reference](https://en.wikipedia.org/wiki/GunZ:_The_Duel) — for K-style / butterfly understanding
