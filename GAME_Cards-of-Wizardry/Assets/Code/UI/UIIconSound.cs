using UnityEngine;

public class UIIconSound : MonoBehaviour
{
    [SerializeField] private AudioClip iconHoverAudio;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private GameObject title;

    public void PlayIconHoverSound()
    {
        audioSource.clip = iconHoverAudio;
        audioSource.Play();

        if (title != null)
        {
            title.SetActive(true);
        }
    }

    public void ExitIcon()
    {
        title.SetActive(false);
    }
}
