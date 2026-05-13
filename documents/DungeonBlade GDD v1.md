**DUNGEON BLADE**

A GunZ-Inspired Action Dungeon Game

GAME DESIGN DOCUMENT \| Version 1.0 \| Phase 1: Dungeon 1

**1. Project Overview**

Dungeon Blade is a fast-paced action game inspired by GunZ: The Duel.
Players navigate a linear dungeon, fight enemies using a combination of
firearms, swords, and acrobatic moves, defeat a boss at the end of the
dungeon, collect loot rewards, and manage their items through an
inventory and bank system.

This document covers Phase 1: one complete dungeon with one difficulty
level. All systems (combat, movement, inventory, bank) must be fully
functional before expansion to additional dungeons.

  -----------------------------------------------------------------------
  **Attribute**         **Value**
  --------------------- -------------------------------------------------
  Genre                 3D Action / Dungeon Crawler

  Perspective           Third-person over-the-shoulder

  Target Platform       PC (Windows)

  Engine Recommendation Unity or Unreal Engine 5 (Lumen optional)

  Phase 1 Scope         1 Dungeon, 1 Boss, Full Inventory & Bank Systems

  Multiplayer           Optional co-op (up to 4 players) --- Phase 2

  Art Style             Low-poly / stylized (keep it light and
                        performant)
  -----------------------------------------------------------------------

**2. Core Gameplay Mechanics**

**2.1 Movement System**

The movement system is the heart of the game and must feel fluid and
responsive at all times.

-   Basic Movement: WASD directional movement, mouse for camera and aim

-   Jump: Single jump + double jump (spacebar). Must feel weightless and
    snappy

-   Dash / Sidestep: Tap A/D twice OR dedicated button (Shift) for a
    directional dodge roll

-   Wall Running: Players can run along walls briefly (1-2 seconds)
    before gravity pulls them down

-   Bunny Hop: Successive jumps maintain/build momentum for skilled
    players

-   Slide: Crouch while running to slide under obstacles

**2.2 Combat System**

Combat is hybrid: players carry both a melee weapon (sword/blade) and
ranged weapons (guns). Switching between them must be instant.

**Melee (Sword)**

-   3-hit light combo: fast slashes, low damage per hit

-   Heavy attack (hold button): charged slash with knockback, longer
    animation

-   Dash-attack: dash into enemy and release heavy attack for a lunge

-   Block / Parry: Hold block key; if timed correctly on enemy attack =
    parry, staggers enemy

-   Sword skills (see Section 4 --- Skills)

**Ranged (Guns)**

-   Primary gun: semi-auto or burst (e.g., pistol, rifle)

-   Secondary gun: heavy/slow (e.g., shotgun, grenade launcher)

-   Reload mechanic: manual reload key (R); caught reloading mid-fight
    is punishing

-   Aim-down-sights (RMB): zoom/tighter spread, slower movement

-   Gun recoil patterns: each gun has a unique spread/recoil profile

**Combo System (GunZ-style)**

The signature mechanic: players can chain sword and gun attacks in fluid
combos.

-   Sword + Gun combos: slash then immediately fire for a \'cancel\'
    that resets sword animation

-   K-style moves (advanced): butterfly (dash + slash + gun), reload
    cancel, tumbling while shooting

-   Combo counter: on-screen counter for consecutive hits. Higher combos
    = bonus EXP at end of dungeon

**2.3 Health, Stamina & Defense**

-   HP (Health Points): Base 100 HP, regenerates slowly out of combat

-   Stamina: Used for dashes, wall runs, and heavy attacks. Regens
    quickly

-   Armor: Equipped items reduce incoming damage by a flat % or value

-   Death: On death, player respawns at last checkpoint with 50% HP;
    limited respawns (3 per dungeon run)

**3. Dungeon 1 --- \"The Forsaken Keep\"**

**3.1 Theme & Atmosphere**

A crumbling medieval fortress overrun by undead soldiers and dark magic.
Stone corridors, torch-lit halls, collapsed bridges, and a grand throne
room for the boss fight. Atmosphere: dark, gothic, intense.

**3.2 Structure & Flow**

The dungeon is linear with branching optional rooms. It is divided into
5 zones:

  ---------------------------------------------------------------------------
  **Zone**   **Name**    **Description**                  **Enemies**
  ---------- ----------- -------------------------------- -------------------
  Zone 1     Gate Hall   Tutorial area. Wide corridors,   Skeleton Soldiers
                         introduces basic enemies and     x4
                         movement obstacles.              

  Zone 2     Barracks    Tight rooms and corridors.       Skeleton Archer x3,
                         Introduces archers and elevation Soldier x4
                         (upper floor).                   

  Zone 3     The Bridge  Outdoor section. Broken bridge   Elite Soldier x2,
                         with gaps; wall-running          Archer x2
                         required. Ambush mid-bridge.     

  Zone 4     Armory      Loot room + optional elite       Armored Knight
                         enemy. Best pre-boss gear found  (Elite) x1
                         here.                            

  Zone 5     Throne Room Boss arena. Large circular room  BOSS: The Undead
                         with pillars for cover. Boss     Warlord
                         fight.                           
  ---------------------------------------------------------------------------

**3.3 Checkpoints**

-   Checkpoint 1: After Zone 1 --- Gate Hall cleared

-   Checkpoint 2: After Zone 3 --- The Bridge cleared (auto-saves
    progress)

-   Checkpoints restore stamina and grant +20 HP

**3.4 Environmental Hazards**

-   Spike traps: floor tiles that trigger periodically

-   Collapsing floors: some platforms crumble after a few seconds of
    standing

-   Arrow walls: corridor traps triggered by pressure plates

-   Gaps / fall damage: falling into pits deals 20 HP damage and
    respawns player at last stable ground

**4. Enemy Design**

**4.1 Regular Enemies**

  ---------------------------------------------------------------------------
  **Enemy**     **HP**   **Behavior**      **Attack**        **Drop**
  ------------- -------- ----------------- ----------------- ----------------
  Skeleton      30       Patrol → melee    Sword slash (10   Gold (1-5),
  Soldier                rush on aggro     dmg)              Common weapon
                                                             part

  Skeleton      20       Stationary or     Arrow (8 dmg,     Gold (1-3),
  Archer                 slow patrol;      knockback)        Arrows
                         ranged                              

  Armored       80       Guards key area,  Heavy slam (25    Gold (10-20),
  Knight                 slow but powerful dmg), Shield bash Rare armor piece
  ---------------------------------------------------------------------------

**4.2 Boss --- The Undead Warlord**

A massive armored undead general. 3 phases that escalate in aggression.
This fight requires players to use all learned mechanics.

  --------------------------------------------------------------------------
  **Phase**   **HP          **New Behavior / Attacks**
              Threshold**   
  ----------- ------------- ------------------------------------------------
  Phase 1     100% - 66% HP Sword combo (3 hits), slow stomp AoE.
                            Telegraphed attacks, learnable patterns.

  Phase 2     66% - 33% HP  Summons 2 Skeleton Soldiers. Adds a ranged bone
                            throw (15 dmg). Faster combos.

  Phase 3     33% - 0% HP   Enrages: glowing red, attack speed +30%, spawns
                            ground shockwaves. Must dodge aggressively.
  --------------------------------------------------------------------------

-   Boss HP: 400

-   Boss death: cinematic death animation (2-3 sec), then reward chest
    spawns in center of arena

-   Optional: allow players to loot the boss body directly for a bonus
    rare item drop

**5. Player Skills & Abilities**

Players start with a base skillset. Additional skills are unlocked by
leveling up or purchasing in the bank\'s skill shop (Phase 2). For Phase
1, the following 6 skills are available from the start:

  -------------------------------------------------------------------------------
  **Skill Name**    **Type**         **Description**               **Cooldown**
  ----------------- ---------------- ----------------------------- --------------
  Blade Dash        Melee/Movement   Dash forward and slash all    6s
                                     enemies in path. Short        
                                     distance.                     

  Shotgun Burst     Ranged           Fire all ammo in shotgun      8s
                                     simultaneously for massive    
                                     burst damage.                 

  Smoke Grenade     Utility          Throws a smoke grenade.       15s
                                     Enemies lose target for 3s.   

  Counter Strike    Melee            After successful parry,       No CD
                                     auto-trigger a 2-hit counter  (requires
                                     combo.                        parry)

  Battle Roll       Movement         Evasive roll with brief       4s
                                     I-frames (invincibility       
                                     frames). Resets on landing.   

  Iron Skin         Defense          Brief defensive stance:       20s
                                     reduce all damage by 50% for  
                                     2s.                           
  -------------------------------------------------------------------------------

**6. Reward System**

**6.1 Item Rarity Tiers**

  ------------------------------------------------------------------------
  **Rarity**   **Color**   **Description**
  ------------ ----------- -----------------------------------------------
  Common       White /     Basic gear. No special stats. Vendor trash or
               Gray        salvage material.

  Uncommon     Green       Minor stat boosts (+ATK, +DEF). Found in
                           regular enemy drops.

  Rare         Blue        Notable stat bonuses. Found in chests and elite
                           enemies.

  Epic         Purple      Significant bonuses + 1 special property (e.g.,
                           lifesteal, piercing shots).

  Legendary    Orange /    Unique items with powerful effects.
               Gold        Boss-exclusive drops only.
  ------------------------------------------------------------------------

**6.2 Boss Reward Table**

Upon defeating The Undead Warlord, a reward chest spawns. The loot table
uses weighted random rolls:

  ------------------------------------------------------------------------
  **Roll**     **Reward**                             **Quantity**
  ------------ -------------------------------------- --------------------
  Guaranteed   Gold (150-300)                         1 roll

  Guaranteed   Uncommon or Rare item (weapon or       1 item
               armor)                                 

  70% chance   Rare item (weapon or armor)            1 item

  30% chance   Epic item                              1 item

  5% chance    Legendary: Warlord\'s Blade (unique    1 item (first kill
               sword)                                 only)

  100%         Dungeon Clear Token (currency for      1 token
               bank)                                  
  ------------------------------------------------------------------------

**6.3 Item Categories**

-   Weapons: Swords (melee damage, attack speed), Guns (damage, fire
    rate, clip size, recoil)

-   Armor: Helmets, Chest, Legs, Boots (DEF, HP, stamina regen)

-   Accessories: Rings, Amulets (passive effects: crit chance, cooldown
    reduction)

-   Consumables: Health potions, Stamina potions, Grenades (used from
    inventory hotbar)

-   Materials: Salvaged from gear. Used for crafting/upgrading (Phase 2)

**7. Inventory System**

**7.1 Overview**

The inventory is accessible at any time outside of combat (pressing I or
Tab). During dungeon runs, it is accessible at checkpoints only.

**7.2 Inventory UI Layout**

-   Grid-based: 6 columns x 8 rows = 48 slots

-   Equipment slots on the left: Head, Chest, Legs, Boots, Main Hand,
    Off Hand, Ring x2, Amulet

-   Hotbar: 4 slots at the bottom for quick-use consumables (bound to 1,
    2, 3, 4)

-   Item tooltip on hover: shows full stats, rarity, level requirement,
    and flavor text

-   Right-click context menu: Equip, Sell (opens bank), Discard, Split
    Stack (for stackable items)

**7.3 Item Stacking**

-   Gold, potions, materials, and grenades stack (max 99 per slot)

-   Weapons and armor do not stack; each occupies one slot

**7.4 Weight System (Optional --- can be toggled off in settings)**

-   Each item has a Weight value. Max carry weight: 100 units

-   Exceeding weight slows movement speed by 20%

-   Consumables and materials are weightless

**8. Banking & Economy System**

**8.1 Overview**

The Bank is the central hub for economy. It is accessible from the main
lobby (pre-dungeon hub). Players can store items, sell loot, and buy
consumables/gear here. Each player\'s bank account is persistent and
tied to their profile.

**8.2 Bank Features**

-   Item Storage (Bank Vault): 120 slots (6 columns x 20 rows) for
    long-term item storage

-   Item Sell: Sell items directly to the bank NPC for gold. Sell price
    = 30% of item base value

-   Item Buy: Purchase consumables, common/uncommon gear from NPC shop
    using gold

-   Dungeon Token Exchange: Spend Dungeon Clear Tokens for exclusive
    gear or cosmetics

-   Player Gold Account: Gold is stored server-side per player. Cannot
    be lost in dungeon

**8.3 Data Persistence & Safety**

This is critical for player trust. Each player\'s bank data must be
protected:

-   All bank data stored server-side (or local encrypted save file for
    single-player)

-   Inventory is NOT lost on dungeon death --- items looted during a run
    are safe once picked up

-   Bank vault items are always safe regardless of outcome

-   Gold in inventory (not yet deposited) is LOST on dungeon failure
    (optional hardcore rule --- can be toggled off in settings)

**8.4 Economy Balance (Dungeon 1 Reference)**

  -----------------------------------------------------------------------
  **Item**                      **Buy Price        **Sell Price (Gold)**
                                (Gold)**           
  ----------------------------- ------------------ ----------------------
  Small Health Potion           25                 7

  Large Health Potion           75                 22

  Stamina Potion                30                 9

  Grenade                       50                 15

  Common Weapon                 100-250            30-75

  Uncommon Weapon               400-800            120-240

  Rare Weapon                   N/A (drop only)    250-600

  Dungeon Clear Token           N/A (earn only)    Cannot sell
  -----------------------------------------------------------------------

**9. Player Progression**

**9.1 Experience & Leveling**

-   EXP awarded for: kills (base), combo bonuses, boss kill, dungeon
    completion, first-clear bonus

-   Level cap for Phase 1: Level 20

-   Each level grants: +5 max HP, +2 max stamina, +1 Skill Point (used
    to upgrade skills in Phase 2)

**9.2 EXP Sources in Dungeon 1**

  -----------------------------------------------------------------------
  **Source**                                 **EXP Reward**
  ------------------------------------------ ----------------------------
  Skeleton Soldier kill                      15 EXP

  Skeleton Archer kill                       15 EXP

  Armored Knight kill                        60 EXP

  Boss kill                                  300 EXP

  Dungeon clear bonus                        200 EXP

  First clear bonus                          +100 EXP (one time)

  Combo bonus (10+ hits)                     +25 EXP per 10-hit milestone
  -----------------------------------------------------------------------

**10. Technical Architecture**

**10.1 Recommended Tech Stack**

  ------------------------------------------------------------------------
  **Component**   **Recommendation**      **Notes**
  --------------- ----------------------- --------------------------------
  Game Engine     Unity 2022 LTS or       Unity for faster iteration; UE5
                  Unreal Engine 5         for higher visual ceiling

  Physics         Engine built-in (PhysX  Ragdoll on enemy death is a
                  / Chaos)                bonus

  Networking      Photon Fusion or Mirror For eventual co-op dungeon runs
  (Phase 2)                               

  Database (Bank) Local: JSON/SQLite \|   Must be per-player, persistent
                  Online: Firebase or     
                  custom REST API         

  Audio           FMOD or Unity Audio     Important: satisfying sword and
                                          gun sound design

  VFX             Particle system +       Hit sparks, muzzle flash, blood
                  shader effects          decals (optional)
  ------------------------------------------------------------------------

**10.2 Input Mapping (Default PC)**

  -----------------------------------------------------------------------
  **Action**                        **Key / Button**
  --------------------------------- -------------------------------------
  Move                              WASD

  Jump / Double Jump                Spacebar (x2)

  Dash / Dodge                      Shift or double-tap A/D

  Light Attack (Sword)              Left Mouse Button

  Heavy Attack (Sword)              Hold Left Mouse Button

  Block / Parry                     Right Mouse Button (melee equipped)

  Aim Down Sights                   Right Mouse Button (gun equipped)

  Fire Gun                          Left Mouse Button (gun equipped)

  Reload                            R

  Switch Weapon                     Q or Mouse Wheel

  Open Inventory                    I or Tab

  Use Hotbar Slot 1-4               Keys 1, 2, 3, 4

  Skill 1-3                         E, G, V

  Interact                          F

  Pause Menu                        Escape
  -----------------------------------------------------------------------

**10.3 Performance Targets**

-   Target: 60 FPS stable on mid-range PC (GTX 1060 equivalent)

-   Dungeon scene: max 500 draw calls, LOD system for distant geometry

-   Enemy AI: behaviour trees with max 10 active AI agents
    simultaneously

-   Keep all assets light: use atlased textures, compressed audio,
    minimal overdraw

**11. HUD & UI Design**

**11.1 In-Dungeon HUD Elements**

-   HP Bar: bottom-left. Numeric value shown. Flashes red when below 25%

-   Stamina Bar: below HP. Color: blue/cyan. Depletes on dash/wall
    run/heavy attack

-   Equipped weapons: bottom-right. Shows main weapon icon + ammo count
    / sword durability

-   Hotbar: bottom-center. 4 slots with item icons and keybind labels

-   Active skill icons: above hotbar. Cooldown overlay shows remaining
    time

-   Combo counter: top-center (fades after 3s of no hits)

-   Mini-map: top-right. Shows current zone layout and player dot

-   Checkpoint indicator: brief top-center notification on checkpoint
    reached

**11.2 Menu Screens Required for Phase 1**

-   Main Menu: Play, Settings, Quit

-   Character Select / Profile screen: shows player name, level, current
    gear

-   Lobby / Hub: Access dungeon entrance + Bank NPC + future dungeon
    list

-   Inventory screen (from lobby or checkpoint)

-   Bank screen (from Bank NPC in lobby)

-   Dungeon clear screen: summary of run (time, kills, combo best, EXP
    gained, loot)

-   Game over screen: shows respawns remaining or defeat message

**12. Audio Design Guidelines**

Audio is critical for combat feel. Even with simple graphics, great
audio sells the game.

-   Sword hit: sharp metallic clang with brief reverb; different sound
    for hit vs. parry vs. miss

-   Gunshots: punchy and distinct per weapon. Pistol vs. shotgun must
    feel very different

-   Enemy death: satisfying crunch/thud. Skeleton shatter for undead
    enemies

-   Boss roar: plays at each phase transition. Distinct and impactful

-   Music: Dungeon ambient (dark, orchestral drone) transitions to
    intense combat music on aggro. Boss fight: dedicated epic track

-   UI sounds: inventory open/close, item pickup, gold collect,
    checkpoint reached

**13. Development Milestones**

Recommended order of development for Phase 1:

  -----------------------------------------------------------------------
  **Milestone**         **Deliverable**                   **Est.
                                                          Duration**
  --------------------- --------------------------------- ---------------
  M1 --- Movement       Player controller: move, jump,    1 week
  Prototype             dash, wall run. No enemies.       

  M2 --- Combat         Sword + gun combat, hit           1.5 weeks
  Prototype             detection, combo system. Dummy    
                        targets.                          

  M3 --- Dungeon 1      Full dungeon geometry: all 5      1.5 weeks
  Layout                zones, checkpoints, traps.        

  M4 --- Enemy AI       All 3 enemy types with            1.5 weeks
                        pathfinding + attack behaviors.   

  M5 --- Boss Fight     Full 3-phase boss fight with all  1 week
                        attacks and phase transitions.    

  M6 --- Inventory      Inventory UI, equip/unequip,      1 week
  System                hotbar, item tooltips.            

  M7 --- Bank & Economy Bank vault, NPC shop, sell        1 week
                        system, gold persistence.         

  M8 --- Reward System  Loot drops, chest spawns, rarity  0.5 weeks
                        system, EXP & leveling.           

  M9 --- Polish & HUD   Full HUD, audio, VFX, UI screens. 1.5 weeks
                        Bug fix pass.                     

  M10 --- QA & Approval Playtest, balance, fix. Submit    1 week
                        for client approval.              
  -----------------------------------------------------------------------

Total estimated Phase 1 duration: \~12 weeks (3 months) with a team of
3-4 developers.

**14. Future Phases (Post-Approval)**

These features are NOT in scope for Phase 1 but should be architected to
support them:

-   Phase 2: Additional dungeons (each with unique theme, enemies, boss,
    exclusive loot)

-   Phase 2: Multiplayer co-op (2-4 players per dungeon run)

-   Phase 2: PvP arena mode (GunZ-style 1v1 or team deathmatch)

-   Phase 2: Skill tree and character class system

-   Phase 2: Crafting system (combine materials from dungeons)

-   Phase 2: Seasonal events and limited-time dungeons

-   Phase 3: Cosmetic system (weapon skins, character outfits) ---
    monetization layer

-   Phase 3: Leaderboards per dungeon (fastest clear, highest combo)

**15. Approval Criteria for Phase 1**

Before proceeding to Phase 2, all of the following must be confirmed:

-   Movement feels responsive and satisfying (jump, dash, wall run all
    work as described)

-   Sword and gun combat both feel impactful with proper VFX and audio

-   Dungeon 1 can be completed start-to-finish without game-breaking
    bugs

-   Boss fight has all 3 phases functioning correctly

-   Reward chest spawns post-boss with correct loot table

-   Inventory system fully functional (equip, unequip, hotbar, tooltips)

-   Bank system persistent across sessions (items and gold saved
    correctly)

-   No progress loss bugs --- checkpoints and save states work as
    documented

--- End of Document ---
