# Varion — Mixamo Auto-Rig + Animation Pipeline

Replace the current animation set with Varion-fitted Mixamo clips. Same pipeline as enemy models, applied to the player character.

**Total time:** ~90 min — Auto-rig (5 min) + Wave 1 animations (60 min) + Unity wiring (25 min).
**Risk:** low — additive workflow; current setup stays as a fallback.

---

## §0 — Why this works

- Mixamo Auto-Rigger reads your Varion mesh and adds a Humanoid skeleton in 5 minutes
- Once rigged, **every** animation in the Mixamo library retargets to Varion (~3000 free clips)
- Same rig means clips made for enemies retarget to Varion and vice versa
- The pipeline you used for enemy models works identically here

**What you DON'T do:**
- Rig from scratch in Blender (multi-day learning curve)
- Hand-animate clips (multi-week per character)
- Custom skeletons (incompatible with Mixamo + Unity Humanoid retargeting)

---

## §1 — Prep Varion's mesh for Mixamo

Mixamo Auto-Rigger needs a clean T-Pose mesh without an existing skeleton. Currently your Varion FBX is *already rigged* — so we need to extract just the mesh.

### Option A — Use the existing T-Pose FBX directly

Varion is at `Assets/_Project/Player/Models/Varion-T-Pose.fbx`. If this is a static T-Pose with mesh+skeleton, Mixamo will reject it (it wants mesh only). Try uploading first; if rejected, use Option B.

### Option B — Strip skeleton in Blender (15 min)

#### B1 — Open Blender with a clean scene

1. Launch Blender.
2. If a default cube/light/camera is visible: hover over the 3D viewport → press **A** (selects all) → press **X** → click **Delete** to confirm. The viewport should now be empty.

If you're working in your existing rigged .blend file (the one shown in your screenshot), skip B2 and jump to B3 — but make a backup first:
- File → Save As → name it `Varion-Backup.blend` (so your rigging work is preserved)
- Then File → Save As again → name it `Varion-Strip-Working.blend` (this is what you'll modify)

#### B2 — Import Varion FBX (only if starting fresh)

1. **File → Import → FBX (.fbx)**
2. A file browser opens. Navigate to your project: `d:/ACER/Dungeon Blade Game/Assets/_Project/Player/Models/`
3. Click `Varion-T-Pose.fbx` to select it.
4. **Before clicking Import**, look at the right-side panel of the file browser for FBX import settings (scroll to see them all):
   - **Include → Custom Properties:** ✅ (keeps Unity bone names if any)
   - **Transform → Manual Orientation:** ❌ (use defaults)
   - **Armature → Ignore Leaf Bones:** ❌
5. Click **Import FBX** (bottom-right corner of the file browser).
6. Wait ~5 seconds. Varion's mesh + armature appears in the viewport.

#### B3 — Identify and delete ALL armatures

This is the critical step. Your screenshot shows TWO armatures (`Armature` + `Armature.004`) — both must go.

1. Make sure you're in **Object Mode** (top-left of viewport, dropdown). If it says "Edit Mode" or "Pose Mode", click it and change to "Object Mode".
2. In the **Outliner** (top-right panel — the tree view of scene objects):
   - Look for items with the orange stick-figure icon (🦴). Those are armatures.
   - You should see at least one named `Armature` or `Armature.001` etc.
3. **Select the first armature:**
   - Left-click on it in the Outliner. The name turns highlighted.
   - The viewport shows the bones turn from blue to bright orange (selected).
4. **Hold Shift and click any additional armatures** in the Outliner to add them to selection. In your case: click `Armature.004` while holding Shift. Both armatures are now selected.
5. **Hover the mouse cursor over the 3D viewport** (NOT over the Outliner — Blender's hotkeys are context-sensitive).
6. Press **X** on the keyboard.
7. A popup appears: **Delete?** → click **Delete** (or press Enter).
8. Both armatures vanish from the Outliner. The mesh stays.

**Verify:** The Outliner should now show only mesh items (the body, clothes, hair, etc. — usually a tree under a "Collection" node). No more orange stick-figure icons.

#### B4 — Remove the Armature modifier from the mesh

When you delete the armature, the mesh STILL has an "Armature modifier" pointing to it (now invalid). Clean it up:

1. In the Outliner or viewport, **click the mesh** (the body of Varion).
2. In the **Properties panel** (bottom-right area of Blender), look for the **wrench icon** 🔧 (Modifier Properties) — usually in the right-side vertical icon strip.
3. Click the wrench. The Modifiers list opens.
4. Look for a modifier named **"Armature"** in the list.
5. Click the **dropdown arrow** next to its name (or the **X** to delete it directly).
6. If you see a delete button (X), click it. If you see a dropdown menu, choose **Delete**.

If you have multiple mesh objects (body + clothes + hair separate), **repeat for each mesh**. Click each mesh in the Outliner and remove its Armature modifier.

#### B5 — Select only the mesh objects

1. In the viewport, press **A** to select all remaining objects (which should be only meshes now since armatures are gone).
2. Verify in the Outliner: all mesh items should be highlighted.
3. If there are objects you DON'T want exported (cameras, lights, helper empties), they should already be deleted from step B1. If any remain, click them with Shift held to DESELECT, OR delete them: select them → X → Delete.

#### B6 — Export as FBX

1. **File → Export → FBX (.fbx)**
2. A file browser opens. Choose where to save — recommended: your **Desktop** or **Downloads** folder (NOT inside the Unity project yet).
3. **Filename:** `Varion-Mesh.fbx`
4. **Before clicking Export**, look at the right-side panel for export settings. Scroll through these sections:

**Include section:**
- **Limit to → Selected Objects:** ✅ CHECKED (this exports only what you selected in B5)
- **Object Types:** click and ensure ONLY **Mesh** is highlighted (deselect Camera, Lamp, Armature, Other, etc. if highlighted)
- **Include → Custom Properties:** ✅

**Transform section:**
- **Scale:** `1.0`
- **Apply Scalings:** `All Local`
- **Forward:** `-Z Forward`
- **Up:** `Y Up`
- **Apply Unit:** ❌ unchecked
- **Use Space Transform:** ✅

**Geometry section:**
- **Smoothing:** `Face` or `Edge` (either works)
- **Apply Modifiers:** ✅ CHECKED
- **Loose Edges:** ❌
- **Triangulate Faces:** ❌ (Unity does this on import)
- **Tangent Space:** ❌

**Armature section:**
- Should be irrelevant since you removed the armature — but if it's visible: **Add Leaf Bones:** ❌, **Primary Bone Axis:** Y, **Secondary Bone Axis:** X

5. Click **Export FBX** (bottom-right of file browser).
6. Wait ~3-5 seconds. The file is saved.

#### B7 — Verify the export

1. Browse to where you saved `Varion-Mesh.fbx` in Windows Explorer.
2. Right-click the file → **Properties** → check the file size:
   - **Healthy:** 5-50 MB depending on mesh complexity
   - **Too small (< 100 KB):** export failed; meshes weren't selected → redo B5-B6
   - **Too large (> 200 MB):** you included too much (likely armature wasn't deleted) → redo B3-B6

3. Quick visual check (optional): drag the FBX into a **fresh Blender scene** (File → New → General, then File → Import → FBX) — you should see Varion's mesh in T-pose with NO bones / armature. If you see bones, B3 didn't work.

You only need to do this ONCE per character. Stripped FBX is what you'll upload to Mixamo in §2.

#### Troubleshooting B-step failures

| Symptom | Likely cause | Fix |
|---|---|---|
| "Nothing was exported" | Selected Objects ✅ but nothing was actually selected | Re-do B5 (press A in viewport to select all) |
| FBX is tiny (< 100 KB) | Object Types had Mesh deselected | Re-do B6 with Mesh ✅ |
| FBX still contains bones | Armature wasn't deleted | Re-do B3 — press X over viewport, not Outliner |
| Mesh appears at wrong scale in Mixamo | Apply Scalings was "FBX All" or "FBX Custom Scale" | Re-do B6 with Apply Scalings = "All Local" |
| Mesh is rotated 90° in Mixamo | Forward/Up axes wrong | Re-do B6 with -Z Forward + Y Up |
| Mesh has black or pink material | Materials weren't included | This is fine — Mixamo doesn't care about materials; we'll re-apply textures later via the rigged FBX |

#### Visual checklist for the end of §1

Before moving to §2 (Mixamo upload), confirm:

- ✅ `Varion-Mesh.fbx` exists at your saved location
- ✅ File size between 5-50 MB
- ✅ Quick re-import test shows mesh only, no bones

---

## §2 — Upload to Mixamo Auto-Rigger

1. Go to https://www.mixamo.com (Adobe account, free)
2. **Upload Character** button (top-right) → choose `Varion-Mesh.fbx`
3. Wait ~30 seconds for upload
4. Auto-Rigger preview opens. Place the markers on the mesh:
   - **Chin** — center of jaw
   - **Wrists** — exact wrist joint center (both arms)
   - **Elbows** — outside of elbow joint
   - **Knees** — front of knee
   - **Groin** — between legs at hip level
5. Click **Next** → Mixamo analyzes for ~30-60 seconds
6. Preview window appears with Varion auto-rigged. Test by clicking the dance preview at the bottom — should animate smoothly without bone snapping.
7. Click **Next** to finalize the rig

**If the result looks broken** (twisted limbs, mesh distortion):
- Markers were misplaced → click "Edit Skeleton" → reposition markers more accurately → retry
- Most common error: wrists placed too low (should be at the joint, not on the hand)

---

## §3 — Download rigged Varion + T-Pose

After rigging is approved:

1. Mixamo shows the rigged character page with an animation list
2. Click **Download** (top-right)
3. Settings:
   - **Format:** FBX Binary
   - **Pose:** **T-Pose** (this is the rigged reference, not an animation)
   - **Frames per Second:** 30
   - **Skin:** With Skin ✅
4. Save as `Varion-Rigged.fbx`

Place in `Assets/_Project/Player/Models/` (replace the existing `Varion-T-Pose.fbx` or alongside).

---

## §4 — Download Wave 1 animations

Stay logged into Mixamo with Varion as the active character. Every animation you download will be **pre-retargeted** to Varion's bone proportions.

### Wave 1 — 14 clips, ~45 min

For each, search Mixamo, select, download. Settings per clip:
- **Format:** FBX Binary
- **Pose:** **Original Pose** (NOT T-Pose — keep the animation data)
- **Skin:** Without Skin ✅ (you only need the bones moving — mesh comes from Varion-Rigged.fbx)
- **In Place:** ✅ for locomotion clips (centers the rig; PlayerMovement drives world position)
- **Frames per Second:** 30

| # | Clip name | Mixamo search | In Place? | Notes |
|---|---|---|---|---|
| 1 | Idle | "Breathing Idle" | n/a (stationary) | The default state |
| 2 | Run Forward | "Standard Run" or "Run Forward In Place" | ✅ | Forward locomotion |
| 3 | Walk Backward | "Walking Backwards" | ✅ | S key |
| 4 | Strafe Left | "Left Strafe Walking" | ✅ | A key |
| 5 | Strafe Right | "Right Strafe Walking" | ✅ | D key |
| 6 | Jump Start | "Standing Jump" or "Jump Up" | ❌ | Single takeoff |
| 7 | Falling | "Falling Idle" | n/a (looped) | Mid-air pose |
| 8 | Land | "Standing Land" | ❌ | Soft landing |
| 9 | Roll Forward | "Roll Forward" or "Diving Forward Roll" | ❌ | Shift+W roll |
| 10 | Roll Back | "Dodging Back" | ❌ | Shift+S dodge |
| 11 | Hit Front | "Standing React Light From Front" | ❌ | Damage taken |
| 12 | Big Hit | "Standing React Large" | ❌ | Heavy damage |
| 13 | Death | "Falling Back Death" or "Dying" | ❌ | Game over |
| 14 | Attack Sword | "Sword And Shield Slash" | ❌ | Combo step 1 |

**Place all downloaded FBXes in:** `Assets/_Project/Player/Animations/<category>/` (Idle/, Locomotion/, Jump/, Combat/, Reactions/, Death/ per existing structure).

### Wave 2 (optional, ~30 min more)

Combo steps 2/3, equip, reload, block, parry. Get these once Wave 1 works in scene.

---

## §5 — Unity import settings per clip

For each downloaded FBX:

### 5a — Rig tab

| Field | Value |
|---|---|
| **Animation Type** | `Humanoid` |
| **Avatar Definition** | `Copy From Other Avatar` |
| **Source** | drag `Varion-Rigged.fbx`'s sub-asset Avatar (named like `VarionAvatar`) |

This is CRITICAL — using a copied avatar (not "Create From This Model") ensures every animation shares Varion's bone setup. Without this, clips break each other.

### 5b — Animation tab → per clip

Apply [ANIMATION_FIX_ROTATION_DRIFT.md](ANIMATION_FIX_ROTATION_DRIFT.md) Bake Into Pose policy:

| Section | Bake Into Pose | Based Upon | Notes |
|---|---|---|---|
| Root Transform Rotation | ✅ | Original | Stops rotation drift on attack/roll spam |
| Root Transform Position (Y) | ✅ | Original | Stops sinking |
| Root Transform Position (XZ) | ✅ | Center of Mass | All world XZ from CharacterController, not animation |

**Exception:** for `Falling Idle` (clip #7), leave Y unbaked so the body can rise/fall visually in mid-air pose.

### 5c — Bulk apply

Multi-select all 14 FBXes at once → multi-edit Inspector → set Bake Pose ✅ on all 3 axes → one Apply click affects all of them. Saves ~10 min vs one-at-a-time.

---

## §6 — Update Avatar on Varion in scene

In `3_Dungeon1.unity` and `2_Lobby.unity`:

1. Hierarchy → click the Varion model under the Player root
2. Inspector → Animator component → **Avatar** field
3. Drag the new `Varion-Rigged.fbx`'s Avatar sub-asset into this field (replaces the old one)
4. Save scene

Repeat for both scenes.

---

## §7 — Replace clip references in PlayerAnimator.controller

The controller currently points to the OLD animation clips. After downloading new ones, you need to swap each state's motion field.

**Recommended — Unity-side (safer):**

1. Open `Assets/_Project/Player/Animations/PlayerAnimator.controller` in the Animator window
2. Click a state (e.g. `Idle`)
3. Inspector → **Motion** field → click the small circle icon → search for the new clip (e.g. `Breathing Idle` from the new FBX)
4. Select the new clip → motion field updates
5. Repeat for every state

For 14+ states this takes ~15-20 minutes. Tedious but zero risk.

**Alternative — I rewrite the YAML:**

I can do this from the chat, but YAML edits to the controller are higher risk. Recommend you do it in Unity unless you want to risk a controller rebuild.

---

## §8 — Test

1. Save scenes (Ctrl+S)
2. Open `3_Dungeon1.unity` → Press Play
3. Test each state:
   - Stand still → Breathing Idle plays smoothly
   - W → Run Forward
   - A/D → Strafe animations (via 2D Blend Tree)
   - S → Walking Backward
   - Space → Jump → Falling → Land
   - Shift+W → Roll Forward (fast, no wind-up if Bake Pose applied correctly)
   - Double-tap A/D → physics burst (no animation change — per [PlayerAnimatorBridge.cs](Assets/_Project/Player/Scripts/Animation/PlayerAnimatorBridge.cs) the Dash trigger is suppressed)
   - LMB → Sword swing
   - Take damage → Hit React
   - Die → Death animation

### Common issues

| Symptom | Fix |
|---|---|
| Varion T-poses | Avatar mismatch — re-verify §5a (Copy From Other Avatar set to VarionAvatar) |
| Animations play but Varion drifts away from capsule | §5b — Bake Pose XZ not applied; reapply |
| Roll plays but Varion sinks into floor | §5b — Bake Pose Y not applied on Roll clip |
| Strafe looks wrong direction | The MoveX/MoveZ blend tree positions may need re-tuning; not animation issue |
| Idle pose looks wrong (T-pose-like) | Idle FBX was downloaded without "Original Pose" setting — re-download with Original Pose ✅ |

---

## §9 — When you're done with Wave 1

Tell me **"Varion Wave 1 done"** and I'll:

1. Walk through verifying the controller transitions are still correct (transition durations, exit times)
2. Help you re-test the timing fixes from earlier (the dash/roll snappiness improvements should carry over since the animator transition tunings are unchanged)
3. Plan Wave 2 (sword combo 2+3, equip, reload, block, parry) once Wave 1 feels solid

---

## §10 — What this fixes vs. what stays

| Problem | Status |
|---|---|
| Rotation drift on attack spam | ✅ Fixed (Bake Rotation on every clip) |
| Sinking into floor | ✅ Fixed (Bake Y) |
| Hips drift away from capsule | ✅ Fixed (Bake XZ) — HipsLock no longer required |
| Animation wind-up delay | ✅ Fixed (Mixamo clips downloaded fresh; Transition Offset still applied) |
| Camera trails behind during roll | ✅ Fixed in CameraSmoothFollow burst-mode (already in code) |
| Varion looks weird because animations don't fit the rig | ✅ Fixed (animations are now Varion-native via Auto-Rigger) |

**What stays:**
- The Animator controller structure (transitions, parameters, sub-state machines)
- All gameplay code (PlayerMovement, PlayerCombat, etc.)
- The Roll trigger logic, dash physics, camera follow
- Inventory, save system, everything else

---

## §11 — Cost summary

| Phase | Time |
|---|---|
| Strip skeleton in Blender (§1) | 15 min (only if Mixamo rejects the existing FBX) |
| Mixamo Auto-Rig + download rigged (§2-§3) | 10 min |
| Download Wave 1 clips (§4) | 45 min |
| Unity import + Bake Pose (§5) | 15 min |
| Update Avatar in scenes (§6) | 5 min |
| Re-wire controller motion fields (§7) | 20 min |
| Test + iterate (§8) | 30 min |
| **Total** | **~90-150 min for Wave 1** |

Compare to:
- Custom rig + Blender authoring: **2-4 weeks**
- Keep current setup: **0 min but problems persist**

---

## Reference

- [ANIMATION_SHOPPING_LIST.md](ANIMATION_SHOPPING_LIST.md) — full clip catalog (this doc uses Wave 1)
- [ANIMATION_FIX_ROTATION_DRIFT.md](ANIMATION_FIX_ROTATION_DRIFT.md) — Bake Into Pose policy reference
- [ANIMATION_SETUP.md](ANIMATION_SETUP.md) — original animation wiring (still valid)
- [PLACE_CHARACTER_MODEL.md](PLACE_CHARACTER_MODEL.md) — Varion's scene placement (already done)
- [Mixamo Auto-Rigger](https://www.mixamo.com/#/?page=1&type=Character) — upload page
