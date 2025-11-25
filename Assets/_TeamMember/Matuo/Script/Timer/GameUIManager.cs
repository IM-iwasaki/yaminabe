using UnityEngine;
using TMPro;
using Mirror;

/// <summary>
/// ゲーム中のUI表示を管理
/// 残り時間とスコア(チーム別のカウント)を表示
/// </summary>
public class GameUIManager : MonoBehaviour {
    public static GameUIManager Instance { get; private set; } // シングルトン参照用

    [Header("UI")]
    [SerializeField] private TMP_Text timerText;          // 残り時間
    [SerializeField] private TMP_Text redTeamScoreText;   // 赤チームスコア
    [SerializeField] private TMP_Text blueTeamScoreText;  // 青チームスコア

    [Header("更新間隔(秒)")]
    [SerializeField] private float updateInterval = 0.2f; // UI更新間隔

    private GameTimer gameTimer;
    private RuleManager ruleManager;
    private float timer;

    private void Awake() {
        // シングルトン
        if (Instance == null) {
            Instance = this;
        } else if (Instance != this) {
            Destroy(gameObject);
            return;
        }
    }

    /// <summary>
    /// MirrorのisClientみたいな
    /// </summary>
    private bool IsClientActive() {
        return NetworkClient.active && NetworkClient.isConnected;
    }

    private void Start() {
        gameTimer = GameTimer.Instance;
        ruleManager = RuleManager.Instance;

        if (gameTimer == null)
            Debug.LogWarning("[GameUIManager] GameTimer がシーン内に見つかりません。");
        if (ruleManager == null)
            Debug.LogWarning("[GameUIManager] RuleManager がシーン内に見つかりません。");
    }

    private void Update() {
        // クライアントのみUI更新
        if (!IsClientActive() || gameTimer == null || ruleManager == null)
            return;

        timer += Time.deltaTime;
        if (timer >= updateInterval) {
            timer = 0f;
            UpdateUI();
        }
    }

    /// <summary>
    /// 残り時間とスコアのUIを一括更新
    /// </summary>
    private void UpdateUI() {
        // 残り時間
        float remaining = gameTimer.GetRemainingTime();
        int minutes = Mathf.FloorToInt(remaining / 60f);
        int seconds = Mathf.FloorToInt(remaining % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";

        bool isCountDownRule =
            ruleManager.currentRule == GameRuleType.Area ||
            ruleManager.currentRule == GameRuleType.Hoko;

        float redScore = 0f;
        float blueScore = 0f;

        if (!ruleManager.TryGetTeamScore(0, out redScore))
            redScore = 0f;

        if (!ruleManager.TryGetTeamScore(1, out blueScore))
            blueScore = 0f;

        if (isCountDownRule) {
            float maxScore = ruleManager.winScores[ruleManager.currentRule];

            float redRemaining = Mathf.Max(0f, maxScore - redScore);
            float blueRemaining = Mathf.Max(0f, maxScore - blueScore);

            string redText = $"RedTeam: {redRemaining:F0}";
            string blueText = $"BlueTeam: {blueRemaining:F0}";

            // ペナルティがある場合は "+ペナルティ" 表示
            if (ruleManager.penaltyScores.TryGetValue(0, out float redPenalty) && redPenalty > 0f)
                redText += $" +{redPenalty:F0}";
            if (ruleManager.penaltyScores.TryGetValue(1, out float bluePenalty) && bluePenalty > 0f)
                blueText += $" +{bluePenalty:F0}";

            redTeamScoreText.text = redText;
            blueTeamScoreText.text = blueText;
        } else {
            redTeamScoreText.text = $"RedTeam: {redScore:F0}";
            blueTeamScoreText.text = $"BlueTeam: {blueScore:F0}";
        }
    }

    /// <summary>
    /// RuleManagerなどから直接スコアを更新するためのメソッド
    /// </summary>
    public void UpdateTeamScore(int teamId, float score) {
        if (!IsClientActive()) return;

        bool isCountDownRule =
            ruleManager.currentRule == GameRuleType.Area ||
            ruleManager.currentRule == GameRuleType.Hoko;

        float displayScore = score;

        if (isCountDownRule) {
            float maxScore = ruleManager.winScores[ruleManager.currentRule];
            displayScore = Mathf.Max(0f, maxScore - score);
        }

        string text = $"Team: {displayScore:F0}";

        // ペナルティがある場合は "+ペナルティ" 表示
        if (isCountDownRule && ruleManager.penaltyScores.TryGetValue(teamId, out float penalty) && penalty > 0f) {
            text = $"Team: {displayScore:F0} +{penalty:F0}";
        }

        switch (teamId) {
            case 0:
                if (redTeamScoreText != null)
                    redTeamScoreText.text = text.Replace("Team", "RedTeam");
                break;
            case 1:
                if (blueTeamScoreText != null)
                    blueTeamScoreText.text = text.Replace("Team", "BlueTeam");
                break;
            default:
                Debug.Log($"対応してないteamId: {teamId}");
                break;
        }
    }
}