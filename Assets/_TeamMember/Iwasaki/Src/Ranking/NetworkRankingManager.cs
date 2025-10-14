using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

/// <summary>
/// サーバーでスコアを集計し、全員にランキングを送る
/// </summary>
public class NetworkRankingManager : NetworkBehaviour {
    public static NetworkRankingManager Instance;

    private void Awake() => Instance = this;

    /// <summary>
    /// ランキングを全員に送信
    /// </summary>
    [Server]
    public void ShowRankingToAll() {
        var allPlayers = FindObjectsOfType<PlayerScoreData>();

        // スコアの高い順にソート
        var sorted = allPlayers
            .OrderByDescending(p => p.PlayerScore)
            .Select(p => $"{p.PlayerName} - {p.PlayerScore}")
            .ToList();

        // クライアントへ送信
        RpcShowRanking(sorted.ToArray());
    }

    /// <summary>
    /// クライアント側でUI表示を更新
    /// </summary>
    [ClientRpc]
    void RpcShowRanking(string[] rankingData) {
        var ui = FindObjectOfType<RankingUIManager>();
        if (ui != null)
            ui.UpdateRankingDisplay(rankingData);
    }
}
