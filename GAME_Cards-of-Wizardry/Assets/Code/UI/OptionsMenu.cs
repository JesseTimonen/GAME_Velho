using UnityEngine;
using UnityEngine.SceneManagement;

public class OptionsMenu : MonoBehaviour
{
    [SerializeField] private InputController inputController;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private AudioClip panelOpenAudio;
    [SerializeField] private AudioClip panelCloseAudio;
    [SerializeField] private AudioSource audioSource;

    private void Update()
    {
        if (inputController.OptionsPanelPressed)
        {
            if (optionsPanel.activeSelf)
            {
                CloseOptionsPanelUI();
            }
            else if (GameManager.Instance.UIPanelOpened == false)
            {
                OpenOptionsPanelUI();
            }
        }

        if (inputController.EscapePressed)
        {
            if (optionsPanel.activeSelf)
            {
                CloseOptionsPanelUI();
            }
        }
    }

    public void OpenOptionsPanelUI()
    {
        GameManager.Instance.HideBasicUI();
        GameManager.Instance.UIPanelOpened = true;
        optionsPanel.SetActive(true);

        audioSource.clip = panelOpenAudio;
        audioSource.Play();

        Time.timeScale = 0;
    }

    public void CloseOptionsPanelUI()
    {
        GameManager.Instance.ShowBasicUI();
        GameManager.Instance.UIPanelOpened = false;
        Time.timeScale = 1;
        optionsPanel.SetActive(false);


        audioSource.clip = panelCloseAudio;
        audioSource.Play();
    }

    public void RestartGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
