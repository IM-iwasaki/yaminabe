using Mirror;
using System.Collections;
using UnityEngine;

/// <summary>
/// ゲーム全体の進行管理
/// ゲーム開始・終了、ルール切替、タイマー管理
/// </summary>
public class GameManager : NetworkSystemObject<GameManager> {
    public CaptureHoko hoko { get; private set; }
    [SyncVar] private bool isGameRunning = false;
    private GameTimer gameTimer;
    private RuleManager ruleManager;

    /// <summary>
    /// 初期化
    /// </summary>
    public override void Initialize() {
        base.Initialize();

        gameTimer = GetComponent<GameTimer>();
        if (gameTimer == null)
            gameTimer = gameObject.AddComponent<GameTimer>();
        ruleManager = RuleManager.Instance;
    }

    /// <summary>
    /// ゲーム開始
    /// </summary>
    /// <param name="rule">開始するルールタイプ</param>
    /// <param name="stageData">生成するステージの晩小郷</param>
    [Server]
    public void StartGame(GameRuleType rule, StageData stageData) {
        if (isGameRunning) return;

        StageManager.Instance.SpawnStage(stageData, rule);

        if (rule == GameRuleType.DeathMatch)
            StageManager.Instance.SetRespawnMode(RespawnMode.Random);
        else
            StageManager.Instance.SetRespawnMode(RespawnMode.Team);

        isGameRunning = true;
        ruleManager.currentRule = rule;

        
    }

    [Server]
    public void StartGameWithCountdown(GameRuleType rule, StageData stageData, int countdownSeconds = 3) {
        if (isGameRunning) return;

        StartGame(rule, stageData);

        // カウントダウン開始をクライアントに通知
        RpcStartCountdown(countdownSeconds);

        // カウントダウン終了後にゲームを開始
        StartCoroutine(CountdownCoroutine(rule, stageData, countdownSeconds));
    }

    private IEnumerator CountdownCoroutine(GameRuleType rule, StageData stageData, int countdownSeconds) {
        yield return new WaitForSeconds(countdownSeconds);

        // スコア初期化（TeamManager を使わず RuleManager の teamScores を利用）
        foreach (var teamId in RuleManager.Instance.teamScores.Keys) {
            if (rule == GameRuleType.DeathMatch)
                RuleManager.Instance.SetInitialScore(teamId, 0f);     // 加算方式
            else
                RuleManager.Instance.SetInitialScore(teamId, 50f);    // カウントダウン方式
        }

        gameTimer.OnTimerFinished += () => {
            if (rule == GameRuleType.DeathMatch)
                ruleManager.EndDeathMatch();
            else
                ruleManager.CheckWinConditionAllTeams();

            EndGame();
        };

        // タイマー開始
        gameTimer.StartTimer();
    }

    // クライアントにカウントダウン開始を通知
    [ClientRpc]
    private void RpcStartCountdown(int seconds) {
        // ここでUIに表示
        CountdownUI.Instance.StartCountdown(seconds);
    }


    [Server]
    public void RegisterHoko(CaptureHoko h) {
        hoko = h;
    }

    /// <summary>
    /// ゲーム終了
    /// </summary>
    [Server]
    public void EndGame() {
        if (!isGameRunning) return;

        isGameRunning = false;
        gameTimer.StopTimer();
        Cursor.lockState = CursorLockMode.None;

        // 勝敗処理
        if (ruleManager.currentRule == GameRuleType.DeathMatch)
            ruleManager.EndDeathMatch();
        else
            ruleManager.CheckWinConditionAllTeams();

        if (hoko != null)
            hoko.HandleGameEnd();
    }

    /// <summary>
    /// ゲーム進行中か
    /// </summary>
    public bool IsGameRunning() => isGameRunning;

    /// <summary>
    /// 残り時間取得
    /// </summary>
    public float GetRemainingTime() => gameTimer != null ? gameTimer.GetRemainingTime() : 0f;
}