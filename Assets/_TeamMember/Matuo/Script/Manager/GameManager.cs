using Mirror;
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
    /// <param name="stageData">生成するステージのステージデータ</param>
    [Server]
    public void StartGame(GameRuleType rule, StageData stageData) {
        if (isGameRunning) return;

        // ステージ生成
        StageManager.Instance.SpawnStage(stageData, rule);

        if (rule == GameRuleType.DeathMatch)
            StageManager.Instance.SetRespawnMode(RespawnMode.Random);
        else
            StageManager.Instance.SetRespawnMode(RespawnMode.Team);

        isGameRunning = true;

        // ルール設定
        ruleManager.currentRule = rule;

        // チームスコアとペナルティを RuleManager 側で初期化
        ruleManager.InitializeScoresForRule(rule);

        // タイマー終了時処理
        gameTimer.OnTimerFinished += () => {
            if (rule == GameRuleType.DeathMatch)
                ruleManager.EndDeathMatch();
            else
                ruleManager.CheckWinConditionAllTeams(true);

            EndGame();
        };

        // タイマー開始
        gameTimer.StartTimer();
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
        //else
        //    ruleManager.CheckWinConditionAllTeams();

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