# Animation Setup Guide — Dungeon Blade

This guide walks through wiring Mixamo FBX animations to the player character in Unity. It covers FBX import settings, Animator Controller creation, state machine wiring, and connecting animation events to existing scripts.

---

## ⚡ Current Status (as of latest cherry-pick)

**Most of this doc's setup work is already done in your project** via the cherry-picked branch:

| Section | Status |
|---|---|
| §1 — Humanoid rig setup | ✅ Done (FBXes imported with Humanoid avatars) |
| §2 — Clip settings | ✅ Done (Bake/Loop configured) |
| §3 — Disable root motion | ⬜ **VERIFY** — make sure Player's Animator has Apply Root Motion = ❌ |
| §4 — Animator Controller | ✅ `Assets/_Project/Player/Animations/PlayerAnimator.controller` already exists |
| §5 — Add core states | ✅ All states wired in the controller |
| §6 — Add parameters | ✅ 24 parameters (Speed, MoveX, MoveZ, Grounded, Jump, BigJump, Roll, Dodge, Dash, Slide, Tired, Attack, HeavyAttack, Combo, Block, Parry, BlockBroken, Reload, Equip, WeaponType, Hit, BigHit, HitBack, Die) |
| §7 — Wire transitions | ✅ State machine fully connected |
| §8 — Bridge script | ✅ `PlayerAnimatorBridge.cs` already exists with parry trigger added |
| §9 — Wire on Player | ⬜ **YOU DO THIS** — attach Animator to Player + drag controller |
| §10 — Animation events | ⬜ Optional polish for hit-frame-synced damage |

**What you actually need to do:** Steps 3 + 9 (wire the Player GameObject) and test. Everything else is already built.

Skip down to §9 to start, or read §1-§8 for context on what was already configured.

---

**Prerequisites:**
- M1-M10 complete (player movement, combat, etc. are all script-driven and working).
- Animation FBX files placed in `Assets/_Project/Player/Animations/<category>/` per the folder structure.
- Player character mesh (rigged FBX) imported and added to the Player GameObject in scenes.

**What this guide does NOT do:**
- Mixamo download/export (assumed already done — files are local).
- Custom animation creation in Blender.
- Enemy/Boss animations (separate guide later — same pattern).

---

## Folder structure (already set up)

```
Assets/_Project/Player/Animations/
├── Idle/                Breathing Idle, Pistol Idle, Sword And Shield Idle
├── Locomotion/          Running, Standard Run, Sword And Shield Run, Dive Roll,
│                        Standing Dive Forward, Dodging Back
├── Jump/                Jump, Forward Jump
├── Combat/              Great Sword Slash, Stable Sword Outward Slash,
│                        Standing Melee Attack Downward, Sword And Shield Slash,
│                        Sword And Shield Block Idle, Firing Rifle, Reloading,
│                        Reloading-Rifle
├── Reactions/           (empty — add hit/flinch later)
└── Death/               (empty — add death animation later)
```

---

## §1 — Set rig to Humanoid (per FBX)

Mixamo animations come with a generic skeleton. Unity needs to retarget them to a humanoid avatar so the same animation works on different character models.

**Per FBX file, do this once:**

1. In the Project window, click the FBX (e.g. `Idle/Breathing Idle.fbx`).
2. Inspector → **Rig** tab.
3. **Animation Type** = `Humanoid`.
4. **Avatar Definition** = `Create From This Model` (first FBX) or `Copy From Other Avatar` (subsequent FBXes — copy from the first one's avatar).
5. Click **Apply**.

**Tip:** start with `Breathing Idle.fbx` — that becomes your reference avatar. Then for every other FBX, set Avatar Definition = Copy From Other Avatar → drag the avatar from `Breathing Idle.fbx` into the Source field. This ensures all animations share the same rig.

**Bulk speed-up:** select multiple FBXes at once in the Project window → set rig settings on all of them in one Apply.

---

## §2 — Configure clip settings (per FBX)

After setting the rig:

1. Click the FBX → Inspector → **Animation** tab.
2. Find the clip in the bottom list (usually one clip per Mixamo FBX, named after the file).
3. Configure based on clip type:

| Clip type | Loop Time | Loop Pose | Bake Into Pose | Notes |
|---|---|---|---|---|
| Idle                  | ✅ | ✅ | ❌ | Loops smoothly while standing |
| Run / Walk            | ✅ | ✅ | ❌ | Continuous locomotion loop |
| Jump (start)          | ❌ | ❌ | ✅ Y, Z | Single takeoff motion |
| Jump (loop / falling) | ✅ | ✅ | ❌ | Held while airborne |
| Jump (land)           | ❌ | ❌ | ❌ | Plays once on touchdown |
| Dash / Dodge / Roll   | ❌ | ❌ | ✅ Y, Z (root motion off) | One-shot burst |
| Attack swings         | ❌ | ❌ | ❌ | One-shot, animation event drives hit frame |
| Death                 | ❌ | ❌ | ❌ | One-shot, no return |
| Block Idle            | ✅ | ✅ | ❌ | Held while blocking |

**Loop Pose checkbox:** ensures the last frame transitions cleanly back to the first frame. Critical for idles and runs.

**Bake Into Pose (Y, Z):** removes root motion translation on those axes. Since `PlayerMovement.cs` drives movement scripted via CharacterController, root motion would cause double-movement (animation moves the character + script moves the character). Bake Y/Z into the pose to neutralize.

**For dash/dodge specifically:** also check **Bake Into Pose: Position Y** to keep the dash visually horizontal.

4. Click **Apply** after each clip's settings.

---

## §3 — Disable root motion on the Animator (one-time, per character)

Once your character has an Animator component:

1. Select the player character in the Hierarchy.
2. Inspector → **Animator** component.
3. **Apply Root Motion** = ❌ unchecked.

This guarantees animations are visual-only. `PlayerMovement.cs` continues to drive position via CharacterController.Move().

---

## §4 — Create the Animator Controller

The state machine that decides which animation plays.

1. Project window → `Assets/_Project/Player/Animations/`.
2. Right-click → **Create → Animator Controller** → name `Player_Animator.controller`.
3. Double-click to open the Animator window.

You'll see an empty state machine with three default states: `Entry`, `Any State`, `Exit`.

---

## §5 — Add core states

Drag these animations into the Animator window from the Project view. Each becomes a state.

**Recommended initial states (start small, add more later):**

| State name | Source clip | Loop | Notes |
|---|---|---|---|
| `Idle` | `Idle/Breathing Idle.fbx` | ✅ | Default state |
| `Run` | `Locomotion/Running.fbx` | ✅ | Locomotion |
| `Dash` | `Locomotion/Standing Dive Forward.fbx` | ❌ | Shift+direction burst |
| `Dodge_Back` | `Locomotion/Dodging Back.fbx` | ❌ | Backward double-tap |
| `Dodge_Roll` | `Locomotion/Dive Roll.fbx` | ❌ | Side double-tap |
| `Jump` | `Jump/Jump.fbx` | ❌ | Vertical jump |
| `Attack_Sword` | `Combat/Sword And Shield Slash.fbx` | ❌ | Default attack |
| `Attack_Gun` | `Combat/Firing Rifle.fbx` | ❌ | Pistol/gun attack |

**To add a state:**
- Drag the FBX from Project window → into the empty Animator graph.
- Unity creates a state with the FBX's clip auto-assigned.
- Right-click an empty state → Create State → Empty if you need a placeholder.

**Set default state:**
- Right-click `Idle` → **Set as Layer Default State**. The state turns orange.

---

## §6 — Add parameters

Parameters drive transitions. Add these in the **Parameters** tab (left side of Animator window):

| Name | Type | Used for |
|---|---|---|
| `Speed` | Float | Idle ↔ Run blend (driven by movement velocity magnitude) |
| `IsGrounded` | Bool | Block jump when grounded, etc. |
| `Dash` | Trigger | Fires once when Shift+direction dash starts |
| `Dodge` | Trigger | Fires once when double-tap dodge starts |
| `Jump` | Trigger | Fires once when jump starts |
| `Attack` | Trigger | Fires once when LMB pressed |
| `Shoot` | Trigger | Fires once when gun equipped + LMB |
| `Die` | Trigger | Fires when player dies |
| `Equipped` | Int | 0 = unarmed, 1 = sword, 2 = gun (for animation variants) |

---

## §7 — Wire transitions

In the Animator window, click and drag from one state to another to create a transition arrow. Click the arrow to configure.

**Per transition, set:**
- **Has Exit Time** = ❌ unchecked (we want immediate response on input).
- **Transition Duration** = `0.1` seconds (smooth but snappy).
- **Conditions** = the parameter that triggers it.

**Example transitions:**

| From | To | Condition | Notes |
|---|---|---|---|
| `Idle` | `Run` | `Speed` Greater `0.1` | Player starts moving |
| `Run` | `Idle` | `Speed` Less `0.1` | Player stops |
| `Idle` | `Dash` | `Dash` (trigger) | Dash from standing |
| `Run` | `Dash` | `Dash` (trigger) | Dash while running |
| `Dash` | `Idle` | Has Exit Time ✅ (auto-return) | Returns when animation finishes |
| Any State | `Jump` | `Jump` (trigger) + `IsGrounded` true | Jump only from ground |
| `Jump` | `Idle` | `IsGrounded` true (after delay) | Land |
| Any State | `Die` | `Die` (trigger) | Death from anywhere |
| `Idle` / `Run` | `Attack_Sword` | `Attack` (trigger) + `Equipped` Equals 1 | Sword swing |
| `Idle` / `Run` | `Attack_Gun` | `Shoot` (trigger) + `Equipped` Equals 2 | Gun fire |

**For "Any State" transitions (Dash, Dodge, Die):**
- Right-click `Any State` → Make Transition → drop on target state.
- Means "from any state, this transition can fire."
- Good for actions that should be instant regardless of current state.

---

## §8 — Bridge script

✅ **Already exists** at `Assets/_Project/Player/Scripts/Animation/PlayerAnimatorBridge.cs`.

The script subscribes to all relevant gameplay events and drives the Animator parameters:

| Event source | Triggers / sets |
|---|---|
| `PlayerMovement.Jumped` | `Jump` trigger; `BigJump` bool reset to false |
| `PlayerMovement.HighJumpStarted` | `BigJump` bool true (lands as Hard Landing) |
| `PlayerMovement.DashStarted` | `Dash` trigger |
| `PlayerMovement.DodgeStarted` | `Dodge` or `Roll` trigger (picked from `LastBurstDirection`) |
| `PlayerMovement.HorizontalSpeed` / `Velocity` | `Speed` float, `MoveX` / `MoveZ` floats for 2D blend tree |
| `PlayerMovement.IsGrounded` / `IsSliding` | `Grounded` bool, `Slide` bool |
| `PlayerCombat.AttackPerformed` | `Attack` trigger |
| `PlayerCombat.HeavyAttackPerformed` | `HeavyAttack` trigger |
| `PlayerCombat.ReloadPerformed` | `Reload` trigger |
| `PlayerCombat.WeaponEquipped` | `WeaponType` int, `Equip` trigger |
| `Sword.IsBlocking` (continuous read) | `Block` bool |
| `PlayerStats.OnDamaged(amount)` | `Hit` (low damage) or `BigHit` (damage ≥ 25) trigger |
| `PlayerStats.OnParry` | `Parry` trigger |
| `PlayerStats.OnDeath` | `Die` trigger |
| `PlayerStats.IsLowStamina` (continuous read) | `Tired` bool |

**Inspector fields:**
- `Movement` — PlayerMovement component (auto-found via `GetComponent` if same GameObject)
- `Combat` — PlayerCombat component
- `Stats` — PlayerStats component
- `Camera Rig` — the camera rig transform (used to project velocity onto camera basis for strafe/forward blends)

**Tuning fields:**
- `Idle Speed Deadzone = 0.1` — horizontal speed below this is treated as idle
- `Speed Smoothing = 12` — how fast Speed / MoveX / MoveZ chase actual values
- `Reference Walk Speed = 6` — normalizes MoveX/MoveZ to [-1, 1] range
- `Big Hit Threshold = 25` — damage amount ≥ this fires BigHit instead of Hit

---

### Old skeleton script (for reference only — REPLACED by PlayerAnimatorBridge.cs)

The original draft below is kept as a learning reference. The actual production script is more comprehensive (see file path above).

**Create file:** `Assets/_Project/Player/Scripts/Animation/PlayerAnimator.cs`

```csharp
using UnityEngine;

namespace DungeonBlade.Player
{
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimator : MonoBehaviour
    {
        [SerializeField] PlayerMovement movement;
        [SerializeField] PlayerStats stats;

        Animator _animator;

        static readonly int SpeedHash = Animator.StringToHash("Speed");
        static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
        static readonly int DashHash = Animator.StringToHash("Dash");
        static readonly int DodgeHash = Animator.StringToHash("Dodge");
        static readonly int JumpHash = Animator.StringToHash("Jump");
        static readonly int AttackHash = Animator.StringToHash("Attack");
        static readonly int ShootHash = Animator.StringToHash("Shoot");
        static readonly int DieHash = Animator.StringToHash("Die");
        static readonly int EquippedHash = Animator.StringToHash("Equipped");

        void Awake() { _animator = GetComponent<Animator>(); }

        void Update()
        {
            if (movement == null) return;
            float speed = movement.HorizontalSpeed;  // expose this on PlayerMovement
            _animator.SetFloat(SpeedHash, speed);
            _animator.SetBool(IsGroundedHash, movement.IsGrounded);
        }

        public void TriggerDash() => _animator.SetTrigger(DashHash);
        public void TriggerDodge() => _animator.SetTrigger(DodgeHash);
        public void TriggerJump() => _animator.SetTrigger(JumpHash);
        public void TriggerAttack() => _animator.SetTrigger(AttackHash);
        public void TriggerShoot() => _animator.SetTrigger(ShootHash);
        public void TriggerDie() => _animator.SetTrigger(DieHash);
        public void SetEquipped(int weaponType) => _animator.SetInteger(EquippedHash, weaponType);
    }
}
```

**Required additions to `PlayerMovement.cs`:**

Expose `HorizontalSpeed` and `IsGrounded` for the animator to read. Add to PlayerMovement:

```csharp
public float HorizontalSpeed => new Vector2(_velocity.x, _velocity.z).magnitude;
public bool IsGrounded => _isGrounded;
```

Then call `playerAnimator.TriggerDash()` etc. from PlayerMovement when the corresponding state starts.

---

## §9 — Wire on the Player GameObject

**Goal:** attach the existing Animator Controller + PlayerAnimatorBridge to your scene's Player.

### 9a — Verify the Player has a visible character model

The Player GameObject should already have a rigged character mesh as a child (e.g. `Aurelia`, `Lyra`, `Kaelen`). If not, drag one from `Assets/_Project/Player/Models/` into the Hierarchy as a child of Player.

### 9b — Add the Animator component

1. Open `3_Dungeon1.unity` (and later `2_Lobby.unity`).
2. Hierarchy → click **the rigged character child** under Player (e.g. `Aurelia`). This is the GameObject that owns the skeleton, NOT the Player root.
3. Inspector → **Add Component** → search `Animator` → click `Animator`.
4. Inspector wiring on the Animator:

   | Field | Value |
   |---|---|
   | **Controller** | drag `Assets/_Project/Player/Animations/PlayerAnimator.controller` from Project window |
   | **Avatar** | drag the Avatar sub-asset from the character's FBX (in `Assets/_Project/Player/Models/`) — usually named `<CharacterName>Avatar` |
   | **Apply Root Motion** | ❌ unchecked (CRITICAL — PlayerMovement drives position) |
   | **Update Mode** | `Normal` (default) |
   | **Culling Mode** | `Cull Update Transforms` (default) |

### 9c — Add the PlayerAnimatorBridge component

The bridge subscribes to gameplay events and pushes parameters to the Animator above.

1. Same Animator-bearing GameObject (the character child) → **Add Component** → search `Player Animator Bridge`.
2. Inspector wiring (most auto-find since they're on Player root):

   | Field | Drag from Hierarchy |
   |---|---|
   | Movement | `Player` GameObject (root — has PlayerMovement) |
   | Combat | `Player` GameObject (has PlayerCombat) |
   | Stats | `Player` GameObject (has PlayerStats) |
   | Camera Rig | the camera rig Transform under Player |

   If the script auto-finds via `GetComponent` (since the character is a child of Player), some of these may already be wired — verify before manually dragging.

3. **Tuning fields:** leave at defaults unless something feels off.

### 9d — Save scene and test

1. Ctrl+S.
2. Press Play.
3. **Expected:**
   - Character idles (Breathing Idle) on spawn
   - Walking forward → Running animation
   - Strafe (A/D) → Strafe animations
   - Backpedal (S) → Walking Backwards
   - Press Space → Jump animation, lands → Landing animation
   - Hold Shift + W → Roll forward
   - Double-tap A → Side dodge / dive roll
   - LMB sword swing → Sword And Shield Slash animation
   - LMB gun fire → Firing Rifle animation
   - Take damage → Hit Reaction
   - Die → Falling Back Death

### Common issues

| Symptom | Fix |
|---|---|
| Character T-poses, no animation | Animator Controller not wired, or Avatar field empty |
| Character animates but moves wrong | Apply Root Motion is ON — uncheck it |
| Walks but doesn't strafe | Camera Rig field on Bridge is empty — wire it |
| Animations play but character feels disconnected | Speed Smoothing = 12 is fine; try 8-15 range |
| Hit reaction never fires | PlayerStats reference on Bridge is empty, OR no damage actually landed |
| Attack animation plays but enemies don't take damage | That's expected — attack damage uses MeleeHitbox sweep, not animation events (yet). See §10 to fix. |

---

## §10 — Animation events for combat hits

Hit detection should fire on a specific frame of an attack animation, not on a fixed timer.

**Per attack clip:**

1. Click the FBX → Inspector → Animation tab → pick the clip.
2. Scrub to the frame where the sword visually contacts the target.
3. Click **Add Event** in the timeline.
4. Set **Function** = `OnAttackHitFrame`.
5. Apply.

**Add the receiver to PlayerCombat.cs:**

```csharp
public void OnAttackHitFrame()
{
    // existing hit detection logic that was previously in Update()
    meleeHitbox.Sweep(...);
}
```

This makes hit detection visually-synced. Players see the swing connect at the exact frame the hitbox checks.

**Same pattern for footstep SFX** (M9 audio):
- Add events `OnFootstepLeft` and `OnFootstepRight` to walk/run clips.
- Receiver plays a footstep sound from a pool.

---

## §11 — Tips and gotchas

1. **Mixamo animations face Z+** by default. If your character spawns facing the wrong direction, rotate the model 180° on Y in the FBX import settings or wrap it in a child GameObject.

2. **Generic vs Humanoid rig:** for Mixamo, always use Humanoid. Generic only works if all your characters share the exact same skeleton.

3. **Avatar mismatch warnings:** if Unity warns about avatar mismatches, the issue is usually one FBX has a different bone setup. Re-set its rig to Humanoid + Copy Avatar From Other Model.

4. **Slow Animator transitions:** if attacks feel sluggish, transition duration is too long. Reduce to `0.05`-`0.08` for snappy combat.

5. **Animation locks input:** if pressing dash mid-attack does nothing, the attack state probably has Has Exit Time ✅. Add a transition `Attack → Dash` with Has Exit Time ❌ + Trigger condition Dash.

6. **Player T-poses:** your Animator Controller field on the Animator component is empty, OR no transition leads from Entry to a default state. Set `Idle` as the layer default state.

7. **Dash visual mismatch:** Mixamo's "Standing Dive Forward" may not match your custom 0.18s dash duration. Either retime the clip (Speed parameter on the state) or accept the visual mismatch — gameplay is what matters.

---

## §12 — Recommended build order

Don't try to wire all 18 animations at once. Layer them in this order to avoid getting stuck:

1. **Idle + Run + Speed parameter** — confirm character animates standing/moving.
2. **Jump trigger** — confirm jumps play correctly.
3. **Dash trigger** — confirm Shift+direction plays the dive forward animation.
4. **Attack trigger** — confirm LMB plays the sword slash and damage syncs to the hit frame.
5. **Death trigger** — confirm dying plays the death animation.
6. **Equipped int + per-weapon idle/attack variants** — switch animations based on equipped weapon.
7. **Polish** — strafe walks, block idle, dodge variants.

After each layer works, test in Play mode before moving on. Roll back if something breaks instead of stacking debugging on top of debugging.

---

## §13 — Future additions

After the above is working, consider grabbing these from Mixamo:

- **Hit reactions** (`Hit Reaction`, `Hit From Front`) → `Reactions/`
- **Death** (`Dying`, `Falling Back Death`) → `Death/`
- **Land** (`Land`) → `Jump/`
- **Strafe walks** (`Walking Left`, `Walking Right`, `Walking Backward`) → `Locomotion/`
- **Climb / vault / wall jump** if you expand traversal mechanics

Drop them into the matching folder and add states + transitions as needed.

---

## §14 — Reference: existing scripts that benefit from animation hooks

| Script | What animation adds |
|---|---|
| `PlayerMovement.cs` | TriggerDash/TriggerDodge/TriggerJump on state start |
| `PlayerCombat.cs` | TriggerAttack/Shoot on input; OnAttackHitFrame from anim event |
| `PlayerStats.cs` | TriggerDie when health hits 0 |
| `EquipmentBinder.cs` | SetEquipped(weaponType) when weapon swaps |
| `RespawnManager.cs` | Reset to Idle state when respawning |

---

## Animation troubleshooting checklist

When something doesn't play:

1. ☐ Animator Controller is set on the Animator component.
2. ☐ Avatar is set on the Animator component.
3. ☐ Default state (orange) is set in the Animator graph.
4. ☐ Apply Root Motion is unchecked.
5. ☐ All FBXes are Humanoid rig.
6. ☐ All transitions have Has Exit Time unchecked unless explicitly desired.
7. ☐ Trigger parameter spelling matches between code (`Animator.StringToHash`) and Animator window.
8. ☐ Player GameObject has the PlayerAnimator script wired with Movement + Stats references.
9. ☐ The state's clip is actually pointing to the FBX's clip, not the FBX itself.
10. ☐ Console shows no `Avatar mismatch` or `Mecanim parameter not found` warnings.

If still stuck, add `Debug.Log` to PlayerAnimator's trigger calls to confirm they fire. If logs print but animation doesn't change → the Animator Controller's transition isn't wired. If logs don't print → PlayerMovement isn't calling the trigger function.
