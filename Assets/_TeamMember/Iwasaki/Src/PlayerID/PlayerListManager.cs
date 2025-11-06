using Mirror;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// サーバー上で全プレイヤーを一元管理。
/// 名前・スコア・IDなどを保持して、いつでも参照可能。
/// </summary>
public class PlayerListManager : NetworkBehaviour {
    public static PlayerListManager Instance;

    private readonly List<NetworkPlayer> players = new List<NetworkPlayer>();

    private void Awake() {
        Instance = this;
    }

    // --- プレイヤー登録 ---
    [Server]
    public void RegisterPlayer(NetworkPlayer player) {
        if (!players.Contains(player)) {
            players.Add(player);
            Debug.Log($"[PlayerListManager] 登録: {player.playerName}");
            RpcUpdateList(GetPlayerNames());
        }
    }

    // --- プレイヤー削除 ---
    [Server]
    public void UnregisterPlayer(NetworkPlayer player) {
        if (players.Remove(player)) {
            RpcUpdateList(GetPlayerNames());
            Debug.Log($"[PlayerListManager] 削除: {player.playerName}");
        }
    }

    // --- 現在の全プレイヤー名を取得（サーバーのみ） ---
    [Server]
    private string[] GetPlayerNames() {
        List<string> names = new List<string>();
        foreach (var p in players)
            names.Add(p.playerName);
        return names.ToArray();
    }

    // --- クライアントへ同期（確認用ログなど） ---
    [ClientRpc]
    private void RpcUpdateList(string[] names) {
        Debug.Log($"[PlayerListManager] 現在の参加者: {string.Join(", ", names)}");
    }

    // --- 外部から使えるゲッターたち ---
    public IReadOnlyList<NetworkPlayer> GetAllPlayers() => players;

    public NetworkPlayer GetPlayerByName(string name) {
        return players.Find(p => p.playerName == name);
    }

    public NetworkPlayer GetPlayerByIndex(int index) {
        if (index < 0 || index >= players.Count) return null;
        return players[index];
    }

    public string GetLocalPlayerName() {
        return PlayerPrefs.GetString("PlayerName", "NoName");
    }
}
