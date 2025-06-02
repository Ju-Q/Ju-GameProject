using StarterAssets;
using UnityEngine;
using System.Collections;


public class PlayerLadderClimb : MonoBehaviour
{
    public float climbSpeed = 2f;
    public bool isOnLadder = false;
    public bool isClimbing = false;

    private CharacterController controller;
    public Animator animator;
    private LadderZone currentLadder;
    private StarterAssetsInputs inputs; // 自带 StarterAssets 输入脚本

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        inputs = GetComponent<StarterAssetsInputs>();
    }

    public void EnterLadder(LadderZone ladder)
    {
        currentLadder = ladder;
        isOnLadder = true;
        // 提示玩家按F
    }

    public void ExitLadder()
    {
        isOnLadder = false;
        isClimbing = false;
        animator.SetBool("Climbing", false);
    }

    private void Update()
    {
        if (isOnLadder && !isClimbing && inputs.interact) // interact为F键输入
        {
            // 锁定角色位置到梯子前
            transform.position = new Vector3(currentLadder.transform.position.x, transform.position.y, currentLadder.transform.position.z);
            transform.forward = -currentLadder.transform.forward; // 面向梯子
            isClimbing = true;
            animator.SetBool("Climbing", true);
        }

        if (isClimbing)
        {
            float vertical = inputs.move.y;

            Vector3 climbDir = Vector3.up * vertical * climbSpeed;
            controller.Move(climbDir * Time.deltaTime);

            animator.SetFloat("ClimbSpeed", Mathf.Abs(vertical));

            // 到达顶端
            if (transform.position.y >= currentLadder.climbEndPoint.position.y)
            {
                StartCoroutine(FinishClimb("ClimbTop"));
            }

            // 到达底部
            if (transform.position.y <= currentLadder.climbStartPoint.position.y)
            {
                StartCoroutine(FinishClimb("ClimbBottom"));
            }
        }
    }

    private IEnumerator FinishClimb(string animTrigger)
    {
        animator.SetTrigger(animTrigger);
        isClimbing = false;

        // 等动画播放完再恢复
        yield return new WaitForSeconds(1.0f);
        animator.SetBool("Climbing", false);
        isOnLadder = false;
    }
}
