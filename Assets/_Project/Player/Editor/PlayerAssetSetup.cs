using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DungeonBlade.Player;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace DungeonBlade.EditorTools
{
    public static class PlayerAssetSetup
    {
        const string PlayerRoot     = "Assets/_Project/Player";
        const string ModelsRoot     = PlayerRoot + "/Models";
        const string AnimationsRoot = PlayerRoot + "/Animations";
        const string SourceModelPath = ModelsRoot + "/Varion-T-Pose.fbx";
        const string ControllerPath  = AnimationsRoot + "/PlayerAnimator.controller";

        // ─────────────────────────────────────────────────────────────────────
        // 1. SHARED AVATAR
        //    Picks Varion as the canonical Mixamo avatar source, then points
        //    every other model + every animation FBX at it via "Copy From Other".
        //    Eliminates per-model auto-mapping drift across 6 characters.
        // ─────────────────────────────────────────────────────────────────────
        [MenuItem("Tools/Dungeon Blade/Player/1. Set Shared Avatar (Varion as source)")]
        public static void SetSharedAvatar()
        {
            var sourceImporter = AssetImporter.GetAtPath(SourceModelPath) as ModelImporter;
            if (sourceImporter == null)
            {
                Debug.LogError($"[Dungeon Blade] Source model not found at {SourceModelPath}");
                return;
            }

            sourceImporter.animationType = ModelImporterAnimationType.Human;
            sourceImporter.avatarSetup   = ModelImporterAvatarSetup.CreateFromThisModel;
            sourceImporter.SaveAndReimport();

            var sourceAvatar = AssetDatabase.LoadAllAssetsAtPath(SourceModelPath)
                .OfType<Avatar>().FirstOrDefault();
            if (sourceAvatar == null)
            {
                Debug.LogError("[Dungeon Blade] Could not locate Varion's auto-generated Avatar after reimport.");
                return;
            }

            int models = 0, anims = 0;

            foreach (var path in EnumerateModels(ModelsRoot))
            {
                if (path == SourceModelPath) continue;
                if (TryApplyCopyAvatar(path, sourceAvatar)) models++;
            }

            foreach (var path in EnumerateModels(AnimationsRoot))
            {
                if (TryApplyCopyAvatar(path, sourceAvatar)) anims++;
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[Dungeon Blade] Shared avatar configured. {models} character model(s) and {anims} animation file(s) now copy from Varion's avatar.");
        }

        static bool TryApplyCopyAvatar(string path, Avatar source)
        {
            var imp = AssetImporter.GetAtPath(path) as ModelImporter;
            if (imp == null) return false;

            bool needsChange =
                imp.animationType != ModelImporterAnimationType.Human ||
                imp.avatarSetup   != ModelImporterAvatarSetup.CopyFromOther ||
                imp.sourceAvatar  != source;

            if (!needsChange) return false;

            imp.animationType = ModelImporterAnimationType.Human;
            imp.avatarSetup   = ModelImporterAvatarSetup.CopyFromOther;
            imp.sourceAvatar  = source;
            imp.SaveAndReimport();
            return true;
        }

        static IEnumerable<string> EnumerateModels(string folder) =>
            AssetDatabase.FindAssets("t:Model", new[] { folder })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => p.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase));

        // ─────────────────────────────────────────────────────────────────────
        // 2. ANIMATOR CONTROLLER
        //    Builds PlayerAnimator.controller wired to the existing FBX clips.
        //    Parameters chosen to align with PlayerMovement / PlayerCombat:
        //      Speed, Grounded, Jump, Roll, Dodge, Attack, HeavyAttack, Block,
        //      Reload, WeaponType (0 Unarmed, 1 Sword+Shield, 2 GreatSword,
        //      3 Pistol, 4 Rifle).
        // ─────────────────────────────────────────────────────────────────────
        [MenuItem("Tools/Dungeon Blade/Player/2. Create Animator Controller")]
        public static void CreateAnimatorController()
        {
            AnimatorController controller;
            if (File.Exists(ControllerPath))
            {
                if (!EditorUtility.DisplayDialog(
                    "Overwrite PlayerAnimator?",
                    $"{ControllerPath} already exists.\n\nOverwrite with a fresh controller? (Preserves the asset GUID so prefab references stay intact.)",
                    "Overwrite", "Cancel"))
                    return;

                // Reset the existing controller IN PLACE so its GUID is preserved
                // and any prefab/scene references to it stay valid. Deleting and
                // recreating would mint a new GUID and break the Player prefab.
                controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);

                // Strip every sub-asset under the controller (states, blend trees,
                // transitions are all stored as nested objects).
                var subAssets = AssetDatabase.LoadAllAssetsAtPath(ControllerPath);
                foreach (var sub in subAssets)
                {
                    if (sub != null && sub != controller)
                        UnityEngine.Object.DestroyImmediate(sub, true);
                }

                // Strip parameters (RemoveParameter takes the param object, not an index)
                var existingParams = controller.parameters;
                foreach (var p in existingParams) controller.RemoveParameter(p);

                // Replace the root layer's state machine with a fresh empty one.
                if (controller.layers.Length == 0) controller.AddLayer("Base Layer");
                var freshSM = new AnimatorStateMachine
                {
                    name = controller.layers[0].name + "_StateMachine",
                    hideFlags = HideFlags.HideInHierarchy,
                };
                AssetDatabase.AddObjectToAsset(freshSM, controller);
                var layers = controller.layers;
                layers[0].stateMachine = freshSM;
                controller.layers = layers;
            }
            else
            {
                controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            }

            controller.AddParameter(new AnimatorControllerParameter { name = "Speed",       type = AnimatorControllerParameterType.Float });
            // Camera-relative movement vector for the 2D Locomotion blend tree.
            // MoveX = strafe (-1 left, +1 right), MoveZ = forward/back (-1 back, +1 forward).
            controller.AddParameter(new AnimatorControllerParameter { name = "MoveX",       type = AnimatorControllerParameterType.Float });
            controller.AddParameter(new AnimatorControllerParameter { name = "MoveZ",       type = AnimatorControllerParameterType.Float });
            controller.AddParameter(new AnimatorControllerParameter { name = "Grounded",    type = AnimatorControllerParameterType.Bool, defaultBool = true });
            controller.AddParameter(new AnimatorControllerParameter { name = "Jump",        type = AnimatorControllerParameterType.Trigger });
            controller.AddParameter(new AnimatorControllerParameter { name = "Roll",        type = AnimatorControllerParameterType.Trigger });
            controller.AddParameter(new AnimatorControllerParameter { name = "Dodge",       type = AnimatorControllerParameterType.Trigger });
            controller.AddParameter(new AnimatorControllerParameter { name = "Dash",        type = AnimatorControllerParameterType.Trigger });
            controller.AddParameter(new AnimatorControllerParameter { name = "Slide",       type = AnimatorControllerParameterType.Bool });
            controller.AddParameter(new AnimatorControllerParameter { name = "Tired",       type = AnimatorControllerParameterType.Bool });
            controller.AddParameter(new AnimatorControllerParameter { name = "Attack",      type = AnimatorControllerParameterType.Trigger });
            controller.AddParameter(new AnimatorControllerParameter { name = "HeavyAttack", type = AnimatorControllerParameterType.Trigger });
            controller.AddParameter(new AnimatorControllerParameter { name = "Combo",       type = AnimatorControllerParameterType.Trigger });
            controller.AddParameter(new AnimatorControllerParameter { name = "Block",       type = AnimatorControllerParameterType.Bool });
            controller.AddParameter(new AnimatorControllerParameter { name = "Parry",       type = AnimatorControllerParameterType.Trigger });
            controller.AddParameter(new AnimatorControllerParameter { name = "BlockBroken", type = AnimatorControllerParameterType.Trigger });
            controller.AddParameter(new AnimatorControllerParameter { name = "Reload",      type = AnimatorControllerParameterType.Trigger });
            controller.AddParameter(new AnimatorControllerParameter { name = "Equip",       type = AnimatorControllerParameterType.Trigger });
            controller.AddParameter(new AnimatorControllerParameter { name = "WeaponType",  type = AnimatorControllerParameterType.Int });
            controller.AddParameter(new AnimatorControllerParameter { name = "Hit",         type = AnimatorControllerParameterType.Trigger });
            controller.AddParameter(new AnimatorControllerParameter { name = "BigHit",      type = AnimatorControllerParameterType.Trigger });
            controller.AddParameter(new AnimatorControllerParameter { name = "HitBack",     type = AnimatorControllerParameterType.Trigger });
            controller.AddParameter(new AnimatorControllerParameter { name = "Die",         type = AnimatorControllerParameterType.Trigger });

            var sm = controller.layers[0].stateMachine;
            sm.entryPosition    = new Vector3(  0,    0, 0);
            sm.anyStatePosition = new Vector3(-300,  100, 0);
            sm.exitPosition     = new Vector3(1100,    0, 0);

            // Locomotion (default state) — 2D directional blend tree so S plays
            // Walking Backwards, A/D play strafes, and W plays Standard Run.
            // Driven by camera-relative MoveX (strafe) and MoveZ (forward/back),
            // both in [-1, 1]. Bridge computes them from velocity ÷ camera basis.
            var locoState = sm.AddState("Locomotion", new Vector3(200, 0, 0));
            sm.defaultState = locoState;

            var locoTree = new BlendTree
            {
                name              = "Locomotion",
                blendType         = BlendTreeType.SimpleDirectional2D,
                blendParameter    = "MoveX",
                blendParameterY   = "MoveZ",
            };
            AssetDatabase.AddObjectToAsset(locoTree, controller);
            locoState.motion = locoTree;

            var idleClip   = LoadClip(AnimationsRoot + "/Idle/Breathing Idle.fbx");
            var runFwd     = LoadClip(AnimationsRoot + "/Locomotion/Standard Run.fbx");
            var walkBack   = LoadClip(AnimationsRoot + "/Locomotion/Walking Backwards.fbx");
            var strafeL    = LoadClip(AnimationsRoot + "/Locomotion/Left Strafe Walking.fbx");
            var strafeR    = LoadClip(AnimationsRoot + "/Locomotion/Right Strafe Walking.fbx");
            if (idleClip != null) locoTree.AddChild(idleClip, new Vector2( 0f,  0f));
            if (runFwd   != null) locoTree.AddChild(runFwd,   new Vector2( 0f,  1f));
            if (walkBack != null) locoTree.AddChild(walkBack, new Vector2( 0f, -1f));
            if (strafeL  != null) locoTree.AddChild(strafeL,  new Vector2(-1f,  0f));
            if (strafeR  != null) locoTree.AddChild(strafeR,  new Vector2( 1f,  0f));

            // Jump (split by Speed) → Land → Locomotion
            var jumpState = sm.AddState("Jump",        new Vector3(550, -150, 0));
            jumpState.motion  = LoadClip(AnimationsRoot + "/Jump/Jump.fbx");
            var fjumpState = sm.AddState("ForwardJump", new Vector3(550,  -50, 0));
            fjumpState.motion = LoadClip(AnimationsRoot + "/Jump/Forward Jump.fbx");

            // Land state plays on touchdown — gives jumps real weight instead of
            // snapping back to idle/run. We use the milder "Landing" clip whose
            // end-pose is closer to a standing rest, so the blend back to
            // Locomotion doesn't pop the way "Hard Landing" did.
            var landState = sm.AddState("Land", new Vector3(800, -100, 0));
            landState.motion = LoadClip(AnimationsRoot + "/Jump/Landing.fbx");
            landState.speed  = 2.2f;

            AddTriggerTransition(locoState, jumpState,  "Jump", ("Speed", AnimatorConditionMode.Less,    1f));
            AddTriggerTransition(locoState, fjumpState, "Jump", ("Speed", AnimatorConditionMode.Greater, 1f));
            AddLandTransition(jumpState,  landState);
            AddLandTransition(fjumpState, landState);
            // Exit Land early so control returns to the player quickly. The
            // 0.18s blend still eats the end-pose mismatch without dragging
            // out the recovery the way the old 0.4 / 0.3 values did.
            var landToLoco = landState.AddTransition(locoState);
            landToLoco.hasExitTime = true;
            landToLoco.exitTime    = 0.15f;
            landToLoco.duration    = 0.18f;

            // Burst movement states (Dash / Roll / Dodge). Mixamo authors these
            // clips at cinematic pace (~1s); for combat realism we play them at
            // 2.5× and exit at 40% so the body is back in Locomotion well within
            // the gameplay-side dash/dodge window (~0.18s) and the player has
            // full control again quickly.
            const float burstSpeed    = 2.5f;
            const float burstExitTime = 0.4f;

            var dashState  = sm.AddState("Dash",  new Vector3(550, -10, 0));
            dashState.motion  = LoadClip(AnimationsRoot + "/Locomotion/Standing Dive Forward.fbx");
            dashState.speed   = burstSpeed;
            var rollState  = sm.AddState("Roll",  new Vector3(550,  80, 0));
            rollState.motion  = LoadClip(AnimationsRoot + "/Locomotion/Dive Roll.fbx");
            rollState.speed   = burstSpeed;
            var dodgeState = sm.AddState("Dodge", new Vector3(550, 180, 0));
            dodgeState.motion = LoadClip(AnimationsRoot + "/Locomotion/Dodging Back.fbx");
            dodgeState.speed  = burstSpeed;

            AddTriggerTransition(locoState, dashState,  "Dash");
            AddTriggerTransition(locoState, rollState,  "Roll");
            AddTriggerTransition(locoState, dodgeState, "Dodge");
            AddExitTimeTransition(dashState,  locoState, burstExitTime);
            AddExitTimeTransition(rollState,  locoState, burstExitTime);
            AddExitTimeTransition(dodgeState, locoState, burstExitTime);

            // Attack (branched by WeaponType)
            var atkUnarmed = sm.AddState("Attack_Unarmed",     new Vector3(850,  60, 0));
            atkUnarmed.motion = LoadClip(AnimationsRoot + "/Combat/Standing Melee Attack Downward.fbx");
            var atkSword   = sm.AddState("Attack_SwordShield", new Vector3(850, 140, 0));
            atkSword.motion = LoadClip(AnimationsRoot + "/Combat/Sword And Shield Slash.fbx");
            var atkGreat   = sm.AddState("Attack_GreatSword",  new Vector3(850, 220, 0));
            atkGreat.motion = LoadClip(AnimationsRoot + "/Combat/Great Sword Slash.fbx");
            var atkGun     = sm.AddState("Attack_Gun",         new Vector3(850, 300, 0));
            atkGun.motion   = LoadClip(AnimationsRoot + "/Combat/Firing Rifle.fbx");

            AddTriggerTransition(locoState, atkUnarmed, "Attack", ("WeaponType", AnimatorConditionMode.Equals,  0));
            AddTriggerTransition(locoState, atkSword,   "Attack", ("WeaponType", AnimatorConditionMode.Equals,  1));
            AddTriggerTransition(locoState, atkGreat,   "Attack", ("WeaponType", AnimatorConditionMode.Equals,  2));
            AddTriggerTransition(locoState, atkGun,     "Attack", ("WeaponType", AnimatorConditionMode.Greater, 2));

            // Combo follow-up: pressing Attack again before atkSword finishes
            // chains into Thrust Slash. Otherwise it exits back to Locomotion.
            var atkCombo = sm.AddState("Attack_SwordCombo2", new Vector3(1100, 140, 0));
            atkCombo.motion = LoadClip(AnimationsRoot + "/Combat/Thrust Slash.fbx");
            atkCombo.speed  = 1.2f;
            var swordToCombo = atkSword.AddTransition(atkCombo);
            swordToCombo.AddCondition(AnimatorConditionMode.If, 0f, "Combo");
            swordToCombo.hasExitTime = true;
            swordToCombo.exitTime    = 0.4f;  // chain window opens after recovery starts
            swordToCombo.duration    = 0.05f;

            // Pistol Whip: close-range melee with the pistol equipped (alt to firing).
            var atkPistolMelee = sm.AddState("Attack_PistolMelee", new Vector3(850, 380, 0));
            atkPistolMelee.motion = LoadClip(AnimationsRoot + "/Combat/Pistol Whip.fbx");
            atkPistolMelee.speed  = 1.3f;
            // Triggered explicitly via HeavyAttack while WeaponType==3 (Pistol).
            AddTriggerTransition(locoState, atkPistolMelee, "HeavyAttack", ("WeaponType", AnimatorConditionMode.Equals, 3));

            AddExitTimeTransition(atkUnarmed,     locoState, 0.85f);
            AddExitTimeTransition(atkSword,       locoState, 0.85f);
            AddExitTimeTransition(atkGreat,       locoState, 0.85f);
            AddExitTimeTransition(atkGun,         locoState, 0.85f);
            AddExitTimeTransition(atkCombo,       locoState, 0.85f);
            AddExitTimeTransition(atkPistolMelee, locoState, 0.85f);

            // Block (loops while bool is held + Sword+Shield equipped)
            var blockState = sm.AddState("Block", new Vector3(550, 380, 0));
            blockState.motion = LoadClip(AnimationsRoot + "/Combat/Sword And Shield Block Idle.fbx");

            var toBlock = locoState.AddTransition(blockState);
            toBlock.AddCondition(AnimatorConditionMode.If,     0f, "Block");
            toBlock.AddCondition(AnimatorConditionMode.Equals, 1f, "WeaponType");
            toBlock.hasExitTime = false;
            toBlock.duration    = 0.1f;

            var fromBlock = blockState.AddTransition(locoState);
            fromBlock.AddCondition(AnimatorConditionMode.IfNot, 0f, "Block");
            fromBlock.hasExitTime = false;
            fromBlock.duration    = 0.1f;

            // Reload (split between Pistol and Rifle by WeaponType)
            var reloadPistol = sm.AddState("Reload_Pistol", new Vector3(850, 380, 0));
            reloadPistol.motion = LoadClip(AnimationsRoot + "/Combat/Reloading.fbx");
            var reloadRifle  = sm.AddState("Reload_Rifle",  new Vector3(850, 460, 0));
            reloadRifle.motion  = LoadClip(AnimationsRoot + "/Combat/Reloading-Rifle.fbx");

            AddTriggerTransition(locoState, reloadPistol, "Reload", ("WeaponType", AnimatorConditionMode.Equals, 3));
            AddTriggerTransition(locoState, reloadRifle,  "Reload", ("WeaponType", AnimatorConditionMode.Equals, 4));
            AddExitTimeTransition(reloadPistol, locoState, 0.95f);
            AddExitTimeTransition(reloadRifle,  locoState, 0.95f);

            // Parry (only meaningful when blocking with Sword + Shield).
            // Re-uses the in-place "Sword And Shield Impact" clip at slightly faster
            // playback so it reads as a sharp deflect, then drops back to Block.
            var parryState = sm.AddState("Parry", new Vector3(800, 280, 0));
            parryState.motion = LoadClip(AnimationsRoot + "/Combat/Sword And Shield Impact.fbx");
            parryState.speed  = 1.6f;
            AddTriggerTransition(blockState, parryState, "Parry");
            AddExitTimeTransition(parryState, blockState, 0.85f);

            // Hit reactions (Any State → HitReact / BigHit). Triggered from
            // PlayerStats.OnDamaged via the bridge. Played fast so they punctuate
            // hits without freezing the player out of control.
            var hitState = sm.AddState("HitReact", new Vector3(200, 380, 0));
            hitState.motion = LoadClip(AnimationsRoot + "/Reactions/Hit Reaction.fbx");
            hitState.speed  = 2.0f;
            var bigHitState = sm.AddState("BigHit", new Vector3(200, 460, 0));
            bigHitState.motion = LoadClip(AnimationsRoot + "/Reactions/Big Hit.fbx");
            bigHitState.speed  = 1.6f;

            AddAnyStateTrigger(sm, hitState,    "Hit");
            AddAnyStateTrigger(sm, bigHitState, "BigHit");
            AddExitTimeTransition(hitState,    locoState, 0.85f);
            AddExitTimeTransition(bigHitState, locoState, 0.85f);

            // Block-broken react: heavy hit absorbed → flinch instead of staying frozen.
            var blockReactState = sm.AddState("BlockReact", new Vector3(800, 280, 0));
            blockReactState.motion = LoadClip(AnimationsRoot + "/Combat/Standing Block React Large.fbx");
            blockReactState.speed  = 1.8f;
            AddTriggerTransition(blockState, blockReactState, "BlockBroken");
            AddExitTimeTransition(blockReactState, blockState, 0.6f);

            // Hit from behind: directional react variant. Same layer as HitReact
            // — bridge picks which to fire based on attacker direction.
            var hitBackState = sm.AddState("HitBack", new Vector3(200, 380, 0));
            hitBackState.motion = LoadClip(AnimationsRoot + "/Reactions/Standing React Small From Back.fbx");
            hitBackState.speed  = 2.0f;
            AddAnyStateTrigger(sm, hitBackState, "HitBack");
            AddExitTimeTransition(hitBackState, locoState, 0.6f);

            // Slide: held while PlayerMovement._isSliding == true. Idle Crouching
            // is a held pose so this loops naturally (configured in menu 5).
            var slideState = sm.AddState("Slide", new Vector3(550, 280, 0));
            slideState.motion = LoadClip(AnimationsRoot + "/Idle/Idle Crouching.fbx");
            var toSlide = locoState.AddTransition(slideState);
            toSlide.AddCondition(AnimatorConditionMode.If, 0f, "Slide");
            toSlide.hasExitTime = false;
            toSlide.duration    = 0.05f;
            var fromSlide = slideState.AddTransition(locoState);
            fromSlide.AddCondition(AnimatorConditionMode.IfNot, 0f, "Slide");
            fromSlide.hasExitTime = false;
            fromSlide.duration    = 0.1f;

            // Equip: plays once when WeaponEquipped fires (bridge sends Equip trigger).
            var equipState = sm.AddState("Equip", new Vector3(550, 480, 0));
            equipState.motion = LoadClip(AnimationsRoot + "/Combat/Unarmed Equip Underarm.fbx");
            equipState.speed  = 1.4f;
            AddAnyStateTrigger(sm, equipState, "Equip");
            AddExitTimeTransition(equipState, locoState, 0.85f);

            // Death (Any State → Death). Plays once and stays — no return transition.
            var deathState = sm.AddState("Death", new Vector3(200, 540, 0));
            deathState.motion = LoadClip(AnimationsRoot + "/Death/Dying.fbx");
            AddAnyStateTrigger(sm, deathState, "Die");

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            Debug.Log($"[Dungeon Blade] Created {ControllerPath}. Open it to verify clip assignments and transitions.");
        }

        static AnimationClip LoadClip(string fbxPath)
        {
            var clip = AssetDatabase.LoadAllAssetsAtPath(fbxPath)
                .OfType<AnimationClip>()
                .FirstOrDefault(c => !c.name.StartsWith("__preview"));
            if (clip == null) Debug.LogWarning($"[Dungeon Blade] No AnimationClip found in {fbxPath}");
            return clip;
        }

        static void AddTriggerTransition(AnimatorState from, AnimatorState to, string trigger,
            (string param, AnimatorConditionMode mode, float threshold) extra = default)
        {
            var t = from.AddTransition(to);
            t.AddCondition(AnimatorConditionMode.If, 0f, trigger);
            if (!string.IsNullOrEmpty(extra.param))
                t.AddCondition(extra.mode, extra.threshold, extra.param);
            t.hasExitTime = false;
            t.duration    = 0.08f;
        }

        static void AddExitTimeTransition(AnimatorState from, AnimatorState to, float exitTime)
        {
            var t = from.AddTransition(to);
            t.hasExitTime = true;
            t.exitTime    = exitTime;
            t.duration    = 0.15f;
        }

        static void AddLandTransition(AnimatorState from, AnimatorState to)
        {
            // No exit time — the moment Grounded flips back to true, we snap
            // straight into the Land state. The previous 0.7 exit time forced
            // the player to watch ~70% of the jump animation play out *after*
            // their feet hit the ground, which read as a heavy landing delay.
            var t = from.AddTransition(to);
            t.AddCondition(AnimatorConditionMode.If, 0f, "Grounded");
            t.hasExitTime = false;
            t.duration    = 0.08f;
        }

        static void AddAnyStateTrigger(AnimatorStateMachine sm, AnimatorState to, string trigger)
        {
            var t = sm.AddAnyStateTransition(to);
            t.AddCondition(AnimatorConditionMode.If, 0f, trigger);
            t.hasExitTime          = false;
            t.duration             = 0.1f;
            t.canTransitionToSelf  = false;
        }

        // ─────────────────────────────────────────────────────────────────────
        // 3. WEAPON ATTACH BONES
        //    Exposes hand/spine/head bones on every character so weapons can be
        //    parented in code without `transform.Find` walking the rig each time.
        // ─────────────────────────────────────────────────────────────────────
        [MenuItem("Tools/Dungeon Blade/Player/3. Expose Weapon Attach Bones")]
        public static void ExposeWeaponAttachBones()
        {
            string[] desired = { "RightHand", "LeftHand", "Spine2", "Head" };

            int updated = 0;
            foreach (var path in EnumerateModels(ModelsRoot))
            {
                var imp = AssetImporter.GetAtPath(path) as ModelImporter;
                if (imp == null) continue;

                var rootGo = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (rootGo == null) continue;

                var found = new List<string>();
                foreach (var fragment in desired)
                {
                    var t = FindByNameContains(rootGo.transform, fragment);
                    if (t != null) found.Add(GetTransformPath(t, rootGo.transform));
                }

                if (found.Count == 0)
                {
                    Debug.LogWarning($"[Dungeon Blade] No matching attach bones on {path}.");
                    continue;
                }

                imp.extraExposedTransformPaths = found.ToArray();
                imp.SaveAndReimport();
                updated++;
            }

            Debug.Log($"[Dungeon Blade] Exposed weapon attach bones on {updated} model(s).");
        }

        static Transform FindByNameContains(Transform root, string fragment)
        {
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
            {
                if (t.name.IndexOf(fragment, StringComparison.OrdinalIgnoreCase) >= 0)
                    return t;
            }
            return null;
        }

        static string GetTransformPath(Transform t, Transform root)
        {
            var stack = new Stack<string>();
            while (t != null && t != root)
            {
                stack.Push(t.name);
                t = t.parent;
            }
            return string.Join("/", stack);
        }

        // ─────────────────────────────────────────────────────────────────────
        // 4. BOOTSTRAP PLAYER PREFAB
        //    Stamps Varion + CharacterController + Player scripts + Animator
        //    (with PlayerAnimator) + camera rig into Player.prefab.
        // ─────────────────────────────────────────────────────────────────────
        const string PrefabPath = PlayerRoot + "/Prefabs/Player.prefab";

        [MenuItem("Tools/Dungeon Blade/Player/4. Bootstrap Player Prefab")]
        public static void BootstrapPlayerPrefab()
        {
            var modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(SourceModelPath);
            if (modelAsset == null)
            {
                Debug.LogError($"[Dungeon Blade] Source character not found at {SourceModelPath}");
                return;
            }

            var animController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ControllerPath);
            if (animController == null)
            {
                Debug.LogError($"[Dungeon Blade] {ControllerPath} not found. Run step 2 first.");
                return;
            }

            if (File.Exists(PrefabPath))
            {
                if (!EditorUtility.DisplayDialog(
                    "Overwrite Player.prefab?",
                    $"{PrefabPath} already exists.\n\nOverwrite?",
                    "Overwrite", "Cancel"))
                    return;
                AssetDatabase.DeleteAsset(PrefabPath);
            }

            Directory.CreateDirectory(Path.GetDirectoryName(PrefabPath));

            // Instantiate the model in the scene as a working copy. We unpack the
            // prefab connection so the Player prefab is standalone (not nested).
            var root = (GameObject)PrefabUtility.InstantiatePrefab(modelAsset);
            PrefabUtility.UnpackPrefabInstance(root, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            root.name = "Player";
            root.transform.position = Vector3.zero;
            root.transform.rotation = Quaternion.identity;

            var cc = root.AddComponent<CharacterController>();
            cc.height    = 1.8f;
            cc.radius    = 0.35f;
            cc.center    = new Vector3(0f, 0.9f, 0f);
            cc.skinWidth = 0.04f;

            root.AddComponent<PlayerStats>();
            var movement = root.AddComponent<PlayerMovement>();
            root.AddComponent<PlayerCombat>();
            root.AddComponent<PlayerAnimatorBridge>();

            var animator = root.GetComponent<Animator>() ?? root.AddComponent<Animator>();
            animator.runtimeAnimatorController = animController;
            animator.applyRootMotion = false;
            if (animator.avatar == null)
            {
                var avatar = AssetDatabase.LoadAllAssetsAtPath(SourceModelPath)
                    .OfType<Avatar>().FirstOrDefault();
                if (avatar != null) animator.avatar = avatar;
            }

            // Camera rig: empty pivot at head height; PlayerMovement rotates this for pitch.
            var cameraRig = new GameObject("CameraRig");
            cameraRig.transform.SetParent(root.transform, false);
            cameraRig.transform.localPosition = new Vector3(0f, 1.6f, 0f);

            var cameraGo = new GameObject("PlayerCamera");
            cameraGo.transform.SetParent(cameraRig.transform, false);
            // Third-person over-the-shoulder offset: slight right, slightly above
            // the head pivot, three meters behind. Tweak in the prefab for taste.
            cameraGo.transform.localPosition = new Vector3(0.4f, 0.4f, -3f);
            var cam = cameraGo.AddComponent<Camera>();
            cam.nearClipPlane = 0.05f;
            cam.fieldOfView   = 70f;
            cameraGo.AddComponent<AudioListener>();

            // Smooth follow detaches the camera from the rig at runtime and
            // chases it with a critically-damped lerp, so CharacterController
            // ground-snap and jump landings don't read as shake. Target is the
            // CameraRig — offset is captured from the camera's current local
            // pose at Start (so the (0.4, 0.4, -3) over-the-shoulder framing
            // is preserved automatically).
            var smooth = cameraGo.AddComponent<CameraSmoothFollow>();
            var smoothSo = new SerializedObject(smooth);
            var targetProp = smoothSo.FindProperty("target");
            if (targetProp != null)
            {
                targetProp.objectReferenceValue = cameraRig.transform;
                smoothSo.ApplyModifiedProperties();
            }

            // Camera tag is intentionally left untagged. To use as primary view, set
            // tag = "MainCamera" and remove or disable the scene's default camera.

            // PlayerMovement.cameraRig is private [SerializeField], so route via SerializedObject.
            var so = new SerializedObject(movement);
            var prop = so.FindProperty("cameraRig");
            if (prop != null)
            {
                prop.objectReferenceValue = cameraRig.transform;
                so.ApplyModifiedProperties();
            }

            // Rebind the SkinnedMeshRenderer materials directly to the
            // extracted .mat files. Without this, an unpacked prefab can hold
            // a stale reference to the FBX's embedded sub-asset material —
            // which renders as solid magenta in URP after extraction.
            RebindRendererMaterialsToExternal(root);

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            UnityEngine.Object.DestroyImmediate(root);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = prefab;

            Debug.Log($"[Dungeon Blade] Created {PrefabPath}. Drag it into a scene; tag PlayerCamera as MainCamera (and disable the scene's default camera) to see through it.");
        }

        // ─────────────────────────────────────────────────────────────────────
        // 5. CONFIGURE ANIMATION CLIP SETTINGS
        //    Per ANIMATION_SETUP.md §2:
        //      - Idle / Run / Block clips → Loop Time + Loop Pose
        //      - Jump / Dash / Dodge / Roll one-shot motion clips →
        //          Bake Into Pose: Position Y + Position XZ (locks root motion
        //          translation so the animation can't double up with the
        //          script-driven CharacterController.Move call)
        //      - Attacks / Reloads / Death → leave as plain one-shot (no loop,
        //          no bake) so animation events can drive hit frames cleanly.
        // ─────────────────────────────────────────────────────────────────────
        // "Walk" added for Walking Backwards / Left Strafe Walking / Right Strafe Walking.
        // "Idle" already catches Idle Crouching. "Run" catches Running Tired.
        static readonly string[] LoopKeywords = { "Idle", "Run", "Block", "Walk" };
        // Names containing any of these are NEVER looped, even if they also match
        // a loop keyword. Example: "Standing Block React Large" matches "Block"
        // but is a one-shot react — exclude it via "React".
        static readonly string[] LoopExclusions = { "React", "Impact" };
        // Bake out root translation on motion-bearing one-shots. Adds:
        //   Landing → Hard Landing, Landing
        //   Hit / Big → Big Hit, Hit Reaction (knockbacks)
        //   Dying / Death → Dying, Falling Back Death
        static readonly string[] BakeMotionKeywords = {
            "Jump", "Roll", "Dodging", "Dive",
            "Landing", "Hit", "Big", "Dying", "Death",
        };

        [MenuItem("Tools/Dungeon Blade/Player/5. Configure Animation Clip Settings")]
        public static void ConfigureAnimationClipSettings()
        {
            int updated = 0;
            foreach (var path in EnumerateModels(AnimationsRoot))
            {
                var imp = AssetImporter.GetAtPath(path) as ModelImporter;
                if (imp == null) continue;

                string fileName = Path.GetFileNameWithoutExtension(path);
                bool isExcludedFromLoop = LoopExclusions.Any(kw =>
                    fileName.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0);
                bool shouldLoop = !isExcludedFromLoop && LoopKeywords.Any(kw =>
                    fileName.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0);
                bool shouldBakeMotion = !shouldLoop && BakeMotionKeywords.Any(kw =>
                    fileName.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0);

                // defaultClipAnimations gives a fresh copy of the auto-generated
                // clip settings. We mutate the copy and write it back via clipAnimations
                // so the importer respects our overrides on subsequent reimports.
                var clips = imp.defaultClipAnimations;
                if (clips == null || clips.Length == 0) continue;

                bool changed = false;
                foreach (var clip in clips)
                {
                    if (clip.loopTime != shouldLoop) { clip.loopTime = shouldLoop; changed = true; }
                    if (clip.loopPose != shouldLoop) { clip.loopPose = shouldLoop; changed = true; }
                    if (clip.lockRootHeightY != shouldBakeMotion)    { clip.lockRootHeightY    = shouldBakeMotion; changed = true; }
                    if (clip.lockRootPositionXZ != shouldBakeMotion) { clip.lockRootPositionXZ = shouldBakeMotion; changed = true; }
                }

                if (!changed) continue;

                imp.clipAnimations = clips;
                imp.SaveAndReimport();
                updated++;
                string category = shouldLoop ? "loop      "
                                : shouldBakeMotion ? "bake-pose "
                                : "one-shot  ";
                Debug.Log($"[Dungeon Blade]   {category}→  {fileName}");
            }

            Debug.Log($"[Dungeon Blade] Configured clip settings on {updated} animation file(s).");
        }

        // ─────────────────────────────────────────────────────────────────────
        // 6. EXTRACT CHARACTER MATERIALS + TEXTURES
        //    Tripo / Mixamo characters embed textures and materials inside the
        //    FBX. With "External materials (Legacy)" import, Unity uses default
        //    white materials, which is why characters render colorless.
        //    This walks every model and:
        //      1. Extracts embedded textures into ../Materials/Textures/
        //      2. Extracts embedded materials into ../Materials/
        //      3. Re-imports the FBX so it remaps to the extracted materials.
        // ─────────────────────────────────────────────────────────────────────
        [MenuItem("Tools/Dungeon Blade/Player/6. Extract Character Materials + Textures")]
        public static void ExtractCharacterMaterials()
        {
            string materialsRoot = ModelsRoot + "/Materials";
            string texturesRoot  = materialsRoot + "/Textures";
            Directory.CreateDirectory(materialsRoot);
            Directory.CreateDirectory(texturesRoot);
            AssetDatabase.Refresh();

            int totalTextures  = 0;
            int totalMaterials = 0;

            foreach (var path in EnumerateModels(ModelsRoot))
            {
                var importer = AssetImporter.GetAtPath(path) as ModelImporter;
                if (importer == null) continue;

                // Step 1: extract textures embedded in the FBX (Tripo bakes them in).
                if (importer.ExtractTextures(texturesRoot))
                {
                    totalTextures++;
                    Debug.Log($"[Dungeon Blade]   textures →  {Path.GetFileName(path)}");
                }

                // Step 2: extract embedded materials.
                var subAssets = AssetDatabase.LoadAllAssetsAtPath(path);
                foreach (var sub in subAssets)
                {
                    if (sub is Material mat)
                    {
                        string matPath = $"{materialsRoot}/{mat.name}.mat";
                        if (File.Exists(matPath)) continue; // already extracted from another model
                        string err = AssetDatabase.ExtractAsset(mat, matPath);
                        if (string.IsNullOrEmpty(err))
                        {
                            totalMaterials++;
                            Debug.Log($"[Dungeon Blade]   material →  {mat.name}.mat");
                        }
                        else
                        {
                            Debug.LogWarning($"[Dungeon Blade] Failed to extract {mat.name}: {err}");
                        }
                    }
                }
            }

            // Step 3: AssetDatabase needs a refresh + per-model reimport so the
            // FBXs pick up the extracted materials via their remap table.
            AssetDatabase.Refresh();
            foreach (var path in EnumerateModels(ModelsRoot))
            {
                var importer = AssetImporter.GetAtPath(path) as ModelImporter;
                if (importer == null) continue;
                importer.SearchAndRemapMaterials(
                    ModelImporterMaterialName.BasedOnMaterialName,
                    ModelImporterMaterialSearch.Local);
                importer.SaveAndReimport();
            }
            AssetDatabase.SaveAssets();

            Debug.Log($"[Dungeon Blade] Extracted {totalMaterials} material(s) and ran texture extraction on {totalTextures} model(s). " +
                      $"Materials → {materialsRoot}, Textures → {texturesRoot}.");
        }

        // ─────────────────────────────────────────────────────────────────────
        // 7. CONVERT EXTRACTED MATERIALS TO URP/Lit
        //    Tripo bakes materials with the legacy Standard shader. URP renders
        //    Standard-shader materials as magenta — that's why characters look
        //    bright pink. This converts each extracted .mat in the Materials
        //    folder to URP/Lit and remaps the key properties (MainTex → BaseMap,
        //    Color → BaseColor, BumpMap stays as BumpMap on URP/Lit).
        // ─────────────────────────────────────────────────────────────────────
        [MenuItem("Tools/Dungeon Blade/Player/7. Convert Materials to URP")]
        public static void ConvertMaterialsToURP()
        {
            string materialsRoot = ModelsRoot + "/Materials";
            if (!Directory.Exists(materialsRoot))
            {
                Debug.LogError($"[Dungeon Blade] Materials folder not found at {materialsRoot}. Run menu 6 first.");
                return;
            }

            var urpLit = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLit == null)
            {
                Debug.LogError("[Dungeon Blade] URP/Lit shader not found. Is URP installed in this project?");
                return;
            }

            var matGuids = AssetDatabase.FindAssets("t:Material", new[] { materialsRoot });
            int converted = 0;
            foreach (var guid in matGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat == null) continue;
                if (mat.shader == urpLit) continue; // already converted

                // Snapshot Standard-shader properties before swapping shader.
                Texture mainTex   = mat.HasProperty("_MainTex") ? mat.GetTexture("_MainTex")  : null;
                Color   color     = mat.HasProperty("_Color")   ? mat.GetColor("_Color")      : Color.white;
                Texture normalTex = mat.HasProperty("_BumpMap") ? mat.GetTexture("_BumpMap")  : null;
                Texture metalTex  = mat.HasProperty("_MetallicGlossMap") ? mat.GetTexture("_MetallicGlossMap") : null;
                float   smoothness = mat.HasProperty("_Glossiness") ? mat.GetFloat("_Glossiness") : 0.5f;

                mat.shader = urpLit;

                // URP/Lit property names: _BaseMap, _BaseColor, _BumpMap (same),
                // _MetallicGlossMap (same), _Smoothness.
                if (mat.HasProperty("_BaseMap")   && mainTex   != null) mat.SetTexture("_BaseMap",   mainTex);
                if (mat.HasProperty("_BaseColor"))                       mat.SetColor("_BaseColor",   color);
                if (mat.HasProperty("_BumpMap")   && normalTex != null) mat.SetTexture("_BumpMap",   normalTex);
                if (mat.HasProperty("_MetallicGlossMap") && metalTex != null) mat.SetTexture("_MetallicGlossMap", metalTex);
                if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);

                EditorUtility.SetDirty(mat);
                converted++;
                Debug.Log($"[Dungeon Blade]   URP/Lit  →  {Path.GetFileName(path)}");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[Dungeon Blade] Converted {converted} material(s) to URP/Lit.");
        }

        // ─────────────────────────────────────────────────────────────────────
        // 8. RE-EXTRACT MATERIALS WITH TEXTURES LINKED (fix for menu 6 bug)
        //    Menu 6 extracted materials before the FBX was re-imported with
        //    extracted textures, so the .mat files lost their texture refs and
        //    rendered empty/pink. This wipes the bad .mat files, clears the
        //    FBX's external-object remap, re-extracts textures, REIMPORTS so
        //    the auto-generated internal materials pick up the extracted
        //    textures, then re-extracts those proper materials. Finally
        //    converts everything to URP/Lit.
        // ─────────────────────────────────────────────────────────────────────
        [MenuItem("Tools/Dungeon Blade/Player/8. Re-extract Materials (Fix Textures)")]
        public static void ReExtractMaterialsFixTextures()
        {
            string materialsRoot = ModelsRoot + "/Materials";
            string texturesRoot  = materialsRoot + "/Textures";
            Directory.CreateDirectory(materialsRoot);
            Directory.CreateDirectory(texturesRoot);

            // Step 1: delete the stale .mat files (textures are fine — keep them).
            int deleted = 0;
            var existingMats = AssetDatabase.FindAssets("t:Material", new[] { materialsRoot });
            foreach (var guid in existingMats)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.StartsWith(texturesRoot, StringComparison.Ordinal)) continue;
                if (AssetDatabase.DeleteAsset(path)) deleted++;
            }
            Debug.Log($"[Dungeon Blade]   deleted {deleted} stale .mat file(s).");

            // Step 2: clear externalObjects remap on every FBX so they fall back
            // to internal auto-generated materials. After reimport, those
            // internal materials will reference the extracted textures.
            foreach (var path in EnumerateModels(ModelsRoot))
            {
                var imp = AssetImporter.GetAtPath(path) as ModelImporter;
                if (imp == null) continue;
                var map = new Dictionary<AssetImporter.SourceAssetIdentifier, UnityEngine.Object>(imp.GetExternalObjectMap());
                foreach (var kv in map) imp.RemoveRemap(kv.Key);
                imp.SaveAndReimport();
            }
            AssetDatabase.Refresh();

            // Step 3: extract textures (idempotent), reimport, then extract
            // materials in that order so the .mat files inherit valid refs.
            int matsExtracted = 0;
            foreach (var path in EnumerateModels(ModelsRoot))
            {
                var imp = AssetImporter.GetAtPath(path) as ModelImporter;
                if (imp == null) continue;

                imp.ExtractTextures(texturesRoot);
                imp.SaveAndReimport();   // CRITICAL: refresh internal materials so they reference extracted textures.

                var subAssets = AssetDatabase.LoadAllAssetsAtPath(path);
                foreach (var sub in subAssets)
                {
                    if (sub is Material mat)
                    {
                        string matPath = $"{materialsRoot}/{mat.name}.mat";
                        if (File.Exists(matPath)) continue;
                        string err = AssetDatabase.ExtractAsset(mat, matPath);
                        if (string.IsNullOrEmpty(err))
                        {
                            matsExtracted++;
                            Debug.Log($"[Dungeon Blade]   re-extracted with textures →  {mat.name}.mat");
                        }
                    }
                }

                imp.SearchAndRemapMaterials(
                    ModelImporterMaterialName.BasedOnMaterialName,
                    ModelImporterMaterialSearch.Local);
                imp.SaveAndReimport();
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Step 4: convert the freshly-extracted materials to URP/Lit while
            // preserving the texture refs we just gained.
            var urpLit = Shader.Find("Universal Render Pipeline/Lit");
            int converted = 0;
            if (urpLit == null)
            {
                Debug.LogWarning("[Dungeon Blade] URP/Lit shader not found — extracted materials will stay on Standard shader and render pink. Install URP or run menu 7 manually.");
            }
            else
            {
                var matGuids = AssetDatabase.FindAssets("t:Material", new[] { materialsRoot });
                foreach (var guid in matGuids)
                {
                    var p = AssetDatabase.GUIDToAssetPath(guid);
                    if (p.StartsWith(texturesRoot, StringComparison.Ordinal)) continue;
                    var mat = AssetDatabase.LoadAssetAtPath<Material>(p);
                    if (mat == null || mat.shader == urpLit) continue;

                    Texture mainTex = mat.HasProperty("_MainTex") ? mat.GetTexture("_MainTex") : null;
                    Color   color   = mat.HasProperty("_Color")   ? mat.GetColor("_Color")     : Color.white;
                    Texture normal  = mat.HasProperty("_BumpMap") ? mat.GetTexture("_BumpMap") : null;

                    mat.shader = urpLit;
                    if (mat.HasProperty("_BaseMap")   && mainTex != null) mat.SetTexture("_BaseMap",  mainTex);
                    if (mat.HasProperty("_BaseColor"))                     mat.SetColor("_BaseColor",  color);
                    if (mat.HasProperty("_BumpMap")   && normal  != null) mat.SetTexture("_BumpMap",  normal);

                    EditorUtility.SetDirty(mat);
                    converted++;
                }
                AssetDatabase.SaveAssets();
            }

            Debug.Log($"[Dungeon Blade] Re-extraction complete. {matsExtracted} material(s) re-extracted, {converted} converted to URP/Lit. Check the scene — characters should now show their proper colors.");
        }

        // ─────────────────────────────────────────────────────────────────────
        // 9. ATTACH CAMERA SMOOTH FOLLOW TO EXISTING PREFAB
        //    Adds CameraSmoothFollow to the PlayerCamera (if missing) and wires
        //    its `target` to the CameraRig. Use this to fix camera shake on a
        //    prefab built before menu 4 added the smooth-follow wiring,
        //    without having to rebuild the whole prefab from scratch.
        // ─────────────────────────────────────────────────────────────────────
        [MenuItem("Tools/Dungeon Blade/Player/9. Attach Camera Smooth Follow")]
        public static void AttachCameraSmoothFollow()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null)
            {
                Debug.LogError($"[Dungeon Blade] {PrefabPath} not found. Run menu 4 first.");
                return;
            }

            // Open the prefab contents for editing — modifying the asset
            // directly via LoadAssetAtPath then SaveAsPrefabAsset is the
            // supported way to patch an existing prefab in-place.
            string tmpPath = PrefabPath;
            var root = PrefabUtility.LoadPrefabContents(tmpPath);
            try
            {
                var camRig = FindChildByName(root.transform, "CameraRig");
                var camGo  = FindChildByName(root.transform, "PlayerCamera");
                if (camRig == null || camGo == null)
                {
                    Debug.LogError("[Dungeon Blade] PlayerCamera or CameraRig missing from prefab — re-run menu 4.");
                    return;
                }

                var smooth = camGo.GetComponent<CameraSmoothFollow>();
                if (smooth == null) smooth = camGo.gameObject.AddComponent<CameraSmoothFollow>();

                var so = new SerializedObject(smooth);
                var targetProp = so.FindProperty("target");
                if (targetProp != null)
                {
                    targetProp.objectReferenceValue = camRig;
                    so.ApplyModifiedProperties();
                }

                PrefabUtility.SaveAsPrefabAsset(root, tmpPath);
                Debug.Log($"[Dungeon Blade] CameraSmoothFollow attached to PlayerCamera (target = CameraRig). Camera shake on jumps/landings should be gone.");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        static Transform FindChildByName(Transform root, string name)
        {
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == name) return t;
            }
            return null;
        }

        // ─────────────────────────────────────────────────────────────────────
        // 10. FIX RENDERER MATERIALS (pink character fix)
        //    When the prefab is built before menu 6/8 extracts the embedded
        //    FBX materials, the SkinnedMeshRenderer captures a reference to
        //    the now-stale embedded sub-asset. After extraction that reference
        //    can no longer resolve to a real material, and URP renders the
        //    mesh as solid magenta.
        //    This walks every Renderer in the Player prefab, looks at each
        //    slot's current material (or its name if null), finds the matching
        //    .mat file in Models/Materials/ by name, and assigns it.
        // ─────────────────────────────────────────────────────────────────────
        [MenuItem("Tools/Dungeon Blade/Player/10. Fix Renderer Materials (Pink Character)")]
        public static void FixRendererMaterials()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null)
            {
                Debug.LogError($"[Dungeon Blade] {PrefabPath} not found. Run menu 4 first.");
                return;
            }

            var root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                int fixedCount = RebindRendererMaterialsToExternal(root);
                if (fixedCount == 0)
                {
                    Debug.LogWarning("[Dungeon Blade] No renderer material slots needed fixing — prefab already pointed at the external .mat files.");
                }
                else
                {
                    PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                    Debug.Log($"[Dungeon Blade] Rebound {fixedCount} renderer material slot(s) to external .mat files. Press Play — character should now show its texture instead of magenta.");
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // 11. NUCLEAR FIX: REBIND + RESET KEYWORDS + PAD SUBMESHES
        //    Menu 10's "no slots needed fixing" message means Unity *resolved*
        //    the FBX-embedded reference to the external .mat at editor time —
        //    so the renderer *appears* correct. But the prefab YAML still
        //    holds the embedded fileID, and any of these can still cause a
        //    magenta render:
        //      - mesh subMeshCount > materials.Length  (extra submeshes have
        //        no material → URP draws them magenta)
        //      - material has stale URP keywords from an old shader version
        //      - material slot points at the FBX sub-asset (not the .mat) and
        //        the remap silently fails at runtime
        //    This menu force-rebinds every slot directly to the external .mat,
        //    pads the array to match subMeshCount, and re-assigns the URP/Lit
        //    shader on each .mat to refresh keywords. Loud diagnostics so we
        //    can actually see what was wrong.
        // ─────────────────────────────────────────────────────────────────────
        [MenuItem("Tools/Dungeon Blade/Player/11. Diagnose + Force Fix Materials")]
        public static void DiagnoseAndForceFixMaterials()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null)
            {
                Debug.LogError($"[Dungeon Blade] {PrefabPath} not found. Run menu 4 first.");
                return;
            }

            // Step 1: refresh URP keywords on every external .mat by re-assigning
            // its shader. Re-assignment runs URP's ShaderGUI keyword setup even
            // when the shader was already URP/Lit. Materials saved by older URP
            // versions sometimes carry a stale keyword set that causes opaque
            // surfaces to render magenta.
            string materialsRoot = ModelsRoot + "/Materials";
            var urpLit = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLit == null)
            {
                Debug.LogError("[Dungeon Blade] URP/Lit shader not found. Is URP installed?");
                return;
            }

            int refreshed = 0;
            foreach (var guid in AssetDatabase.FindAssets("t:Material", new[] { materialsRoot }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.IndexOf("/Textures/", StringComparison.OrdinalIgnoreCase) >= 0) continue;
                var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat == null) continue;

                Texture baseMap = mat.HasProperty("_BaseMap") ? mat.GetTexture("_BaseMap") : null;
                Color   baseCol = mat.HasProperty("_BaseColor") ? mat.GetColor("_BaseColor") : Color.white;

                mat.shader = urpLit;
                if (baseMap != null && mat.HasProperty("_BaseMap"))   mat.SetTexture("_BaseMap", baseMap);
                if (mat.HasProperty("_BaseColor"))                     mat.SetColor("_BaseColor", baseCol);
                // Sensible defaults that show texture detail.
                if (mat.HasProperty("_Metallic"))   mat.SetFloat("_Metallic", 0f);
                if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.35f);

                EditorUtility.SetDirty(mat);
                refreshed++;
                string baseName = baseMap != null ? baseMap.name : "<none>";
                Debug.Log($"[Dungeon Blade]   refreshed  {Path.GetFileName(path)}  baseMap={baseName}");
            }
            AssetDatabase.SaveAssets();

            // Step 2: force-rebind the prefab's renderers, padding to subMeshCount.
            var root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                var matLookup = BuildExternalMatLookup();
                int totalRebound = 0, totalPadded = 0;

                foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
                {
                    Mesh mesh = ResolveMesh(renderer);
                    int subMeshCount = mesh != null ? mesh.subMeshCount : 1;

                    var current = renderer.sharedMaterials;
                    Debug.Log($"[Dungeon Blade]   {renderer.name}: mesh={(mesh != null ? mesh.name : "<null>")} subMeshes={subMeshCount} materialSlots={current.Length}");

                    // Pick the canonical external .mat: prefer one matching the
                    // current slot's name; otherwise fall back to the first .mat
                    // that matches this character (most player FBXs are single-mat).
                    Material canonical = null;
                    for (int i = 0; i < current.Length; i++)
                    {
                        if (current[i] == null) continue;
                        if (matLookup.TryGetValue(current[i].name, out var ext)) { canonical = ext; break; }
                    }
                    if (canonical == null && matLookup.Count == 1) canonical = matLookup.Values.First();
                    if (canonical == null)
                    {
                        // Multi-character project, no name match — try matching by FBX name.
                        canonical = GuessMatForRenderer(renderer, matLookup);
                    }
                    if (canonical == null)
                    {
                        Debug.LogWarning($"[Dungeon Blade]     no canonical .mat found for {renderer.name} — leaving slots unchanged.");
                        continue;
                    }

                    var rebuilt = new Material[Mathf.Max(subMeshCount, current.Length)];
                    for (int i = 0; i < rebuilt.Length; i++)
                    {
                        Material existing = i < current.Length ? current[i] : null;
                        // Always overwrite — even if a slot already points at the
                        // right .mat, writing it back normalizes the prefab YAML
                        // away from the FBX-embedded fileID.
                        rebuilt[i] = (existing != null && matLookup.ContainsKey(existing.name))
                            ? matLookup[existing.name]
                            : canonical;
                    }

                    int rebound = 0, padded = 0;
                    for (int i = 0; i < rebuilt.Length; i++)
                    {
                        if (i >= current.Length) padded++;
                        else if (current[i] != rebuilt[i]) rebound++;
                    }
                    renderer.sharedMaterials = rebuilt;
                    totalRebound += rebound;
                    totalPadded  += padded;
                    Debug.Log($"[Dungeon Blade]     → rebound {rebound} slot(s), padded {padded} slot(s) with {canonical.name}");
                }

                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                Debug.Log($"[Dungeon Blade] DONE. Refreshed {refreshed} material(s), rebound {totalRebound} slot(s), padded {totalPadded} slot(s). If the character is still magenta, the FBX itself is missing UVs or has a sub-mesh that doesn't exist in the external .mat folder — tell me which character.");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        static Dictionary<string, Material> BuildExternalMatLookup()
        {
            string materialsRoot = ModelsRoot + "/Materials";
            var lookup = new Dictionary<string, Material>(StringComparer.OrdinalIgnoreCase);
            foreach (var guid in AssetDatabase.FindAssets("t:Material", new[] { materialsRoot }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.IndexOf("/Textures/", StringComparison.OrdinalIgnoreCase) >= 0) continue;
                var m = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (m != null) lookup[m.name] = m;
            }
            return lookup;
        }

        static Mesh ResolveMesh(Renderer renderer)
        {
            if (renderer is SkinnedMeshRenderer smr) return smr.sharedMesh;
            var mf = renderer.GetComponent<MeshFilter>();
            return mf != null ? mf.sharedMesh : null;
        }

        static Material GuessMatForRenderer(Renderer renderer, Dictionary<string, Material> lookup)
        {
            // Walk up parents to find the FBX root name (e.g. "Varion-T-Pose").
            // Cross-reference with each .mat's external-objects map to find the one
            // whose owning FBX matches.
            Transform t = renderer.transform;
            string rootName = null;
            while (t != null) { rootName = t.name; t = t.parent; }
            if (string.IsNullOrEmpty(rootName)) return null;
            // Best-effort: look for a tripo_node_* sibling and match its hex tail
            // to a tripo_mat_* entry in lookup.
            foreach (var sib in renderer.GetComponentsInChildren<Transform>(true))
            {
                if (!sib.name.StartsWith("tripo_node_", StringComparison.OrdinalIgnoreCase)) continue;
                string tail = sib.name.Substring("tripo_node_".Length);
                string matName = "tripo_mat_" + tail;
                if (lookup.TryGetValue(matName, out var m)) return m;
            }
            return null;
        }

        /// Walks every Renderer under <paramref name="root"/> and replaces any
        /// material slot whose ref is null OR whose ref lives inside an FBX
        /// (i.e. the embedded sub-asset that broke after extraction) with the
        /// matching .mat from Models/Materials/. Match is by material name.
        /// Returns the number of slots that were rebound.
        static int RebindRendererMaterialsToExternal(GameObject root)
        {
            string materialsRoot = ModelsRoot + "/Materials";
            var matLookup = new Dictionary<string, Material>(StringComparer.OrdinalIgnoreCase);
            foreach (var guid in AssetDatabase.FindAssets("t:Material", new[] { materialsRoot }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                // Skip the Textures sub-folder — only top-level .mat files.
                if (path.IndexOf("/Textures/", StringComparison.OrdinalIgnoreCase) >= 0) continue;
                var m = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (m != null) matLookup[m.name] = m;
            }

            if (matLookup.Count == 0)
            {
                Debug.LogWarning($"[Dungeon Blade] No external materials found in {materialsRoot}. Run menu 6 / 8 first.");
                return 0;
            }

            int rebound = 0;
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                // sharedMaterials returns a copy; mutate then assign back.
                var mats = renderer.sharedMaterials;
                bool changed = false;
                for (int i = 0; i < mats.Length; i++)
                {
                    var current = mats[i];
                    string assetPath = current != null ? AssetDatabase.GetAssetPath(current) : null;
                    bool fromFbx = assetPath != null && assetPath.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase);

                    // Only rebind if current slot is null or points inside an FBX.
                    // A slot already pointing at an external .mat is left alone.
                    if (current != null && !fromFbx) continue;

                    string lookupName = current != null ? current.name : null;
                    if (string.IsNullOrEmpty(lookupName))
                    {
                        // Fall back to the first material as a guess for prefabs
                        // with broken refs. Most player FBXs are single-material.
                        if (matLookup.Count == 1)
                        {
                            mats[i] = matLookup.Values.First();
                            changed = true;
                            rebound++;
                        }
                        continue;
                    }

                    if (matLookup.TryGetValue(lookupName, out var external))
                    {
                        mats[i] = external;
                        changed = true;
                        rebound++;
                        Debug.Log($"[Dungeon Blade]   {renderer.name}[{i}]: {lookupName} → external .mat");
                    }
                }
                if (changed) renderer.sharedMaterials = mats;
            }

            return rebound;
        }
    }
}
