using System;
using StarterAssets;
using UnityEngine;

public class SlowZone : MonoBehaviour
{
    [Header("��������")]
    public float slowMoveSpeed = 1.5f;
    public bool disableSprint = true;
    public bool cancelCurrentSprint = true;

    [Header("��������")]
    public bool useSlowWalkAnimation = true;
    public string slowWalkAnimParameter = "SlowWalk";
    public float animationTransitionTime = 0.2f;

    [Tooltip("�ֶ�ָ������Animator���Ӷ���������ڸ������ϣ�")]
    public Transform animatorTarget; // �������ֶ�ָ��Animator���ڶ���

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
            // ��ȡ�������ϵ����
            _playerController = player.GetComponentInParent<ThirdPersonController>();
            _playerInputs = player.GetComponentInParent<StarterAssetsInputs>();
            _characterController = player.GetComponentInParent<CharacterController>();

            // ����ʹ���ֶ�ָ����AnimatorĿ�꣬�����Զ�����
            if (animatorTarget != null)
            {
                _playerAnimator = animatorTarget.GetComponent<Animator>();
            }
            else
            {
                // �������㼶�в���Animator
                _playerAnimator = player.GetComponentInChildren<Animator>();
            }

            if (_playerAnimator == null)
            {
                Debug.LogError("δ�ҵ�Animator���������㼶���ֶ�ָ��animatorTarget");
            }

            // ����ԭʼֵ
            _originalMoveSpeed = _playerController.MoveSpeed;
            _originalSprintSpeed = _playerController.SprintSpeed;
        }
    }

    private void ApplySlowEffects()
    {
        // ���ü���
        _playerController.MoveSpeed = slowMoveSpeed;

        if (disableSprint)
        {
            _playerController.SprintSpeed = slowMoveSpeed;
            if (cancelCurrentSprint && _playerInputs != null)
            {
                _playerInputs.sprint = false;
            }
        }

        // ���ö�������
        if (useSlowWalkAnimation && _playerAnimator != null)
        {
            Debug.Log($"�������ö������� {slowWalkAnimParameter} Ϊ true (��ǰֵ: {_playerAnimator.GetBool(slowWalkAnimParameter)})");

            // ȷ��Animator������
            _playerAnimator.enabled = true;

            _playerAnimator.SetBool(slowWalkAnimParameter, true);
            _playerAnimator.Update(0f); // ǿ����������

            Debug.Log($"���ú�ֵ: {_playerAnimator.GetBool(slowWalkAnimParameter)} | ��������: {ParameterExists(_playerAnimator, slowWalkAnimParameter)}");
        }
    }

    private void Update()
    {
        if (isInSlowZone && _playerInputs != null)
        {
            _playerAnimator.speed = _playerInputs.move == Vector2.zero ? 0 : 1;
        }
        // float inputMagnitude = _playerInputs.analogMovement ? _playerInputs.move.magnitude : 1f;
        // Debug.Log("��������" + inputMagnitude)
    }

    // �������Ƿ���ڵĸ�������
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