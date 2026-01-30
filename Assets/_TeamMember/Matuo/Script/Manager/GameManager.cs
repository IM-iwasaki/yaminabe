using Mirror;
using UnityEngine;
using System.Collections;

/// <summary>
/// ゲーム全体の進行管理
/// ゲーム開始・終了、ルール切替、タイマー管理
/// </summary>
public class GameManager : NetworkSystemObject<GameManager> {
    public CaptureHoko hoko { get; private set; }
    [SyncVar] private bool isGameRunning = false;
    private GameTimer gameTimer;
    private RuleManager ruleManager;

    [SyncVar] private bool isPveMode = false;
    private PVEStageData currentPveStage;

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
    /// ゲーム開始（準備フェーズ）
    /// </summary>
    /// <param name="rule">開始するルールタイプ</param>
    /// <param name="stageData">生成するステージのステージデータ</param>
    [Server]
    public void StartGame(GameRuleType rule, StageData stageData) {
        if (isGameRunning) return;

        // 試合開始前の初期化
        isGameRunning = false;
        gameTimer?.ResetTimer();

        // PVE分岐
        if (isPveMode && currentPveStage != null) {
            StartPveInternal(currentPveStage);
            return;
        }

        // PVP処理
        StageManager.Instance.SpawnStage(stageData, rule);

        StageManager.Instance.SetRespawnMode(
            rule == GameRuleType.DeathMatch
                ? RespawnMode.Random
                : RespawnMode.Team
        );

        // ルール設定 & スコア初期化
        ruleManager.InitializeScoresForRule(rule);

        // カウントダウン開始
        CountdownManager.Instance.SendCountdown(3);
        StartCoroutine(StartGameAfterCountdown(rule));
    }

    [Server]
    public void SetPveStage(PVEStageData stage) {
        isPveMode = true;
        currentPveStage = stage;
    }

    /// <summary>
    /// PVEモードスタート
    /// </summary>
    [Server]
    private void StartPveInternal(PVEStageData stage) {
        isGameRunning = false;

        // PvEステージ生成
        StageManager.Instance.SpawnPveStage(stage);

        gameTimer.ResetTimer();
        gameTimer.SetLimitTime(stage.timeLimit);

        // PvE用ゲーム開始
        StartPveRound();
    }

    [Server]
    private void StartPveRound() {
        isGameRunning = true;

        GameTimer.Instance.ClearOnTimerFinished();
        GameTimer.Instance.OnTimerFinished += OnPveTimeUp;

        GameTimer.Instance.StartTimer();
    }

    /// <summary>
    /// PVEモードのタイムアップ
    /// </summary>
    [Server]
    private void OnPveTimeUp() {
        EndGame(); // 失敗扱い予定
    }

    /// <summary>
    /// カウントダウン終了後にゲームを開始する
    /// </summary>
    [Server]
    private IEnumerator StartGameAfterCountdown(GameRuleType rule) {
        yield return new WaitForSeconds(4f);

        isGameRunning = true;

        // 前試合のイベントを破棄
        gameTimer.ClearOnTimerFinished();

        gameTimer.OnTimerFinished += () => {
            if (rule == GameRuleType.DeathMatch)
                ruleManager.EndDeathMatch();
            else
                ruleManager.CheckWinConditionAllTeams(true);

            EndGame();
        };

        // タイマー開始（GO!と同時）
        gameTimer.StartTimer();
    }

    /// <summary>
    /// ホコオブジェクト登録
    /// </summary>
    [Server]
    public void RegisterHoko(CaptureHoko h) {
        hoko = h;
    }

    /// <summary>
    /// ゲーム終了処理
    /// </summary>
    [Server]
    public void EndGame() {
        if (!isGameRunning) return;

        isGameRunning = false;
        isPveMode = false;
        currentPveStage = null;

        gameTimer.StopTimer();
        Cursor.lockState = CursorLockMode.None;

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