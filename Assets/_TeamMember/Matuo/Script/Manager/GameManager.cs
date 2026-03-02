using Mirror;
using UnityEngine;
using System.Collections;

/// <summary>
/// ゲーム全体の進行を管理するクラス
/// PVP / PVE開始処理
/// タイマー管理
/// ルール終了処理
/// </summary>
public class GameManager : NetworkSystemObject<GameManager> {
    #region 定数

    private const int COUNTDOWN_SECONDS = 3;
    private const float GAME_START_DELAY = 4f;
    private const float DEFAULT_REMAINING_TIME = 0f;

    #endregion

    #region 変数

    public CaptureHoko Hoko { get; private set; }

    [SyncVar]
    private bool isGameRunning = false;

    private GameTimer gameTimer;
    private RuleManager ruleManager;
    private PVEStageData currentPveStage;

    #endregion

    #region 初期化

    /// <summary>
    /// 初期化処理
    /// </summary>
    public override void Initialize() {
        base.Initialize();

        // GameTimer取得（なければ追加）
        gameTimer = GetComponent<GameTimer>();
        if (gameTimer == null) {
            gameTimer = gameObject.AddComponent<GameTimer>();
        }

        // ルールマネージャ取得
        ruleManager = RuleManager.Instance;
    }

    #endregion

    #region PVP関連処理

    /// <summary>
    /// PVPゲーム開始
    /// </summary>
    [Server]
    public void StartPvpGame(GameRuleType rule, StageData stageData) {
        // 既に進行中なら開始しない
        if (isGameRunning) {
            return;
        }

        ServerManager.instance.ResetCharacterStatusOnGameStart();

        // 試合前初期化
        ResetGameState();

        // タイマーリセット
        gameTimer?.ResetTimer();

        // プレイヤー支出リセット
        PlayerWallet.Instance.ResetMatchSpentMoney();

        // ステージ生成
        SpawnPvpStage(rule, stageData);

        // ルールスコア初期化
        ruleManager.InitializeScoresForRule(rule);

        // カウントダウン開始
        CountdownManager.Instance.SendCountdown(COUNTDOWN_SECONDS);
        StartCoroutine(StartGameAfterCountdown(rule));
    }

    /// <summary>
    /// PVP用ステージ生成
    /// </summary>
    private void SpawnPvpStage(GameRuleType rule, StageData stageData) {
        StageManager.Instance.SpawnStage(stageData, rule);

        // リスポーン方式設定
        RespawnMode mode =
            rule == GameRuleType.DeathMatch
            ? RespawnMode.Random
            : RespawnMode.Team;

        StageManager.Instance.SetRespawnMode(mode);
    }

    #endregion

    #region PVE関連処理

    /// <summary>
    /// PVEゲーム開始（リストから取得）
    /// </summary>
    [Server]
    public void StartPveGameFromList(bool random = false) {
        if (isGameRunning) {
            return;
        }

        // ステージ取得
        PVEStageData stage = StageManager.Instance.GetNextPveStage(random);
        if (stage == null) {
            return;
        }

        StartPveGame(stage);
    }

    /// <summary>
    /// PVEゲーム開始
    /// </summary>
    [Server]
    public void StartPveGame(PVEStageData stage) {
        if (isGameRunning) {
            return;
        }

        if (stage == null) {
            return;
        }

        // PVE状態設定
        currentPveStage = stage;
        isGameRunning = false;

        // ステージ生成
        StageManager.Instance.SpawnPveStage(stage);

        // ラウンド開始
        StartPveRound();
    }

    /// <summary>
    /// PVEステージのみ設定
    /// </summary>
    [Server]
    public void SetPveStage(PVEStageData stage) {
        currentPveStage = stage;
    }

    /// <summary>
    /// PVEラウンド開始
    /// </summary>
    [Server]
    private void StartPveRound() {
        isGameRunning = true;
    }

    #endregion

    #region タイマー・進行管理

    /// <summary>
    /// カウントダウン終了後にゲーム開始
    /// </summary>
    [Server]
    private IEnumerator StartGameAfterCountdown(GameRuleType rule) {
        // カウントダウン待機
        yield return new WaitForSeconds(GAME_START_DELAY);

        isGameRunning = true;

        // 前回イベント削除
        gameTimer.ClearOnTimerFinished();

        // タイマー終了時処理登録
        gameTimer.OnTimerFinished += () => {
            if (rule == GameRuleType.DeathMatch) {
                ruleManager.EndDeathMatch();
            } else {
                // 勝敗判定のみ
                ruleManager.CheckWinConditionAllTeams(true);
            }
        };

        // タイマー開始
        gameTimer.StartTimer();
    }

    /// <summary>
    /// ゲーム終了処理
    /// </summary>
    [Server]
    public void EndGame() {
        if (!isGameRunning) {
            return;
        }

        if (Hoko != null) {
            Hoko.Drop();
        }

        // 状態リセット
        ResetGameState();

        // タイマー停止
        gameTimer.StopTimer();
        gameTimer.ClearOnTimerFinished();

        // カーソル解放
        Cursor.lockState = CursorLockMode.None;
    }

    #endregion

    #region 状態取得

    /// <summary>
    /// ゲーム進行中か
    /// </summary>
    public bool IsGameRunning() {
        return isGameRunning;
    }

    /// <summary>
    /// 残り時間取得
    /// </summary>
    public float GetRemainingTime() {
        return gameTimer != null
            ? gameTimer.GetRemainingTime()
            : DEFAULT_REMAINING_TIME;
    }

    #endregion

    #region 共通内部処理

    /// <summary>
    /// ゲーム状態初期化
    /// </summary>
    private void ResetGameState() {
        isGameRunning = false;
        currentPveStage = null;
    }

    #endregion

    #region 登録処理

    /// <summary>
    /// ホコオブジェクト登録(hokoルール用)
    /// </summary>
    [Server]
    public void RegisterHoko(CaptureHoko h) {
        Hoko = h;
    }

    #endregion
}