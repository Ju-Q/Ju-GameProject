using UnityEngine;

public class AnimationEventReceiver : MonoBehaviour
{
    public AudioSource footstepAudio;
    public AudioClip[] footstepClips;
    public AudioSource onlandAudio;
    public AudioClip[] onlandClips;

    // ½Å²½ÊÂ¼þ
    public void OnFootstep()
    {
        if (footstepAudio != null && footstepClips.Length > 0)
        {
            int index = Random.Range(0, footstepClips.Length);
            footstepAudio.PlayOneShot(footstepClips[index]);
        }
    }

    public void OnLand()
    {
        if (onlandAudio != null && onlandClips.Length > 0)
        {
            int index = Random.Range(0, onlandClips.Length);
            onlandAudio.PlayOneShot(onlandClips[index]);
        }
    }


}