using Mirror;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 現在のプレイヤー一覧をサーバーで管理するマネージャー
/// 名前・ID・スコアを一元管理
/// ID（0〜5）を自動的に割り当て、抜けたら前詰めする
/// </summary>
public class PlayerListManager : NetworkBehaviour {
    public static PlayerListManager Instance;

    // 各プレイヤーの情報をまとめて持つ構造体
    [System.Serializable]
    public class PlayerInfo {
        public int id;        // プレイヤー番号（0〜5）
        public string name;   // プレイヤー名
        public int score;     // スコア

        public PlayerInfo(int id, string name, int score = 0) {
            this.id = id;
            this.name = name;
            this.score = score;
        }
    }

    // 現在参加中のプレイヤー一覧
    private readonly List<PlayerInfo> players = new();

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
    #region プレイヤー登録
    //==============================================================
    // ▼ 登録・削除処理
    //==============================================================

    /// <summary>
    /// プレイヤー登録（サーバー側）
    /// </summary>
    [Server]
    public void RegisterPlayer(CharacterBase player) {
        if (players.Exists(p => p.name == player.PlayerName)) return;

        // 空いているIDを探す（0〜5まで）
        int assignedId = -1;
        for (int i = 0; i < 6; i++) {
            if (!players.Exists(p => p.id == i)) {
                assignedId = i;
                break;
            }
        }

        if (assignedId == -1) {
            Debug.LogWarning("プレイヤー上限に達しています！（6人まで）");
            return;
        }

        players.Add(new PlayerInfo(assignedId, player.PlayerName));
        player.playerId = assignedId;

        Debug.Log($"[PlayerListManager] 登録: {player.PlayerName} → Player{assignedId + 1}");
    }

    /// <summary>
    /// プレイヤー削除（切断時）
    /// </summary>
    [Server]
    public void UnregisterPlayer(CharacterBase player) {
        var target = players.Find(p => p.id == player.playerId);
        if (target != null) {
            Debug.Log($"[PlayerListManager] 退室: {target.name} (Player{target.id + 1})");
            players.Remove(target);
        }

        ReassignPlayerIds();
    }

    /// <summary>
    /// IDを詰め直す（抜けたとき）
    /// </summary>
    [Server]
    private void ReassignPlayerIds() {
        players.Sort((a, b) => a.id.CompareTo(b.id));
        for (int i = 0; i < players.Count; i++) {
            players[i].id = i;
            Debug.Log($"[PlayerListManager] 再割り当て: {players[i].name} → Player{i + 1}");
        }
    }
    #endregion


    #region スコア関係
    //==============================================================
    // ▼ スコア管理
    //==============================================================

    /// <summary>
    /// 指定プレイヤーにスコアを加算
    /// </summary>
    [Server]
    public void PlayerAddScore(CharacterBase player, int value) {
        var target = players.Find(p => p.id == player.playerId);
        if (target != null) {
            target.score += value;
            Debug.Log($"[PlayerListManager] {target.name} のスコアを {value} 加算（合計 {target.score}）");
        }
    }

    /// <summary>
    /// 指定プレイヤーのスコアをリセット
    /// </summary>
    [Server]
    public void ResetScore(CharacterBase player) {
        var target = players.Find(p => p.id == player.playerId);
        if (target != null) {
            target.score = 0;
            Debug.Log($"[PlayerListManager] {target.name} のスコアをリセット");
        }
    }

    /// <summary>
    /// 全プレイヤーのスコアをリセット
    /// </summary>
    [Server]
    public void ResetAllScores() {
        foreach (var p in players) p.score = 0;
        Debug.Log("[PlayerListManager] 全スコアをリセットしました");
    }
    #endregion


    //==============================================================
    // ▼ リザルト連携
    //==============================================================

    /// <summary>
    /// 現在のスコアをリザルト用データに変換
    /// </summary>
    [Server]
    public List<ResultScoreData> GetResultDataList() {
        List<ResultScoreData> list = new();
        foreach (var p in players) {
            list.Add(new ResultScoreData {
                playerName = p.name,
                score = p.score
            });
        }
        Debug.Log("[PlayerListManager] リザルト用データを作成しました");
        return list;
    }

    //==============================================================
    // ▼ 補助関数
    //==============================================================

    /// <summary>
    /// 現在の全プレイヤー情報を取得
    /// </summary>
    public List<PlayerInfo> GetAllPlayers() => players;

    /// <summary>
    /// ID指定でプレイヤー情報を取得
    /// </summary>
    public PlayerInfo GetPlayerById(int id) => players.Find(p => p.id == id);
}
