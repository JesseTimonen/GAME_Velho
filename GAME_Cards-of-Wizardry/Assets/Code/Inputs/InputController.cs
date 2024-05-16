using UnityEngine;


public class InputController : MonoBehaviour
{
    private PlayerInputActions playerInputActions;

    public Vector2 Move { get; private set; }
    public bool EscapePressed { get; private set; }
    public bool ScreenshotPressed { get; private set; }
    public bool CharacterPanelPressed { get; private set; }
    public bool MasteryPanelPressed { get; private set; }
    public bool SpellBookPanelPressed { get; private set; }
    public bool ChallengesPanelPressed { get; private set; }
    public bool LeaderboardsPanelPressed { get; private set; }
    public bool OptionsPanelPressed { get; private set; }
    public bool HelpPanelPressed { get; private set; }
    public bool IsHorizontalMovementPressed => Mathf.Abs(Move.x) > 0;
    public bool IsVerticalMovementPressed => Mathf.Abs(Move.y) > 0;


    private PlayerInputActions PlayerInputActions
    {
        get
        {
            if (playerInputActions == null)
            {
                playerInputActions = new PlayerInputActions();
            }
            return playerInputActions;
        }
    }


    private void OnEnable()
    {
        PlayerInputActions.Enable();
    }


    private void OnDisable()
    {
        PlayerInputActions.Disable();
    }


    private void Update()
    {
        if (GameManager.Instance.gameHasStarted)
        {
            // Process inputs for this frame
            Move = PlayerInputActions.DefaultGameplay.Move.ReadValue<Vector2>();
            ScreenshotPressed = PlayerInputActions.DefaultGameplay.Screenshot.WasPressedThisFrame();
            EscapePressed = PlayerInputActions.DefaultGameplay.Escape.WasPressedThisFrame();
            CharacterPanelPressed = PlayerInputActions.DefaultGameplay.CharacterPanel.WasPressedThisFrame();
            MasteryPanelPressed = PlayerInputActions.DefaultGameplay.MasteryPanel.WasPressedThisFrame();
            SpellBookPanelPressed = PlayerInputActions.DefaultGameplay.SpellBookPanel.WasPressedThisFrame();
            ChallengesPanelPressed = PlayerInputActions.DefaultGameplay.ChallengesPanel.WasPressedThisFrame();
            LeaderboardsPanelPressed = PlayerInputActions.DefaultGameplay.LeaderboardsPanel.WasPressedThisFrame();
            OptionsPanelPressed = PlayerInputActions.DefaultGameplay.OptionsPanel.WasPressedThisFrame();
            HelpPanelPressed = PlayerInputActions.DefaultGameplay.HelpPanel.WasPressedThisFrame();
        }
    }
}
