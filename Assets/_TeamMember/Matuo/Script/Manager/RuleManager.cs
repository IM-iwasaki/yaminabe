using UnityEngine;
using Mirror;
using System.Collections.Generic;

/// <summary>
/// ルール管理
/// エリア / ホコ / デスマッチのスコア管理・勝敗判定
/// </summary>
public class RuleManager : NetworkSystemObject<RuleManager> {
    public Dictionary<int, float> teamScores = new(); // チームスコア
    public GameRuleType currentRule = GameRuleType.Area;

    public Dictionary<GameRuleType, float> winScores = new() {
        { GameRuleType.Area, 50f },
        { GameRuleType.Hoko, 50f },
        { GameRuleType.DeathMatch, 0f }
    };

    // 報酬配布の二重防止
    private bool hasDistributedRewards = false;

    public override void Initialize() {
        base.Initialize();
        teamScores.Clear();
        InitializeScores();
    }

    /// <summary>
    /// 全チームのスコアを 0 に初期化する
    /// </summary>
    [Server]
    public void InitializeScores() {
        foreach (int teamId in new int[] { 0, 1 }) {
            SetInitialScore(teamId, 0f);
        }
        hasDistributedRewards = false;
    }

    /// <summary>
    /// 指定チームのスコアを初期化してクライアントに通知
    /// </summary>
    [Server]
    public void SetInitialScore(int teamId, float value) {
        teamScores[teamId] = value;
        RpcUpdateScore(teamId, value);
    }

    /// <summary>
    /// エリア / ホコの進行度加算通知
    /// </summary>
    [Server]
    public void OnCaptureProgress(int teamId, float amount) {
        if (!GameManager.Instance.IsGameRunning())
            return;
        if (currentRule != GameRuleType.DeathMatch)
        AddScore(teamId, amount, currentRule);
    }

    /// <summary>
    /// デスマッチのキル通知
    /// </summary>
    [Server]
    public void OnTeamKillByTeam(int teamId) {
        AddScore(teamId, 1f, GameRuleType.DeathMatch);
    }

    /// <summary>
    /// スコア加算処理（全ルール共通でカウントアップ）
    /// </summary>
    [Server]
    private void AddScore(int teamId, float amount, GameRuleType rule) {
        // ゲームが動いていない or タイマー終了なら加算しない
        if (!GameManager.Instance.IsGameRunning() || GameTimer.Instance.GetRemainingTime() <= 0f)
            return;

        if (!teamScores.ContainsKey(teamId))
            teamScores[teamId] = 0f;

        // Area / Hoko の場合、スコア上限に達していたら加算しない
        if (rule != GameRuleType.DeathMatch) {
            float targetScore = winScores[rule];
            if (teamScores[teamId] >= targetScore)
                return;
        }

        teamScores[teamId] += amount;
        RpcUpdateScore(teamId, teamScores[teamId]);

        if (rule != GameRuleType.DeathMatch)
            CheckWinConditionAllTeams();
    }

    /// <summary>
    /// クライアントにスコア更新通知
    /// </summary>
    [ClientRpc]
    private void RpcUpdateScore(int teamId, float newScore) {
        teamScores[teamId] = newScore;
        GameUIManager.Instance?.UpdateTeamScore(teamId, newScore);
    }

    /// <summary>
    /// 勝利条件チェック
    /// Area / Hoko：目標値到達
    /// 時間切れ時：最大スコア
    /// </summary>
    [Server]
    public void CheckWinConditionAllTeams(bool isTimeUp = false) {
        if (!GameManager.Instance.IsGameRunning())
            return;

        if (currentRule == GameRuleType.DeathMatch) {
            EndDeathMatch();
            return;
        }

        float target = winScores[currentRule];
        int winnerId = -1;
        bool multiple = false;

        foreach (var kvp in teamScores) {
            if (kvp.Value >= target) {
                if (winnerId == -1)
                    winnerId = kvp.Key;
                else
                    multiple = true;
            }
        }

        if (multiple)
            winnerId = -1;
        else if (winnerId == -1 && isTimeUp) {
            float max = -1f;
            foreach (var kvp in teamScores) {
                if (kvp.Value > max) {
                    max = kvp.Value;
                    winnerId = kvp.Key;
                } else if (Mathf.Approximately(kvp.Value, max)) {
                    winnerId = -1;
                }
            }
        } else if (winnerId == -1) {
            return;
        }

        SendTeamResultToAll(winnerId);
        PlayerRankingManager.instance.ApplyRateAllPlayers(winnerId);
        GameManager.Instance.EndGame();
    }

    /// <summary>
    /// デスマッチ終了時の勝利判定（最大スコア）
    /// </summary>
    [Server]
    public void EndDeathMatch() {
        float maxScore = -1f;
        List<int> topTeams = new();

        foreach (var kvp in teamScores) {
            if (kvp.Value > maxScore) {
                maxScore = kvp.Value;
                topTeams.Clear();
                topTeams.Add(kvp.Key);
            } else if (Mathf.Approximately(kvp.Value, maxScore)) {
                topTeams.Add(kvp.Key);
            }
        }

        SendTeamResultToAll(topTeams.Count == 1 ? topTeams[0] : -1);
    }

    /// <summary>
    /// 指定チームのスコア取得
    /// </summary>
    public bool TryGetTeamScore(int teamId, out float score) {
        return teamScores.TryGetValue(teamId, out score);
    }

    /// <summary>
    /// 勝敗結果と報酬を全プレイヤーに送信
    /// </summary>
    [Server]
    private void SendTeamResultToAll(int winningTeamId) {
        if (hasDistributedRewards) return;
        hasDistributedRewards = true;

        if (ResultManager.Instance == null) return;

        foreach (var conn in NetworkServer.connections) {
            var client = conn.Value;
            if (client.identity == null) continue;

            var player = client.identity.GetComponent<CharacterBase>();
            if (player == null) continue;

            int myTeam = player.parameter.TeamID;
            int reward = (winningTeamId == -1) ? 50 : (myTeam == winningTeamId ? 100 : 50);
            TargetRewardMoney(client, reward);
        }

        // 勝利結果表示
        string winnerName = winningTeamId switch {
            0 => "Red",
            1 => "Blue",
            _ => "Draw"
        };

        List<ResultManager.TeamScoreEntry> teamScoreList = new();
        foreach (var kvp in teamScores) {
            teamScoreList.Add(new ResultManager.TeamScoreEntry {
                teamId = kvp.Key,
                teamScore = kvp.Value
            });
        }

        ResultManager.Instance.ShowTeamResult(new ResultManager.ResultData {
            isTeamBattle = true,
            winnerName = winnerName,
            scores = new ResultScoreData[0],
            rule = currentRule,
            teamScores = teamScoreList.ToArray(),
        });
    }

    /// <summary>
    /// クライアント側で報酬を付与する
    /// </summary>
    [TargetRpc]
    private void TargetRewardMoney(NetworkConnection target, int reward) {
        PlayerWallet.Instance?.AddMoney(reward);
    }

    /// <summary>
    /// ルール切替時にスコアを初期化する
    /// </summary>
    [Server]
    public void InitializeScoresForRule(GameRuleType rule) {
        currentRule = rule;
        InitializeScores();
    }

    /// <summary>
    /// 現在のルールがデスマッチか
    /// </summary>
    public bool IsDeathMatch() {
        return currentRule == GameRuleType.DeathMatch;
    }
}