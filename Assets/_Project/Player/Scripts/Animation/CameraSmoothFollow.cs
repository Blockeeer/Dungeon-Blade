using UnityEngine;

namespace DungeonBlade.Player
{
    /// Smooths out camera position jitter that otherwise reads as "shake" when
    /// CharacterController.Move ground-snaps or applies sub-frame corrections
    /// during jumps and landings. Attach to the PlayerCamera GameObject; assign
    /// the CameraRig as the target. The camera detaches from its parent on
    /// Start so animation/controller updates don't push it rigidly each frame,
    /// then in LateUpdate it chases (target.position + target.rotation * offset)
    /// with a critically-damped lerp. Rotation is copied 1:1 (mouse aim must
    /// stay direct).
    public class CameraSmoothFollow : MonoBehaviour
    {
        [Tooltip("Transform whose position+rotation the camera should follow. Usually the Player's CameraRig.")]
        [SerializeField] Transform target;

        [Tooltip("Smoothing time for vertical position. Higher = smoother but more lag. Tuned just high enough to swallow CharacterController ground-snap jitter without making the camera visibly trail the player during jumps.")]
        [SerializeField] float verticalSmoothTime = 0.035f;

        [Tooltip("Smoothing time for horizontal position. Lower than vertical so horizontal motion still feels responsive.")]
        [SerializeField] float horizontalSmoothTime = 0.02f;

        [Tooltip("If ON, recomputes the local offset from current transform on Start. If OFF, uses the manual offset below — useful for runtime-adjusting the third-person rig.")]
        [SerializeField] bool captureOffsetOnStart = true;

        [Tooltip("Local-space offset from target. Auto-filled at Start if captureOffsetOnStart is true.")]
        [SerializeField] Vector3 localOffset = new Vector3(0.4f, 0.4f, -3f);

        Vector3 _yVelocity;
        Vector3 _xzVelocity;
        bool _initialized;

        void Start()
        {
            if (target == null)
            {
                Debug.LogWarning($"[{nameof(CameraSmoothFollow)}] Target not assigned on {name}; disabling.", this);
                enabled = false;
                return;
            }

            if (captureOffsetOnStart)
            {
                // Cache the camera's current local offset relative to the target
                // BEFORE detaching, so the over-the-shoulder framing is preserved.
                localOffset = target.InverseTransformPoint(transform.position);
            }

            // Detach so this camera is no longer rigidly carried by the rig —
            // sub-frame movement of the player root won't propagate as shake.
            // Worldspace pose is preserved.
            transform.SetParent(null, true);
            _initialized = true;
        }

        void LateUpdate()
        {
            if (!_initialized || target == null) return;

            Vector3 desired = target.TransformPoint(localOffset);
            Vector3 current = transform.position;

            // Smooth Y separately from XZ so vertical jumps don't bleed into
            // horizontal lag. Vertical gets more smoothing because that's where
            // CharacterController jitter shows up most (ground-snap, gravity
            // sub-step corrections at the edge of isGrounded toggles).
            float newY  = Mathf.SmoothDamp(current.y, desired.y, ref _yVelocity.y, verticalSmoothTime);
            Vector3 horizCurrent  = new Vector3(current.x, 0f, current.z);
            Vector3 horizDesired  = new Vector3(desired.x, 0f, desired.z);
            Vector3 horizSmoothed = Vector3.SmoothDamp(horizCurrent, horizDesired, ref _xzVelocity, horizontalSmoothTime);

            transform.position = new Vector3(horizSmoothed.x, newY, horizSmoothed.z);
            // Rotation is direct — mouse aim must feel 1:1 with input.
            transform.rotation = target.rotation;
        }
    }
}
