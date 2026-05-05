using System;
using DungeonBlade.Core;
using UnityEngine;

namespace DungeonBlade.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Walk")]
        [SerializeField] float walkSpeed = 6f;
        [SerializeField] float airAcceleration = 30f;
        [SerializeField] float groundAcceleration = 60f;
        [SerializeField] float groundFriction = 12f;

        [Header("Jump")]
        [SerializeField] float jumpHeight = 1.6f;
        [SerializeField] int maxAirJumps = 1;
        [SerializeField] float gravity = -22f;
        [SerializeField] float coyoteTime = 0.12f;
        [SerializeField] float jumpBufferTime = 0.12f;
        [Tooltip("Pressing Jump again within this window of the ground jump replaces the second jump with a single high-jump arc.")]
        [SerializeField] float highJumpTapWindow = 0.25f;
        [Tooltip("Extra effective height the high-jump boost adds on top of jumpHeight.")]
        [SerializeField] float highJumpBoostHeight = 1.8f;

        [Header("Bunny Hop")]
        [Tooltip("Window after landing in which jumping again preserves horizontal momentum.")]
        [SerializeField] float bhopWindow = 0.18f;
        [Tooltip("Speed multiplier applied per chained bhop, capped by bhopMaxSpeed.")]
        [SerializeField] float bhopGain = 1.05f;
        [SerializeField] float bhopMaxSpeed = 14f;

        [Header("Dash (Shift + WASD)")]
        [SerializeField] float dashSpeed = 18f;
        [SerializeField] float dashDuration = 0.18f;
        [SerializeField] float dashCooldown = 0.6f;
        [SerializeField] float dashStaminaCost = 20f;

        [Header("Dodge (Double-tap WASD)")]
        [SerializeField] float dodgeSpeed = 16f;
        // Bumped from 0.15s → 0.35s. The forward roll animation runs ~0.4s
        // after head trim — the old 0.15s burst stopped horizontal momentum
        // halfway through, which read as the character "stopping then rolling".
        // Sustaining the burst for the whole roll keeps motion continuous.
        [SerializeField] float dodgeDuration = 0.35f;
        [SerializeField] float dodgeCooldown = 0.4f;
        [SerializeField] float dodgeStaminaCost = 15f;
        [Tooltip("Maximum time between two presses of the same direction key to count as a double-tap.")]
        [SerializeField] float doubleTapWindow = 0.25f;

        [Header("Slide")]
        [SerializeField] float slideSpeed = 12f;
        [SerializeField] float slideDuration = 0.6f;
        [SerializeField] float slideHeight = 1.0f;

        [Header("Wall Run")]
        [SerializeField] float wallRunMaxDuration = 1.5f;
        [SerializeField] float wallRunSpeed = 8f;
        [SerializeField] float wallRunGravity = -3f;
        [SerializeField] float wallCheckDistance = 0.7f;
        [SerializeField] float wallRunMinAirTime = 0.1f;
        [SerializeField] float wallJumpForce = 7f;
        [SerializeField] LayerMask wallLayers = ~0;

        [Header("Camera")]
        [SerializeField] Transform cameraRig;
        [SerializeField] float lookSensitivity = 0.12f;
        [SerializeField] float minPitch = -80f;
        [SerializeField] float maxPitch = 80f;

        [Header("Character Facing")]
        [Tooltip("If ON: body rotates to face movement direction (action-game style; needs forward-only run anim). If OFF: body stays facing camera direction (shooter style; needs strafe + backpedal anims). Default OFF since the 2D Locomotion BlendTree provides directional clips.")]
        [SerializeField] bool faceMovementDirection = false;
        [Tooltip("Degrees per second the character body rotates toward the movement direction. Only used when faceMovementDirection is ON.")]
        [SerializeField] float characterTurnSpeed = 720f;

        CharacterController _controller;
        PlayerStats _stats;
        PlayerInputActions _input;

        Vector3 _velocity;
        Vector2 _moveInput;
        float _yaw;
        float _pitch;

        bool _isGrounded;
        float _lastGroundedTime;
        float _lastJumpPressedTime = -999f;
        float _lastJumpExecuteTime = -999f;
        bool _highJumpAvailable;
        int _airJumpsUsed;

        float _lastLandTime = -999f;
        float _carriedHorizontalSpeed;

        bool _isDashing;
        float _dashEndTime;
        float _nextDashTime;
        float _nextDodgeTime;
        Vector3 _dashDirection;
        float _activeBurstSpeed;

        Vector2 _lastMoveInput;
        float _lastForwardTapTime = -999f;
        float _lastBackTapTime = -999f;
        float _lastLeftTapTime = -999f;
        float _lastRightTapTime = -999f;
        bool _hasPendingDodge;
        Vector3 _pendingDodgeDirection;

        public bool IsDashing => _isDashing;
        public bool IsInvulnerable => _isDashing;
        public bool IsGrounded => _isGrounded;
        public bool IsSliding => _isSliding;
        public Vector3 Velocity => _velocity;
        public Vector2 MoveInput => _moveInput;
        public float HorizontalSpeed => new Vector2(_velocity.x, _velocity.z).magnitude;
        // World-space direction of the most recent dash or dodge — bridge reads
        // this in OnDodgeStarted to pick between Roll (sideways) and Dodge-Back.
        public Vector3 LastBurstDirection => _dashDirection;

        public event Action Jumped;
        public event Action HighJumpStarted;
        public event Action DashStarted;
        public event Action DodgeStarted;

        bool _isSliding;
        float _slideEndTime;
        float _defaultHeight;
        Vector3 _defaultCenter;

        bool _isWallRunning;
        float _wallRunEndTime;
        Vector3 _wallNormal;
        float _airTime;

        void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _stats = GetComponent<PlayerStats>();
            _defaultHeight = _controller.height;
            _defaultCenter = _controller.center;
        }

        void Start()
        {
            _input = InputManager.Instance != null ? InputManager.Instance.Actions : new PlayerInputActions();
            if (InputManager.Instance == null) _input.Enable();

            _yaw = transform.eulerAngles.y;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void Update()
        {
            if (_stats != null && _stats.IsDead) return;

            if (MenuState.IsAnyOpen)
            {
                _moveInput = Vector2.zero;
                UpdateGrounding();
                ApplyMovement();
                return;
            }

            ReadInput();
            UpdateLook();
            UpdateGrounding();
            UpdateWallRun();
            HandleDash();
            HandleSlide();
            HandleJump();
            ApplyMovement();
        }

        void ReadInput()
        {
            _moveInput = _input.Move.ReadValue<Vector2>();
        }

        void UpdateLook()
        {
            float sens = SettingsManager.Instance != null ? SettingsManager.Instance.MouseSensitivity : lookSensitivity;
            Vector2 look = _input.Look.ReadValue<Vector2>() * sens;
            _yaw += look.x;
            _pitch = Mathf.Clamp(_pitch - look.y, minPitch, maxPitch);

            if (faceMovementDirection)
            {
                // Action-game mode: body turns toward movement (in ApplyMovement).
                // Camera must compensate so its world rotation stays at (pitch, yaw)
                // regardless of body spin.
                if (cameraRig != null)
                {
                    Quaternion desiredCameraWorldRot = Quaternion.Euler(_pitch, _yaw, 0f);
                    cameraRig.localRotation = Quaternion.Inverse(transform.rotation) * desiredCameraWorldRot;
                }
            }
            else
            {
                // Shooter mode: body always faces camera-yaw direction; cameraRig
                // (a child) only handles pitch locally. Strafe / backpedal clips
                // in the 2D Locomotion blend tree handle directional animation.
                transform.rotation = Quaternion.Euler(0f, _yaw, 0f);
                if (cameraRig != null)
                {
                    cameraRig.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
                }
            }
        }

        Vector3 GetCameraForward()
        {
            if (cameraRig == null) return transform.forward;
            Vector3 fwd = cameraRig.forward;
            fwd.y = 0f;
            return fwd.sqrMagnitude > 0.0001f ? fwd.normalized : transform.forward;
        }

        Vector3 GetCameraRight()
        {
            if (cameraRig == null) return transform.right;
            Vector3 right = cameraRig.right;
            right.y = 0f;
            return right.sqrMagnitude > 0.0001f ? right.normalized : transform.right;
        }

        void UpdateGrounding()
        {
            bool wasGrounded = _isGrounded;
            _isGrounded = _controller.isGrounded;

            if (_isGrounded)
            {
                _lastGroundedTime = Time.time;
                _airTime = 0f;
                if (!wasGrounded)
                {
                    OnLand();
                }
            }
            else
            {
                _airTime += Time.deltaTime;
            }
        }

        void OnLand()
        {
            _airJumpsUsed = 0;
            _highJumpAvailable = false;

            float horizontalSpeed = new Vector3(_velocity.x, 0f, _velocity.z).magnitude;
            _carriedHorizontalSpeed = horizontalSpeed;
            _lastLandTime = Time.time;
            _isWallRunning = false;
        }

        void HandleJump()
        {
            if (_input.Jump.WasPressedThisFrame())
            {
                _lastJumpPressedTime = Time.time;
            }

            bool jumpBuffered = Time.time - _lastJumpPressedTime <= jumpBufferTime;
            if (!jumpBuffered) return;

            bool canGroundJump = _isGrounded || (Time.time - _lastGroundedTime <= coyoteTime && _velocity.y <= 0f);
            bool canAirJump = !_isGrounded && _airJumpsUsed < maxAirJumps;
            bool canWallJump = _isWallRunning;

            // Second Space tap shortly after the ground jump → boost the SAME
            // jump arc into a single high-jump instead of triggering a separate
            // air-jump (which read as "two jumps" visually).
            bool isHighJumpBoost = _highJumpAvailable
                && !_isGrounded
                && Time.time - _lastJumpExecuteTime <= highJumpTapWindow;

            if (canWallJump)
            {
                Vector3 jumpDir = (_wallNormal + Vector3.up).normalized;
                _velocity = jumpDir * wallJumpForce;
                _velocity.y = Mathf.Sqrt(-2f * gravity * jumpHeight);
                _isWallRunning = false;
                _lastJumpPressedTime = -999f;
                Jumped?.Invoke();
                return;
            }

            if (canGroundJump)
            {
                bool isBhop = Time.time - _lastLandTime <= bhopWindow;
                _velocity.y = Mathf.Sqrt(-2f * gravity * jumpHeight);

                if (isBhop && _carriedHorizontalSpeed > 0.1f)
                {
                    Vector3 horiz = new Vector3(_velocity.x, 0f, _velocity.z);
                    float currentMag = horiz.magnitude;
                    Vector3 dir = currentMag > 0.01f ? horiz / currentMag : transform.forward;
                    float boosted = Mathf.Min(bhopMaxSpeed, _carriedHorizontalSpeed * bhopGain);
                    _velocity.x = dir.x * boosted;
                    _velocity.z = dir.z * boosted;
                }

                _isGrounded = false;
                _lastJumpPressedTime = -999f;
                _lastJumpExecuteTime = Time.time;
                _highJumpAvailable = true;
                Jumped?.Invoke();
            }
            else if (isHighJumpBoost)
            {
                // Replace the climbing velocity with a single bigger arc.
                _velocity.y = Mathf.Sqrt(-2f * gravity * (jumpHeight + highJumpBoostHeight));
                _highJumpAvailable = false;
                _lastJumpPressedTime = -999f;
                // Don't re-fire Jumped — we're already in the Jump animator
                // state and the second trigger only ever caused leakage into
                // a duplicate Land. HighJumpStarted is the dedicated signal
                // the bridge uses to switch the upcoming Land into HardLand.
                HighJumpStarted?.Invoke();
            }
            else if (canAirJump)
            {
                _velocity.y = Mathf.Sqrt(-2f * gravity * jumpHeight);
                _airJumpsUsed++;
                _lastJumpPressedTime = -999f;
                Jumped?.Invoke();
            }
        }

        void HandleDash()
        {
            if (_isDashing)
            {
                if (Time.time >= _dashEndTime)
                {
                    _isDashing = false;
                }
                return;
            }

            DetectDoubleTaps();

            if (TryStartDash()) return;
            TryStartDodge();
        }

        void DetectDoubleTaps()
        {
            bool forwardNow = _moveInput.y > 0.5f;
            bool forwardPrev = _lastMoveInput.y > 0.5f;
            bool backNow = _moveInput.y < -0.5f;
            bool backPrev = _lastMoveInput.y < -0.5f;
            bool rightNow = _moveInput.x > 0.5f;
            bool rightPrev = _lastMoveInput.x > 0.5f;
            bool leftNow = _moveInput.x < -0.5f;
            bool leftPrev = _lastMoveInput.x < -0.5f;

            Vector3 cameraFwd = GetCameraForward();
            Vector3 cameraRight = GetCameraRight();

            if (forwardNow && !forwardPrev) HandleDirectionTap(ref _lastForwardTapTime, cameraFwd);
            else if (backNow && !backPrev) HandleDirectionTap(ref _lastBackTapTime, -cameraFwd);
            else if (rightNow && !rightPrev) HandleDirectionTap(ref _lastRightTapTime, cameraRight);
            else if (leftNow && !leftPrev) HandleDirectionTap(ref _lastLeftTapTime, -cameraRight);

            _lastMoveInput = _moveInput;
        }

        void HandleDirectionTap(ref float lastTapTime, Vector3 direction)
        {
            float now = Time.time;
            if (now - lastTapTime <= doubleTapWindow)
            {
                _pendingDodgeDirection = direction.normalized;
                _hasPendingDodge = true;
                lastTapTime = -999f;
            }
            else
            {
                lastTapTime = now;
            }
        }

        bool TryStartDash()
        {
            if (Time.time < _nextDashTime) return false;
            if (!_input.Dash.WasPressedThisFrame()) return false;
            if (_stats != null && !_stats.TryConsumeStamina(dashStaminaCost)) return false;

            Vector3 cameraFwd = GetCameraForward();
            Vector3 cameraRight = GetCameraRight();
            Vector3 inputDir = cameraRight * _moveInput.x + cameraFwd * _moveInput.y;
            if (inputDir.sqrMagnitude < 0.01f) inputDir = cameraFwd;
            _dashDirection = inputDir.normalized;

            _isDashing = true;
            _activeBurstSpeed = dashSpeed;
            _dashEndTime = Time.time + dashDuration;
            _nextDashTime = Time.time + dashCooldown;
            _hasPendingDodge = false;
            DashStarted?.Invoke();
            return true;
        }

        bool TryStartDodge()
        {
            if (!_hasPendingDodge) return false;
            _hasPendingDodge = false;

            if (Time.time < _nextDodgeTime) return false;
            if (_stats != null && !_stats.TryConsumeStamina(dodgeStaminaCost)) return false;

            _dashDirection = _pendingDodgeDirection;
            _isDashing = true;
            _activeBurstSpeed = dodgeSpeed;
            _dashEndTime = Time.time + dodgeDuration;
            _nextDodgeTime = Time.time + dodgeCooldown;
            DodgeStarted?.Invoke();
            return true;
        }

        void HandleSlide()
        {
            bool crouchPressed = _input.Crouch.WasPressedThisFrame();

            if (!_isSliding && crouchPressed && _isGrounded && _moveInput.sqrMagnitude > 0.1f)
            {
                _isSliding = true;
                _slideEndTime = Time.time + slideDuration;
                _controller.height = slideHeight;
                _controller.center = new Vector3(_defaultCenter.x, slideHeight * 0.5f, _defaultCenter.z);
            }
            else if (_isSliding && (Time.time >= _slideEndTime || !_isGrounded))
            {
                EndSlide();
            }
        }

        void EndSlide()
        {
            _isSliding = false;
            _controller.height = _defaultHeight;
            _controller.center = _defaultCenter;
        }

        void UpdateWallRun()
        {
            if (_isGrounded || _airTime < wallRunMinAirTime)
            {
                _isWallRunning = false;
                return;
            }

            if (_isWallRunning && Time.time >= _wallRunEndTime)
            {
                _isWallRunning = false;
                return;
            }

            bool hasInput = _moveInput.sqrMagnitude > 0.1f;
            if (!hasInput && !_isWallRunning)
            {
                return;
            }

            Vector3 origin = transform.position + Vector3.up * (_controller.height * 0.5f);
            bool foundWall = false;

            if (Physics.Raycast(origin, transform.right, out RaycastHit rightHit, wallCheckDistance, wallLayers, QueryTriggerInteraction.Ignore)
                && IsVerticalWall(rightHit.normal))
            {
                _wallNormal = rightHit.normal;
                foundWall = true;
            }
            else if (Physics.Raycast(origin, -transform.right, out RaycastHit leftHit, wallCheckDistance, wallLayers, QueryTriggerInteraction.Ignore)
                && IsVerticalWall(leftHit.normal))
            {
                _wallNormal = leftHit.normal;
                foundWall = true;
            }

            if (foundWall)
            {
                if (!_isWallRunning)
                {
                    _isWallRunning = true;
                    _wallRunEndTime = Time.time + wallRunMaxDuration;
                }
            }
            else
            {
                _isWallRunning = false;
            }
        }

        static bool IsVerticalWall(Vector3 normal)
        {
            return Mathf.Abs(normal.y) < 0.3f;
        }

        void ApplyMovement()
        {
            if (_isDashing)
            {
                Vector3 dashStep = _dashDirection * _activeBurstSpeed;
                _controller.Move(dashStep * Time.deltaTime);
                _velocity.x = dashStep.x;
                _velocity.z = dashStep.z;
                _velocity.y = 0f;
                return;
            }

            if (_isSliding)
            {
                Vector3 slideDir = transform.forward * slideSpeed;
                _velocity.x = slideDir.x;
                _velocity.z = slideDir.z;
                _velocity.y += gravity * Time.deltaTime;
                _controller.Move(_velocity * Time.deltaTime);
                return;
            }

            if (_isWallRunning)
            {
                Vector3 wallForward = Vector3.Cross(_wallNormal, Vector3.up);
                if (Vector3.Dot(wallForward, transform.forward) < 0f) wallForward = -wallForward;

                Vector3 wallVel = wallForward * wallRunSpeed;
                _velocity.x = wallVel.x;
                _velocity.z = wallVel.z;
                _velocity.y += wallRunGravity * Time.deltaTime;
                _controller.Move(_velocity * Time.deltaTime);
                return;
            }

            Vector3 cameraFwd = GetCameraForward();
            Vector3 cameraRight = GetCameraRight();
            Vector3 wishDir = cameraRight * _moveInput.x + cameraFwd * _moveInput.y;
            float wishSpeed = walkSpeed;

            Vector3 horizontalVel = new Vector3(_velocity.x, 0f, _velocity.z);

            if (_isGrounded)
            {
                Vector3 target = wishDir * wishSpeed;
                horizontalVel = Vector3.MoveTowards(horizontalVel, target, groundAcceleration * Time.deltaTime);

                if (wishDir.sqrMagnitude < 0.01f)
                {
                    horizontalVel = Vector3.MoveTowards(horizontalVel, Vector3.zero, groundFriction * Time.deltaTime);
                }
            }
            else
            {
                if (wishDir.sqrMagnitude > 0.01f)
                {
                    horizontalVel += wishDir * (airAcceleration * Time.deltaTime);
                    float airCap = Mathf.Max(wishSpeed, bhopMaxSpeed);
                    if (horizontalVel.magnitude > airCap)
                    {
                        horizontalVel = horizontalVel.normalized * airCap;
                    }
                }
            }

            _velocity.x = horizontalVel.x;
            _velocity.z = horizontalVel.z;

            if (_isGrounded && _velocity.y < 0f) _velocity.y = -2f;
            _velocity.y += gravity * Time.deltaTime;

            _controller.Move(_velocity * Time.deltaTime);

            // Body rotates toward movement direction so the forward-run animation
            // visually matches the world-space travel direction (S backpedals,
            // A/D strafe). Camera rotation is independent — see UpdateLook.
            if (faceMovementDirection && wishDir.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(wishDir, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation, targetRot, characterTurnSpeed * Time.deltaTime);
            }
        }
    }
}
