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
    private StarterAssetsInputs inputs; // �Դ� StarterAssets ����ű�

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
        // ��ʾ��Ұ�F
    }

    public void ExitLadder()
    {
        isOnLadder = false;
        isClimbing = false;
        animator.SetBool("Climbing", false);
    }

    private void Update()
    {
        if (isOnLadder && !isClimbing && inputs.interact) // interactΪF������
        {
            // ������ɫλ�õ�����ǰ
            transform.position = new Vector3(currentLadder.transform.position.x, transform.position.y, currentLadder.transform.position.z);
            transform.forward = -currentLadder.transform.forward; // ��������
            isClimbing = true;
            animator.SetBool("Climbing", true);
        }

        if (isClimbing)
        {
            float vertical = inputs.move.y;

            Vector3 climbDir = Vector3.up * vertical * climbSpeed;
            controller.Move(climbDir * Time.deltaTime);

            animator.SetFloat("ClimbSpeed", Mathf.Abs(vertical));

            // ���ﶥ��
            if (transform.position.y >= currentLadder.climbEndPoint.position.y)
            {
                StartCoroutine(FinishClimb("ClimbTop"));
            }

            // ����ײ�
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

        // �ȶ����������ٻָ�
        yield return new WaitForSeconds(1.0f);
        animator.SetBool("Climbing", false);
        isOnLadder = false;
    }
}
