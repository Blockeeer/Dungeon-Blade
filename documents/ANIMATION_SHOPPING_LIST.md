# Animation Inventory — Dungeon Blade Player

Audit of what's currently in `Assets/_Project/Player/Animations/` and what's still needed for full coverage.

**Status legend:**
- ✅ Already in project — ready to wire
- ❌ Missing — need to download from Mixamo
- 💡 Optional — only grab if you want that specific feature

---

## `Idle/` (12 files present)

### ✅ Have

| Filename | Used for |
|---|---|
| `Breathing Idle.fbx` | Default standing — primary idle for unarmed/sword |
| `Pistol Idle.fbx` | Idle while gun is equipped (low ready) |
| `Pistol Aim.fbx` | Idle while aiming gun |
| `Sword And Shield Idle.fbx` | Idle while sword is equipped |
| `Sword And Shield Power Up.fbx` | Optional weapon-ready flourish |
| `Great Sword Idle.fbx` | Optional — for great sword variant |
| `Ninja Idle.fbx` | Optional — alternate stance |
| `Idle Male.fbx` / `Idle Female.fbx` | Character-specific variants |
| `Idle Crouching.fbx` | Crouch idle (after slide?) |
| `Inspecting.fbx` | Optional flair / flavor idle |
| `Grab Rifle From Back.fbx` | Weapon swap to rifle animation |

### ❌ Missing

None — Idle category is **complete**.

---

## `Locomotion/` (10 files present)

### ✅ Have

| Filename | Used for |
|---|---|
| `Running.fbx` | Run forward (primary locomotion) |
| `Standard Run.fbx` | Alternate forward run |
| `Sword And Shield Run.fbx` | Run with sword equipped |
| `Running Tired.fbx` | Optional — low stamina visual state |
| `Walking Backwards.fbx` | Move S (backpedal) |
| `Left Strafe Walking.fbx` | Move A (left strafe) |
| `Right Strafe Walking.fbx` | Move D (right strafe) |
| `Dive Roll.fbx` | Sideways dodge (double-tap A/D) |
| `Standing Dive Forward.fbx` | Forward roll (Shift + W) |
| `Dodging Back.fbx` | Backward dodge (Shift + S) |

### ❌ Missing

| Filename | Mixamo search | Why you need it |
|---|---|---|
| `Walk_Forward.fbx` | "Walking" | Slow forward walk — currently you'd play Running for both walk and run, missing speed differentiation |

### 💡 Optional

| Filename | Mixamo search | Why bother |
|---|---|---|
| `Roll_Left.fbx` | "Roll Left" | Currently using Dive Roll for both left/right — looks identical. Add for variety. |
| `Roll_Right.fbx` | "Roll Right" | Same |

---

## `Jump/` (4 files present)

### ✅ Have

| Filename | Used for |
|---|---|
| `Jump.fbx` | Vertical jump (in-place) |
| `Forward Jump.fbx` | Running jump variant |
| `Landing.fbx` | Soft land after low fall |
| `Hard Landing.fbx` | Impact land after big fall |

### ❌ Missing

None — Jump category is **complete**.

---

## `Combat/` (13 files present)

### ✅ Have

| Filename | Used for |
|---|---|
| `Sword And Shield Slash.fbx` | Sword attack 1 (default) |
| `Stable Sword Outward Slash.fbx` | Sword attack 2 (combo) |
| `Standing Melee Attack Downward.fbx` | Sword attack 3 (combo finisher) |
| `Great Sword Slash.fbx` | Heavy attack alt |
| `Thrust Slash.fbx` | Sword thrust variant |
| `Sword And Shield Block Idle.fbx` | Block stance held |
| `Sword And Shield Impact.fbx` | Block reacts to incoming hit |
| `Standing Block React Large.fbx` | Heavier block reaction |
| `Firing Rifle.fbx` | Gun fire animation |
| `Pistol Whip.fbx` | Gun melee strike |
| `Reloading.fbx` | Generic reload |
| `Reloading-Rifle.fbx` | Rifle-specific reload |
| `Unarmed Equip Underarm.fbx` | Weapon draw / sheath |

### ❌ Missing

None — Combat category is **very complete**.

### 💡 Optional

| Filename | Mixamo search | Why bother |
|---|---|---|
| `Sword_Heavy_Charge.fbx` | "Sword Heavy" or "Sword Charge" | If you implement charged heavy attack — currently uses regular slash |

---

## `Reactions/` (3 files present)

### ✅ Have

| Filename | Used for |
|---|---|
| `Hit Reaction.fbx` | Light flinch on damage |
| `Big Hit.fbx` | Heavy flinch on big damage |
| `Standing React Small From Back.fbx` | Hit from behind |

### ❌ Missing

None — Reactions category is **complete**.

### 💡 Optional

| Filename | Mixamo search | Why bother |
|---|---|---|
| `Stagger.fbx` | "Stagger" | If stamina runs out and you want a "out of breath" stagger |
| `Knockback.fbx` | "Knockback" or "Push" | If enemies have knockback attacks |

---

## `Death/` (2 files present)

### ✅ Have

| Filename | Used for |
|---|---|
| `Dying.fbx` | Standard death — fall forward |
| `Falling Back Death.fbx` | Alt death — fall backward |

### ❌ Missing

None — Death category is **complete**.

---

## Summary — what's actually missing

**Critical (recommended to grab now):**
- `Walking.fbx` (forward walk) — only real gap. Without it, you can't have a 2-speed locomotion (walk vs run).

**Nice-to-have (defer):**
- Side roll variants (Left/Right) — current Dive Roll covers both
- Heavy attack charge — only if you build that mechanic

**You're at ~95% animation coverage.** Almost everything for a full Phase 1 character is here.

---

## What this means for the Animator setup

You can build a fully-functional Animator state machine right now without grabbing anything else. The forward walk animation is missing, but your locomotion blend tree can use Running at lower speeds (just slow the playback rate) as a stopgap until you grab Walking.

**Recommended next step:** wire what you have. Tell me when ready and I'll write the bridge script + setup doc using the actual filenames you have (so the doc references real clips, not generic ones).

---

## Mixamo shopping run (5 minutes)

If you want to fill the Walking gap before wiring:

1. Go to https://www.mixamo.com (free, requires Adobe ID).
2. Search **"Walking"** (without quotes).
3. Pick the basic in-place walk cycle.
4. Download settings:
   - Format: **FBX Binary (.fbx)**
   - Skin: **Without Skin**
   - Frames per Second: **30**
   - Keyframe Reduction: **None**
5. Save the file as `Walking.fbx` (or `Walk_Forward.fbx` for clarity).
6. Drop into `Assets/_Project/Player/Animations/Locomotion/`.

That's it. Then we wire the full state machine.
