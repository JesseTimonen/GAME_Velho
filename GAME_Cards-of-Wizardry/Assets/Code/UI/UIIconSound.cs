using UnityEngine;
using UnityEngine.UI;

public class UIIconSound : MonoBehaviour
{
    [SerializeField] private AudioClip iconHoverAudio;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private GameObject title;
    private Button button;

    private void Start()
    {
        button = GetComponent<Button>();
    }

    public void PlayIconHoverSound()
    {
        if (title != null)
        {
            title.SetActive(true);
        }

        if (button != null)
        {
            if (!button.interactable)
            {
                return;
            }
        }

        audioSource.clip = iconHoverAudio;
        audioSource.Play();
    }

    public void ExitIcon()
    {
        title.SetActive(false);
    }
}
