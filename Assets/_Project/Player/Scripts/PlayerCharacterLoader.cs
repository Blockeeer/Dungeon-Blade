using System;
using System.Collections.Generic;
using DungeonBlade.Core;
using UnityEngine;

namespace DungeonBlade.Player
{
    // Reads the chosen hero id from SaveSystem.Profile.characterId and swaps the
    // visual mesh+skeleton on the Player prefab to the matching FBX, while
    // leaving Animator / PlayerMovement / PlayerCombat / CameraRig untouched.
    //
    // Works because all six character FBXs share the Mixamo humanoid avatar
    // (configured by PlayerAssetSetup.SetSharedAvatar), so the same
    // PlayerAnimator.controller drives any of them — only the bones+mesh under
    // the Player root need to be replaced.
    //
    // ExecutionOrder is forced negative so the bone swap completes before any
    // sibling script (e.g. HipsLock) tries to cache references to the bones
    // we're about to destroy.
    [DefaultExecutionOrder(-200)]
    [DisallowMultipleComponent]
    public class PlayerCharacterLoader : MonoBehaviour
    {
        [Serializable]
        public class CharacterMapping
        {
            [Tooltip("Lowercase id matching CharacterSelectController.Roster (kaelen, lyra, wayne, mira, varion, aurelia).")]
            public string id;

            [Tooltip("FBX prefab from Assets/_Project/Player/Models/. Must have the shared Mixamo avatar (use Tools ▸ Dungeon Blade ▸ Player ▸ 1. Set Shared Avatar).")]
            public GameObject modelPrefab;

            [Tooltip("Optional avatar override. Leave empty to use the avatar baked into the FBX prefab.")]
            public Avatar avatarOverride;
        }

        [Header("Character → Model")]
        [SerializeField] CharacterMapping[] mappings;

        [Tooltip("Used when no profile exists yet (fresh editor play from Lobby with no save).")]
        [SerializeField] string fallbackCharacterId = "lyra";

        [Header("Cleanup")]
        [Tooltip("Direct children matching any of these name prefixes are destroyed before the new model is spawned. Bones imported from Mixamo are prefixed 'mixamorig'.")]
        [SerializeField] string[] strippedChildPrefixes = { "mixamorig" };

        Animator _animator;

        void Awake()
        {
            _animator = GetComponent<Animator>();
            ApplyCharacter(ResolveCharacterId());
        }

        string ResolveCharacterId()
        {
            var gm = GameManager.Instance;
            if (gm != null && gm.SaveSystem != null && gm.SaveSystem.Profile != null
                && !string.IsNullOrEmpty(gm.SaveSystem.Profile.characterId))
                return gm.SaveSystem.Profile.characterId;

            var standalone = new SaveSystem();
            standalone.Load();
            if (standalone.ProfileLoaded
                && standalone.Profile != null
                && !string.IsNullOrEmpty(standalone.Profile.characterId))
                return standalone.Profile.characterId;

            return fallbackCharacterId;
        }

        public void ApplyCharacter(string id)
        {
            if (mappings == null || mappings.Length == 0)
            {
                Debug.LogWarning("[PlayerCharacterLoader] No character mappings configured.", this);
                return;
            }

            CharacterMapping match = null;
            for (int i = 0; i < mappings.Length; i++)
            {
                var m = mappings[i];
                if (m != null && !string.IsNullOrEmpty(m.id)
                    && string.Equals(m.id, id, StringComparison.OrdinalIgnoreCase))
                {
                    match = m;
                    break;
                }
            }

            if (match == null || match.modelPrefab == null)
            {
                Debug.LogWarning($"[PlayerCharacterLoader] No mapping for character id '{id}'. Keeping existing model.", this);
                return;
            }

            SwapModel(match.modelPrefab, match.avatarOverride);
            Debug.Log($"[PlayerCharacterLoader] Loaded character '{match.id}' ({match.modelPrefab.name}).", this);
        }

        void SwapModel(GameObject modelPrefab, Avatar avatarOverride)
        {
            // Freeze animation during the swap so we never render a partially-bound
            // skeleton (would cause a one-frame T-pose / flicker on spawn).
            bool animatorWasEnabled = _animator != null && _animator.enabled;
            if (_animator != null) _animator.enabled = false;

            // Strip current visual subtree: any direct child that owns a SkinnedMeshRenderer
            // or whose name matches a stripped prefix (bone roots). CameraRig and other
            // gameplay children are preserved.
            var toDestroy = new List<GameObject>();
            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                if (child == null) continue;
                if (child.GetComponentInChildren<SkinnedMeshRenderer>(true) != null
                    || NameMatchesAnyPrefix(child.name, strippedChildPrefixes))
                {
                    toDestroy.Add(child.gameObject);
                }
            }
            for (int i = 0; i < toDestroy.Count; i++)
                DestroyImmediate(toDestroy[i]);

            // Spawn the new FBX as a temporary container under the Player root.
            var inst = Instantiate(modelPrefab, transform);
            inst.transform.localPosition = Vector3.zero;
            inst.transform.localRotation = Quaternion.identity;
            inst.transform.localScale    = Vector3.one;

            // FBX prefabs ship with their own Animator on the root; we drive
            // everything from the parent Animator, so strip it.
            var childAnim = inst.GetComponent<Animator>();
            if (childAnim != null) DestroyImmediate(childAnim);

            // Reparent the FBX's children (bones + skinned meshes) up to the
            // Player root, then destroy the empty FBX container. This preserves
            // the original bone paths (e.g. "mixamorig:Hips/...") that the
            // humanoid avatar expects when Rebind() walks the hierarchy.
            var promoted = new List<Transform>(inst.transform.childCount);
            for (int i = 0; i < inst.transform.childCount; i++)
                promoted.Add(inst.transform.GetChild(i));
            for (int i = 0; i < promoted.Count; i++)
                promoted[i].SetParent(transform, false);
            DestroyImmediate(inst);

            // The new SkinnedMeshRenderers inherit bounds from the FBX import,
            // which are computed in the original FBX's local space. After
            // reparenting bones, those bounds don't match the live skeleton and
            // the renderer can self-cull mid-frame — visible as flicker.
            // updateWhenOffscreen forces per-frame bounds recompute and removes
            // the artifact at a small CPU cost (negligible for one character).
            var renderers = GetComponentsInChildren<SkinnedMeshRenderer>(true);
            for (int i = 0; i < renderers.Length; i++)
                renderers[i].updateWhenOffscreen = true;

            if (_animator != null)
            {
                var useAvatar = avatarOverride != null ? avatarOverride : ExtractAvatar(modelPrefab);
                if (useAvatar != null) _animator.avatar = useAvatar;
                _animator.enabled = animatorWasEnabled;
                _animator.Rebind();
                // Tick once with dt=0 so the first rendered frame already has
                // the locomotion blend tree's idle pose applied — no T-pose flash.
                _animator.Update(0f);
            }
        }

        static bool NameMatchesAnyPrefix(string name, string[] prefixes)
        {
            if (prefixes == null || string.IsNullOrEmpty(name)) return false;
            for (int i = 0; i < prefixes.Length; i++)
            {
                var p = prefixes[i];
                if (!string.IsNullOrEmpty(p) && name.StartsWith(p, StringComparison.Ordinal))
                    return true;
            }
            return false;
        }

        static Avatar ExtractAvatar(GameObject prefab)
        {
            if (prefab == null) return null;
            var anim = prefab.GetComponent<Animator>();
            if (anim != null && anim.avatar != null) return anim.avatar;
            var inChild = prefab.GetComponentInChildren<Animator>(true);
            return inChild != null ? inChild.avatar : null;
        }
    }
}
