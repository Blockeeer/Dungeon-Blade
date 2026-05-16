# Fix — Character rotation drifts during repeated attacks

**Symptom:** spam LMB attacks → character body slowly rotates each swing → eventually faces a different direction than the camera.

**Cause:** the attack FBX clips have **root rotation** baked into the Mixamo animation. Even with `Apply Root Motion = OFF` on the Animator (which gates root **translation**), Humanoid retargeting still feeds **rotation** from the clip into the character's body. Each attack adds a small yaw delta. Over many attacks, the drift accumulates.

**Fix:** bake the rotation into the clip's default pose so the character stays planted while the upper body animates. Per-clip Animation Import setting.

---

## Step 1 — Open each attack FBX

Affected clips (the ones you fire repeatedly during gameplay):

| FBX | Location | Why |
|---|---|---|
| `Sword And Shield Slash.fbx` | `Assets/_Project/Player/Animations/Combat/` | Light attack — primary culprit |
| `Stable Sword Outward Slash.fbx` | same folder | Combo attack 2 |
| `Standing Melee Attack Downward.fbx` | same folder | Combo attack 3 |
| `Great Sword Slash.fbx` | same folder | Heavy / alt swing |
| `Sword And Shield Slash.fbx` | same folder | (listed again — confirm both combo states) |
| `Thrust Slash.fbx` | same folder | Optional combo |
| `Firing Rifle.fbx` | same folder | Gun fire |
| `Pistol Whip.fbx` | same folder | Gun melee |

Apply the same fix to ALL Combat clips. Even ones that don't seem to rotate now — the fix is harmless on already-correct clips.

---

## Step 2 — Set Bake Into Pose for rotation

For each FBX:

1. Project window → click the FBX (e.g. `Sword And Shield Slash.fbx`).
2. Inspector → **Animation** tab.
3. Scroll down to the **clip list** (usually one clip per FBX).
4. Click the clip name to select it.
5. Look at the section under the clip preview labeled **Root Transform Rotation**:

   | Field | Set to |
   |---|---|
   | **Bake Into Pose** | ✅ **CHECKED** |
   | **Based Upon** | `Original` (default) |
   | **Offset** | `0` |

6. Also for **Root Transform Position (Y)**:
   - Bake Into Pose: ✅
   - Based Upon: `Original`
   - Offset: `0`

7. Also for **Root Transform Position (XZ)**:
   - Bake Into Pose: ✅ (keeps the character from sliding during the swing)
   - Based Upon: `Center of Mass`
   - Offset: `0`

8. Click **Apply** at the bottom of the Inspector. (Important — without Apply, changes are lost.)

### Quick-find tip

After clicking the clip in step 4, scroll the Inspector to find a section like this:

```
Root Transform Rotation
[ ] Bake Into Pose
Based Upon: Body Orientation
Offset: 0

Root Transform Position (Y)
[ ] Bake Into Pose
Based Upon: Center of Mass
Offset: 0

Root Transform Position (XZ)
[ ] Bake Into Pose
Based Upon: Center of Mass
```

You want all three "Bake Into Pose" boxes **checked**.

---

## Step 3 — Apply same fix to roll/dodge clips (so backward dodge doesn't drift forward, etc.)

The same fix applies to one-shot bursts:

| FBX | Why |
|---|---|
| `Dive Roll.fbx` | Side dodge |
| `Standing Dive Forward.fbx` | Forward roll |
| `Dodging Back.fbx` | Backward dodge |

For these:
- Root Transform Rotation → Bake Into Pose ✅
- Root Transform Position (Y) → Bake Into Pose ✅
- Root Transform Position (XZ) → **leave UNchecked** (we want a tiny forward push from the animation, BUT only on the local Z axis — the PlayerMovement script provides the world translation, and we want the animation's translation curve baked-out except for Z to keep visual momentum)

Actually, simpler rule for all bursts: **bake all 3 (Rotation, Position Y, Position XZ) into pose**. The PlayerMovement code drives the actual motion via `_dashDirection * _activeBurstSpeed`. The animation should be visual-only.

Set all 3 baked for: `Dive Roll`, `Standing Dive Forward`, `Dodging Back`.

---

## Step 4 — Locomotion clips (running/walking)

For loop animations:

| FBX | Settings |
|---|---|
| `Running.fbx` / `Standard Run.fbx` | Root Rotation Baked ✅; Position Y Baked ✅; Position XZ Baked ✅ |
| `Walking Backwards.fbx` | Same — all 3 baked |
| `Left Strafe Walking.fbx` / `Right Strafe Walking.fbx` | Same |
| `Breathing Idle.fbx` | All 3 baked (idle shouldn't move at all) |

---

## Step 5 — Jump clips

| FBX | Settings |
|---|---|
| `Jump.fbx` | Rotation baked ✅; Position Y baked ✅; XZ baked ✅ |
| `Forward Jump.fbx` | Same |
| `Landing.fbx` / `Hard Landing.fbx` | Same — all 3 baked |

The PlayerMovement script drives the jump arc with physics. Animations are visual.

---

## Step 6 — Verify

After applying to all clips:

1. Save Unity (Ctrl+S in any scene to force a save and recompile).
2. Press Play.
3. **Test rotation drift:**
   - Stand still, face a wall.
   - Spam LMB 20 times.
   - **Expected:** character body stays facing the wall the entire time. Only arms move.
   - **If still drifting:** one of the attack clips still has Rotation un-baked. Re-check each combat FBX.

4. **Test dash drift:**
   - Stand facing wall.
   - Press Shift+W → forward roll → check facing direction after.
   - Should still face wall. Body doesn't pivot.

---

## Why this is happening

Unity's **Humanoid retargeting** uses two concepts:

- **Body Transform** — the abstract "this is the character" rotation/position (drives the visible mesh).
- **Bone Transforms** — individual limb positions/rotations.

By default, Mixamo clips encode small **body rotation deltas** to give swings momentum ("the body twists into the strike"). Mecanim plays these back, rotating the entire character. With `Apply Root Motion = OFF`, the **translation** is killed but **rotation** still applies.

"Bake Into Pose" on Root Transform Rotation says: "treat this clip's body rotation as if the body never rotated — only the bones did." Result: the swing looks the same visually (bones swing), but the character's overall facing stays fixed.

---

## Common questions

**Q: Won't this make the swings look stiff?**

A: No. The arms / spine / shoulders still rotate via bone curves. Only the **invisible root transform rotation** is suppressed. Visually identical for attacks.

**Q: Should I bake rotation on locomotion clips?**

A: Yes — same fix prevents drift over time when running. Mixamo run cycles sometimes have a hidden 1-2 degree pivot.

**Q: What if I want a dash that DOES rotate the character (e.g. spinning attack)?**

A: That's a separate intentional rotation, not Mixamo drift. You'd implement it in PlayerMovement.cs by rotating the transform during the dash duration. Don't unbake rotation in the clip.

---

## Reference

- [ANIMATION_SETUP.md §2](ANIMATION_SETUP.md) — original Bake Into Pose table (this doc supersedes for combat clips)
- [PlayerMovement.cs](Assets/_Project/Player/Scripts/Movement/PlayerMovement.cs) — `faceMovementDirection` field controls whether body rotates with input; should be `false` for shooter-style HUDs
