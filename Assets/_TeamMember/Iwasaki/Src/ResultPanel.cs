using UnityEngine;
using Mirror;
using UnityEngine.UI;

/// <summary>
/// リザルト画面のUI制御クラス（Commandを使わないシンプル版）
/// ・ホストはボタン操作でサーバー処理を直接実行
/// ・クライアントはボタンが非表示なので触れない
/// </summary>
public class ResultPanel : NetworkBehaviour {
    [Header("UI参照")]
    [SerializeField] private Button rematchButton;     // 再戦ボタン（ホスト専用）
    [SerializeField] private Button returnLobbyButton; // ロビー戻りボタン（ホスト専用）

    private bool isResultActive = true;
    private ResultManager resultManager;

    void Start() {
        // ResultManager の参照取得
        resultManager = FindObjectOfType<ResultManager>();

        // ボタンイベント登録
        if (rematchButton != null)
            rematchButton.onClick.AddListener(OnClickRematch);

        if (returnLobbyButton != null)
            returnLobbyButton.onClick.AddListener(OnClickReturnLobby);
        
    }

    /// <summary>
    /// 全員の画面にリザルトを表示（ResultManagerから呼ばれる）
    /// </summary>
    public void RpcShowResult() {
        Debug.Log("[ResultPanel] リザルト画面表示");

        // ホストのみボタン表示
        bool isHost = NetworkServer.active;
        if (rematchButton != null)
            rematchButton.gameObject.SetActive(isHost);
        if (returnLobbyButton != null)
            returnLobbyButton.gameObject.SetActive(isHost);

        isResultActive = true;
    }

    // ==================================================
    // ボタン処理（Commandを使わずに直接サーバー処理へ）
    // ==================================================

    private void OnClickRematch() {
        if (!isResultActive) return;
        isResultActive = false;

        Debug.Log("[ResultPanel] ホストが『再戦』を選択");

        // サーバー上でのみ実行
        if (NetworkServer.active && resultManager != null) {
            resultManager.HideResult();
            // TODO: 再戦ロジックをここに追加（シーンリロードなど）
            GameSceneManager.Instance.LoadGameSceneForAll();
        }
    }

    private void OnClickReturnLobby() {
        if (!isResultActive) return;
        isResultActive = false;

        Debug.Log("[ResultPanel] ホストが『ロビーに戻る』を選択");

        // サーバー上でのみ実行
        if (NetworkServer.active && resultManager != null) {
            resultManager.HideResult();
            // TODO: ロビーシーンに戻る処理をここに追加
            GameSceneManager.Instance.LoadTitleSceneForAll();
        }
    }
}
