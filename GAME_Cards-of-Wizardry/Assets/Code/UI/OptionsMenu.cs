using UnityEngine;

public class OptionsMenu : MonoBehaviour
{
    [SerializeField] private InputController inputController;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private AudioClip panelOpenAudio;
    [SerializeField] private AudioClip panelCloseAudio;
    [SerializeField] private AudioSource audioSource;
    private bool isOpen = false;

    private void Update()
    {
        if (inputController.OptionsPanelPressed && GameManager.Instance.UIPanelOpened == false)
        {
            OpenOptionsPanel();
        }
        else if ((inputController.OptionsPanelPressed || inputController.EscapePressed) && isOpen)
        {
            CloseOptionsPanel();
        }
    }

    public void OpenOptionsPanel()
    {
        GameManager.Instance.HideBasicUI();
        GameManager.Instance.UIPanelOpened = true;
        optionsPanel.SetActive(true);

        audioSource.clip = panelOpenAudio;
        audioSource.Play();

        Time.timeScale = 0;

        isOpen = true;
    }

    public void CloseOptionsPanel()
    {
        GameManager.Instance.ShowBasicUI();
        Time.timeScale = 1;
        optionsPanel.SetActive(false);


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
