# Camera Shake — Unity Setup

Wires camera shake into combat and boss events. Two scripts, both already created:

- `CameraShake.cs` — the shake driver (offsets a Transform's local position).
- `CameraShakeBindings.cs` — subscribes to gameplay events and calls Shake().

---

## How it works

When a hit/death/phase transition happens, `CameraShakeBindings` calls `CameraShake.Instance.Shake(strength, duration)`. The shake script offsets its own GameObject's local position by random values for the duration, decaying to zero. The offset is applied in `LateUpdate` so it layers on top of normal camera-follow logic without fighting it.

---

## Where to attach the components

You attach them to **two different GameObjects:**

1. **CameraShake** → on the `cameraRig` GameObject (the one that follows the player).
2. **CameraShakeBindings** → on a separate GameObject (e.g. `[CameraShake]` root manager).

Why split them? The shake script needs to live on the actual camera Transform so its localPosition offset directly affects what the player sees. The bindings script is a pure event subscriber — it can live anywhere.

---

## Step 1 — Add CameraShake to the camera

1. Open `3_Dungeon1.unity`.
2. Find the `Player` GameObject in the Hierarchy. Expand it to find the camera rig (likely named `CameraRig`, `Camera`, or similar — whatever you wired into PlayerMovement's `cameraRig` field).
3. Click that GameObject.
4. Add Component → search **Camera Shake** → click `DungeonBlade.Core.CameraShake`.
5. Inspector shows fields with these defaults:

   | Field | Value |
   |---|---|
   | Decay Rate | `0.85` |
   | Max Strength | `1.0` |
   | Light Shake Strength | `0.08` |
   | Light Shake Duration | `0.10` |
   | Medium Shake Strength | `0.18` |
   | Medium Shake Duration | `0.18` |
   | Heavy Shake Strength | `0.35` |
   | Heavy Shake Duration | `0.30` |

   Defaults work. You can tune them later if shakes feel too soft/hard.

---

## Step 2 — Add CameraShakeBindings as a manager

1. Hierarchy → right-click empty space → Create Empty → name `[CameraShake]`.
2. Add Component → search **Camera Shake Bindings** → click `DungeonBlade.Core.CameraShakeBindings`.
3. Wire fields by dragging from the Hierarchy:

   | Inspector field | Drag from Hierarchy |
   |---|---|
   | Player Stats | `Player` GameObject |
   | Player Movement | `Player` GameObject |
   | Boss | `UndeadWarlord` (or whichever boss exists in the scene) |

4. Tune the per-event values if you want different feel. Defaults:

   | Group | Field | Value |
   |---|---|---|
   | Damage | Damage Strength Base | `0.18` |
   | Damage | Damage Strength Per HP | `0.004` (so a 50-damage hit shakes harder than a 10-damage hit) |
   | Damage | Damage Duration | `0.18` |
   | Death | Death Strength | `0.35` |
   | Death | Death Duration | `0.45` |
   | Burst | Burst Strength | `0.06` (small flick on dash/dodge) |
   | Burst | Burst Duration | `0.08` |
   | Phase | Phase Transition Strength | `0.25` (Phase 2 entry) |
   | Phase | Phase Transition Duration | `0.35` |
   | Phase | Enrage Strength | `0.45` (Phase 3 entry — boss enrage) |
   | Phase | Enrage Duration | `0.6` |

---

## Step 3 — Save scene + test

1. Ctrl+S.
2. Press Play.
3. Test each event:

   | Action | Expected shake |
   |---|---|
   | Take damage from any enemy / trap | Small punch (scales with damage) |
   | Press Shift + W to dash | Tiny flick |
   | Double-tap A or D to dodge | Tiny flick |
   | Boss reaches 66% HP | Medium shake — Phase 2 entry |
   | Boss reaches 33% HP | Big shake — Phase 3 enrage |
   | Take fatal damage (run out of respawns) | Heaviest shake — death |

If any shake doesn't fire, the corresponding event subscription failed. Most common cause: a field on CameraShakeBindings is empty.

---

## Step 4 — Repeat for the Lobby scene (optional)

If you want shake reactions in the Lobby (bumping into traps, etc — although Lobby has no enemies):

1. Open `2_Lobby.unity`.
2. Same Step 1 (CameraShake on the cameraRig).
3. Same Step 2 (CameraShakeBindings on a `[CameraShake]` GameObject), but **leave the Boss field empty** (no boss in lobby).
4. Save.

If you skip the Lobby scene, shake just doesn't apply there. Harmless.

---

## Tuning tips

If shakes feel **too violent**:
- Lower the per-event Strength values (try halving them).
- Increase Decay Rate to `0.92` so they fade faster.

If shakes feel **too weak / unnoticeable**:
- Bump per-event Strength values up by 50%.
- Increase Duration so you see the shake longer.

If shakes feel **jittery / nauseating**:
- Lower Decay Rate to `0.75` for a snappier fade.
- Make Duration shorter — `0.10s` per event instead of `0.18s`.

If shakes **stack weirdly when many events fire at once**:
- Already handled — `Shake()` only takes the strongest current request, doesn't add up. But if it still feels off, lower Max Strength from `1.0` to `0.5` to cap total intensity.

---

## Common issues

- **No shakes at all** → CameraShake script not on the cameraRig GameObject, or `CameraShake.Instance` is null. Check the Hierarchy for the script.
- **Shakes affect more than the camera** → CameraShake is attached to the wrong GameObject (e.g. the Player root, which moves the whole player). Move it to cameraRig only.
- **Camera drifts after a shake** → the shake script's `_baseLocal` got out of sync. Restart Play mode. If persistent, the camera's localPosition is being modified by another script — check for conflicting movement code.
- **Phase 2/3 shakes don't fire** → Boss field on CameraShakeBindings is empty. Drag the boss in.
- **Death shake doesn't fire** → check `PlayerStats.OnDeath` is invoked. It should fire when health reaches 0.
- **Burst shake fires twice** → Dash and Dodge events both fire when Shift+direction is pressed. That's fine — they don't double-trigger because `Shake()` takes max strength rather than adding.

---

## What you can extend later (M9 polish)

- **Directional shake** — instead of pure random offset, push the camera in the direction of the hit. Requires tracking which direction damage came from.
- **Frame freeze** on heavy hits — pause `Time.timeScale = 0` for ~0.05s before resuming. Combos well with shake.
- **Chromatic aberration / vignette flash** — full-screen post effects on hit. Needs URP volume profile.
- **Boss room low-frequency rumble** — sustained low shake during Phase 3 to convey dread.

These are all optional layers on top of the basic shake.
