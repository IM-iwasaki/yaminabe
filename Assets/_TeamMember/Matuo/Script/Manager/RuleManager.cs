using UnityEngine;
using Mirror;
using System.Collections.Generic;

/// <summary>
/// ルール管理
/// エリア / ホコ / デスマッチのスコア管理・勝敗判定
/// </summary>
public class RuleManager : NetworkSystemObject<RuleManager> {
    public Dictionary<int, float> teamScores = new(); // チームスコア
    public GameRuleType currentRule = GameRuleType.Area; // 現在のルール
    public HashSet<int> winningTeams = new(); // 勝利チーム
    public Dictionary<int, float> penaltyScores = new(); // ペナルティスコア保持用

    public Dictionary<GameRuleType, float> winScores = new()
    {
        // ゲームルール , 勝利に必要なカウント(デスマッチは最終的なキル数で決めるため0)
        { GameRuleType.Area, 50f },
        { GameRuleType.Hoko, 50f },
        { GameRuleType.DeathMatch, 0f } // デスマッチは時間終了後に判定
    };

    public override void Initialize() {
        base.Initialize();
        teamScores.Clear();
        penaltyScores.Clear();
    }

    [Server]
    public void SetInitialScore(int teamId, float value) {
        teamScores[teamId] = value;
        RpcUpdateScore(teamId, value);
    }

    /// <summary>
    /// オブジェクトを取った時の通知
    /// </summary>
    /// <param name="obj">勝利オブジェクト</param>
    /// <param name="teamId">取ったチーム</param>
    [Server]
    public void OnObjectCaptured(CaptureObjectBase obj, int teamId) {
        if (currentRule != GameRuleType.DeathMatch)
            AddScore(teamId, 1f, currentRule);
    }

    /// <summary>
    /// カウント通知 (エリアやホコなどのカウントを使うルール用)
    /// </summary>
    [Server]
    public void OnCaptureProgress(int teamId, float amount) {
        if (currentRule != GameRuleType.DeathMatch)
            AddScore(teamId, amount, currentRule);
    }

    /// <summary>
    /// キル通知 (デスマッチ用)
    /// </summary>
    [Server]
    public void OnTeamKillByTeam(int teamId) {
        if (winningTeams.Contains(teamId))
            return; // 勝利済みならキル加算しない

        if (!teamScores.ContainsKey(teamId))
            teamScores[teamId] = 0f;

        teamScores[teamId] += 1f;
        // UI 更新
        RpcUpdateScore(teamId, teamScores[teamId]);
    }

    /// <summary>
    /// スコア加算処理
    /// </summary>
    [Server]
    private void AddScore(int teamId, float amount, GameRuleType rule) {
        if (winningTeams.Contains(teamId))
            return;

        if (!teamScores.ContainsKey(teamId))
            teamScores[teamId] = winScores[rule];

        if (rule == GameRuleType.DeathMatch) {
            // デスマッチは加算
            teamScores[teamId] += amount;
        } else {
            // Area/Hokoは減算
            teamScores[teamId] -= amount;

            // 相手チームにペナルティ加算
            foreach (var kvp in teamScores) {
                int otherTeamId = kvp.Key;
                if (otherTeamId != teamId) {
                    if (!penaltyScores.ContainsKey(otherTeamId))
                        penaltyScores[otherTeamId] = 0f;

                    float maxScore = winScores[rule];
                    float penaltyAmount = (amount / maxScore) * 50f; // 自動計算比率
                    penaltyScores[otherTeamId] += penaltyAmount;
                }
            }

            if (teamScores[teamId] < 0)
                teamScores[teamId] = 0;
        }

        RpcUpdateScore(teamId, teamScores[teamId]);
    }

    /// <summary>
    /// クライアント全員にスコア更新を通知
    /// </summary>
    [ClientRpc]
    private void RpcUpdateScore(int teamId, float newScore) {
        // ローカルにも保持しておく（UIなどが参照できるように）
        teamScores[teamId] = newScore;

        // UIを更新
        GameUIManager.Instance?.UpdateTeamScore(teamId, newScore);
    }

    /// <summary>
    /// 勝利条件チェック（エリアとホコ）
    /// </summary>
    [Server]
    public void CheckWinConditionAllTeams() {
        if (currentRule == GameRuleType.DeathMatch) {
            EndDeathMatch();
            return;
        }

        float minScore = float.MaxValue;
        List<int> winners = new();

        foreach (var kvp in teamScores) {
            if (kvp.Value < minScore) {
                minScore = kvp.Value;
                winners.Clear();
                winners.Add(kvp.Key);
            } else if (Mathf.Approximately(kvp.Value, minScore)) {
                winners.Add(kvp.Key);
            }
        }

        if (winners.Count == 1) {
            int winningTeam = winners[0];

            SendTeamResultToAll(winningTeam);
            PlayerRankingManager.instance.ApplyRateAllPlayers(winningTeam);
        } else {
            SendTeamResultToAll(-1);
        }

        GameManager.Instance.EndGame();
    }

    /// <summary>
    /// デスマッチ終了時に勝利チーム判定（同点なら引き分け）
    /// </summary>
    [Server]
    public void EndDeathMatch() {
        float maxScore = -1f;
        List<int> topTeams = new();

        // 最高スコアを持つチームを抽出
        foreach (var kvp in teamScores) {
            if (kvp.Value > maxScore) {
                maxScore = kvp.Value;
                topTeams.Clear();
                topTeams.Add(kvp.Key);
            } else if (Mathf.Approximately(kvp.Value, maxScore)) {
                // 同点の場合もリストに追加
                topTeams.Add(kvp.Key);
            }
        }

        // 勝利判定
        if (topTeams.Count == 1) {
            SendTeamResultToAll(topTeams[0]);
        } else {
            SendTeamResultToAll(-1);
        }
    }

    /// <summary>
    /// チームの現在スコアを取得
    /// </summary>
    public bool TryGetTeamScore(int teamId, out float score) {
        return teamScores.TryGetValue(teamId, out score);
    }

    /// <summary>
    /// チームの勝敗結果を ResultManager に送信する（ルール対応版）
    /// </summary>
    [Server]
    private void SendTeamResultToAll(int winningTeamId) {
        if (ResultManager.Instance == null) {
            Debug.LogError("[RuleManager] ResultManager が存在しません！");
            return;
        }

        // チーム名 0=Red, 1=Blue, それ以外は Team {id}
        string winnerName;
        if (winningTeamId == 0)
            winnerName = "Red";
        else if (winningTeamId == 1)
            winnerName = "Blue";
        else
            winnerName = "Draw";

        // チームスコアを ResultData 形式に変換
        // 配列に変換
        List<ResultManager.TeamScoreEntry> teamScoreList = new();

        foreach (var kvp in teamScores) {
            teamScoreList.Add(new ResultManager.TeamScoreEntry {
                teamId = kvp.Key,
                teamScore = kvp.Value
            });
        }

        // ResultData を作成
        ResultManager.ResultData data = new ResultManager.ResultData {
            isTeamBattle = true,
            winnerName = winnerName,
            scores = new ResultScoreData[0],
            rule = currentRule,
            teamScores = teamScoreList.ToArray(),
        };

        // ResultManager に送信
        ResultManager.Instance.ShowTeamResult(data);
    }
}