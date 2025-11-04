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
    }

    /// <summary>
    /// 勝者 or チーム名をUIに反映。
    /// </summary>
    public void ShowWinner(string name, bool isTeamBattle) {
        if (winnerText == null) return;

        if (isTeamBattle) {
            winnerText.text = $"{name} Team Win!";
            // チームカラーに合わせて色分け（例：Red / Blue）
            winnerText.color = (name == "Red") ? Color.red : Color.blue;
        }
        else {
            winnerText.text = $"Winner : {name}";
            winnerText.color = Color.white;
        }

        Debug.Log($"[ResultPanel] 勝敗表示: {winnerText.text}");
    }

    //================================================================
    // ボタンイベント（ホストのみ有効）
    //================================================================

    private void OnClickRematch() {
        if (!isResultActive) return;
        isResultActive = false;

        Debug.Log("[ResultPanel] 再戦ボタン押下");
        if (NetworkServer.active && resultManager != null)

            GameSceneManager.Instance.LoadGameSceneForAll();
            resultManager.HideResult(); // 仮: UI削除のみ（再戦処理は後で追加）
    }

    private void OnClickReturnLobby() {
        if (!isResultActive) return;
        isResultActive = false;

        Debug.Log("[ResultPanel] ロビー戻りボタン押下");
        if (NetworkServer.active && resultManager != null)
                GameSceneManager.Instance.LoadLobbySceneForAll();
        resultManager.HideResult(); // 仮: UI削除のみ（シーン切り替え処理は後で追加）
    }
}
