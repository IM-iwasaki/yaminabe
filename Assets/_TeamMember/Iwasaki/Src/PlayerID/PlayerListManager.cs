using Mirror;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 現在のプレイヤー一覧をサーバーで管理するマネージャー
/// 名前・ID・スコアを一元管理
/// ID（0〜5）を自動的に割り当て、抜けたら前詰めする
/// スコア加算　スコア送信
/// </summary>
public class PlayerListManager : NetworkBehaviour {
    public static PlayerListManager Instance;

    // 各プレイヤーの情報をまとめて持つ構造体
    [System.Serializable]
    public class PlayerInfo {
        public int id;        // プレイヤー番号（0〜5）
        public string name;   // プレイヤー名
        public int score;     // スコア

        public int kills;   // キル
        public int deaths;  // デス

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
    /// 名前からスコアを加算（サーバー専用）
    /// </summary>
    [Server]
    public void AddScoreByName(string playerName, int value) {
        var target = players.Find(p => p.name == playerName);
        if (target != null) {
            target.score += value;
            Debug.Log($"[PlayerListManager] {playerName} のスコアを {value} 加算（合計 {target.score}）");
        }
        else {
            Debug.LogWarning($"[PlayerListManager] 名前 '{playerName}' のプレイヤーが見つかりません。");
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




    // キル数
    [Server]
    public void AddKill(string name) {
        var p = players.Find(x => x.name == name);
        if (p != null) p.kills++;
    }
    // デス数
    [Server]
    public void AddDeath(string name) {
        var p = players.Find(x => x.name == name);
        if (p != null) p.deaths++;
    }




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
                PlayerName = p.name,
                Score = p.score,
                Kills = p.kills,
                Deaths = p.deaths,
                
            });
        }
        return list;
    }


    //==============================================================
    // ▼ リザルト送信処理（名前＋スコア付き）
    //==============================================================

    /// <summary>
    /// 現在の全プレイヤーの名前とスコアをリザルトに送信
    /// </summary>
    [Server]
    public void SendScoresToResult() {
        if (ResultManager.Instance == null) {
            Debug.LogWarning("[PlayerListManager] ResultManagerが見つかりません。");
            return;
        }

        // 名前とスコアをまとめて取得
        List<ResultScoreData> scoreList = GetResultDataList();

        // リザルトデータを作成
        ResultManager.ResultData resultData = new ResultManager.ResultData {
            isTeamBattle = false,                // チーム戦でなければfalse
            winnerName = "スコア一覧",           // 今は仮のタイトル（勝敗判定は別処理）
            scores = scoreList.ToArray()         // ← ここに名前＋スコアが入ってる
        };

        // 全クライアントへ送信
        ResultManager.Instance.ShowResult(resultData);
        Debug.Log("[PlayerListManager] 名前とスコアをResultManagerへ送信しました。");
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
