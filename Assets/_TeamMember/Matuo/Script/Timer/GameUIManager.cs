using UnityEngine;
using TMPro;
using Mirror;

/// <summary>
/// ゲーム中のUI表示を管理
/// 残り時間とスコア(チーム別のカウント)を表示
/// ※ 内部スコアは「残りカウント (減算式)」で保持される前提
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
    /// MirrorのisClientみたいな（クライアントでのみUI更新したい場合）
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

        // 初回即時更新（遅延なく表示したい場合）
        if (IsClientActive()) {
            UpdateUI();
        }
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
    /// 内部スコアは「残りカウント（減算）」である想定なので、そのまま表示する
    /// </summary>
    private void UpdateUI() {
        if (timerText == null || redTeamScoreText == null || blueTeamScoreText == null) {
            // UI が割り当てられていない場合は無視
            return;
        }

        // 残り時間
        float remaining = 0f;
        if (gameTimer != null)
            remaining = gameTimer.GetRemainingTime();

        int minutes = Mathf.FloorToInt(remaining / 60f);
        int seconds = Mathf.FloorToInt(remaining % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";

        bool isCountDownRule =
            ruleManager != null &&
            (ruleManager.currentRule == GameRuleType.Area ||
             ruleManager.currentRule == GameRuleType.Hoko);

        float redScore = 0f;
        float blueScore = 0f;

        if (ruleManager != null) {
            ruleManager.TryGetTeamScore(0, out redScore);
            ruleManager.TryGetTeamScore(1, out blueScore);
        }

        if (isCountDownRule) {
            // 内部スコアは "残りカウント" の想定 → UIにはそのまま表示する
            float redRemaining = Mathf.Max(0f, redScore);
            float blueRemaining = Mathf.Max(0f, blueScore);

            string redText = $"RedTeam: {redRemaining:F0}";
            string blueText = $"BlueTeam: {blueRemaining:F0}";

            // ペナルティが同期されていれば表示（注意：RuleManager側で同期されていることが前提）
            if (ruleManager != null && ruleManager.penaltyScores != null) {
                if (ruleManager.penaltyScores.TryGetValue(0, out float redPenalty) && redPenalty > 0f)
                    redText += $" +{redPenalty:F0}";
                if (ruleManager.penaltyScores.TryGetValue(1, out float bluePenalty) && bluePenalty > 0f)
                    blueText += $" +{bluePenalty:F0}";
            }

            redTeamScoreText.text = redText;
            blueTeamScoreText.text = blueText;
        } else {
            // デスマッチなどは内部スコアをそのまま表示（キル数など）
            redTeamScoreText.text = $"RedTeam: {redScore:F0}";
            blueTeamScoreText.text = $"BlueTeam: {blueScore:F0}";
        }
    }

    /// <summary>
    /// RuleManagerなどから直接スコアを更新するためのメソッド
    /// </summary>
    public void UpdateTeamScore(int teamId, float score) {
        if (!IsClientActive()) return;
        if (redTeamScoreText == null || blueTeamScoreText == null) return;
        if (ruleManager == null) return;

        bool isCountDownRule =
            ruleManager.currentRule == GameRuleType.Area ||
            ruleManager.currentRule == GameRuleType.Hoko;

        // 内部スコアは "残り" なので、そのまま表示（負にならないよう保護）
        float displayScore = Mathf.Max(0f, score);

        string text = $"Team: {displayScore:F0}";

        // ペナルティ表示（同様に penaltyScores がクライアントに同期されている前提）
        if (isCountDownRule && ruleManager.penaltyScores.TryGetValue(teamId, out float penalty) && penalty > 0f) {
            text = $"Team: {displayScore:F0} +{penalty:F0}";
        }

        switch (teamId) {
            case 0:
                redTeamScoreText.text = text.Replace("Team", "RedTeam");
                break;
            case 1:
                blueTeamScoreText.text = text.Replace("Team", "BlueTeam");
                break;
            default:
                Debug.Log($"[GameUIManager] 対応してないteamId: {teamId}");
                break;
        }
    }
}