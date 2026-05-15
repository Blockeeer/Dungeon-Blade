# Audio Setup — Tier 1 (Dungeon Blade)

Wires the 8 essential SFX into the game. Time: ~30 minutes Unity setup + however long you spend hunting clips on Freesound.

**What you'll have when done:**

- Player damaged → grunt
- Player death → death sound
- Dash / dodge → whoosh
- Jump → grunt / breath
- Item pickup → ding
- Sword swing (call from PlayerCombat)
- Sword hit (call from PlayerCombat hit detection)
- UI button click (call from buttons)

**What's NOT in Tier 1:** music, footsteps, enemy sounds, ambient background hums. Those go in Tier 2/3 later.

---

## Step 1 — Get the audio clips

You need 8 clips. All free from these sources:

### Best free CC0 sources

- **Freesound.org** — search the keywords below. Filter by **License: CC0** for safest reuse.
- **OpenGameArt.org** — game-specific, often pre-trimmed.
- **Sonniss GDC Game Audio Bundles** — yearly free pro-quality SFX packs (Google "Sonniss GDC bundle").

### Search terms per clip

| Clip           | Filename suggestion | Search keywords                     |
| -------------- | ------------------- | ----------------------------------- |
| Sword swing    | `SwordSwing.wav`    | "sword swing whoosh"                |
| Sword hit      | `SwordHit.wav`      | "sword hit flesh" or "blade impact" |
| Player damaged | `PlayerHurt.wav`    | "male grunt pain" or "human hurt"   |
| Player death   | `PlayerDeath.wav`   | "male death scream"                 |
| Dash whoosh    | `DashWhoosh.wav`    | "whoosh fast"                       |
| Jump grunt     | `JumpGrunt.wav`     | "male jump short"                   |
| Item pickup    | `ItemPickup.wav`    | "ui pickup ding" or "coin pickup"   |
| UI click       | `UIClick.wav`       | "ui click button"                   |

**Tips when picking:**

- Aim for clips **0.3 - 1.0 seconds long**. Anything longer gets annoying when triggered repeatedly.
- Avoid clips with built-in reverb tails — they overlap weirdly when fired in quick succession.
- Mono (1-channel) is fine and lighter than stereo.
- WAV or OGG format works; Unity converts to its own internal format on import.

### Drop the files into Unity

1. Create folder `Assets/_Project/Audio/SFX/` if it doesn't exist.
2. Drop all 8 audio files into it.
3. Click each in the Project window — Inspector → confirm:
   - **Load Type** = `Decompress on Load` (best for short SFX)
   - **Compression Format** = `PCM` (highest quality for short clips)
   - Click **Apply** if you change anything.

---

## Step 2 — Create an AudioMixer with Music / SFX buses

### 2a — Create the asset

1. In the Project window, navigate to `Assets/_Project/Audio/`.
2. Right-click → **Create → Audio Mixer** → name `MainMixer`.
3. **Double-click** the new `MainMixer.mixer` asset to open the Audio Mixer window.

If the window doesn't open: menu bar → **Window → Audio → Audio Mixer**.

### 2b — Understand the Audio Mixer window layout

The window has 4 main panels, but only the **Groups** panel matters for this step. By default it shows one entry: `Master`.

`Master` is the final output bus — every sound flows through here before reaching the speakers.

### 2c — Add the Music child group

1. In the **Groups** panel (left side of the Mixer window), **right-click** `Master`.
2. A context menu appears.
3. Click **Add child group...**.
4. A new group appears, indented under Master, with a default name like `Group` or `New Group`.
5. The name field is editable — type `Music` and press Enter. (If not editable, click the name once → F2 → rename.)

### 2d — Add the SFX child group

1. **Right-click `Master` again** (not the Music group — you want SFX as a sibling of Music, both directly under Master).
2. Add child group... → name it `SFX`.

### 2e — Verify the hierarchy

The Groups panel should show:

```
Master
├── Music
└── SFX
```

Both groups indented one level under Master. **If SFX ended up indented under Music** (you right-clicked Music instead of Master), drag SFX up to be a sibling, or delete and recreate.

The main editor area now shows 3 vertical fader strips side by side: Master / Music / SFX. Each has a volume slider in dB.

### Why this structure

- All Music plays → routed through `Music` group → through `Master` → out.
- All SFX plays → routed through `SFX` group → through `Master` → out.
- Adjusting Music volume affects only music. Adjusting SFX volume affects only SFX. Adjusting Master affects everything.

This is what enables independent volume sliders in your Settings menu.

### Expose Music + SFX volumes as parameters

This step makes the Music/SFX volume sliders **scriptable** — without this, code can't change them at runtime, and your Settings menu won't be able to control music/SFX independently.

#### What "exposing a parameter" means

Audio Mixer groups have many properties (volume, pitch, send level, effects). Most are editor-only and locked at runtime. **Exposing** a property tells Unity "let scripts read/write this parameter while the game is running" — Unity then assigns it a string name you reference in code (`mixer.SetFloat("MusicVolume", -10f)`).

You'll do this for the Music group's Volume and the SFX group's Volume.

#### 2f — Expose the Music group's volume

1. In the **Audio Mixer window**, click on the `Music` group in the Groups panel (left side).
2. **Look at the Inspector window** (the same Inspector you use for GameObjects — it's now showing the Music group's properties because Music is selected).
3. The Inspector shows several sections at the top:
   - **Attenuation** (with a Volume slider) ← this is the one you want
   - **Pitch**
   - **Send**
   - etc.
4. Find the **Volume** slider inside the Attenuation section. It's labeled `Volume` with a slider showing a value in dB (default `0.00 dB`).
5. **Right-click directly on the word "Volume"** (the label, not the slider track or the dB value).
6. A small context menu appears with options.
7. Click **Expose 'Volume (of Music)' to script**.
8. The label changes — Unity now shows a small **asterisk (\*)** or similar marker next to the Volume label, indicating it's been exposed.

If the right-click menu doesn't show "Expose to script", you might have right-clicked on the slider itself instead of the label. Try again on the text "Volume".

#### 2g — Rename the exposed parameter

The parameter was auto-named something like `MyExposedParam` or `Volume (of Music)`. Rename it for clarity.

1. Look at the **top-right of the Audio Mixer window** — there's a small dropdown labeled **Exposed Parameters** with a number next to it (now showing `1`).
2. **Click** that dropdown.
3. A list opens showing your one exposed parameter (something like `MyExposedParam (Volume of Music)`).
4. **Right-click the parameter name** in the list.
5. A context menu appears with options like Rename, Unexpose, etc.
6. Click **Rename**.
7. Type `MusicVolume` exactly (case-sensitive — must match what SettingsManager expects).
8. Press Enter to confirm.

#### 2h — Repeat for the SFX group

Do the same two-step process for SFX:

1. In the Audio Mixer window, click `SFX` in the Groups panel.
2. Inspector → find Volume slider → **right-click "Volume" label** → **Expose 'Volume (of SFX)' to script**.
3. Top-right of Mixer window → Exposed Parameters dropdown (now shows `2`).
4. Find the new entry → right-click → Rename → type `SfxVolume`.

#### 2i — Verify

Click the **Exposed Parameters** dropdown in the top-right one more time. You should see exactly two entries:

- `MusicVolume` (linked to the Music group's volume)
- `SfxVolume` (linked to the SFX group's volume)

If either is missing or named differently:

- Re-do the expose step for the missing one.
- If the names don't match, click the dropdown → right-click the wrong name → Rename → type the correct name.

The names must match these strings exactly because `SettingsManager.cs` uses them:

```csharp
mixer.SetFloat("MusicVolume", ...);
mixer.SetFloat("SfxVolume", ...);
```

If you typed `MusicVol` instead of `MusicVolume`, the slider in your Settings menu will silently do nothing.

---

## Step 3 — Wire SettingsManager to the Mixer

The script is already updated to route Music/SFX volumes through the mixer. Just wire the field:

1. Open `1_MainMenu.unity`.
2. Hierarchy → click `[Settings]` GameObject.
3. Inspector → find the **Settings Manager** component → in the new "Audio routing" section:

   | Field              | Value                                                 |
   | ------------------ | ----------------------------------------------------- |
   | Mixer              | drag `MainMixer` from Project window                  |
   | Music Volume Param | `MusicVolume` (default — leave unless you renamed it) |
   | Sfx Volume Param   | `SfxVolume` (default)                                 |

4. Save scene.
5. Repeat for `2_Lobby.unity` and `3_Dungeon1.unity` (each scene's `[Settings]` GameObject needs the same wiring).

---

## Step 4 — Add SfxPool to a persistent place

`SfxPool` is `DontDestroyOnLoad`, so it survives scene transitions. Add it once to the first scene that loads.

1. Open `0_LandingScene.unity`.
2. Hierarchy → right-click → Create Empty → name `[SfxPool]`.
3. Add Component → search **Sfx Pool** → click `DungeonBlade.Core.Audio.SfxPool`.
4. Inspector:

   | Field     | Value                                                     |
   | --------- | --------------------------------------------------------- |
   | Pool Size | `12` (default — enough for combat overlap)                |
   | Sfx Group | drag `MainMixer/SFX` from the Mixer window into this slot |

5. Save scene.

---

## Step 5 — (Optional) Add MusicPlayer

Skip if you're only doing Tier 1 SFX (no music yet). Add later when you have music tracks.

1. Same scene (`0_LandingScene.unity`).
2. Create Empty → name `[MusicPlayer]`.
3. Add Component → **Music Player**.
4. Wire `Music Group` → drag `MainMixer/Music`.
5. Save.

---

## Step 6 — Add AudioBindings to the Dungeon scene

This is where the sounds actually fire on events.

1. Open `3_Dungeon1.unity`.
2. Hierarchy → Create Empty → name `[AudioBindings]`.
3. Add Component → **Audio Bindings** (`DungeonBlade.Core.Audio.AudioBindings`).
4. Wire references:

   | Field           | Drag from Hierarchy                               |
   | --------------- | ------------------------------------------------- |
   | Player Stats    | `Player`                                          |
   | Player Movement | `Player`                                          |
   | Player Combat   | `Player` (PlayerCombat is on the same GameObject) |
   | Inventory       | `[Inventory]` GameObject (has InventoryManager)   |

5. Wire SFX clips from the Project window:

   | Field          | Drag from Project |
   | -------------- | ----------------- |
   | Sword Swing    | `SwordSwing.wav`  |
   | Sword Hit      | `SwordHit.wav`    |
   | Player Damaged | `PlayerHurt.wav`  |
   | Player Death   | `PlayerDeath.wav` |
   | Dash Whoosh    | `DashWhoosh.wav`  |
   | Jump Grunt     | `JumpGrunt.wav`   |
   | Item Pickup    | `ItemPickup.wav`  |
   | UI Click       | `UIClick.wav`     |

6. Pitch Variation = `0.08` (default — gives slight randomness so repeated sounds aren't identical).
7. Save scene.

**Repeat for `2_Lobby.unity`** with the same setup if you want pickup/UI sounds in the lobby. Otherwise skip.

---

## Step 7 — Combat sounds (already automatic)

If you wired the **Player Combat** field in Step 6, sword swing and hit sounds fire automatically:

- **Sword swing** → fires when you press Fire (LMB). Subscribed to `PlayerCombat.AttackPerformed`.
- **Sword hit** → fires when the sword's hitbox connects with a damageable. Subscribed to `Sword.OnHit`.
- **Gun hit** → same sound as sword hit (you can split into a separate field later if needed). Subscribed to `Gun.OnHitDamageable`.

The AudioBindings script automatically re-subscribes when you swap weapons (via the `WeaponEquipped` event), so swords/guns/whatever you add later work without extra wiring.

**No additional setup required.** Just confirm:

- Player Combat field is wired in AudioBindings
- Sword Swing AudioClip is assigned
- Sword Hit AudioClip is assigned

Then test — pressing LMB plays the swing, hitting an enemy plays the hit sound.

---

## Step 8 — Save + test

1. Save all open scenes.
2. Press Play.
3. Test each event:

   | Action                 | Expected sound     |
   | ---------------------- | ------------------ |
   | Take damage            | Player hurt grunt  |
   | Press Shift + W        | Dash whoosh        |
   | Press Space            | Jump grunt         |
   | Pick up an item / loot | Item pickup ding   |
   | Die from boss          | Player death sound |

4. Open Settings → drag the SFX slider down to 0 → all SFX should mute. Up to 1 → full volume.
5. Drag Master slider — should affect everything (already worked before this change).

If any sound doesn't play:

- **Check Console for errors** about missing AudioClip references.
- **Verify SfxPool exists** in scene (should persist from LandingScene).
- **Check the AudioMixer's SFX group volume** isn't set to silent in the editor.

---

## Common issues

- **No sound at all** → SfxPool wasn't created (e.g. you started Play in Dungeon directly, skipping LandingScene). Either start from LandingScene, or add a `[SfxPool]` GameObject to the Dungeon scene as a fallback.
- **Sound plays but Master/SFX sliders don't work** → SettingsManager's Mixer field isn't wired. Re-check Step 3.
- **Sound is too loud / quiet** → adjust the per-clip volume floats in `AudioBindings.cs` (currently 0.6 - 1.0 per event). Or tune the SFX bus volume in the Mixer.
- **Too many sounds at once → clipping/cutoff** → bump SfxPool's Pool Size from 12 to 20.
- **Settings sliders mute audio entirely at midpoint** → the LinearToDb conversion is probably right, but if it sounds too aggressive, tune the curve.
- **Same sound triggers twice on one event** → an event you wired in fires multiple times (e.g. inventory changed fires per stack-add). Annoying but not broken — could add a debounce later.

---

## What's next (optional Tier 2)

When you're ready to expand:

- **Footsteps** — needs animations (animation events fire `PlaySfx(footstep)` on contact frames). Skip until animations are wired.
- **Music** — drop ambient tracks at `Assets/_Project/Audio/Music/`, wire them via a per-scene `MusicTrigger` script that calls `MusicPlayer.Play(clip)` on scene load. Easy add — tell me if you want this.
- **Coin pickup, level up, menu open/close** — same pattern as Tier 1, just more clip slots and event subscriptions.
- **Enemy / boss sounds** — per-enemy script with attack/hit/death AudioClips. Fires from `EnemyBase.ApplyDamage` and `BossBase.OnPhaseChanged`.

Tier 1 is a complete working baseline. Layer on as time allows.
