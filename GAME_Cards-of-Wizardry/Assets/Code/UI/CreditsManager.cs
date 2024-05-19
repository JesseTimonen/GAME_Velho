using UnityEngine;


public class CreditsManager : MonoBehaviour
{
    public void OpenLink(string url)
    {
        Application.OpenURL(url);
    }
}
