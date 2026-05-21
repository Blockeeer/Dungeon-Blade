using DungeonBlade.Combat;
using UnityEngine;

namespace DungeonBlade.Player
{
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimatorBridge : MonoBehaviour
    {
        [SerializeField] PlayerMovement movement;
        [SerializeField] PlayerCombat combat;
        [SerializeField] PlayerStats stats;

        [Header("Tuning")]
        [SerializeField, Tooltip("Horizontal speed below this is treated as idle.")]
        float idleSpeedDeadzone = 0.1f;
        [SerializeField, Tooltip("How fast Speed / MoveX / MoveZ animator parameters chase actual horizontal speed.")]
        float speedSmoothing = 12f;
        [SerializeField, Tooltip("Reference walk speed used to normalize MoveX/MoveZ into [-1, 1] for the 2D blend tree.")]
        float referenceWalkSpeed = 6f;
        [SerializeField, Tooltip("Camera Rig transform (used to project velocity onto camera basis for MoveX/MoveZ).")]
        Transform cameraRig;
        [SerializeField, Tooltip("Damage amount at and above which a hit becomes a Big Hit reaction.")]
        float bigHitThreshold = 25f;

        Animator _animator;
        Sword _sword;

        static readonly int HashSpeed       = Animator.StringToHash("Speed");
        static readonly int HashMoveX       = Animator.StringToHash("MoveX");
        static readonly int HashMoveZ       = Animator.StringToHash("MoveZ");
        static readonly int HashGrounded    = Animator.StringToHash("Grounded");
        static readonly int HashJump        = Animator.StringToHash("Jump");
        static readonly int HashBigJump     = Animator.StringToHash("BigJump");
        static readonly int HashRoll        = Animator.StringToHash("Roll");
        static readonly int HashDodge       = Animator.StringToHash("Dodge");
        // Dash trigger intentionally not declared — sideways dash is
        // physics-only, no animation fired (see OnDashStarted).
        static readonly int HashSlide       = Animator.StringToHash("Slide");
        static readonly int HashTired       = Animator.StringToHash("Tired");
        static readonly int HashAttack      = Animator.StringToHash("Attack");
        static readonly int HashHeavyAttack = Animator.StringToHash("HeavyAttack");
        static readonly int HashCombo       = Animator.StringToHash("Combo");
        static readonly int HashBlock       = Animator.StringToHash("Block");
        static readonly int HashParry       = Animator.StringToHash("Parry");
        static readonly int HashBlockBroken = Animator.StringToHash("BlockBroken");
        static readonly int HashReload      = Animator.StringToHash("Reload");
        static readonly int HashEquip       = Animator.StringToHash("Equip");
        static readonly int HashWeaponType  = Animator.StringToHash("WeaponType");
        static readonly int HashHit         = Animator.StringToHash("Hit");
        static readonly int HashBigHit      = Animator.StringToHash("BigHit");
        static readonly int HashHitBack     = Animator.StringToHash("HitBack");
        static readonly int HashDie         = Animator.StringToHash("Die");

        float _smoothedSpeed;
        float _smoothedMoveX;
        float _smoothedMoveZ;
        bool _wasGrounded = true;

        void Awake()
        {
            _animator = GetComponent<Animator>();
            if (movement == null) movement = GetComponent<PlayerMovement>();
            if (combat   == null) combat   = GetComponent<PlayerCombat>();
            if (stats    == null) stats    = GetComponent<PlayerStats>();
            if (cameraRig == null)
            {
                // Best-effort auto-find: the prefab puts CameraRig as a child of root.
                var rig = transform.Find("CameraRig");
                if (rig != null) cameraRig = rig;
            }
        }

        void OnEnable()
        {
            if (movement != null)
            {
                movement.Jumped           += OnJumped;
                movement.HighJumpStarted  += OnHighJumpStarted;
                movement.DashStarted      += OnDashStarted;
                movement.DodgeStarted     += OnDodgeStarted;
            }
            if (combat != null)
            {
                combat.AttackPerformed      += OnAttack;
                combat.HeavyAttackPerformed += OnHeavyAttack;
                combat.ReloadPerformed      += OnReload;
                combat.WeaponEquipped       += OnWeaponEquipped;
                // PlayerCombat.Start may have already equipped a weapon before this OnEnable ran;
                // sync the initial state so WeaponType isn't stuck at 0.
                if (combat.Active != null) OnWeaponEquipped(combat.Active);
            }
            if (stats != null)
            {
                stats.OnDamaged += OnDamaged;
                stats.OnDeath   += OnDied;
                stats.OnParry   += OnParried;
            }
        }

        void OnDisable()
        {
            if (movement != null)
            {
                movement.Jumped           -= OnJumped;
                movement.HighJumpStarted  -= OnHighJumpStarted;
                movement.DashStarted      -= OnDashStarted;
                movement.DodgeStarted     -= OnDodgeStarted;
            }
            if (combat != null)
            {
                combat.AttackPerformed      -= OnAttack;
                combat.HeavyAttackPerformed -= OnHeavyAttack;
                combat.ReloadPerformed      -= OnReload;
                combat.WeaponEquipped       -= OnWeaponEquipped;
            }
            if (stats != null)
            {
                stats.OnDamaged -= OnDamaged;
                stats.OnDeath   -= OnDied;
                stats.OnParry   -= OnParried;
            }
        }

        void Update()
        {
            if (_animator == null) return;

            if (movement != null)
            {
                float target = movement.HorizontalSpeed;
                if (target < idleSpeedDeadzone) target = 0f;
                _smoothedSpeed = Mathf.Lerp(_smoothedSpeed, target, speedSmoothing * Time.deltaTime);
                _animator.SetFloat(HashSpeed, _smoothedSpeed);

                bool nowGrounded = movement.IsGrounded;
                _animator.SetBool(HashGrounded, nowGrounded);
                _animator.SetBool(HashSlide,    movement.IsSliding);

                // Rising edge of Grounded → just landed. Clear any Jump trigger
                // that may still be pending from a high-jump boost (the second
                // Space-tap fires Jumped a second time mid-air, but no
                // transition consumes the trigger because we're already in
                // the Jump state). Without this reset, the lingering trigger
                // re-fires Locomotion → Jump immediately after Land exits,
                // which then snaps right back to Land — visible as the
                // landing animation playing twice.
                if (nowGrounded && !_wasGrounded)
                {
                    _animator.ResetTrigger(HashJump);
                }
                _wasGrounded = nowGrounded;

                // Project world velocity onto camera basis to get strafe / forward
                // components for the 2D Locomotion blend tree. Normalize by
                // referenceWalkSpeed so MoveX/MoveZ stay in roughly [-1, 1].
                Vector3 vel = movement.Velocity;
                vel.y = 0f;
                Vector2 targetMove = Vector2.zero;
                if (cameraRig != null && referenceWalkSpeed > 0.01f)
                {
                    Vector3 fwd   = cameraRig.forward; fwd.y = 0f;   fwd.Normalize();
                    Vector3 right = cameraRig.right;   right.y = 0f; right.Normalize();
                    float forwardAmt = Vector3.Dot(vel, fwd)   / referenceWalkSpeed;
                    float strafeAmt  = Vector3.Dot(vel, right) / referenceWalkSpeed;
                    targetMove = new Vector2(
                        Mathf.Clamp(strafeAmt, -1f, 1f),
                        Mathf.Clamp(forwardAmt, -1f, 1f));
                }
                _smoothedMoveX = Mathf.Lerp(_smoothedMoveX, targetMove.x, speedSmoothing * Time.deltaTime);
                _smoothedMoveZ = Mathf.Lerp(_smoothedMoveZ, targetMove.y, speedSmoothing * Time.deltaTime);
                _animator.SetFloat(HashMoveX, _smoothedMoveX);
                _animator.SetFloat(HashMoveZ, _smoothedMoveZ);
            }

            if (stats != null)
            {
                _animator.SetBool(HashTired, stats.IsLowStamina);
            }

            bool blocking = _sword != null && _sword.State == Sword.SwordState.Blocking;
            _animator.SetBool(HashBlock, blocking);
        }

        void OnJumped()
        {
            // Default every fresh jump to "small" — HighJumpStarted will flip
            // BigJump back to true on the second-tap path. The Land transitions
            // gate on BigJump to pick Locomotion vs Hard Landing.
            _animator.SetBool(HashBigJump, false);
            _animator.SetTrigger(HashJump);
        }
        void OnHighJumpStarted() => _animator.SetBool(HashBigJump, true);
        // Intentionally NOT firing HashDash for sideways double-tap dashes —
        // the physics burst is enough; the locomotion blend tree keeps showing
        // whatever the player was doing (idle / strafe), which reads cleaner
        // than the Standing Dive Forward clip used to play.
        void OnDashStarted()  { /* no-op: dash is physics-only, no anim */ }

        void OnDodgeStarted()
        {
            // Pick the right react clip by comparing the dodge direction to the
            // camera's forward axis: opposite-of-forward → backpedal Dodge,
            // anything else (sideways or forward) → Dive Roll.
            if (movement == null || cameraRig == null)
            {
                _animator.SetTrigger(HashRoll);
                return;
            }
            Vector3 dir = movement.LastBurstDirection;
            Vector3 fwd = cameraRig.forward; fwd.y = 0f; fwd.Normalize();
            float forwardDot = Vector3.Dot(dir.normalized, fwd);
            _animator.SetTrigger(forwardDot < -0.3f ? HashDodge : HashRoll);
        }
        void OnAttack()       => _animator.SetTrigger(HashAttack);
        void OnHeavyAttack()  => _animator.SetTrigger(HashHeavyAttack);
        void OnReload()       => _animator.SetTrigger(HashReload);
        void OnDied()         => _animator.SetTrigger(HashDie);

        void OnDamaged(float amount)
        {
            // Pick which react animation to play based on hit weight.
            _animator.SetTrigger(amount >= bigHitThreshold ? HashBigHit : HashHit);
        }

        void OnParried() => _animator.SetTrigger(HashParry);

        void OnWeaponEquipped(WeaponBase weapon)
        {
            _sword = weapon as Sword;
            _animator.SetInteger(HashWeaponType, GetWeaponType(weapon));
            // Don't fire Equip on the very first equip in PlayerCombat.Start —
            // otherwise the player T-pose-swings through Equip on scene load.
            if (Time.timeSinceLevelLoad > 0.2f)
            {
                _animator.SetTrigger(HashEquip);
            }
        }

        static int GetWeaponType(WeaponBase weapon)
        {
            // 0 Unarmed · 1 Sword+Shield · 2 GreatSword · 3 Pistol · 4 Rifle.
            // Sword vs GreatSword and Pistol vs Rifle aren't distinguishable at the WeaponBase level today.
            // When subclasses or a tag are added, branch here.
            if (weapon == null) return 0;
            if (weapon is Sword) return 1;
            if (weapon is Gun)   return 4;
            return 0;
        }
    }
}
