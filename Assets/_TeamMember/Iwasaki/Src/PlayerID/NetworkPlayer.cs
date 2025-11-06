using Mirror;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour {
    [SyncVar] public string playerName;  // 全員に同期される
    [SyncVar] public int score;

    public override void OnStartLocalPlayer() {
        // --- ① 名前を取得 ---
        string localName = PlayerPrefs.GetString("PlayerName", "NoName");

        // --- ② サーバーへ送信 ---
        CmdSetPlayerName(localName);
    }

    [Command]
    private void CmdSetPlayerName(string name) {
        playerName = name;
        score = 0;

        // --- ③ PlayerListManagerに登録 ---
        if (PlayerListManager.Instance != null)
            PlayerListManager.Instance.RegisterPlayer(this);

        Debug.Log($"[NetworkPlayer] サーバー登録完了: {playerName}");
    }

    public override void OnStopServer() {
        if (PlayerListManager.Instance != null)
            PlayerListManager.Instance.UnregisterPlayer(this);
    }
}
