﻿using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class ThirdPersonController : MonoBehaviour
    {
        [Header("Player Movement Speeds")]
        public float MoveSpeed = 2.0f;
        public float SprintSpeed = 5.335f;
        public float CrouchSpeed = 1.0f;

        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;
        public float SpeedChangeRate = 10.0f;

        [Header("Jump and Gravity")]
        public float JumpHeight = 1.2f;
        public float Gravity = -15.0f;
        public float JumpTimeout = 0.50f;
        public float FallTimeout = 0.15f;

        [Header("Grounded")]
        public bool Grounded = true;
        public float GroundedOffset = -0.14f;
        public float GroundedRadius = 0.28f;
        public LayerMask GroundLayers;

        [Header("Camera")]
        public GameObject CinemachineCameraTarget;
        public float TopClamp = 70.0f;
        public float BottomClamp = -30.0f;
        public float CameraAngleOverride = 0.0f;
        public bool LockCameraPosition = false;

        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;
        private float _speed;
        private float _animationBlend;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;
        private int _animIDCrouch;
        private int _animIDCrouchWalk;
        private int _animIDStandToCrouch;
        private int _animIDCrouchToStand;

        [Header("Crouch Settings")]
        public float crouchHeight = 1.0f;  // 蹲下时的碰撞体高度
        private float standingHeight;      // 保存原始高度
        private Vector3 standingCenter;    // 保存原始中心点

        private bool _isTransitioningCrouch = false;
        [Header("模型引用")]
        public Transform characterModel; // 在 Inspector 拖入角色模型
        [Header("站立动画时间")]
        public float standUpAnimationTime = 1.0f; // 起立动画的预计时长（秒）

        [Header("平滑归零时间")]
        public float smoothResetTime = 0.5f; // 位置/旋转归零的过渡时间

        private float _standUpTimer = 0f;
        private bool _isStandingUp = false;
        private bool _isSmoothingReset = false; // 是否正在平滑归零
        private Vector3 _startPosition; // 过渡起始位置
        private Quaternion _startRotation; // 过渡起始旋转
        private float _smoothResetTimer = 0f; // 平滑归零计时器

        [Header("Skill Charge")]
        public float chargeTimeThreshold = 1.15f; // 充能所需时间
        private float chargeTimer = 0f;
        private bool isCharging = false;
        private bool isChargeComplete = false;
        private bool isReversing = false;
        private bool isCastingSkill = false;

        public string skillChargeAnim = "Charge";
        public string skillReleaseAnim = "Release";


#if ENABLE_INPUT_SYSTEM
        private PlayerInput _playerInput;
#endif
        public Animator _animator;
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private GameObject _mainCamera;
        private const float _threshold = 0.01f;
        private bool _hasAnimator = true;

        public bool isCrouching = false;



        private void Awake()
        {
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }

        private void Start()
        {
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();

            // 新增：记录初始高度和中心点
            standingHeight = _controller.height;
            standingCenter = _controller.center;

#if ENABLE_INPUT_SYSTEM
            _playerInput = GetComponent<PlayerInput>();
#endif
            AssignAnimationIDs();
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
        }

        private void Update()
        {
            HandleCrouchToggle();
            HandleStandUpTimer(); // 新增计时器逻辑
            GroundedCheck();
            JumpAndGravity();
            Move();
            HandleSkillCharging(); // 放在最底部调用



        }

        private void HandleStandUpTimer()
        {
            if (_isStandingUp)
            {
                _standUpTimer -= Time.deltaTime;

                // 倒计时结束，开始平滑归零
                if (_standUpTimer <= 0f && !_isSmoothingReset)
                {
                    _isStandingUp = false;
                    _isSmoothingReset = true;
                    _smoothResetTimer = 0f;

                    // 记录当前的位置和旋转，作为插值起点
                    _startPosition = _animator.transform.localPosition;
                    _startRotation = _animator.transform.localRotation;
                }
            }

            // 平滑归零阶段
            if (_isSmoothingReset)
            {
                _smoothResetTimer += Time.deltaTime;
                float progress = Mathf.Clamp01(_smoothResetTimer / smoothResetTime);

                // 使用 Lerp/Slerp 逐渐归零
                _animator.transform.localPosition = Vector3.Lerp(_startPosition, Vector3.zero, progress);
                _animator.transform.localRotation = Quaternion.Slerp(_startRotation, Quaternion.identity, progress);

                // 过渡完成
                if (progress >= 1f)
                {
                    _isSmoothingReset = false;
                    _animator.applyRootMotion = false; // 完全归零后关闭 Root Motion
                }
            }
        }

        private void LateUpdate()
        {
            CameraRotation();
        }

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
            _animIDCrouch = Animator.StringToHash("IsCrouching");
            _animIDCrouchWalk = Animator.StringToHash("CrouchWalk");
            _animIDStandToCrouch = Animator.StringToHash("StandToCrouch");
            _animIDCrouchToStand = Animator.StringToHash("CrouchToStand");
        }

        private void GroundedCheck()
        {
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
            if (_hasAnimator) _animator.SetBool(_animIDGrounded, Grounded);
        }

        private void CameraRotation()
        {
            if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;
                _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
                _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
            }

            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(
                _cinemachineTargetPitch + CameraAngleOverride,
                _cinemachineTargetYaw,
                0.0f
            );
        }

        private void Move()
        {
            float targetSpeed = isCrouching ? CrouchSpeed : (_input.sprint ? SprintSpeed : MoveSpeed);
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;
            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            if (_input.move != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }

            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;
            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);

                _animator.SetBool(_animIDCrouchWalk, isCrouching && _input.move.magnitude > 0);
            }
        }

        private void JumpAndGravity()
        {
            if (Grounded)
            {
                // reset the fall timeout timer
                _fallTimeoutDelta = FallTimeout;

                // update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                // Jump
                if (_input.jump && _jumpTimeoutDelta <= 0.0f)
                {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDJump, true);
                    }
                }

                // jump timeout
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                // reset the jump timeout timer
                _jumpTimeoutDelta = JumpTimeout;

                // fall timeout
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDFreeFall, true);
                    }
                }

                // if we are not grounded, do not jump
                _input.jump = false;
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        // 在 HandleCrouchToggle 中仅触发动画
        private void HandleCrouchToggle()
        {
            if (Keyboard.current.cKey.wasPressedThisFrame && !_isTransitioningCrouch)
            {
                isCrouching = !isCrouching;
                if (_hasAnimator)
                {
                    if (isCrouching)
                    {
                        // 蹲下逻辑（不变）
                        _animator.applyRootMotion = false;
                        _animator.SetTrigger(_animIDStandToCrouch);
                        _controller.height = crouchHeight;
                        _controller.center = new Vector3(0f, crouchHeight / 2f, 0f);
                    }
                    else
                    {
                        // 起立逻辑：启动计时器
                        _animator.applyRootMotion = true; // 允许 Root Motion 影响位移
                        _animator.SetTrigger(_animIDCrouchToStand);
                        _isStandingUp = true;
                        _standUpTimer = standUpAnimationTime; // 设置倒计时
                    }
                    _animator.SetBool(_animIDCrouch, isCrouching);
                }
            }
        }

    
        public void OnCrouchToStandComplete()
        {
            _controller.height = standingHeight;
            _controller.center = standingCenter;

            // Reset model's local position and rotation
            if (_animator != null)
            {
                _animator.applyRootMotion = false;
                // Assuming your character model is a child of the GameObject with this script
                Transform modelTransform = _animator.transform;
                modelTransform.localPosition = Vector3.zero;
                modelTransform.localRotation = Quaternion.identity;
            }
        }


        private void HandleSkillCharging()
        {
            // 技能释放中不允许移动
            if (isCastingSkill)
                return;

            if (_input.skillHold)
            {
                if (!isCharging)
                {
                    // 开始充能
                    isCharging = true;
                    chargeTimer = 0f;
                    isChargeComplete = false;
                    _animator.SetFloat(skillChargeAnim, 1f); // 正向播放动画
                    _animator.speed = 1f;
                    _input.DisableMovement(); // 禁止移动（需要你在 StarterAssetsInputs 中实现）
                }

                chargeTimer += Time.deltaTime;

                if (chargeTimer >= chargeTimeThreshold && !isChargeComplete)
                {
                    isChargeComplete = true;
                    _animator.SetFloat(skillChargeAnim, 2f); // 播放到蓄力完成的段
                }
            }
            else if (isCharging)
            {
                // 松开按键
                if (isChargeComplete)
                {
                    // 充能完成，释放技能
                    isCastingSkill = true;
                    _animator.SetFloat(skillChargeAnim, 3f); // 播放技能释放段
                    _animator.speed = 1f;
                    Invoke(nameof(FinishSkillCast), 0.5f); // 等动画播放完，再恢复（根据动画长度改）
                    //FindObjectOfType<SkillManager>()?.ForceReleaseSkill();

                }
                else
                {
                    // 未完成充能，动画反向播放回到Idle
                    _animator.SetFloat(skillChargeAnim, -1f); // 反向动画
                    _animator.speed = 1f;
                    Invoke(nameof(ResetSkillCharge), 0.3f); // 根据动画长度调整
                }

                isCharging = false;
            }
        }

        private void ResetSkillCharge()
        {
            _animator.SetFloat(skillChargeAnim, 0f);
            _animator.speed = 1f;
            _input.EnableMovement();
        }

        private void FinishSkillCast()
        {
            isCastingSkill = false;
            _animator.SetFloat(skillChargeAnim, 0f);
            _animator.speed = 1f;
            _input.EnableMovement();
        }





        private static float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360f) angle += 360f;
            if (angle > 360f) angle -= 360f;
            return Mathf.Clamp(angle, min, max);
        }

        private bool IsCurrentDeviceMouse =>
#if ENABLE_INPUT_SYSTEM
            _playerInput.currentControlScheme == "KeyboardMouse";
#else
            false;
#endif
    }
}
