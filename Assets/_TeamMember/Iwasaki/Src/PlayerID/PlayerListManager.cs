using Mirror;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Mirrorサーバー上で全プレイヤー（CharacterBase）を一元管理
/// </summary>
public class PlayerListManager : NetworkBehaviour {
    public static PlayerListManager Instance;

    private readonly List<CharacterBase> players = new();

    private void Awake() {
        Instance = this;

        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else {
            Destroy(gameObject);
        }
    }

    [Server]
    public void RegisterPlayer(CharacterBase player) {
        if (!players.Contains(player)) {
            players.Add(player);
            Debug.Log($"[PlayerListManager] 登録: {player.PlayerName}");
            RpcUpdatePlayerList(GetPlayerNames());
        }
    }

    [Server]
    public void UnregisterPlayer(CharacterBase player) {
        if (players.Remove(player)) {
            Debug.Log($"[PlayerListManager] 削除: {player.PlayerName}");
            RpcUpdatePlayerList(GetPlayerNames());
        }
    }

    [Server]
    public void UpdatePlayerName(CharacterBase player, string newName) {
        Debug.Log($"[PlayerListManager] 名前更新: {newName}");
        RpcUpdatePlayerList(GetPlayerNames());
    }

    [Server]
    private string[] GetPlayerNames() {
        List<string> names = new();
        foreach (var p in players)
            names.Add(p.PlayerName);
        return names.ToArray();
    }

    [ClientRpc]
    private void RpcUpdatePlayerList(string[] names) {
        Debug.Log($"[PlayerListManager] 参加者一覧: {string.Join(", ", names)}");
    }

    public IReadOnlyList<CharacterBase> GetAllPlayers() => players;

    public CharacterBase GetPlayerByName(string name) {
        return players.Find(p => p.PlayerName == name);
    }
}
