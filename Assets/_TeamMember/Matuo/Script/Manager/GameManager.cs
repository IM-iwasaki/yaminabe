using UnityEngine;
using Mirror;

/// <summary>
/// ゲーム全体の進行管理
/// ゲーム開始・終了、ルール切替、タイマー管理
/// </summary>

// ＜猿でもわかる使い方(ゲームの開始と終了)＞ 
// GameManager.Instance.StartGame(GameRuleType.Area); この場合エリアが始まる(残りはGameRuleTypeを見て)
// GameManager.Instance.EndGame();
public class GameManager : NetworkSystemObject<GameManager> {
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

        ruleManager = FindAnyObjectByType<RuleManager>();
    }

    /// <summary>
    /// ゲーム開始
    /// </summary>
    /// <param name="rule">開始するルールタイプ</param>
    [Server]
    public void StartGame(GameRuleType rule) {
        if (isGameRunning) return;

        isGameRunning = true;
        ruleManager.currentRule = rule;

        // タイマー開始
        gameTimer.StartTimer();

        // デスマッチの場合はGameTimerのイベントで終了処理に移行
        if (rule == GameRuleType.DeathMatch) {
            gameTimer.OnTimerFinished += () => {
                ruleManager.EndDeathMatch();
                EndGame();
            };
        }
    }

    /// <summary>
    /// ゲーム終了
    /// </summary>
    [Server]
    public void EndGame() {
        if (!isGameRunning) return;

        isGameRunning = false;
        gameTimer.StopTimer();
        Debug.Log("ゲーム終了");
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