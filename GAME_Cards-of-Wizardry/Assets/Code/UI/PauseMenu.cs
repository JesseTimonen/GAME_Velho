using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private InputController inputController;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private AudioClip panelOpenAudio;
    [SerializeField] private AudioClip panelCloseAudio;
    [SerializeField] private AudioSource audioSource;
    private bool isOpen = false;

    private void Update()
    {
        // No other UI panel is open and escape pressed
        if (inputController.EscapePressed && GameManager.Instance.UIPanelOpened == false)
        {
            openPausePanel();
        }
        else if (isOpen && inputController.EscapePressed)
        {
            closePausePanel();
        }
    }

    public void openPausePanel()
    {
        GameManager.Instance.HideBasicUI();
        GameManager.Instance.UIPanelOpened = true;
        pausePanel.SetActive(true);

        audioSource.clip = panelOpenAudio;
        audioSource.Play();

        Time.timeScale = 0;

        isOpen = true;
    }

    public void closePausePanel()
    {
        GameManager.Instance.ShowBasicUI();
        Time.timeScale = 1;
        pausePanel.SetActive(false);


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
