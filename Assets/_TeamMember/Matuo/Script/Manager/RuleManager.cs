using UnityEngine;
using Mirror;
using System.Collections.Generic;

/// <summary>
/// ルール管理
/// エリア制圧 / ホコ / デスマッチのスコア管理・勝敗判定
/// </summary>
public class RuleManager : NetworkSystemObject<RuleManager> {
    private Dictionary<int, float> teamScores = new();
    public GameRuleType currentRule = GameRuleType.Area;

    public Dictionary<GameRuleType, float> winScores = new()
    {
        // ゲームルール , 勝利に必要なカウント(デスマッチは最終的なキル数で決める)
        { GameRuleType.Area, 15f },
        { GameRuleType.Hoko, 20f },
        { GameRuleType.DeathMatch, 0f } // デスマッチは時間終了後判定
    };

    public override void Initialize() {
        base.Initialize();
        teamScores.Clear();
    }

    /// <summary>
    /// オブジェクト制圧時の通知
    /// </summary>
    [Server]
    public void OnObjectCaptured(CaptureObjectBase obj, int teamId) {
        if (currentRule != GameRuleType.DeathMatch)
            AddScore(teamId, 1f, currentRule);
    }

    /// <summary>
    /// カウント通知
    /// </summary>
    [Server]
    public void OnCaptureProgress(int teamId, float amount) {
        if (currentRule != GameRuleType.DeathMatch)
            AddScore(teamId, amount, currentRule);
    }

    /// <summary>
    /// キル通知
    /// </summary>
    [Server]
    public void OnTeamKill(int teamId, int kills) {
        if (currentRule == GameRuleType.DeathMatch)
            AddScore(teamId, kills, GameRuleType.DeathMatch);
    }

    /// <summary>
    /// スコア加算処理
    /// </summary>
    [Server]
    private void AddScore(int teamId, float amount, GameRuleType rule) {
        if (!teamScores.ContainsKey(teamId))
            teamScores[teamId] = 0f;

        teamScores[teamId] += amount;
        Debug.Log($"Team {teamId} Score: {teamScores[teamId]} (Rule: {rule})");

        if (rule != GameRuleType.DeathMatch)
            CheckWinConditionAllTeams();
    }

    /// <summary>
    /// 勝利条件チェック（エリア制圧・ホコ）
    /// </summary>
    [Server]
    private void CheckWinConditionAllTeams() {
        float maxScore = -1f;
        List<int> winners = new();

        foreach (var kvp in teamScores) {
            if (kvp.Value > maxScore) {
                maxScore = kvp.Value;
                winners.Clear();
                winners.Add(kvp.Key);
            } else if (kvp.Value == maxScore) {
                winners.Add(kvp.Key);
            }
        }

        if (maxScore >= winScores[currentRule]) {
            if (winners.Count == 1)
                Debug.Log($"Team {winners[0]} 勝利！(Rule: {currentRule})");
            else
                // 現在は引き分けになる。終了時にカウントが多い方の勝ちにする予定

            GameManager.Instance.EndGame();
        }
    }

    /// <summary>
    /// デスマッチ終了時に勝利チーム判定
    /// </summary>
    [Server]
    public void EndDeathMatch() {
        float maxScore = -1f;
        int winningTeam = -1;

        foreach (var kvp in teamScores) {
            if (kvp.Value > maxScore) {
                maxScore = kvp.Value;
                winningTeam = kvp.Key;
            }
        }

        if (winningTeam >= 0) {
            Debug.Log($"デスマッチ終了！Team {winningTeam} の勝利！");
        }
    }
}