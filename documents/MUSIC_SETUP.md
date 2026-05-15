# Music Setup — Dungeon Blade

How to wire ambient music into the 3 main scenes (Lobby / Dungeon / Boss arena). Time: ~10 minutes per scene.

**Prerequisites:**
- Audio system already wired (`SfxPool` + `MainMixer.mixer` + `MusicPlayer` exist) — see [AUDIO_SETUP.md](AUDIO_SETUP.md).
- 3 music files downloaded and dropped into `Assets/_Project/Audio/Music/`.

---

## Step 1 — Add MusicPlayer to LandingScene (one-time)

`MusicPlayer` is `DontDestroyOnLoad` so it persists across scenes. Add it once.

1. Open `0_LandingScene.unity`.
2. Hierarchy → right-click → Create Empty → name `[MusicPlayer]`.
3. Add Component → search **Music Player** → click `DungeonBlade.Core.Audio.MusicPlayer`.
4. Inspector wiring:

   | Field | Value |
   |---|---|
   | Music Group | drag `MainMixer/Music` from the Audio Mixer window |
   | Crossfade Duration | `1.5` (seconds — how long it takes to switch between tracks) |
   | Default Volume | `0.6` |

5. Save scene.

If you also want music on the LandingScene splash, add a MusicTrigger here too (Step 3 covers that).

---

## Step 2 — Import settings for the music files

Each music file should be configured for streaming since they're long.

1. Click each `.mp3` / `.ogg` in `Assets/_Project/Audio/Music/`.
2. Inspector:

   | Field | Value |
   |---|---|
   | Force To Mono | ❌ (keep stereo for music) |
   | Load In Background | ✅ |
   | Load Type | `Streaming` (best for tracks > 5 seconds — saves memory) |
   | Compression Format | `Vorbis` (default — good quality + small size) |
   | Quality | `0.7` (default) |

3. Click **Apply** if you change anything.

Skip these settings only if your tracks are very short loops (< 30 seconds) — those are better as `Decompress on Load` for snappy playback.

---

## Step 3 — Add MusicTrigger to each scene

One MusicTrigger per scene that should have music.

### 3a — Lobby music

1. Open `2_Lobby.unity`.
2. Hierarchy → right-click → Create Empty → name `[Music]`.
3. Add Component → **Music Trigger**.
4. Inspector:

   | Field | Value |
   |---|---|
   | Track | drag your **lobby music** AudioClip (e.g. `LobbyAmbient.ogg`) |
   | Volume | `0.5` (lobby is calm — keep music understated) |
   | Play On Scene Load Only | ✅ |
   | Arena Track | leave empty |

5. Save scene.

### 3b — Dungeon music

1. Open `3_Dungeon1.unity`.
2. Hierarchy → right-click → Create Empty → name `[Music]`.
3. Add Component → **Music Trigger**.
4. Inspector:

   | Field | Value |
   |---|---|
   | Track | drag your **dungeon music** AudioClip (e.g. `DungeonTense.ogg`) |
   | Volume | `0.6` |
   | Play On Scene Load Only | ✅ |
   | Arena Track | drag your **boss music** AudioClip (e.g. `BossEpic.ogg`) |

5. Save scene.

### 3c — MainMenu music (optional)

1. Open `1_MainMenu.unity`.
2. Hierarchy → right-click → Create Empty → name `[Music]`.
3. Add Component → **Music Trigger**.
4. Inspector:

   | Field | Value |
   |---|---|
   | Track | drag your **lobby music** clip (or any thematic clip) |
   | Volume | `0.5` |
   | Play On Scene Load Only | ✅ |

5. Save scene.

Skip if you'd rather have the main menu silent until the player clicks New Game.

---

## Step 4 — Wire boss arena to swap to boss music (Dungeon only)

When the player crosses the boss arena trigger, the dungeon track swaps to the boss track. When the boss dies (or the player respawns outside the arena), the dungeon track resumes.

### 4a — Add the boss music to the Dungeon's MusicTrigger

You already have a `[Music]` GameObject in the Dungeon scene from Step 3b. Add the boss track to it.

1. Open `3_Dungeon1.unity`.
2. Hierarchy → click `[Music]`.
3. Inspector → **Music Trigger** component → find the **Arena Track** field (currently empty).
4. Drag your **boss music** AudioClip (e.g. `BossEpic.ogg`) from `Assets/_Project/Audio/Music/` into the Arena Track field.

   | Field | Value |
   |---|---|
   | Track | dungeon music (e.g. `DungeonRock.ogg`) — already wired |
   | Volume | `0.6` |
   | Play On Scene Load Only | ✅ |
   | Arena Track | drag `BossEpic.ogg` (boss music) ← new |

### 4b — Wire the MusicTrigger to BossArenaTrigger

`BossArenaTrigger.cs` already supports automatic music swap. You just need to drag the MusicTrigger reference in.

1. Hierarchy → find your **BossArenaTrigger** GameObject (the trigger zone at the arena entrance).
2. Inspector → **Boss Arena Trigger** component → find the new **Music Trigger** field.
3. Drag the `[Music]` GameObject from the Hierarchy into the **Music Trigger** field.

The field shows `[Music] (Music Trigger)` after wiring. Save scene.

### 4c — How it works

- **Player crosses the arena trigger** → `OnTriggerEnter` fires → `musicTrigger.SwitchToArena()` is called → boss track crossfades in over 1.5s.
- **Boss dies** → `OnBossDefeated` fires → `musicTrigger.RestoreMain()` is called → dungeon track returns.
- **Player dies in arena, respawns outside** → boss state resets + `musicTrigger.RestoreMain()` → dungeon track returns. When player walks back into the arena, boss track starts again.
- **Player dies in arena, respawns inside arena (with arena checkpoint)** → boss is reset but the arena stays sealed and music KEEPS playing the boss track (no Restore call in that branch).

### 4d — Test

1. Save Dungeon scene.
2. Press Play → walk into Dungeon.
3. Dungeon track plays.
4. Walk into the boss arena trigger.
5. Console: `[Boss] Player entered arena — sealing in 2.5s.`
6. Boss music crossfades in.
7. Defeat the boss.
8. Console: `[Boss] Defeated — arena unsealed.`
9. Dungeon track returns.

### 4e — Common issues

- **Music doesn't swap** → the `Music Trigger` field on BossArenaTrigger is empty. Re-check Step 4b.
- **Boss track plays but doesn't return when boss dies** → `OnBossDefeated` event isn't firing. Check the boss script logs `[Boss] Defeated`.
- **Music swap is jarring/instant** → MusicPlayer's `Crossfade Duration` is too short. Raise to `2.0` for a smoother transition.
- **Both tracks play at once briefly** → expected during the 1.5s crossfade. If both stay playing, the previous track's AudioSource didn't stop properly — restart Unity and retry.

---

## Step 5 — Test

1. Save all scenes.
2. Press Play from `0_LandingScene`.
3. Expected:
   - LandingScene: silent (or your splash music if Step 3c done).
   - MainMenu: optional music if Step 3c done.
   - Click **New Game** → fades to Lobby → lobby music starts (with crossfade if previous music was playing).
   - Walk to portal → enter Dungeon → dungeon music starts, crossfading from lobby.
   - (Future: enter boss arena → boss music swap when wired.)

4. Open Settings → drag the **Music slider** down → music volume drops independently of SFX. Drag SFX to 0 → SFX silent but music continues. This confirms the AudioMixer routing works.

---

## Common issues

- **No music plays at all** → MusicPlayer isn't in the scene. Check LandingScene has `[MusicPlayer]`. Or you started Play in Dungeon directly — add MusicPlayer to Dungeon too as a fallback.
- **Music plays but volume slider does nothing** → MusicPlayer's `Music Group` field is empty, OR the MainMixer's Music group's Volume isn't exposed as `MusicVolume`. Re-check [AUDIO_SETUP.md §2f-2i](AUDIO_SETUP.md).
- **Music starts/stops abruptly without crossfade** → MusicPlayer.crossfadeDuration is 0 or too small. Default 1.5s.
- **Music stutters / clicks at loop point** → the source file isn't a clean loop. Trim in Audacity or use a different clip.
- **Music is way too loud / quiet** → adjust the per-MusicTrigger Volume field, OR adjust the Music group's volume in the Mixer (the master multiplier).
- **Same track plays in two scenes (no crossfade)** → MusicTrigger's track field points to the same clip in both scenes. The MusicPlayer.Play() check `if (clip == _currentClip)` skips replaying the same clip. Use different tracks per scene.

---

## Crediting (legal paper trail)

If any of your music tracks require attribution (CC-BY license), add credits to the game.

1. Create `documents/AUDIO_CREDITS.md`:

```markdown
# Audio Credits

## Music
- **LobbyAmbient.ogg** — "Track Name" by Composer Name. License: CC-BY 4.0. Source: <URL>
- **DungeonTense.ogg** — "Track Name" by Composer Name. License: Pixabay License. Source: <URL>
- **BossEpic.ogg** — "Track Name" by Composer Name. License: CC0. Source: <URL>

## SFX
- (List SFX clips and sources here)
```

2. (Optional) Add a Credits screen accessible from MainMenu that displays the list. Or just keep this file in the repo as a paper trail — sufficient for legal coverage if anyone ever asks.

---

## What's next

If music works:
- **Boss arena music swap** — tell me Option A or B and I'll wire it.
- **Footsteps + animations** — both come together once Mixamo animations are re-imported.
- **Item rarity tiers** — your earlier suggestion, do this once weapons have real models.
