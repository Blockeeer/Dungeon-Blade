# Animation Setup Guide — Dungeon Blade

This guide walks through wiring Mixamo FBX animations to the player character in Unity. It covers FBX import settings, Animator Controller creation, state machine wiring, and connecting animation events to existing scripts.

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
| Idle | ✅ | ✅ | ❌ | Loops smoothly while standing |
| Run / Walk | ✅ | ✅ | ❌ | Continuous locomotion loop |
| Jump (start) | ❌ | ❌ | ✅ Y, Z | Single takeoff motion |
| Jump (loop / falling) | ✅ | ✅ | ❌ | Held while airborne |
| Jump (land) | ❌ | ❌ | ❌ | Plays once on touchdown |
| Dash / Dodge / Roll | ❌ | ❌ | ✅ Y, Z (root motion off) | One-shot burst |
| Attack swings | ❌ | ❌ | ❌ | One-shot, animation event drives hit frame |
| Death | ❌ | ❌ | ❌ | One-shot, no return |
| Block Idle | ✅ | ✅ | ❌ | Held while blocking |

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

## §8 — Create the PlayerAnimator script

Bridge between gameplay code and animator parameters.

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

1. Open the Dungeon scene (or wherever your Player lives).
2. Select the Player GameObject.
3. Add Component → **Animator** (if not auto-added by the rigged FBX).
4. **Controller** field → drag `Player_Animator.controller`.
5. **Avatar** field → drag the avatar from your character's rigged FBX.
6. **Apply Root Motion** = ❌.
7. Add Component → **Player Animator** (the script from §8).
8. Wire its fields:
   - **Movement** → drag the same Player GameObject (PlayerMovement is on it).
   - **Stats** → same.

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
