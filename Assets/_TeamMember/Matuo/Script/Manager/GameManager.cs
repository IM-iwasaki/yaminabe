using Mirror;
using UnityEngine;

/// <summary>
/// ゲーム全体の進行管理
/// ゲーム開始・終了、ルール切替、タイマー管理
/// </summary>
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

        StageManager.Instance.SpawnStage(stageData);

        if (rule == GameRuleType.DeathMatch)
            StageManager.Instance.SetRespawnMode(RespawnMode.Random);
        else
            StageManager.Instance.SetRespawnMode(RespawnMode.Team);

        isGameRunning = true;
        ruleManager.currentRule = rule;

        gameTimer.OnTimerFinished += () =>
        {
            if (rule == GameRuleType.DeathMatch)
                ruleManager.EndDeathMatch();
            else
                ruleManager.CheckWinConditionAllTeams();

            EndGame();
        };

        // タイマー開始
        gameTimer.StartTimer();
    }

    /// <summary>
    /// ゲーム終了
    /// </summary>
    [Server]
    public void EndGame() {
        if (!isGameRunning) return;

        isGameRunning = false;
        gameTimer.StopTimer();
        //リザルト表示処理追加:タハラ-----------------------------------------------------------------
        //ResultManager resultManager = FindObjectOfType<ResultManager>();
        //if (resultManager == null) {
        //    ChatManager.instance.CmdSendSystemMessage("resultManager is not Found");
        //    return;
        //}
           
        var ResultData = new ResultManager.ResultData {
            isTeamBattle = false,
            winnerName = "Alice",
            scores = new ResultScoreData[]
            {
                new ResultScoreData { playerName = "Alice", score = 1200 },
                new ResultScoreData { playerName = "Bob", score = 800 },
                new ResultScoreData { playerName = "Charlie", score = 1500 },
                new ResultScoreData { playerName = "Delta", score = 600 }
            }
        };
        ResultManager.Instance.ShowResult(ResultData);
        //--------------------------------------------------------------------------------------------
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