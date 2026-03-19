using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class ShowResultPanelEvent : PVEStageEvent {

    [Header("•\¦‚·‚éƒŠƒUƒ‹ƒgƒpƒlƒ‹")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button returnLobbyButton;

<<<<<<< HEAD
#pragma warning disable CS0414
    private bool isResultActive = false;     // “ñd‰Ÿ‚µ–h~
#pragma warning restore CS0414
=======
    private bool isResultActive = true;     // “ñd‰Ÿ‚µ–h~
>>>>>>> parent of bcf97692 (è­¦å‘Šã®åŸå› æ¶ˆã—ã¦ã¿ãŸ)
    private ResultManager resultManager;

    private void Start() {
        // ƒ{ƒ^ƒ“ƒCƒxƒ“ƒg“o˜^
        if (nextButton != null)
            nextButton.onClick.AddListener(OnClickNextStage);
        if (returnLobbyButton != null)
            returnLobbyButton.onClick.AddListener(OnClickReturnLobby);
    }

    protected override void Execute() {
        if (resultPanel == null) return;

        bool isHost = NetworkServer.active;   // ƒzƒXƒg”»’è

        if (isHost) {
            GameManager.Instance.EndGame();
        }

        // PvEƒŠƒUƒ‹ƒg‚ÉŒÂlƒXƒRƒA‚ğ‘—‚é
        ResultManager.Instance.ShowPvEOnResult(new ResultManager.ResultData {
            isTeamBattle = true,
            scores = new ResultScoreData[0],
        });

        // ƒzƒXƒg‚ªƒIƒvƒVƒ‡ƒ“‚ğŠJ‚¢‚Ä‚¢‚½‚ç•Â‚¶‚é
        if (isHost) {
            OptionMenu optionMenu = FindObjectOfType<OptionMenu>();
            if (optionMenu != null && optionMenu.isOpen) {
                optionMenu.ToggleMenu();
            }
        }

        // ‚±‚±‚ÅƒzƒXƒg‚¾‚¯ƒ{ƒ^ƒ“•\¦
        if (nextButton != null)
            nextButton.gameObject.SetActive(isHost);

        if (returnLobbyButton != null)
            returnLobbyButton.gameObject.SetActive(isHost);

        resultPanel.SetActive(true);

        isResultActive = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void OnClickNextStage() {
        if (!NetworkServer.active) return;

        RuleManager.Instance?.Initialize();
        GameManager.Instance.EndGame();

        // Ÿ‚ÌƒXƒe[ƒW‚Ö
        GameSceneManager.Instance.LoadPvESceneForAll();
        PlayerListManager.Instance.ResetAllScores();
    }

    public void OnClickReturnLobby() {
        if (!NetworkServer.active) return;

        RuleManager.Instance?.Initialize();
        GameManager.Instance.EndGame();
        //ƒvƒŒƒCƒ„[‚Ìó‘Ô‚ğ–ß‚·
        ServerManager.instance.ResetCharacterStatus();
        GameSceneManager.Instance.LoadLobbySceneForAll();
        PlayerListManager.Instance.ResetAllScores();
    }
}