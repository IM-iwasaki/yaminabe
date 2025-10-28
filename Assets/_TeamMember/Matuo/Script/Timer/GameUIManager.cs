using UnityEngine;
using TMPro;
using Mirror;

/// <summary>
/// ゲーム中のUI表示を管理
/// 残り時間とスコア(チーム別のカウント)を表示
/// </summary>
public class GameUIManager : NetworkBehaviour {
    public static GameUIManager Instance { get; private set; } // Singleton参照用

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
        // Singleton設定
        if (Instance == null) {
            Instance = this;
        } else if (Instance != this) {
            Destroy(gameObject);
            return;
        }
    }

    private void Start() {
        gameTimer = FindObjectOfType<GameTimer>();
        ruleManager = RuleManager.Instance;

        if (gameTimer == null)
            Debug.LogWarning("[GameUIManager] GameTimer がシーン内に見つかりません。");
        if (ruleManager == null)
            Debug.LogWarning("[GameUIManager] RuleManager がシーン内に見つかりません。");
    }

    private void Update() {
        // Mirror: クライアントのみUI更新
        if (!isClient || gameTimer == null || ruleManager == null)
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

        // スコア表示更新
        if (ruleManager.TryGetTeamScore(1, out float redScore))
            redTeamScoreText.text = $"RedTeam: {redScore:F0}";
        else
            redTeamScoreText.text = "RedTeam: 0";

        if (ruleManager.TryGetTeamScore(2, out float blueScore))
            blueTeamScoreText.text = $"BlueTeam: {blueScore:F0}";
        else
            blueTeamScoreText.text = "BlueTeam: 0";
    }

    /// <summary>
    /// RuleManagerなどから直接スコアを更新するためのメソッド
    /// </summary>
    public void UpdateTeamScore(int teamId, float score) {
        if (!isClient) return;

        switch (teamId) {
            case 1:
                if (redTeamScoreText != null)
                    redTeamScoreText.text = $"RedTeam: {score:F0}";
                break;
            case 2:
                if (blueTeamScoreText != null)
                    blueTeamScoreText.text = $"BlueTeam: {score:F0}";
                break;
            default:
                Debug.Log($"対応してないteamId: {teamId}");
                break;
        }
    }
}