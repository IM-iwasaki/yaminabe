using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

/// <summary>
/// ゲーム中のUI表示を管理
/// 残り時間とスコア(チーム別カウント)を表示
/// </summary>
public class GameUIManager : NetworkBehaviour {
    [Header("UI参照")]
    [SerializeField] private TMP_Text timerText;     // 残り時間
    [SerializeField] private TMP_Text redTeamScoreText; // 赤チームスコア
    [SerializeField] private TMP_Text blueTeamScoreText; // 青チームスコア

    [Header("更新間隔(秒)")]
    [SerializeField] private float updateInterval = 0.2f; // UI更新間隔

    private GameTimer gameTimer;
    private RuleManager ruleManager;
    private float timer;

    private void Start() {
        gameTimer = FindObjectOfType<GameTimer>();
        ruleManager = RuleManager.Instance;
    }

    private void Update() {
        if (gameTimer == null || ruleManager == null) return;

        timer += Time.deltaTime;
        if (timer >= updateInterval) {
            timer = 0f;
            UpdateUI();
        }
    }

    /// <summary>
    /// UI更新
    /// </summary>
    private void UpdateUI() {
        // 残り時間表示
        float remaining = gameTimer.GetRemainingTime();
        int minutes = Mathf.FloorToInt(remaining / 60f);
        int seconds = Mathf.FloorToInt(remaining % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";

        // スコア表示
        if (ruleManager.TryGetTeamScore(1, out float redTeamScore))
            redTeamScoreText.text = $"RedTeam: {redTeamScore:F0}";
        else
            redTeamScoreText.text = "RedTeam: 0";

        if (ruleManager.TryGetTeamScore(2, out float blueTeamScore))
            blueTeamScoreText.text = $"BlueTeam: {blueTeamScore:F0}";
        else
            blueTeamScoreText.text = "BlueTeam: 0";
    }
}