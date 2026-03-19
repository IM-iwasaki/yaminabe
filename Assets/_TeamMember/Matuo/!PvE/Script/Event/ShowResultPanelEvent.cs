using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class ShowResultPanelEvent : PVEStageEvent {

    [Header("表示するリザルトパネル")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button returnLobbyButton;

    private bool isResultActive = false;     // 二重押し防止
    private ResultManager resultManager;

    private void Start() {
        // ボタンイベント登録
        if (nextButton != null)
            nextButton.onClick.AddListener(OnClickNextStage);
        if (returnLobbyButton != null)
            returnLobbyButton.onClick.AddListener(OnClickReturnLobby);
    }

    protected override void Execute() {
        if (resultPanel == null) return;

        bool isHost = NetworkServer.active;   // ホスト判定

        if (isHost) {
            GameManager.Instance.EndGame();
        }

        // PvEリザルトに個人スコアを送る
        ResultManager.Instance.ShowPvEOnResult(new ResultManager.ResultData {
            isTeamBattle = true,
            scores = new ResultScoreData[0],
        });

        // ホストがオプションを開いていたら閉じる
        if (isHost) {
            OptionMenu optionMenu = FindObjectOfType<OptionMenu>();
            if (optionMenu != null && optionMenu.isOpen) {
                optionMenu.ToggleMenu();
            }
        }

        // ここでホストだけボタン表示
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

        // 次のステージへ
        GameSceneManager.Instance.LoadPvESceneForAll();
        PlayerListManager.Instance.ResetAllScores();
    }

    public void OnClickReturnLobby() {
        if (!NetworkServer.active) return;

        RuleManager.Instance?.Initialize();
        GameManager.Instance.EndGame();
        //プレイヤーの状態を戻す
        ServerManager.instance.ResetCharacterStatus();
        GameSceneManager.Instance.LoadLobbySceneForAll();
        PlayerListManager.Instance.ResetAllScores();
    }
}