using UnityEngine;
using Mirror;
using System.Collections.Generic;

/// <summary>
/// ゲームルール管理
/// エリア制圧 / ホコ保持のカウント進行度を統合してスコア加算
/// </summary>
public class RuleManager : NetworkSystemObject<RuleManager> {
    private Dictionary<int, float> teamScores = new();

    public override void Initialize() {
        base.Initialize();
        teamScores.Clear();
    }

    /// <summary>
    /// オブジェクト制圧時
    /// </summary>
    [Server]
    public void OnObjectCaptured(CaptureObjectBase obj, int teamId) {
        if (!teamScores.ContainsKey(teamId))
            teamScores[teamId] = 0f;

        teamScores[teamId] += 1f;
        Debug.Log($"Team {teamId} Score: {teamScores[teamId]}");

        CheckWinCondition(teamId);
    }

    /// <summary>
    /// 進行度加算時（エリア制圧中やホコ保持中）
    /// </summary>
    [Server]
    public void OnCaptureProgress(int teamId, float amount) {
        if (!teamScores.ContainsKey(teamId))
            teamScores[teamId] = 0f;

        teamScores[teamId] += amount;
        Debug.Log($"Team {teamId} Score: {teamScores[teamId]}");

        CheckWinCondition(teamId);
    }

    /// <summary>
    /// 勝利条件チェック
    /// </summary>
    [Server]
    private void CheckWinCondition(int teamId) {
        if (teamScores[teamId] >= 10) {
            GameManager.Instance.EndGame();
            Debug.Log($"Team {teamId} 勝利！");
        }
    }
}