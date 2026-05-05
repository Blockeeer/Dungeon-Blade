using UnityEngine;

namespace DungeonBlade.Player
{
    /// Forcibly snaps the Mixamo Hips bone's local XZ back to its baseline
    /// every LateUpdate. Mixamo character clips bake forward translation into
    /// the Hips bone curve itself — separate from avatar root motion — so even
    /// with applyRootMotion=false and lockRootPositionXZ correctly configured,
    /// the visible character can still drift forward during a clip and snap
    /// back during the blend, which reads as the character being "pulled
    /// backward" out from under the player.
    ///
    /// This script runs after the Animator has posed the bones and resets
    /// only the XZ component of the Hips local position, leaving Y intact so
    /// the visual leap-up / knee-bend during jumps and landings still shows.
    /// Body bones (legs, arms, torso) animate normally relative to the locked
    /// Hips, so the leap pose, roll motion, and dodge react read as expected
    /// — the character just doesn't physically translate from the animation.
    /// All visible XZ travel comes from CharacterController.Move via PlayerMovement.
    [DefaultExecutionOrder(10000)] // run after the Animator and after CameraSmoothFollow
    public class HipsLock : MonoBehaviour
    {
        [Tooltip("Mixamo Hips bone (mixamorig:Hips). Auto-located on Awake if left null.")]
        [SerializeField] Transform hips;

        [Tooltip("Lock vertical too. Off by default so jumps and landings still show the body's vertical bone motion.")]
        [SerializeField] bool lockY = false;

        Vector3 _baseline;
        bool _ready;

        void Awake()
        {
            if (hips == null) hips = FindHips(transform);
            if (hips == null)
            {
                Debug.LogWarning($"[{nameof(HipsLock)}] No Hips bone found under {name}; disabling.", this);
                enabled = false;
                return;
            }
            _baseline = hips.localPosition;
            _ready = true;
        }

        void LateUpdate()
        {
            if (!_ready) return;
            Vector3 lp = hips.localPosition;
            hips.localPosition = new Vector3(
                _baseline.x,
                lockY ? _baseline.y : lp.y,
                _baseline.z);
        }

        static Transform FindHips(Transform root)
        {
            // Mixamo names are stable across all Tripo characters in this project.
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
            {
                if (t.name.IndexOf("Hips", System.StringComparison.OrdinalIgnoreCase) >= 0) return t;
            }
            return null;
        }
    }
}
