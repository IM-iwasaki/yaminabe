using UnityEngine;
using UnityEngine.UI;
using Mirror;

/// <summary>
/// リザルト画面の勝敗表示およびボタン操作を制御するクラス。
/// 
/// ・ホストのみ「再戦」「ロビーに戻る」ボタンを押せる
/// ・チーム戦なら「Red Team Win!」、個人戦なら「Winner : 名前」
/// </summary>
public class ResultPanel : NetworkBehaviour {
    [Header("UI参照")]
    [SerializeField] private Text winnerText;           // 勝者テキスト
    [SerializeField] private Button rematchButton;      // 再戦ボタン（ホスト専用）
    [SerializeField] private Button returnLobbyButton;  // ロビー戻りボタン（ホスト専用）



    [Header("ルール別 UI")]
    [SerializeField] private GameObject areaPanel;       // エリア戦用
    [SerializeField] private GameObject hokoPanel;       // ホコ戦用
    [SerializeField] private GameObject deathMatchPanel; // デスマッチ用

    // チームスコア表示テキスト
    [SerializeField] private Text areaRedScoreText;
    [SerializeField] private Text areaBlueScoreText;
    [SerializeField] private Text hokoRedProgressText;
    [SerializeField] private Text hokoBlueProgressText;
    [SerializeField] private Text deathRedKillText;
    [SerializeField] private Text deathBlueKillText;





    private bool isResultActive = true;                 // 二重押し防止
    private ResultManager resultManager;

    private void Start() {
        resultManager = FindObjectOfType<ResultManager>();

        // ボタンイベント登録
        if (rematchButton != null)
            rematchButton.onClick.AddListener(OnClickRematch);
        if (returnLobbyButton != null)
            returnLobbyButton.onClick.AddListener(OnClickReturnLobby);
    }


    // ================================================================
    // ▼ ルールごとの UI を切り替えるための関数
    // ================================================================
    public void ShowRuleUI(GameRuleType rule) {

        // 一旦すべて非表示
        areaPanel?.SetActive(false);
        hokoPanel?.SetActive(false);
        deathMatchPanel?.SetActive(false);

        switch (rule) {
            case GameRuleType.Area:
                areaPanel?.SetActive(true);
                break;

            case GameRuleType.Hoko:
                hokoPanel?.SetActive(true);
                break;

            case GameRuleType.DeathMatch:
                deathMatchPanel?.SetActive(true);
                break;
        }
    }


    /// <summary>
    /// 全クライアントでリザルトUIを表示するRPC。
    /// </summary>

    public void RpcShowResult() {
        bool isHost = NetworkServer.active;

        if (rematchButton != null)
            rematchButton.gameObject.SetActive(isHost);
        if (returnLobbyButton != null)
            returnLobbyButton.gameObject.SetActive(isHost);

        if (winnerText != null)
            winnerText.text = "";

        isResultActive = true;


        // ルール別UIはいったん全部非表示にしておく
        areaPanel?.SetActive(false);
        hokoPanel?.SetActive(false);
        deathMatchPanel?.SetActive(false);

    }

    /// <summary>
    /// 勝者 or チーム名をUIに反映。
    /// </summary>
    public void ShowWinner(string name, bool isTeamBattle) {
        if (winnerText == null) return;

        // 引き分け専用処理
        if (name == "Draw") {
            winnerText.text = "Draw";
            winnerText.color = Color.yellow; // 見やすい色に
            return;
        }

        if (isTeamBattle) {
            winnerText.text = $"{name} Team Win!";
            // チームカラーに合わせて色分け（例：Red / Blue）
            winnerText.color = (name == "Red") ? Color.red : Color.blue;
        } else {
            winnerText.text = $"Winner : {name}";
            winnerText.color = Color.white;
        }
    }

    // エリアチームスコア表示用関数
    public void SetAreaScores(float redScore, float blueScore) {
        if (areaRedScoreText != null)
            areaRedScoreText.text = $"Red : {redScore:F0}%";

        if (areaBlueScoreText != null)
            areaBlueScoreText.text = $"Blue : {blueScore:F0}%";
    }

    // ホコチームスコア表示用関数
    public void SetHokoScores(float red, float blue, float holdTime = 0) {
        if (hokoRedProgressText != null)
            hokoRedProgressText.text = $"Red : {red:F0}";

        if (hokoBlueProgressText != null)
            hokoBlueProgressText.text = $"Blue : {blue:F0}";

    }

    // デスマッチチームスコア表示用関数
    public void SetDeathMatchScores(int redKills, int blueKills) {
        if (deathRedKillText != null)
            deathRedKillText.text = $"Red : {redKills}";

        if (deathBlueKillText != null)
            deathBlueKillText.text = $"Blue : {blueKills}";
    }





    //================================================================
    // ボタンイベント（ホストのみ有効）
    //================================================================

    private void OnClickRematch() {
        if (!isResultActive) return;
        isResultActive = false;

        Debug.Log("[ResultPanel] 再戦ボタン押下");
        if (NetworkServer.active && resultManager != null) {
            //  アイテムスポナーの自動リスポーン機能を停止
            ItemSpawnManager.Instance.ResetSpawnPoint();

            // スコア初期化
            RuleManager.Instance?.Initialize();
            GameManager.Instance.EndGame();
            GameSceneManager.Instance.LoadGameSceneForAll();
            resultManager.HideResult(); // 仮: UI削除のみ（再戦処理は後で追加）
        }
    }

    private void OnClickReturnLobby() {
        if (!isResultActive) return;
        isResultActive = false;

        Debug.Log("[ResultPanel] ロビー戻りボタン押下");
        if (NetworkServer.active && resultManager != null) {
            //  アイテムスポナーの自動リスポーン機能を停止
            ItemSpawnManager.Instance.ResetSpawnPoint();
            // スコア初期化
            RuleManager.Instance?.Initialize();
            GameManager.Instance.EndGame();
            GameSceneManager.Instance.LoadLobbySceneForAll();
            resultManager.HideResult(); // 仮: UI削除のみ（シーン切り替え処理は後で追加）
        }         
    }
}
