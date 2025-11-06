using Mirror;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 現在のプレイヤー一覧をサーバーで管理するマネージャー
/// ID（0〜5）を自動的に割り当て、抜けたら前詰めする
/// </summary>
public class PlayerListManager : NetworkBehaviour {
    public static PlayerListManager Instance;

    private readonly List<CharacterBase> players = new();

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else {
            Destroy(gameObject);
            return;
        }
    }

    /// <summary>
    /// プレイヤー登録（接続時に呼ばれる）
    /// </summary>
    [Server]
    public void RegisterPlayer(CharacterBase player) {
        if (players.Contains(player))
            return;

        // 空いているIDを探す（0〜5まで）
        int assignedId = -1;
        for (int i = 0; i < 6; i++) {
            bool idUsed = players.Exists(p => p.playerId == i);
            if (!idUsed) {
                assignedId = i;
                break;
            }
        }

        if (assignedId == -1) {
            Debug.LogWarning("プレイヤー上限に達しています！（6人まで）");
            return;
        }

        player.playerId = assignedId;
        players.Add(player);

        Debug.Log($"[PlayerListManager] 登録: {player.PlayerName} → Player{assignedId + 1}");
    }

    /// <summary>
    /// プレイヤー削除（切断時に呼ばれる）
    /// </summary>
    [Server]
    public void UnregisterPlayer(CharacterBase player) {
        if (players.Contains(player)) {
            Debug.Log($"[PlayerListManager] 退室: {player.PlayerName} (Player{player.playerId + 1})");
            players.Remove(player);
        }

        // 抜けたら前詰め
        ReassignPlayerIds();
    }

    /// <summary>
    /// IDを詰め直す（抜けた時などに呼ぶ）
    /// </summary>
    [Server]
    private void ReassignPlayerIds() {
        players.Sort((a, b) => a.playerId.CompareTo(b.playerId));

        for (int i = 0; i < players.Count; i++) {
            players[i].playerId = i;
            Debug.Log($"[PlayerListManager] 再割り当て: {players[i].PlayerName} → Player{i + 1}");
        }
    }

    /// <summary>
    /// 登録されている全プレイヤーを取得
    /// </summary>
    public List<CharacterBase> GetAllPlayers() => players;

    /// <summary>
    /// 指定IDのプレイヤーを取得
    /// </summary>
    public CharacterBase GetPlayerById(int id) {
        return players.Find(p => p.playerId == id);
    }
}
