using UnityEngine;

public class FadeOutOnTrigger : MonoBehaviour
{
    public GameObject targetObject; // 要逐漸消失的物體
    public string playerTag = "Player"; // 主角的Tag
    public float fadeDuration = 1f; // 消失所需的時間

    private bool isFading = false;
    private Material targetMaterial;
    private Color originalColor;

    void Start()
    {
        if (targetObject != null)
        {
            Renderer renderer = targetObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                targetMaterial = renderer.material; // 注意：這會在運行時複製一份材質
                originalColor = targetMaterial.color;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag) && !isFading)
        {
            StartCoroutine(FadeOut());
        }
    }

    private System.Collections.IEnumerator FadeOut()
    {
        isFading = true;

        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(originalColor.a, 0f, elapsedTime / fadeDuration);

            Color newColor = targetMaterial.color;
            newColor.a = alpha;
            targetMaterial.color = newColor;

            yield return null;
        }

        // 最後確保完全透明
        Color finalColor = targetMaterial.color;
        finalColor.a = 0f;
        targetMaterial.color = finalColor;

        // 你可以選擇直接關閉物體
        // targetObject.SetActive(false);
    }
}
