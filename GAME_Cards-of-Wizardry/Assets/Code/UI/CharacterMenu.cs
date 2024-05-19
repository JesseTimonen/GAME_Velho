using UnityEngine;

public class CharacterMenu : MonoBehaviour
{
    [SerializeField] private InputController inputController;
    [SerializeField] private GameObject characterPanel;
    [SerializeField] private AudioClip panelOpenAudio;
    [SerializeField] private AudioClip panelCloseAudio;
    [SerializeField] private AudioSource audioSource;
    private bool isOpen = false;

    private void Update()
    {
        if (inputController.CharacterPanelPressed && GameManager.Instance.UIPanelOpened == false)
        {
            OpenCharacterPanel();
        }
        else if ((inputController.CharacterPanelPressed || inputController.EscapePressed) && isOpen)
        {
            CloseCharacterPanel();
        }
    }

    public void OpenCharacterPanel()
    {
        GameManager.Instance.HideBasicUI();
        GameManager.Instance.UIPanelOpened = true;
        characterPanel.SetActive(true);

        audioSource.clip = panelOpenAudio;
        audioSource.Play();

        Time.timeScale = 0;

        isOpen = true;
    }

    public void CloseCharacterPanel()
    {
        GameManager.Instance.ShowBasicUI();
        Time.timeScale = 1;
        characterPanel.SetActive(false);


        audioSource.clip = panelCloseAudio;
        audioSource.Play();

        isOpen = false;

        Invoke("CloseUIPanelReference", 0.1f);
    }

    private void CloseUIPanelReference()
    {
        GameManager.Instance.UIPanelOpened = false;
    }
}
