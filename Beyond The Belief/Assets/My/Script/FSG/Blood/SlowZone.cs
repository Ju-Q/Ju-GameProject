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
    public Transform animatorTarget;

    [Header("血量检测 & 伤害设置")]
    public PlayerHealthSlider playerHealthSlider;
    public float damagePerSecond = 10f;

    [Header("拾取管理器引用")]
    public ItemPickupManager pickupManager;

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
            _playerController = player.GetComponentInParent<ThirdPersonController>();
            _playerInputs = player.GetComponentInParent<StarterAssetsInputs>();
            _characterController = player.GetComponentInParent<CharacterController>();

            if (animatorTarget != null)
            {
                _playerAnimator = animatorTarget.GetComponent<Animator>();
            }
            else
            {
                _playerAnimator = player.GetComponentInChildren<Animator>();
            }

            if (_playerAnimator == null)
            {
                Debug.LogError("未找到Animator组件！请检查层级或手动指定animatorTarget");
            }

            _originalMoveSpeed = _playerController.MoveSpeed;
            _originalSprintSpeed = _playerController.SprintSpeed;

            if (pickupManager == null)
            {
                pickupManager = player.GetComponentInParent<ItemPickupManager>();
                if (pickupManager == null)
                    Debug.LogWarning("SlowZone: 未指定 ItemPickupManager，无法检测 PropA 数量");
            }
        }
    }

    private void ApplySlowEffects()
    {
        _playerController.MoveSpeed = slowMoveSpeed;

        if (disableSprint)
        {
            _playerController.SprintSpeed = slowMoveSpeed;
            if (cancelCurrentSprint && _playerInputs != null)
            {
                _playerInputs.sprint = false;
            }
        }

        if (useSlowWalkAnimation && _playerAnimator != null)
        {
            _playerAnimator.enabled = true;
            _playerAnimator.SetBool(slowWalkAnimParameter, true);
            _playerAnimator.Update(0f);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player") || playerHealthSlider == null)
            return;

        // ✅ 修复 return 逻辑
        if (pickupManager != null && pickupManager.propACount >= 3)
        {
            // Debug.Log("拾取3个PropA，停止扣血");
            return;
        }

        float damage = damagePerSecond * Time.deltaTime;
        playerHealthSlider.healthSlider.value = Mathf.Max(0f, playerHealthSlider.healthSlider.value - damage);
    }

    private void Update()
    {
        if (!isInSlowZone || _playerInputs == null || _playerAnimator == null || _playerController.isDead)
            return;

        // ✅ 空引用保护 + 动画速度控制
        if (playerHealthSlider != null && playerHealthSlider.healthSlider.value <= 0f)
        {
            _playerAnimator.speed = 1;
        }
        else
        {
            _playerAnimator.speed = _playerInputs.move == Vector2.zero ? 0 : 1;
        }
    }

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
