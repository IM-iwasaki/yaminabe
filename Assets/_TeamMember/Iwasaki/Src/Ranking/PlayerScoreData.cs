using Mirror;
using UnityEngine;

/// <summary>
/// 各プレイヤーのスコアと名前を保持する
/// </summary>
public class PlayerScoreData : NetworkBehaviour {
    [SyncVar]
    private string playerName; // プレイヤー名（自動同期）

    [SyncVar]
    private int playerScore; // スコア（自動同期）

    public string PlayerName => playerName;
    public int PlayerScore => playerScore;

    public override void OnStartServer() {
        // サーバーでのみ名前を設定（仮の例）
        playerName = $"Player_{Random.Range(1, 1000)}";
        playerScore = 0;
    }

    /// <summary>
    /// スコアを加算する（サーバーでのみ呼ぶ）
    /// </summary>
    [Server]
    public void AddScore(int amount) {
        playerScore += amount;
    }

    /// <summary>
    /// 現在のスコアを取得
    /// </summary>
    public int GetScore() {
        return playerScore;
    }

    /// <summary>
    /// 現在の名前を取得
    /// </summary>
    public string GetName() {
        return playerName;
    }



    /// <summary>
    /// クライアント → サーバーへ スコア加算リクエスト
    /// </summary>
    [Command]
    public void CmdAddScore(int value) {
        AddScore(value); // サーバーで実行
        Debug.Log($"[Server] {playerName} のスコアを {value} 加算 (合計 {playerScore})");
    }

    /// <summary>
    /// クライアント → サーバーへ スコアリセットリクエスト
    /// </summary>
    [Command]
    public void CmdResetScore() {
        playerScore = 0;
        Debug.Log($"[Server] {playerName} のスコアをリセットしました");
    }
}
