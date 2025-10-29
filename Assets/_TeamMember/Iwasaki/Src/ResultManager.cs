using UnityEngine;
using Mirror;
using UnityEngine.UI;

/// <summary>
/// ゲーム終了後のリザルト画面管理
/// ・全員に表示（ClientRpc）
/// ・再戦・ロビー戻りボタンはホストのみ
/// </summary>
/// 


// 使い方

// これをゲームマネージャーかなんかに書く

//private ResultManager resultManager;

// void Start() {
// シーン内の ResultManager を探す
//resultManager = FindObjectOfType<ResultManager>();
//if (resultManager == null)
//    Debug.LogError("ResultManager がシーンに存在しません");
//}

//リザルト呼びたいタイミングでこれ呼ぶ
//resultManager.RpcShowResult(); // 全員にリザルト表示




public class ResultManager : NetworkBehaviour {
    [Header("UI参照")]
    [SerializeField] private GameObject resultPanel;   // リザルト全体パネル
    [SerializeField] private Button rematchButton;     // 再戦ボタン（ホスト専用）
    [SerializeField] private Button returnLobbyButton; // ロビー戻りボタン（ホスト専用）
    [SerializeField] private GameObject backgroundBlocker; // 背景半透明で操作ブロック

    private bool isResultActive = true;

    void Start() {
        if (resultPanel != null)
            resultPanel.SetActive(false);
        if (backgroundBlocker != null)
            backgroundBlocker.SetActive(false);

        // ボタンイベント登録
        if (rematchButton != null)
            rematchButton.onClick.AddListener(OnClickRematch);
        if (returnLobbyButton != null)
            returnLobbyButton.onClick.AddListener(OnClickReturnLobby);
    }

    // サーバーから呼ぶと全クライアントに表示される
    [ClientRpc]
    public void RpcShowResult() {
        if (resultPanel != null)
            resultPanel.SetActive(true);
        if (backgroundBlocker != null)
            backgroundBlocker.SetActive(true);

        // ボタンはホストのみ表示
        bool isHost = NetworkServer.active;
        if (rematchButton != null)
            rematchButton.gameObject.SetActive(isHost);
        if (returnLobbyButton != null)
            returnLobbyButton.gameObject.SetActive(isHost);

        isResultActive = true;
    }


    //　プレイヤーのスコアを取得(プレイヤーのスコア一覧表示)



    //　どっちのチームが勝ったかorデスマッチで誰が勝ったかどうか


    //　獲得した通貨などを表示










    private void OnClickRematch() {
        if (!isResultActive) return;

        Debug.Log("ホストが『再戦』を選択");
        CmdRequestRematch();
    }

    private void OnClickReturnLobby() {
        if (!isResultActive) return;

        Debug.Log("ホストが『ロビーに戻る』を選択");
        CmdReturnToLobby();
    }

    // サーバー側処理（実際のロジックは後で実装）
    [Command(requiresAuthority = false)]
    private void CmdRequestRematch() {
        Debug.Log("サーバー側で再戦処理を実装予定");
        // ここで全員を初期状態に戻す処理などを呼ぶ

    }

    [Command(requiresAuthority = false)]
    private void CmdReturnToLobby() {
        Debug.Log("サーバー側でロビー戻り処理を実装予定");
        // ここでロビーシーンに戻す処理を呼ぶ

    }
}
