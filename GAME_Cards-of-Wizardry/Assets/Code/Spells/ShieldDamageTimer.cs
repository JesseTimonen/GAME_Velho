using TMPro;
using UnityEngine;

public class ShieldDamageTimer : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    private float shieldDuration = 10f;
    private TextMeshProUGUI countdownText;
    private float currentTime;
    private bool timerIsActive = false;


    private void Start()
    {
        countdownText = GetComponent<TextMeshProUGUI>();
    }


    private void Update()
    {
        if (timerIsActive)
        {
            currentTime -= Time.deltaTime;
            if (currentTime <= 0)
            {
                currentTime = 0;
                timerIsActive = false;
            }

            countdownText.text = Mathf.Ceil(currentTime).ToString("F0") + "s";
        }
    }


    public void InitializeTimer()
    {
        currentTime = shieldDuration;
        timerIsActive = true;
    }
}
