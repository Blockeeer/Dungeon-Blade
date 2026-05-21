# Enemy Models Setup — Mixamo Pipeline

Replace the primitive placeholder visuals on `Skeleton_Soldier`, `Skeleton_Archer`, and `Armored_Knight` prefabs with rigged Mixamo Humanoid models.

**Time:** ~30-40 min for all 3 (10 min per enemy + Mixamo browsing).
**Risk:** low — model is a visual child of the prefab, code (`EnemyBase.cs` etc.) is untouched.

---

## §0 — Why Mixamo for enemies

- Same Humanoid rig as the player → animations interchangeable
- All 3 enemies + future enemies can share one Animator Controller if you reuse the rig
- Free with Adobe account; instant download as FBX

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

## §2 — Recommended Mixamo skins per enemy

Mixamo lets you pick a "Character" (skin) before downloading. These skins are what your enemy will look like:

| Enemy script | Mixamo skin | Why |
|---|---|---|
| `SkeletonSoldier` | **Mremireh O'Desbiens** | Looks like a tribal/skeleton warrior with bone-pale skin tone; instantly reads as undead at distance |
| `SkeletonArcher` | **Mremireh O'Desbiens** (same as Soldier) | Reuse skin → free visual consistency; differentiate via bow weapon prop |
| `ArmoredKnight` | **Paladin Nevin** or **Knight** | Full plate armor; reads as armored melee enemy |

**Alternative skeleton skins** (if you want a more cartoony skeleton look):
- "Big Vegas" → bandaged mummy vibe
- "Y Bot" → robotic, futuristic — not lore-appropriate for Dungeon Blade
- "Skeletonzombie T Avelange" → if Mixamo's catalog has it, this is the most "skeleton" looking option

---

## §3 — Download from Mixamo

For each unique skin (so once for skeleton, once for knight):

1. Go to https://www.mixamo.com (Adobe account, free)
2. **Characters** tab → pick the skin (e.g. Mremireh O'Desbiens)
3. Click **Download** (top-right)
4. Settings:
   - **Format:** FBX Binary (.fbx)
   - **Pose:** **T-Pose** (CRITICAL — gives a clean rig with no clip baked in)
   - **Frames per Second:** 30
5. Click Download → saves a file like `Mremireh_O_Desbiens.fbx`

Repeat for each skin.

---

## §4 — Import into Unity

For each downloaded FBX:

### 4a — Place the file

| Enemy | Target folder |
|---|---|
| Skeleton Soldier + Archer | `Assets/_Project/Enemies/Skeleton/Models/` (create if missing) |
| Armored Knight | `Assets/_Project/Enemies/Knight/Models/` (create if missing) |

Drag the FBX from your downloads folder into the Project window at the right path.

### 4b — Configure Rig

1. Click the FBX → Inspector → **Rig** tab.
2. Set:

   | Field | Value |
   |---|---|
   | **Animation Type** | `Humanoid` |
   | **Avatar Definition** | `Create From This Model` |
   | **Skin Weights** | `Standard (4 Bones)` |

3. Click **Apply** (bottom of Inspector).
4. After Apply finishes, a sub-asset Avatar appears under the FBX in the Project window (e.g. `MremirenhAvatar`).
5. If Rig tab shows red error "missing required bones," the FBX is broken — re-download from Mixamo.

### 4c — Configure Model tab

1. Inspector → **Model** tab.
2. Settings:

   | Field | Value |
   |---|---|
   | **Scale Factor** | `1` (DO NOT change to 100 — Mixamo characters are pre-scaled for Unity at 1) |
   | **Mesh Compression** | `Off` |
   | **Read/Write Enabled** | ❌ unchecked |
   | **Optimize Game Objects** | ❌ unchecked (need bone GameObjects for IK / weapon parenting later) |

3. Apply.

### 4d — Configure Materials tab

1. Inspector → **Materials** tab.
2. Settings:

   | Field | Value |
   |---|---|
   | **Material Creation Mode** | `Standard (Legacy)` or `Import via MaterialDescription` (whichever extracts materials cleanly) |
   | **Location** | `Use External Materials (Legacy)` |
   | **Naming** | `By Base Texture Name` |
   | **Search** | `Recursive-Up` |

3. Click **Extract Materials...** → choose `Assets/_Project/Enemies/<type>/Models/Materials/` (create subfolder).
4. Click **Extract Textures...** → same Materials folder.
5. Apply.

This extracts the embedded textures so you can swap them later (e.g. tint the Skeleton white/grey, give the Knight a darker armor).

---

## §5 — Attach to prefab

For each enemy:

### 5a — Open the prefab

1. Project window → `Assets/_Project/Enemies/Prefabs/Skeleton_Soldier.prefab` → **double-click** to open in Prefab Edit mode (white background scene).

### 5b — Find/remove the placeholder visual

1. Hierarchy (Prefab Edit) → expand the root.
2. Look for a child named like `Capsule`, `Cube`, `Visual`, or similar primitive.
3. If found: right-click → **Delete** (placeholder is no longer needed).
4. If no visible primitive — the root GameObject's MeshFilter+MeshRenderer might BE the placeholder; in that case, click the root, Inspector → right-click on MeshFilter → Remove Component, same for MeshRenderer.

### 5c — Drag the FBX into the prefab

1. Project window → click the imported FBX (e.g. `Mremireh_O_Desbiens.fbx`).
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
   | **Avatar** | drag the Avatar sub-asset from the enemy's FBX (e.g. `MremiremnAvatar`) |
   | **Apply Root Motion** | ❌ unchecked (NavMeshAgent drives motion) |

4. Save (← exit Prefab Edit).

**Note:** the enemy will likely T-pose or look weird because the player Animator expects player events. This is temporary — proper enemy animations come in a follow-up doc. For now you'll see the model rendered correctly at least, instead of a primitive.

---

## §7 — Differentiate Skeleton_Soldier vs Skeleton_Archer

Both use the same Mremireh skin. To make them visually distinct:

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

- [ ] §3 — Mremireh O'Desbiens FBX downloaded
- [ ] §3 — Paladin Nevin (or knight skin) FBX downloaded
- [ ] §4 — Both FBXes imported, Rig=Humanoid, Materials extracted
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

- [Mixamo](https://www.mixamo.com) — character + animation source
- [ANIMATION_SETUP.md](ANIMATION_SETUP.md) — animation principles (apply same Bake Pose policy when wiring enemy animations later)
- [PLACE_CHARACTER_MODEL.md](PLACE_CHARACTER_MODEL.md) — same pattern, applied to the player
- [DungeonBlade GDD v1.md](DungeonBlade%20GDD%20v1.md) §3.1 — enemy roster
