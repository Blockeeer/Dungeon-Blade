# Rigify Pipeline — Varion Animation in Blender

End-to-end workflow for using Blender's Rigify addon to rig + animate Varion, then export to Unity.

**Time investment:** **3-6 weeks** for a Wave 1 animation set if learning Rigify + Blender animation from scratch.
**Skill required:** Beginner-to-intermediate Blender — rigging, weight painting, keyframe animation, action editor.
**Output:** Custom-authored animations fully under your control.

**Honest disclaimer:** This is a long path. Mixamo gives 80% of the result in ~90 minutes. Rigify gives you the remaining 20% of polish + full control over Phase 2-3 weeks. Choose this path only if you specifically want to learn character animation as a craft.

---

## §0 — Vocabulary

Confusing Blender terms cleared up first:

| Term | Meaning | In your scene |
|---|---|---|
| **Mesh** | The visible 3D body (skin, clothes, hair) | The grey Varion model |
| **Armature** | The Blender object containing bones | `Armature`, `rig`, or `rig.001` in the Outliner |
| **Bone** | Individual skeleton segment that deforms mesh | Lines/shapes inside the armature |
| **Metarig** | Rigify's INPUT — basic bone skeleton you place on the mesh | The simple armature you placed first |
| **Generated rig** | Rigify's OUTPUT — the full control rig with IK/FK/widgets | The orange-shape rig (`rig`, `rig.001`) |
| **Object Mode** | Manipulate whole objects (move/scale/rotate) | Top-left dropdown — default mode |
| **Edit Mode** | Edit individual bones / mesh vertices | Switch from dropdown when editing rig structure |
| **Pose Mode** | Pose the rig for animation | Switch when ready to animate |
| **Parent** | Link mesh to rig so the mesh follows bones | Ctrl+P after selecting both |
| **Weight paint** | How much each bone influences each vertex | Hidden by Auto Weights; visible in Weight Paint mode |
| **Action** | A single animation clip (Idle, Run, Attack) | Each clip is one Action |

---

## §1 — Pre-flight check

Before continuing, verify you have all the pieces:

- ✅ Varion mesh visible in viewport
- ✅ Metarig placed and matching Varion's body proportions
- ✅ Generated rig (`rig` or `rig.001`) exists in Outliner
- ✅ Mesh parented to generated rig with Automatic Weights

**You can test parenting now.** See §2 immediately to verify before going further.

---

## §2 — Test that the mesh follows the rig

**This is the most important debug step.** If parenting is broken, every later step fails.

### Step 2.1 — Switch to Pose Mode

1. In the **Outliner** (top-right panel), click the **generated rig** (named `rig` or `rig.001`).
2. The top-left mode dropdown says **Object Mode** by default. Click it → choose **Pose Mode**.
3. The orange control shapes around the body should now be selectable.

### Step 2.2 — Try to pose a bone

1. In the viewport, **left-click any orange control shape** (e.g. a hand control, or the spine).
2. The shape turns bright cyan = selected.
3. Press **R** on the keyboard, then move the mouse.
4. **Watch the mesh:**
   - ✅ **Mesh deforms with the bone** = parenting works, you're ready for §3
   - ❌ **Bone moves but mesh stays still** = parenting failed, redo §2.3
5. Press **Ctrl+Z** to undo your test pose.

### Step 2.3 — If mesh doesn't follow, redo parenting

The "With Automatic Weights" parent likely targeted the wrong rig (metarig instead of generated rig). Redo:

1. Switch back to **Object Mode** (top-left dropdown).
2. **First, click the MESH** (the grey body of Varion, or its entry in the Outliner like `tripo_node_4a6343c1`).
3. **Hold Shift, then click the GENERATED RIG** (`rig.001`). Order matters — last-selected = the "active" parent.
4. The active rig has a brighter orange outline than the mesh.
5. Press **Ctrl+P**.
6. In the popup menu, choose **Armature Deform → With Automatic Weights**.
7. Wait 5-10 seconds (Blender calculates weight painting).
8. Re-test §2.2 — mesh should now follow.

### Common parenting issues

| Symptom | Cause | Fix |
|---|---|---|
| Mesh doesn't follow at all | Parented to wrong armature OR weights failed | §2.3 redo |
| Mesh deforms but limb twists weirdly | Auto-weights got confused near joints | Switch to **Weight Paint** mode, manually paint that bone's weight |
| Only part of mesh follows (e.g. legs work, arms don't) | Some vertices have zero weight on those bones | Same fix — weight paint |
| Mesh stretches/explodes when rotating bone | Bone scale isn't 1.0, or extra modifiers on mesh | Object Mode → mesh → Properties → Modifiers → check for stray modifiers |
| **You're in Object Mode and rotating the rig moves the mesh** | That's just Object Mode — Object rotation moves children. Test in Pose Mode instead | §2.1 again |

---

## §3 — Learn Rigify controls (1-2 evenings)

You can't animate without understanding what each control does. Don't skip this.

### Essentials to learn

| Control | Purpose | How to find |
|---|---|---|
| **Root bone** | Moves the whole character in world space | Yellow ring on the ground at Varion's feet |
| **IK foot controls** | Pull feet to position; legs auto-bend | Boxes around each foot |
| **IK hand controls** | Pull hands to position; arms auto-bend | Boxes around each hand |
| **FK arm/leg chains** | Rotate each bone individually | Circle controls along arms/legs (alternative to IK) |
| **IK/FK switch sliders** | Swap arm/leg between IK and FK modes per limb | N-panel (press N in viewport) → Item tab |
| **Spine controls** | Bend torso (FK) or torso target (IK) | Boxes/circles along the spine |
| **Head control** | Aim head independent of body | Ring above head |
| **Master controls** | Bulk operations | Larger boxes around the rig |

### Recommended learning resources

1. **CGDive YouTube channel** — "Rigify in 60 seconds" + "Rigify Tutorial Beginner" videos (~2 hours of high-quality, focused content)
2. **Blender Studio** — official Rigify documentation: https://docs.blender.org/manual/en/latest/addons/rigging/rigify/index.html
3. **YouTube: "P2design Animation tutorial"** — workflow for game character animation in Blender

**Watch ~2 hours of tutorial before attempting Wave 1 clips.** Authoring without understanding the controls = frustration spiral.

---

## §4 — Author your first test clip (~1-2 days)

Don't start with Wave 1's 14 clips. Start with **ONE short idle loop** to learn the workflow end-to-end.

### Step 4.1 — Set up the Animation workspace

1. Top of Blender, click the **Animation** workspace tab (next to Layout, Modeling, etc.).
2. The layout changes — timeline at bottom, dope sheet, big viewport.
3. At the bottom, the editor that looks like a horizontal grid is the **Dope Sheet**. Click its mode dropdown (top-left of that panel) → change to **Action Editor**.

### Step 4.2 — Create a new Action

1. In the Action Editor, click **New** button at the top → an action called `Action` is created.
2. Click the action name → rename to `Varion_Idle`.

### Step 4.3 — Set the timeline range

1. Bottom of viewport, find the timeline.
2. **Start frame:** `1`
3. **End frame:** `60` (2 seconds at 30 fps — short idle loop)

### Step 4.4 — Enable Auto-Keying

1. Bottom of timeline, find the small **record button** (red circle when active).
2. Click it ON. Now any bone you pose will auto-create a keyframe at the current frame.

### Step 4.5 — Pose at key frames

1. Make sure you're in **Pose Mode** on the rig.
2. **Frame 1:** Don't change anything (this is your neutral T-pose).
3. **Frame 30:** Slightly raise the chest control (R then drag up small amount), maybe raise hands a millimeter. This is the "breath in" pose.
4. **Frame 60:** Return to neutral by clicking the Frame 1 keyframe in the dope sheet → press Shift+D to duplicate → set to frame 60.

### Step 4.6 — Make it loopable

1. The pose at frame 60 must EXACTLY match frame 1.
2. Verify by clicking frame 1 in dope sheet, noting all bone positions; click frame 60 — should be identical.
3. If not, manually copy frame 1 keyframes to frame 60.

### Step 4.7 — Preview

1. Press **Space** to play the timeline.
2. Varion should breathe gently in a loop.
3. If anything jerks or pops → frames don't loop cleanly. Fix by aligning frame 60 to frame 1.

---

## §5 — Configure rig for game export (CRITICAL — most tutorials skip this)

Rigify generates **hundreds of bones** (control bones, IK bones, helper bones). Unity needs ONLY the **deform bones** (the ones prefixed `DEF-`). Without filtering, your FBX export bloats and Unity Humanoid mapping fails.

### Step 5.1 — Install Game Rig Tools addon (recommended)

1. Open browser → https://blendermarket.com/products/game-rig-tools (free version exists)
   - OR install via Blender Edit → Preferences → Add-ons → search "Game Rig"
2. Enable the addon.

### Step 5.2 — Generate a Game Rig from your Rigify rig

1. Select your generated rig (`rig.001`).
2. Side panel (N to open) → look for **Game Rig Tools** tab.
3. Click **Generate Game Rig** (or similar).
4. A clean, Unity-friendly armature is created (e.g. `GameRig`).
5. **Re-parent the mesh** to this new GameRig (repeat §2.3 with GameRig as the target).
6. Test in Pose Mode — mesh should still follow.

### Step 5.3 — Bake actions onto the GameRig

Animations you authored on the Rigify control rig don't automatically transfer to the GameRig. Bake them:

1. In the Action Editor, with GameRig selected and Pose Mode active.
2. Object → Animation → **Bake Action**.
3. Settings:
   - Start Frame: 1
   - End Frame: (your end frame, e.g. 60)
   - Visual Keying ✅
   - Clear Constraints ✅
   - Bake Data: Pose
4. Click OK.
5. The Game Rig now has the animation baked into its bones.

---

## §6 — Export to Unity

### Step 6.1 — Select objects to export

1. Object Mode.
2. Select the **mesh** + the **GameRig** (Shift-click to multi-select).
3. Do NOT select the metarig, the Rigify control rig, or other helper objects.

### Step 6.2 — File → Export → FBX

Settings (most important shown):

- **Include → Limit to → Selected Objects:** ✅
- **Include → Object Types:** Mesh + Armature only
- **Transform → Scale:** 1.0, **Apply Scalings:** All Local
- **Forward:** -Z Forward, **Up:** Y Up
- **Geometry → Apply Modifiers:** ✅
- **Armature → Add Leaf Bones:** ❌
- **Armature → Primary Bone Axis:** Y, **Secondary:** X
- **Bake Animation:** ✅ — set Start and End frames matching your action

Save as `Varion-Rigged.fbx`.

### Step 6.3 — Unity import

1. Drag the FBX into `Assets/_Project/Player/Models/`.
2. Click it → Inspector → **Rig** tab.
3. **Animation Type:** Humanoid
4. **Avatar Definition:** Create From This Model
5. Click Apply.
6. Click **Configure** button next to Avatar.
7. Manually map each Unity Humanoid bone slot (Hips, Spine, Chest, etc.) to your GameRig bones. This takes ~15 min for first character. Save when done.

### Step 6.4 — Verify in scene

1. Open `3_Dungeon1.unity`.
2. Drag Varion-Rigged.fbx into the Player GameObject.
3. Set up Animator + Controller per [ANIMATION_SETUP.md](ANIMATION_SETUP.md) §9.

---

## §7 — Authoring the Wave 1 14-clip set

Once the test idle works end-to-end (Blender → Unity rendering correctly), commit to Wave 1.

For each clip in [ANIMATION_SHOPPING_LIST.md](ANIMATION_SHOPPING_LIST.md) §16 (Wave 1):

1. Create new Action in Blender (Varion_Run, Varion_Attack, etc.)
2. Author the animation
3. Export FBX with that single action baked
4. Repeat §6.1-§6.4 in Unity
5. Drop into PlayerAnimator controller's appropriate state

**Estimated effort per clip:**

| Clip type | Time (beginner) | Time (intermediate) |
|---|---|---|
| Idle loop | 4-6 hours | 1-2 hours |
| Walk/Run cycle | 8-12 hours | 3-4 hours |
| Single attack swing | 6-10 hours | 2-3 hours |
| Roll/dodge | 6-8 hours | 2 hours |
| Hit reaction | 3-5 hours | 1 hour |
| Death | 6-8 hours | 2 hours |

**Wave 1 total (beginner):** ~80-130 hours of focused work = 3-6 weeks of evenings.

---

## §8 — Bail-out plan

If after authoring your first test clip you feel "this is too slow," you have a clean escape:

1. Keep the Rigify rig in Blender for future Phase 2 polish work
2. For Phase 1, **upload Varion-Mesh to Mixamo Auto-Rigger** (per [VARION_RIG_ANIMATE.md](VARION_RIG_ANIMATE.md))
3. Use Mixamo animations for Phase 1 ship
4. Replace with custom Blender animations during Phase 2 over time

This way no work is wasted — you learned Rigify, the game ships, and you can swap clips later.

---

## §9 — Honest summary

| Aspect | Reality |
|---|---|
| **Quality ceiling** | Higher than Mixamo (full custom control) |
| **Time to Wave 1** | 3-6 weeks beginner |
| **Skill investment** | Significant — pose animation, weight painting, IK/FK, Rigify quirks |
| **Risk of restart** | High — many things can fail (parenting, weights, retargeting in Unity) |
| **Phase 1 ship impact** | Pushes ship date back substantially |
| **Long-term value** | Real animator skill you keep forever |

If you ship Phase 1 with Mixamo first, you can come back to Rigify-authoring for Phase 2 with zero pressure. That's the path I'd take.

But if learning animation is the actual goal (not just shipping the game), Rigify is the right tool.

---

## §10 — What to do RIGHT NOW

1. **Test parenting** (§2) — does mesh follow when you pose a bone?
   - If ✅: proceed to §3 (learn controls via tutorials)
   - If ❌: redo parenting (§2.3)
2. **Watch ~2 hours of Rigify tutorial** before opening the Action Editor
3. **Author one test idle loop** (§4) end-to-end before committing to Wave 1
4. **Decide after the test:** continue Rigify path, or fall back to Mixamo (§8)

Don't author Wave 1 clips before completing steps 1-3. Test the pipeline on one clip first.

---

## Reference

- [VARION_RIG_ANIMATE.md](VARION_RIG_ANIMATE.md) — Mixamo Auto-Rigger fallback if Rigify path becomes too slow
- [ANIMATION_SHOPPING_LIST.md](ANIMATION_SHOPPING_LIST.md) — full Wave 1 clip list
- [ANIMATION_SETUP.md](ANIMATION_SETUP.md) — Unity-side Animator wiring
- [Rigify documentation](https://docs.blender.org/manual/en/latest/addons/rigging/rigify/index.html) — official docs
- [Game Rig Tools addon](https://blendermarket.com/products/game-rig-tools) — Rigify → game-export bridge
- [CGDive YouTube](https://www.youtube.com/@CGDive) — best free Rigify tutorials
