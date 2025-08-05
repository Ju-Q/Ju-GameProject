using UnityEngine;
using System.Collections;

public class AreaTriggerAnimatorAndSink : MonoBehaviour
{
    public Animator animator;
    public string triggerName;
    public bool sinkAfterTrigger = false;
    public GameObject sinkObject;
    public float delayBeforeSink = 2f;
    public float sinkDistance = 2f;
    public float sinkDuration = 1f;
    public string playerTag = "Player";

    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!triggered && other.CompareTag(playerTag))
        {
            triggered = true;
            animator.SetTrigger(triggerName);

            if (sinkAfterTrigger)
            {
                StartCoroutine(SinkObjectAfterDelay());
            }
        }
    }

    private IEnumerator SinkObjectAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeSink);

        if (sinkObject != null)
        {
            Vector3 startPos = sinkObject.transform.position;
            Vector3 endPos = startPos - new Vector3(0, sinkDistance, 0);

            float elapsedTime = 0f;
            while (elapsedTime < sinkDuration)
            {
                elapsedTime += Time.deltaTime;
                sinkObject.transform.position = Vector3.Lerp(startPos, endPos, elapsedTime / sinkDuration);
                yield return null;
            }

            sinkObject.SetActive(false);
        }
    }
}
