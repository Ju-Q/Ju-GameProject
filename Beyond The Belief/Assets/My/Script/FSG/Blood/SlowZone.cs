using System;
using StarterAssets;
using UnityEngine;

public class SlowZone : MonoBehaviour
{
    [Header("减速设置")]
    public float slowMoveSpeed = 1.5f;
    public bool disableSprint = true;
    public bool cancelCurrentSprint = true;

    [Header("动画设置")]
    public bool useSlowWalkAnimation = true;
    public string slowWalkAnimParameter = "SlowWalk";
    public float animationTransitionTime = 0.2f;

    [Tooltip("手动指定包含Animator的子对象（如果不在根对象上）")]
    public Transform animatorTarget; // 新增：手动指定Animator所在对象

    private ThirdPersonController _playerController;
    private StarterAssetsInputs _playerInputs;
    private Animator _playerAnimator;
    private CharacterController _characterController;

    private float _originalMoveSpeed;
    private float _originalSprintSpeed;
    public bool isInSlowZone;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        isInSlowZone = true;
        InitializePlayerComponents(other);
        ApplySlowEffects();
    }

    private void InitializePlayerComponents(Collider player)
    {
        if (_playerController == null)
        {
            // 获取根对象上的组件
            _playerController = player.GetComponentInParent<ThirdPersonController>();
            _playerInputs = player.GetComponentInParent<StarterAssetsInputs>();
            _characterController = player.GetComponentInParent<CharacterController>();

            // 优先使用手动指定的Animator目标，否则自动查找
            if (animatorTarget != null)
            {
                _playerAnimator = animatorTarget.GetComponent<Animator>();
            }
            else
            {
                // 在整个层级中查找Animator
                _playerAnimator = player.GetComponentInChildren<Animator>();
            }

            if (_playerAnimator == null)
            {
                Debug.LogError("未找到Animator组件！请检查层级或手动指定animatorTarget");
            }

            // 保存原始值
            _originalMoveSpeed = _playerController.MoveSpeed;
            _originalSprintSpeed = _playerController.SprintSpeed;
        }
    }

    private void ApplySlowEffects()
    {
        // 设置减速
        _playerController.MoveSpeed = slowMoveSpeed;

        if (disableSprint)
        {
            _playerController.SprintSpeed = slowMoveSpeed;
            if (cancelCurrentSprint && _playerInputs != null)
            {
                _playerInputs.sprint = false;
            }
        }

        // 设置动画参数
        if (useSlowWalkAnimation && _playerAnimator != null)
        {
            Debug.Log($"正在设置动画参数 {slowWalkAnimParameter} 为 true (当前值: {_playerAnimator.GetBool(slowWalkAnimParameter)})");

            // 确保Animator已启用
            _playerAnimator.enabled = true;

            _playerAnimator.SetBool(slowWalkAnimParameter, true);
            _playerAnimator.Update(0f); // 强制立即更新

            Debug.Log($"设置后值: {_playerAnimator.GetBool(slowWalkAnimParameter)} | 参数存在: {ParameterExists(_playerAnimator, slowWalkAnimParameter)}");
        }
    }

    private void Update()
    {
        if (isInSlowZone && _playerInputs != null)
        {
            _playerAnimator.speed = _playerInputs.move == Vector2.zero ? 0 : 1;
        }
        // float inputMagnitude = _playerInputs.analogMovement ? _playerInputs.move.magnitude : 1f;
        // Debug.Log("控制输入" + inputMagnitude)
    }

    // 检查参数是否存在的辅助方法
    private bool ParameterExists(Animator animator, string paramName)
    {
        foreach (var param in animator.parameters)
        {
            if (param.name == paramName) return true;
        }
        return false;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        isInSlowZone = false;
        _playerAnimator.speed = 1;
        if (_playerController != null)
        {
            _playerController.MoveSpeed = _originalMoveSpeed;
            _playerController.SprintSpeed = _originalSprintSpeed;

            if (useSlowWalkAnimation && _playerAnimator != null)
            {
                _playerAnimator.SetBool(slowWalkAnimParameter, false);
            }
        }
    }
}