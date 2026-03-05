using UnityEngine;
using Mirror;
using System.Collections.Generic;

/// <summary>
/// ルール管理
/// エリア / ホコ / デスマッチのスコア管理・勝敗判定
/// </summary>
public class RuleManager : NetworkSystemObject<RuleManager> {
    public Dictionary<int, float> teamScores = new(); // チームスコア
    [SyncVar(hook = nameof(OnRuleChanged))]
    public GameRuleType currentRule = GameRuleType.Area;

    [SyncVar]
    private bool isOvertime = false;    // 延長戦用

    public Dictionary<GameRuleType, float> winScores = new() {
        { GameRuleType.Area, 50f },
        { GameRuleType.Hoko, 50f },
        { GameRuleType.DeathMatch, 0f }
    };

    // 報酬配布の二重防止
    private bool hasDistributedRewards = false;

    public override void Initialize() {
        base.Initialize();
        if (currentRule == GameRuleType.PvE)
            currentRule = GameRuleType.Hoko;
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
    /// ルール変更をクライアントに送る
    /// </summary>
    /// <param name="oldRule"></param>
    /// <param name="newRule"></param>
    private void OnRuleChanged(GameRuleType oldRule, GameRuleType newRule) {
        // クライアント側で UI を更新
        GameUIManager.Instance?.UpdateUI();
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
    /// 延長戦を開始する
    /// </summary>
    [Server]
    private void StartOvertime() {
        isOvertime = true;          // サーバー側で状態変更
        RpcStartOvertime();         // 全クライアントに通知
    }

    /// <summary>
    /// クライアント側で延長UIを表示する
    /// </summary>
    [ClientRpc]
    private void RpcStartOvertime() {
        GameUIManager.Instance?.ShowOvertime();
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
        // ゲームが止まっているなら何もしない
        if (!GameManager.Instance.IsGameRunning())
            return;

        // 通常時間でタイマーが0なら加算しない
        // ただし延長戦中は許可する
        if (GameTimer.Instance.GetRemainingTime() <= 0f && !isOvertime)
            return;

        // スコア辞書に未登録なら初期化
        if (!teamScores.ContainsKey(teamId))
            teamScores[teamId] = 0f;

        // Area / Hoko の場合は目標値を超えないように制限
        if (rule != GameRuleType.DeathMatch) {
            float targetScore = winScores[rule];
            if (teamScores[teamId] >= targetScore)
                return;
        }

        // スコア加算
        teamScores[teamId] += amount;

        // 全クライアントへスコア同期
        RpcUpdateScore(teamId, teamScores[teamId]);

        // 勝利条件チェック
        if (rule != GameRuleType.DeathMatch)
            CheckWinConditionAllTeams(false);
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
    /// 目標到達勝利
    /// 時間切れ時の延長判定
    /// 延長終了判定
    /// </summary>
    [Server]
    public void CheckWinConditionAllTeams(bool isTimeUp = false) {
        if (!GameManager.Instance.IsGameRunning())
            return;

        if (currentRule == GameRuleType.DeathMatch) {
            EndDeathMatch();
            return;
        }

        float red = teamScores.ContainsKey(0) ? teamScores[0] : 0f;
        float blue = teamScores.ContainsKey(1) ? teamScores[1] : 0f;
        float target = winScores[currentRule];

        // 目標到達勝利
        if (red >= target || blue >= target) {
            int winner = -1;

            if (red >= target && blue >= target)
                winner = -1;
            else if (red >= target)
                winner = 0;
            else
                winner = 1;

            SendTeamResultToAll(winner);
            PlayerRankingManager.Instance.ApplyRateAllPlayers(winner);
            GameManager.Instance.EndGame();
            return;
        }
        // 通常時間終了時の延長判定
        if (isTimeUp && !isOvertime) {
            int losingTeam = -1;

            if (red > blue) losingTeam = 1;
            else if (blue > red) losingTeam = 0;

            if (losingTeam != -1 && IsTeamControllingObject(losingTeam)) {
                StartOvertime();
                return;
            }

            // 延長条件なし → 即終了
            int winner = -1;
            if (red > blue) winner = 0;
            else if (blue > red) winner = 1;

            SendTeamResultToAll(winner);
            PlayerRankingManager.Instance.ApplyRateAllPlayers(winner);
            GameManager.Instance.EndGame();
            return;
        }

        // 延長中の終了判定
        if (isOvertime) {
            int losingTeam = -1;

            if (red > blue) losingTeam = 1;
            else if (blue > red) losingTeam = 0;

            // 同点ならまだ継続
            if (losingTeam == -1)
                return;

            // 負けチームが保持していない → 終了
            if (!IsTeamControllingObject(losingTeam)) {
                int winner = red > blue ? 0 : 1;

                SendTeamResultToAll(winner);
                PlayerRankingManager.Instance.ApplyRateAllPlayers(winner);
                GameManager.Instance.EndGame();
            }

            return;
        }
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
    /// 延長戦終了判定用
    /// </summary>
    [Server]
    public void NotifyObjectStateChanged() {
        if (!isOvertime) return;

        CheckWinConditionAllTeams(false);
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
            int reward = (winningTeamId == -1) ? 50 : (myTeam == winningTeamId ? 300 : 100);
            TargetRewardMoney(client, reward);

            if (winningTeamId != -1 && myTeam == winningTeamId) {
                ApplyMillionairePassive(player, client);
            }
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
    /// 指定チームが現在オブジェクトに関与しているか判定
    /// Area → 負けチームのプレイヤーが1人でもエリア内にいればOK
    /// Hoko → ホコ保持していればOK
    /// </summary>
    private bool IsTeamControllingObject(int teamId) {
        if (teamId == -1)
            return false;

        // Areaルールの場合
        if (currentRule == GameRuleType.Area) {
            var area = FindObjectOfType<CaptureArea>();
            if (area == null) return false;

            // エリア内にいるプレイヤーを確認
            foreach (var player in area.playersInArea) {
                if (player.parameter.TeamID == teamId) {
                    // 負けチームのプレイヤーが1人でもいれば延長
                    return true;
                }
            }

            return false;
        }

        // Hokoルールの場合
        if (currentRule == GameRuleType.Hoko) {
            var hoko = FindObjectOfType<CaptureHoko>();
            if (hoko == null) return false;

            return hoko.GetHolderTeam() == teamId;
        }

        return false;
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

    /// <summary>
    /// 古谷　ミリオネア専用
    /// </summary>
    /// <param name="data"></param>

    [Server]
    private void ApplyMillionairePassive(CharacterBase player, NetworkConnection conn) {
        var param = player.parameter;
        if (param == null) return;

        foreach (var passive in param.equippedPassives) {
            if (passive is Passive_Millionaire millionaire) {
                // クライアント側のWalletに返還させる
                TargetRefundMoney(conn, millionaire.multiple);
            }
        }
    }

    /// <summary>
    /// 古谷　Walletに返還
    /// </summary>
    [TargetRpc]
    private void TargetRefundMoney(NetworkConnection target, float multiplier) {
        PlayerWallet.Instance?.RefundSpentMoney(multiplier);
    }
}