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





    //================================================================
    // ボタンイベント（ホストのみ有効）
    //================================================================

    private void OnClickRematch() {
        if (!isResultActive) return;
        isResultActive = false;

        Debug.Log("[ResultPanel] 再戦ボタン押下");
        if (NetworkServer.active && resultManager != null) {
            // スコア初期化
            RuleManager.Instance?.Initialize();

            GameSceneManager.Instance.LoadGameSceneForAll();
            resultManager.HideResult(); // 仮: UI削除のみ（再戦処理は後で追加）
        }
    }

    private void OnClickReturnLobby() {
        if (!isResultActive) return;
        isResultActive = false;

        Debug.Log("[ResultPanel] ロビー戻りボタン押下");
        if (NetworkServer.active && resultManager != null) {
            // スコア初期化
            RuleManager.Instance?.Initialize();
            GameSceneManager.Instance.LoadLobbySceneForAll();
            resultManager.HideResult(); // 仮: UI削除のみ（シーン切り替え処理は後で追加）
        }         
    }
}
