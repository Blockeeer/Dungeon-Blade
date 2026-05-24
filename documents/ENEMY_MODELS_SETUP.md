# Enemy Models Setup

Replace the primitive placeholder visuals on `Skeleton_Soldier`, `Skeleton_Archer`, and `Armored_Knight` prefabs with the rigged Humanoid FBXes already in the project.

**Time:** ~15-20 min for all 3 (≈5 min per enemy).
**Risk:** low — model is a visual child of the prefab, code (`EnemyBase.cs` etc.) is untouched.

---

## §0 — Why these FBXes work

- Already imported with Humanoid rig — animations retarget cleanly
- All 3 enemies + future enemies can share one Animator Controller (same rig)
- Already in your project; no external download needed

---

## §1 — Pre-flight check (read this once)

Before starting, confirm in Unity:

1. Open `3_Dungeon1.unity`
2. Hierarchy → find a placed `Skeleton_Soldier`, `Skeleton_Archer`, or `Armored_Knight` instance (or drag the prefab from `Assets/_Project/Enemies/Prefabs/` into the scene to inspect)
3. Note: enemy GameObject has these components (auto-required by `EnemyBase.cs`):
   - `NavMeshAgent`
   - `CapsuleCollider`
   - The enemy-specific MonoBehaviour (`SkeletonSoldier`, etc.)
4. Note the current visual: probably a primitive (capsule/cube) child or a temp mesh

You'll add the rigged FBX **as a child of this root**, NOT replace the root.

---

## §2 — Your existing enemy FBXes (no Mixamo download needed)

**You already have the enemy models imported.** Skip the Mixamo step. Use the FBXes already in your project:

| Enemy script | FBX file | Location |
|---|---|---|
| `SkeletonSoldier` | `Skeleton Soldier-T-Pose.fbx` | `Assets/_Project/Enemies/Skeleton/Models/` |
| `SkeletonArcher` | `Skeleton-Archer-T-Pose.fbx` | `Assets/_Project/Enemies/Archer/Models/` |
| `ArmoredKnight` | `Armored-Knight-T-Pose.fbx` | `Assets/_Project/Enemies/Knight/Models/` |

These are already:
- ✅ Rig = Humanoid (`animationType: 3`)
- ✅ Avatar Definition = Create From This Model (`avatarSetup: 1`)
- ✅ Textures auto-extracted to `.fbm` cache folders

So §3 (Mixamo download) and §4 (import settings) below are **skippable** unless you want to verify or re-extract materials. Jump to **§5** to attach the FBX to the prefab.

---

## §3 — Mixamo download (skip — only here for reference)

Only do this if you want to *replace* an existing enemy model with a different look. Otherwise skip to §5.

1. Go to https://www.mixamo.com (Adobe account, free)
2. **Characters** tab → pick the skin
3. Click **Download** (top-right)
4. Settings:
   - **Format:** FBX Binary (.fbx)
   - **Pose:** **T-Pose**
   - **Frames per Second:** 30

---

## §4 — Import settings (skip — already done)

Only revisit if your FBX rig isn't recognized as Humanoid. For verification only:

### 4a — Verify Rig

1. Click your enemy FBX (e.g. `Skeleton Soldier-T-Pose.fbx`) → Inspector → **Rig** tab.
2. Confirm:

   | Field | Value |
   |---|---|
   | **Animation Type** | `Humanoid` ✅ |
   | **Avatar Definition** | `Create From This Model` ✅ |
   | **Skin Weights** | `Standard (4 Bones)` |

3. If anything's wrong, change it and Apply. Your project's enemies are already correct.

### 4b — Verify Model

1. Inspector → **Model** tab.
2. Confirm:

   | Field | Value |
   |---|---|
   | **Scale Factor** | `1` (DO NOT change to 100) |
   | **Read/Write Enabled** | ❌ |
   | **Optimize Game Objects** | ❌ |

### 4c — Materials (only if textures look broken)

If the enemy appears purple/missing-texture in the scene:
1. Inspector → **Materials** tab → **Extract Textures...** → `Assets/_Project/Enemies/<type>/Models/Materials/`
2. **Extract Materials...** → same folder
3. Apply

---

## §5 — Attach to prefab

For each enemy:

### 5a — Open the prefab

1. Project window → `Assets/_Project/Enemies/Prefabs/Skeleton_Soldier.prefab` → **double-click** to open in Prefab Edit mode (white background scene).

### 5b — Find/remove the placeholder visual

The placeholder can live in one of two places. Check both:

**Case 1 — Placeholder is a child GameObject** (e.g. `Capsule`, `Cube`, `Visual`):

1. Hierarchy (Prefab Edit) → expand the root.
2. Look for a child named like `Capsule`, `Cube`, `Visual`, or similar primitive.
3. If found: right-click → **Delete**.

**Case 2 — Placeholder is on the root GameObject itself** (MeshFilter + MeshRenderer with a capsule mesh):

1. Click the prefab **root** in Hierarchy.
2. Inspector → find **MeshFilter** component → right-click its header → **Remove Component**.
3. Inspector → find **MeshRenderer** component → right-click its header → **Remove Component**.

**⚠️ DO NOT touch these components on the root:**

| Component | Why keep it |
|---|---|
| **CapsuleCollider** | Required by EnemyBase script — physics hitbox + NavMesh footprint. Removing it breaks damage detection. |
| **NavMeshAgent** | Drives enemy movement; removing it kills the AI. |
| **SkeletonSoldier / SkeletonArcher / ArmoredKnight script** | The enemy's brain — keep it. |
| **Transform** | Can't remove this — it's required. |

**Visual check:** before dragging the FBX, the Scene view should show only the **green capsule wireframe** (that's the CapsuleCollider), NOT a solid white/grey capsule (that was the placeholder mesh, now removed).

### 5c — Drag the FBX into the prefab

1. Project window → click the FBX for the enemy you're editing:
   - Skeleton_Soldier → `Assets/_Project/Enemies/Skeleton/Models/Skeleton Soldier-T-Pose.fbx`
   - Skeleton_Archer → `Assets/_Project/Enemies/Archer/Models/Skeleton-Archer-T-Pose.fbx`
   - Armored_Knight → `Assets/_Project/Enemies/Knight/Models/Armored-Knight-T-Pose.fbx`
2. Drag onto the prefab root in the Hierarchy → drops as a child.
3. Click the new child → Inspector → Transform:
   - Position: `(0, 0, 0)` — should already be at the root's local origin
   - Rotation: `(0, 0, 0)`
   - Scale: `(1, 1, 1)`

### 5d — Verify foot alignment

In the Prefab Edit scene view:
- The enemy mesh should stand with its feet AT the root's origin (Y=0).
- If feet float above Y=0: select the FBX child, set **Position Y = -0.9** (or similar) until feet rest on the visible origin grid.
- If feet sink into the floor: increase Y slightly.

This matters because the CapsuleCollider's center is set assuming feet-at-origin. Mismatch = enemy floats or sinks in the scene.

### 5e — Adjust CapsuleCollider to match new size

1. Click the prefab root → Inspector → CapsuleCollider.
2. Set:

   | Field | Value (typical for humanoid 1.8m tall) |
   |---|---|
   | **Center** | `(0, 0.9, 0)` |
   | **Radius** | `0.4` |
   | **Height** | `1.8` |
   | **Direction** | `Y-Axis` |

3. Adjust until the green gizmo cylinder roughly encloses the new model.

### 5f — Adjust NavMeshAgent radius

1. Inspector → NavMeshAgent.
2. **Radius** = match CapsuleCollider radius (e.g. `0.4`).
3. **Height** = match CapsuleCollider height (e.g. `1.8`).
4. **Base Offset** = if the agent ring renders above/below feet, tweak this until it sits at feet level (usually `0`).

### 5g — Save and exit Prefab Edit

1. Click ← (back arrow) at top-left of Prefab Edit toolbar to exit.
2. Unity saves the prefab automatically.

Repeat §5 for `Skeleton_Archer.prefab` and `Armored_Knight.prefab`.

**Tip for Skeleton_Archer:** since you're reusing the Skeleton skin, drag the same FBX in. To differentiate visually, see §7 (tint or scale).

---

## §6 — Wire Animator (basic)

For now, all 3 enemies can share the existing player Animator Controller (`PlayerAnimator.controller`) as a quick start — it has Idle, Run, Hit, Death which is enough for enemy basic behavior. Later you'll create dedicated enemy controllers.

For each prefab:

1. Open prefab → click the FBX child (NOT the root).
2. Inspector → **Add Component** → search `Animator` → add.
3. Wire:

   | Field | Value |
   |---|---|
   | **Controller** | drag `Assets/_Project/Player/Animations/PlayerAnimator.controller` (TEMP — replace with enemy controller later) |
   | **Avatar** | drag the Avatar sub-asset from the enemy's FBX (expand the FBX in the Project window — the Avatar sub-asset is named `<FbxName>Avatar`, e.g. `Skeleton Soldier-T-PoseAvatar`) |
   | **Apply Root Motion** | ❌ unchecked (NavMeshAgent drives motion) |

4. Save (← exit Prefab Edit).

**Note:** the enemy will likely T-pose or look weird because the player Animator expects player events. This is temporary — proper enemy animations come in a follow-up doc. For now you'll see the model rendered correctly at least, instead of a primitive.

---

## §7 — Differentiate Skeleton_Soldier vs Skeleton_Archer

Your project already has two separate FBXes (`Skeleton Soldier-T-Pose` and `Skeleton-Archer-T-Pose`), so they may already look different. If they happen to use the same mesh, here are options to make them visually distinct:

### Option A — Material tint

1. Open `Skeleton_Archer.prefab`.
2. Click the FBX child → expand to find a Renderer.
3. Inspector → Materials → instance the material (right-click Material slot → Edit asset) → change Base Color to **dark green** or **black**.
4. Soldier stays at default skin color; Archer wears darker.

### Option B — Scale

1. Make the Archer slightly smaller (e.g. Scale = 0.92) so it reads as "leaner" archer vs bulky soldier.

### Option C — Weapon prop (best)

1. Find a free bow model on Sketchfab or Asset Store.
2. Drop the bow GameObject as a child of `mixamorig:RightHand` bone of the Archer prefab's skeleton.
3. Rotate/position until it sits in hand.
4. Soldier gets a sword prop in the same way (find a longsword model).

Recommend Option A or B for fastest result; Option C is the visual win but more work.

---

## §8 — Verify in the dungeon scene

1. Open `3_Dungeon1.unity`.
2. Hierarchy → find existing enemy spawn references OR drag a `Skeleton_Soldier` prefab into the scene at ground level.
3. Save scene.
4. Press Play.
5. **Expected:**
   - Enemy now appears as a humanoid figure (not a capsule)
   - It still patrols / chases / attacks per existing AI
   - When hit, the renderer flashes red (the existing `EnemyBase.OnHit` flash code finds the new child Renderers automatically — no code change needed)
   - On death, it fades out 2s later per existing fade code

### If something looks wrong

| Symptom | Fix |
|---|---|
| Enemy floats above ground | §5d — adjust FBX child Position Y down |
| Enemy sunk into ground | §5d — adjust FBX child Position Y up |
| CapsuleCollider doesn't match model | §5e — re-tune collider Center/Height |
| Enemy doesn't take damage | Renderer hierarchy issue OR collider missing — verify CapsuleCollider is still on root |
| Enemy T-poses instead of animating | Expected for now (using player animator) — proper enemy animator is next step |
| NavMeshAgent warning "off mesh" | §5f — Base Offset wrong, OR you placed the enemy off the baked NavMesh — rebake NavMesh |

---

## §9 — Rebake NavMesh

If you changed CapsuleCollider/NavMeshAgent dimensions a lot, the existing NavMesh data may no longer fit the new enemy size.

1. Open `3_Dungeon1.unity`.
2. Window → AI → Navigation.
3. **Bake** tab.
4. Confirm Agent settings: Radius `0.4`, Height `1.8` (or whatever you set in §5e/f).
5. Click **Bake** → wait ~30s.

If the existing baked NavMesh has different agent radius, the enemy will refuse to spawn on it.

---

## §10 — Going forward

After this session:

- ✅ Skeleton_Soldier looks like a skeleton warrior
- ✅ Skeleton_Archer looks like a (darker / smaller) skeleton archer
- ✅ Armored_Knight looks like a plate-armored knight
- ⏳ All 3 use temp player animator — proper enemy animations come next

**Next M9 follow-up:** create `EnemyAnimator.controller` with these states:
- Idle (looped)
- Run (looped, blended with Idle by Agent.velocity.magnitude)
- Attack_Sword (one-shot, triggered by `Attack` parameter)
- Hit (one-shot, triggered by `Hit` parameter)
- Death (one-shot, triggered by `Die` parameter)

5 states, ~30 min to wire. I can write that doc when you finish §1-§9.

---

## §11 — Boss model (Undead Warlord) — separate task

Don't tackle the boss in this session. Boss model wants:
- Bigger scale (~1.3x)
- Phase-change material swap (Phase 1 normal → Phase 2 cracks → Phase 3 enrage glow)
- A crown/cape prop for silhouette
- Dedicated boss animator with attack variety

That's a separate ~1hr session. Mention "ready for boss model" when these 3 enemies look right and we'll do it.

---

## Recap checklist

- [x] §2 — All 3 enemy FBXes already in project (Skeleton Soldier, Skeleton Archer, Armored Knight)
- [x] §4 — FBXes already Humanoid-rigged with Create From This Model
- [ ] §5 — Skeleton_Soldier.prefab visual replaced
- [ ] §5 — Skeleton_Archer.prefab visual replaced
- [ ] §5 — Armored_Knight.prefab visual replaced
- [ ] §6 — Animator added to each (using temp PlayerAnimator)
- [ ] §7 — Archer differentiated from Soldier (tint/scale/prop)
- [ ] §8 — Tested in `3_Dungeon1.unity` — all 3 render, take damage, die
- [ ] §9 — NavMesh rebaked if collider sizes changed

When done, tell me "enemy models done" and I'll write the enemy animator setup as the next step.

---

## Reference

- [Mixamo](https://www.mixamo.com) — animation source (used for future Mixamo animation clips retargeting onto these enemy rigs)
- [ANIMATION_SETUP.md](ANIMATION_SETUP.md) — animation principles (apply same Bake Pose policy when wiring enemy animations later)
- [PLACE_CHARACTER_MODEL.md](PLACE_CHARACTER_MODEL.md) — same pattern, applied to the player
- [DungeonBlade GDD v1.md](DungeonBlade%20GDD%20v1.md) §3.1 — enemy roster
