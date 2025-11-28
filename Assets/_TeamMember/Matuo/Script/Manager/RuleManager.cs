using UnityEngine;
using Mirror;
using System.Collections.Generic;

/// <summary>
/// ルール管理
/// エリア / ホコ / デスマッチのスコア管理・勝敗判定
/// </summary>
public class RuleManager : NetworkSystemObject<RuleManager> {
    public Dictionary<int, float> teamScores = new(); // チームスコア
    public Dictionary<int, float> penaltyScores = new(); // ペナルティスコア
    public GameRuleType currentRule = GameRuleType.Area;
    public HashSet<int> winningTeams = new();

    public Dictionary<GameRuleType, float> winScores = new()
    {
        { GameRuleType.Area, 50f },
        { GameRuleType.Hoko, 50f },
        { GameRuleType.DeathMatch, 0f }
    };

    // 報酬配布の二重防止フラグ
    private bool hasDistributedRewards = false;

    public override void Initialize() {
        base.Initialize();
        teamScores.Clear();
        penaltyScores.Clear();

        // 初期スコアを設定（ルールに応じて 50 または 0）
        InitializeScores();
    }

    /// <summary>
    /// ルールに応じてチームスコアを初期化
    /// Area/Hoko: 50スタート、DeathMatch: 0スタート
    /// </summary>
    [Server]
    public void InitializeScores() {
        float initialScore = (currentRule == GameRuleType.Area || currentRule == GameRuleType.Hoko)
            ? winScores[currentRule]
            : 0f;

        foreach (int teamId in new int[] { 0, 1 }) {
            SetInitialScore(teamId, initialScore);
        }
    }

    /// <summary>
    /// 指定チームのスコアを初期化してクライアントに通知
    /// </summary>
    [Server]
    public void SetInitialScore(int teamId, float value) {
        teamScores[teamId] = value;
        penaltyScores[teamId] = 0f;
        RpcUpdateScore(teamId, value);
    }

    /// <summary>
    /// 進行度通知（エリア / ホコ用）
    /// </summary>
    [Server]
    public void OnCaptureProgress(int teamId, float amount) {
        if (currentRule != GameRuleType.DeathMatch)
            AddScore(teamId, amount, currentRule);
    }

    /// <summary>
    /// キル通知（デスマッチ用）
    /// </summary>
    [Server]
    public void OnTeamKillByTeam(int teamId) {
        if (winningTeams.Contains(teamId))
            return;

        if (!teamScores.ContainsKey(teamId))
            teamScores[teamId] = 0f;

        teamScores[teamId] += 1f;
        RpcUpdateScore(teamId, teamScores[teamId]);
    }

    /// <summary>
    /// スコア加算処理
    /// Area/Hokoは減算方式
    /// DeathMatchは加算方式
    /// </summary>
    [Server]
    private void AddScore(int teamId, float amount, GameRuleType rule) {
        if (winningTeams.Contains(teamId))
            return;

        if (!teamScores.ContainsKey(teamId))
            teamScores[teamId] = (rule == GameRuleType.DeathMatch) ? 0f : winScores[rule];
        if (!penaltyScores.ContainsKey(teamId))
            penaltyScores[teamId] = 0f;

        if (rule == GameRuleType.DeathMatch) {
            teamScores[teamId] += amount;
        } else {
            // Area/Hoko の減算
            teamScores[teamId] -= amount;
            if (teamScores[teamId] < 0f)
                teamScores[teamId] = 0f;
        }

        RpcUpdateScore(teamId, teamScores[teamId]);

        if (rule == GameRuleType.Area || rule == GameRuleType.Hoko) {
            bool anyZero = false;
            foreach (var score in teamScores.Values) {
                if (score <= 0f) {
                    anyZero = true;
                    break;
                }
            }
            if (anyZero) {
                CheckWinConditionAllTeams();
            }
        }
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
    /// 勝利条件チェック（Area / Hoko 用）
    /// 0になったチームがいればそのチームが勝利
    /// 時間切れの場合はスコアが最小のチームを勝利にする
    /// </summary>
    [Server]
    public void CheckWinConditionAllTeams(bool isTimeUp = false) {
        if (currentRule == GameRuleType.DeathMatch) {
            EndDeathMatch();
            return;
        }

        // 0カウントのチームを抽出
        List<int> zeroCountTeams = new List<int>();
        foreach (var kvp in teamScores) {
            if (kvp.Value <= 0f)
                zeroCountTeams.Add(kvp.Key);
        }

        int winnerId = -1;

        if (zeroCountTeams.Count == 1) {
            // 通常通り、0になったチームが勝利
            winnerId = zeroCountTeams[0];
        } else if (zeroCountTeams.Count > 1) {
            // 複数チームが0になった場合は引き分け
            winnerId = -1;
        } else if (isTimeUp) {
            // 時間切れ時、0になったチームがいない場合はスコア最小チームを勝者にする
            float minScore = float.MaxValue;
            foreach (var kvp in teamScores) {
                if (kvp.Value < minScore) {
                    minScore = kvp.Value;
                    winnerId = kvp.Key;
                } else if (Mathf.Approximately(kvp.Value, minScore)) {
                    // 同率の場合は引き分け
                    winnerId = -1;
                }
            }
        } else {
            // まだ0になったチームがいない場合、勝者判定不要
            return;
        }

        SendTeamResultToAll(winnerId);
        GameManager.Instance.EndGame();
    }

    /// <summary>
    /// デスマッチ終了時の勝利判定
    /// </summary>
    [Server]
    public void EndDeathMatch() {
        float maxScore = -1f;
        List<int> topTeams = new List<int>();

        foreach (var kvp in teamScores) {
            if (kvp.Value > maxScore) {
                maxScore = kvp.Value;
                topTeams.Clear();
                topTeams.Add(kvp.Key);
            } else if (Mathf.Approximately(kvp.Value, maxScore)) {
                topTeams.Add(kvp.Key);
            }
        }

        if (topTeams.Count == 1)
            SendTeamResultToAll(topTeams[0]);
        else
            SendTeamResultToAll(-1);
    }

    /// <summary>
    /// 指定チームのスコア取得
    /// </summary>
    public bool TryGetTeamScore(int teamId, out float score) {
        return teamScores.TryGetValue(teamId, out score);
    }

    /// <summary>
    /// チームの勝敗結果を ResultManager に送信
    /// </summary>
    [Server]
    private void SendTeamResultToAll(int winningTeamId) {
        if (hasDistributedRewards) return; // ← 二重配布防止
        hasDistributedRewards = true;

        if (ResultManager.Instance == null) {
            Debug.LogError("[RuleManager] ResultManager が存在しません！");
            return;
        }

        foreach (var conn in NetworkServer.connections) {
            NetworkConnectionToClient client = conn.Value;
            if (client.identity == null)
                continue;

            var player = client.identity.GetComponent<CharacterBase>();
            if (player == null)
                continue;

            int myTeam = player.TeamID;

            int reward = (winningTeamId == -1) ? 50 : (myTeam == winningTeamId ? 100 : 50);
            TargetRewardMoney(client, reward);
        }

        // 勝利結果表示
        string winnerName = winningTeamId switch {
            0 => "Red",
            1 => "Blue",
            _ => "Draw"
        };

        List<ResultManager.TeamScoreEntry> teamScoreList = new List<ResultManager.TeamScoreEntry>();
        foreach (var kvp in teamScores) {
            teamScoreList.Add(new ResultManager.TeamScoreEntry {
                teamId = kvp.Key,
                teamScore = kvp.Value
            });
        }

        ResultManager.ResultData data = new ResultManager.ResultData {
            isTeamBattle = true,
            winnerName = winnerName,
            scores = new ResultScoreData[0],
            rule = currentRule,
            teamScores = teamScoreList.ToArray(),
        };

        ResultManager.Instance.ShowTeamResult(data);
    }

    /// <summary>
    /// クライアント側で報酬を付与する
    /// </summary>
    [TargetRpc]
    private void TargetRewardMoney(NetworkConnection target, int reward) {
        PlayerWallet.Instance?.AddMoney(reward);
        Debug.Log($"報酬 {reward} 円を受け取りました");
    }

    /// <summary>
    /// ルールに応じてチームスコアとペナルティを初期化
    /// GameManager から呼ぶと安全
    /// </summary>
    [Server]
    public void InitializeScoresForRule(GameRuleType rule) {
        currentRule = rule;
        float initial = (rule == GameRuleType.DeathMatch) ? 0f : winScores[rule];

        foreach (int teamId in new int[] { 0, 1 }) {
            SetInitialScore(teamId, initial);
            penaltyScores[teamId] = 0f;
        }
    }
}