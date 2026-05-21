# Place Character Model on Player — Unity Setup

How to attach a character mesh (Varion, Aurelia, Kaelen, Lyra, Mira, Wayne) as a child of the Player GameObject so animations can render. Required before wiring Animator + PlayerAnimatorBridge.

**Time:** ~10 minutes per scene.

Repeat in every scene that has a Player (Dungeon, Lobby, anywhere else the character should be visible).

---

## Pre-check — which character to use

Available player models in `Assets/_Project/Player/Models/`:

- Varion
- Aurelia
- Kaelen
- Lyra
- Mira
- Wayne

The character-select screen in MainMenu picks one of these for the run, but for now you just need to drop one into the scene for testing. **Use Varion** as the example here — substitute any other name as needed.

---

## §1 — Verify the model's rig is set to Humanoid

Before placing the model in the scene, confirm Unity treats it as a Humanoid rig (so animations retarget correctly).

1. Project window → `Assets/_Project/Player/Models/Varion.fbx`.
2. Click it.
3. Inspector → **Rig** tab.
4. Confirm:

   | Field             | Value                          |
   | ----------------- | ------------------------------ |
   | Animation Type    | `Humanoid`                     |
   | Avatar Definition | `Create From This Model`       |
   | Skin Weights      | `Standard (4 Bones)` (default) |

5. If anything is wrong, fix it and click **Apply** (bottom of Inspector).

If the Rig tab shows red errors like "Missing required bones," the FBX is broken. Tell me — we'd need to re-export from Mixamo with skin.

---

## §2 — Place the model under the Player

### 2a — Open the scene

Open `3_Dungeon1.unity` (do Lobby and any other scenes after).

### 2b — Find the Player GameObject

1. Hierarchy → look for a root GameObject named `Player` (or use search bar `t:PlayerStats` to find it).
2. Expand it by clicking the ▶ arrow.

### 2c — Remove any existing visual placeholder

If there's a placeholder mesh (cube, capsule, or temp model) as a child of Player:

1. Click it.
2. **Disable** it (uncheck the checkbox at the top-left of Inspector) — safer than deleting in case you need it back.
3. OR **delete** it (right-click → Delete) if you're sure it's a placeholder.

Skip this if there are no existing visual children.

### 2d — Drag the FBX into Player

1. Project window → click `Assets/_Project/Player/Models/Varion.fbx` once to select.
2. **Drag** it from the Project window.
3. **Drop it onto the `Player` GameObject** in the Hierarchy (not into empty space — the drop target matters).
4. Unity creates a `Varion` GameObject as a child of Player, with the rigged mesh + skeleton.

### 2e — Verify hierarchy

```
Player                       (root — PlayerMovement, PlayerStats, PlayerCombat, CharacterController)
├── Varion                   (the model you just added)
│   ├── Hips                 (skeleton bones underneath — auto-generated)
│   │   └── ...
│   └── (meshes)
├── CameraRig                (existing, unchanged)
└── (other existing children)
```

---

## §3 — Fix position, rotation, and scale

When you drop an FBX, Unity uses its native transform. Often this is correct, but sometimes the model is offset or sized wrong.

### 3a — Reset local transform

1. Click `Varion` (the child you just added).
2. Inspector → **Transform**:

   | Field    | Initial Value |
   | -------- | ------------- |
   | Position | `(0, 0, 0)`   |
   | Rotation | `(0, 0, 0)`   |
   | Scale    | `(1, 1, 1)`   |

### 3b — Verify height matches the CharacterController

1. Click `Player` (root).
2. Inspector → **Character Controller** component → note **Height** (typically `2`) and **Center** Y (typically `0` to `1`).
3. Click `Varion` (child) and look at the Scene view.
   - **Looks right (human-sized, standing where capsule was)** → ✅ continue.
   - **Slightly too tall / short** → adjust the Varion CHILD's instance Scale: try `(0.9, 0.9, 0.9)` or `(1.1, 1.1, 1.1)` until height matches the capsule. Do NOT touch FBX Scale Factor.
   - **Way too big (multiple times taller than the map)** → you likely changed Scale Factor in the FBX import. Revert it (see §3c).
   - **Tiny (looks like a stick)** → uncommon for this project's models, but possible. See §3c.

### 3c — DO NOT change FBX Scale Factor unless your model genuinely needs it

The character models in `Assets/_Project/Player/Models/` (Varion, Aurelia, Kaelen, Lyra, Mira, Wayne) are **already at the correct scale** with **Scale Factor = 1** (Unity default). Do NOT raise this value — doing so multiplies the model's size and breaks the scene.

If you did set Scale Factor higher and Varion is now huge:

1. Project window → click `Varion.fbx`.
2. Inspector → **Model** tab → **Scale Factor** → set back to `1`.
3. Click **Apply**.

To resize at the scene level instead, use the **Varion child's** Transform → Scale field in the scene (not the FBX import setting). Example: scale `(0.8, 0.8, 0.8)` shrinks the in-scene Varion to 80% without touching the source FBX.

**Why scale at instance level, not FBX:**

- FBX Scale Factor changes EVERY instance of the model across all scenes — risky.
- Instance Scale on the Varion child only affects this one scene placement — safe, reversible, per-scene-tunable.

### 3d — Foot alignment

If Varion's feet hover above the ground or sink into it:

1. Click `Varion` (child of Player).
2. Inspector → Transform → adjust **Position Y**:
   - Feet hovering → decrease Y (e.g. `-0.5` or `-0.9`).
   - Feet sunk in floor → increase Y (e.g. `0.1`).
3. Watch the Scene view — feet should rest exactly on the level floor at Y=0 of the parent.

---

## §4 — Verify facing direction

Mixamo models typically face Z+ which matches Unity's forward. After everything else is wired (Animator + Bridge from [ANIMATION_SETUP.md §9](ANIMATION_SETUP.md)), test in Play mode:

1. Press Play.
2. Press W (forward).
3. **Expected:** Varion runs forward, away from the camera.
4. **If Varion runs backward:** click Varion, set Rotation Y = `180`. Save.
5. **If Varion runs sideways:** try Rotation Y = `90` or `-90`.

---

## §5 — Apply to other scenes

Do the same for `2_Lobby.unity` so Varion appears in the Lobby too. If Lobby uses a different Player setup or has its own model placement, you may want to use the **same character** for consistency across scenes.

If you want different characters per scene (or a character-select system that swaps at runtime), that's a Phase 2 task — for now, drop the same Varion into each scene.

---

## §6 — Next: wire Animator + Bridge

Once Varion is placed and looks right:

1. Open [ANIMATION_SETUP.md](ANIMATION_SETUP.md) → jump to §9.
2. Add Animator + PlayerAnimatorBridge components to **the Varion child** (NOT the Player root).
3. Wire Controller, Avatar, and Bridge references.
4. Press Play and test all animations.

---

## Common issues

| Symptom                                                   | Fix                                                                                                                                                                               |
| --------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Drop into Hierarchy didn't make Varion a child of Player  | Re-drag. Drop precisely onto the Player GameObject's name, not below it. Click Player first to highlight, then drag the FBX onto the highlighted row.                             |
| Varion appears outside the level                          | Position not at (0,0,0). Reset transform.                                                                                                                                         |
| Two Varions visible                                       | You forgot to disable the placeholder. Click the old visual → uncheck in Inspector.                                                                                               |
| Varion has no skeleton in Hierarchy (just one GameObject) | The FBX wasn't rigged. Check Rig tab is set to Humanoid + Avatar = Create From This Model.                                                                                        |
| Animator added but Varion T-poses on Play                 | Avatar field empty on Animator, OR Apply Root Motion is ON — see ANIMATION_SETUP.md §3 + §9b.                                                                                     |
| Bones look distorted at runtime                           | Mixamo animations don't match the model's bone proportions perfectly. Some clipping is normal; major distortion = re-import the FBX from Mixamo with the **Without Skin** option. |
| Camera goes inside Varion's head                          | CameraRig is at the wrong height. Move CameraRig up (Position Y +1.5 or so).                                                                                                      |

---

## Recap — minimal checklist

- [ ] §1 — Varion.fbx has Rig = Humanoid + Avatar = Create From This Model
- [ ] §2c — old placeholder disabled/deleted
- [ ] §2d — Varion dragged onto Player in Hierarchy
- [ ] §3a — Varion's local Transform reset to (0, 0, 0) / (0, 0, 0) / (1, 1, 1)
- [ ] §3b/c — Varion's height looks right; if size is off, adjust the Varion CHILD's instance Scale (e.g. 0.9 or 1.1) — do NOT change FBX Scale Factor
- [ ] §3d — Varion's feet on the floor (adjust Position Y if needed)
- [ ] Save scene (Ctrl+S)
- [ ] §6 — proceed to ANIMATION_SETUP.md §9 to wire Animator + Bridge

---

## Reference

- [ANIMATION_SETUP.md](ANIMATION_SETUP.md) — main animation wiring guide (§9 covers the Animator/Bridge step after this doc)
- [DungeonBlade GDD v1.md](DungeonBlade%20GDD%20v1.md) §11.2 — Character Select / Profile screen (Phase 1 scope)
