using UnityEngine;
using TMPro;
using Mirror;
using System.Collections;

/// <summary>
/// ゲーム中のUI表示を管理するクラス
/// </summary>
public class GameUIManager : MonoBehaviour {
    public static GameUIManager Instance { get; private set; } // シングルトン参照用

    [Header("UI")]
    [SerializeField] private TMP_Text timerText;          // 残り時間表示
    [SerializeField] private TMP_Text redTeamScoreText;   // 赤チームのスコアUI
    [SerializeField] private TMP_Text blueTeamScoreText;  // 青チームのスコアUI

    [Header("更新間隔(秒)")]
    [SerializeField] private float updateInterval = 0.2f; // UI更新間隔

    [Header("UI Animation (Score Pop)")]
    [SerializeField] private float popScale = 1.25f;      // 最大拡大率
    [SerializeField] private float popTime = 0.25f;       // 1ステップのアニメ時間
    [SerializeField]
    private AnimationCurve popCurve =    // アニメの滑らかさを調整
        AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Coroutine redPopCoroutine;
    private Coroutine bluePopCoroutine;

    private GameTimer gameTimer;
    private RuleManager ruleManager;

    private float timer;

    private void Awake() {
        // シングルトン設定
        if (Instance == null)
            Instance = this;
        else if (Instance != this) {
            Destroy(gameObject);
            return;
        }
    }

    /// <summary>
    /// Mirror のクライアントチェック
    /// </summary>
    private bool IsClientActive() {
        return NetworkClient.active && NetworkClient.isConnected;
    }

    private void Start() {
        gameTimer = GameTimer.Instance;
        ruleManager = RuleManager.Instance;

        if (gameTimer == null)
            Debug.LogWarning("[GameUIManager] GameTimer が見つかりません。");
        if (ruleManager == null)
            Debug.LogWarning("[GameUIManager] RuleManager が見つかりません。");

        // 初回のみ即時UI更新
        if (IsClientActive())
            UpdateUI();
    }

    private void Update() {
        // クライアントのみ処理
        if (!IsClientActive() || gameTimer == null || ruleManager == null)
            return;

        // インターバルごとにUI更新
        timer += Time.deltaTime;
        if (timer >= updateInterval) {
            timer = 0f;
            UpdateUI();
        }
    }

    /// <summary>
    /// 残り時間とスコアをまとめて更新する。
    /// </summary>
    private void UpdateUI() {
        if (timerText == null || redTeamScoreText == null || blueTeamScoreText == null)
            return;

        // 残り時間更新
        float remaining = gameTimer?.GetRemainingTime() ?? 0f;

        int minutes = Mathf.FloorToInt(remaining / 60f);
        int seconds = Mathf.FloorToInt(remaining % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";

        // スコア更新
        bool isCountDownRule =
            ruleManager != null &&
            (ruleManager.currentRule == GameRuleType.Area ||
             ruleManager.currentRule == GameRuleType.Hoko);

        float redScore = 0f;
        float blueScore = 0f;

        ruleManager?.TryGetTeamScore(0, out redScore);
        ruleManager?.TryGetTeamScore(1, out blueScore);

        if (isCountDownRule) {
            // カウントダウン方式の残数表示
            float redRemaining = Mathf.Max(0, redScore);
            float blueRemaining = Mathf.Max(0, blueScore);

            string redText = $"Remaining\n{redRemaining:F0}";
            string blueText = $"Remaining\n{blueRemaining:F0}";

            // ペナルティがあれば追記
            if (ruleManager.penaltyScores != null) {
                if (ruleManager.penaltyScores.TryGetValue(0, out float redPenalty) && redPenalty > 0)
                    redText += $" +{redPenalty:F0}";

                if (ruleManager.penaltyScores.TryGetValue(1, out float bluePenalty) && bluePenalty > 0)
                    blueText += $" +{bluePenalty:F0}";
            }

            redTeamScoreText.text = redText;
            blueTeamScoreText.text = blueText;
        } else {
            // キル数などの普通のスコア
            redTeamScoreText.text = $"RedTeam: {redScore:F0}";
            blueTeamScoreText.text = $"BlueTeam: {blueScore:F0}";
        }
    }

    /// <summary>
    /// RuleManager などからスコア変化を直接受け取る場合の更新メソッド
    /// </summary>
    public void UpdateTeamScore(int teamId, float score) {
        if (!IsClientActive()) return;
        if (redTeamScoreText == null || blueTeamScoreText == null) return;
        if (ruleManager == null) return;

        bool isCountDownRule =
            ruleManager.currentRule == GameRuleType.Area ||
            ruleManager.currentRule == GameRuleType.Hoko;

        string text;

        if (isCountDownRule) {
            // カウントダウン式残数
            float displayScore = Mathf.Max(0f, score);
            text = $"Remaining\n{displayScore:F0}";

            // ペナルティがあれば追加
            if (ruleManager.penaltyScores.TryGetValue(teamId, out float penalty) && penalty > 0)
                text += $" +{penalty:F0}";
        } else {
            // 通常スコア（チーム名付き）
            string teamName =
                teamId == 0 ? "RedTeam" :
                teamId == 1 ? "BlueTeam" : "Team";

            text = $"{teamName}: {score:F0}";
        }

        // スコアUI反映
        switch (teamId) {
            case 0:
                redTeamScoreText.text = text;
                break;
            case 1:
                blueTeamScoreText.text = text;
                break;
            default:
                Debug.Log($"[GameUIManager] 未対応 teamId: {teamId}");
                return;
        }

        // ポップアニメーション発火
        PlayPopEffect(teamId);
    }

    /// <summary>
    /// UI を伸び縮みさせるポップアニメーション
    /// </summary>
    private void PlayPopEffect(int teamId) {
        if (teamId == 0) {
            if (redPopCoroutine != null) StopCoroutine(redPopCoroutine);
            redPopCoroutine = StartCoroutine(PopAnimation(redTeamScoreText));
        } else if (teamId == 1) {
            if (bluePopCoroutine != null) StopCoroutine(bluePopCoroutine);
            bluePopCoroutine = StartCoroutine(PopAnimation(blueTeamScoreText));
        }
    }

    /// <summary>
    /// ポップアニメーション
    /// </summary>
    private IEnumerator PopAnimation(TMP_Text uiText) {
        Transform tf = uiText.transform;

        Vector3 baseScale = Vector3.one;
        Vector3 peakScale = Vector3.one * popScale;             // 最大膨張
        Vector3 overShootScale = Vector3.one * (popScale - 0.15f); // 少し縮みすぎる位置

        float t;

        // 膨らむ
        t = 0;
        while (t < popTime) {
            float n = t / popTime;
            tf.localScale = Vector3.LerpUnclamped(baseScale, peakScale, popCurve.Evaluate(n));
            t += Time.deltaTime;
            yield return null;
        }

        // 少し縮む
        t = 0;
        float bounceTime = popTime * 0.7f;
        while (t < bounceTime) {
            float n = t / bounceTime;
            tf.localScale = Vector3.LerpUnclamped(peakScale, overShootScale, popCurve.Evaluate(n));
            t += Time.deltaTime;
            yield return null;
        }

        // 元に戻る
        t = 0;
        float returnTime = popTime * 0.5f;
        while (t < returnTime) {
            float n = t / returnTime;
            tf.localScale = Vector3.LerpUnclamped(overShootScale, baseScale, popCurve.Evaluate(n));
            t += Time.deltaTime;
            yield return null;
        }

        tf.localScale = baseScale;
    }
}