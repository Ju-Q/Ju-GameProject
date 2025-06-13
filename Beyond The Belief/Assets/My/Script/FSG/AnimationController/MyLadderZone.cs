using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyLadderZone : MonoBehaviour
{
    [Header("爬梯子速度设置")]
    public float climbSpeed = 1.5f;

    [Header("动画设置")]
    public Animator _playerAnimator;
    private string climbarameter = "Climbing";

    public bool isOnLadder;

    private ThirdPersonController _playerController;
    private StarterAssetsInputs _playerInputs;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        InitializePlayerComponents(other);
        isOnLadder = true;
        _playerController.isInLadderZone = true;
        _playerAnimator.SetBool(climbarameter, true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        isOnLadder = false;

        _playerAnimator.speed = 1;
        _playerAnimator.SetBool(climbarameter, false);
        _playerController.isInLadderZone = false;
    }

    private void InitializePlayerComponents(Collider player)
    {
        if (_playerController == null)
        {
            // 获取根对象上的组件
            _playerController = player.GetComponentInParent<ThirdPersonController>();
            _playerInputs = player.GetComponentInParent<StarterAssetsInputs>();

            if (_playerAnimator == null)
            {
                Debug.LogError("未找到Animator组件！请检查层级或手动指定animatorTarget");
            }
        }
    }

    private void Update()
    {
        /*if(_playerInputs.move == Vector2.zero)
        {
            _playerAnimator.speed = 0;
        }
        else
        {
            _playerAnimator.speed = 1;
        }*/

        _playerAnimator.speed = _playerInputs.move == Vector2.zero ? 0 : 1;
    }
}
