using UnityEngine;
using TMPro;
using Mirror;
using System.Collections;

/// <summary>
/// ゲーム中のUI表示を管理するクラス
/// </summary>
public class GameUIManager : MonoBehaviour {
    public static GameUIManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text redTeamScoreText;
    [SerializeField] private TMP_Text blueTeamScoreText;

    [Header("更新間隔(秒)")]
    [SerializeField] private float updateInterval = 0.2f;

    [Header("UI Animation (Score Pop)")]
    [SerializeField] private float popScale = 1.25f;
    [SerializeField] private float popTime = 0.25f;
    [SerializeField]
    private AnimationCurve popCurve =
        AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Coroutine redPopCoroutine;
    private Coroutine bluePopCoroutine;

    private GameTimer gameTimer;
    private RuleManager ruleManager;
    private float timer;

    private void Awake() {
        // シングルトン
        if (Instance == null)
            Instance = this;
        else {
            Destroy(gameObject);
            return;
        }
    }

    /// <summary>
    /// クライアントとして有効か判定
    /// </summary>
    private bool IsClientActive() {
        return NetworkClient.active && NetworkClient.isConnected;
    }

    private void Start() {
        gameTimer = GameTimer.Instance;
        ruleManager = RuleManager.Instance;

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
    /// タイマーとスコアのUIを更新
    /// </summary>
    private void UpdateUI() {
        float remaining = gameTimer.GetRemainingTime();
        timerText.text =
            $"{Mathf.FloorToInt(remaining / 60f):00}:{Mathf.FloorToInt(remaining % 60f):00}";

        float redScore = 0f;
        float blueScore = 0f;
        ruleManager.TryGetTeamScore(0, out redScore);
        ruleManager.TryGetTeamScore(1, out blueScore);

        bool isAreaOrHoko =
            ruleManager.currentRule == GameRuleType.Area ||
            ruleManager.currentRule == GameRuleType.Hoko;

        if (isAreaOrHoko) {
            float target = ruleManager.winScores[ruleManager.currentRule];
            redTeamScoreText.text = $"Count\n{redScore:F0}/{target:F0}";
            blueTeamScoreText.text = $"Count\n{blueScore:F0}/{target:F0}";

        } else {
            // キル数などの普通のスコア
            redTeamScoreText.text = $"RedTeam: {redScore:F0}";
            blueTeamScoreText.text = $"BlueTeam: {blueScore:F0}";
        }
    }

    /// <summary>
    /// スコア更新通知を受け取りUIに即時反映
    /// </summary>
    public void UpdateTeamScore(int teamId, float score) {
        if (!IsClientActive()) return;

        bool isAreaOrHoko =
            ruleManager.currentRule == GameRuleType.Area ||
            ruleManager.currentRule == GameRuleType.Hoko;

        string text;

        if (isAreaOrHoko) {
            float target = ruleManager.winScores[ruleManager.currentRule];
            text = $"Remaining\n{score:F0}/{target:F0}";
        } else {
            text = (teamId == 0 ? "RedTeam" : "BlueTeam") + $": {score:F0}";
        }

        if (teamId == 0) {
            redTeamScoreText.text = text;
            PlayPopEffect(0);
        } else if (teamId == 1) {
            blueTeamScoreText.text = text;
            PlayPopEffect(1);
        }
    }

    /// <summary>
    /// スコア変動時のポップアニメーション再生
    /// </summary>
    private void PlayPopEffect(int teamId) {
        if (teamId == 0) {
            if (redPopCoroutine != null) StopCoroutine(redPopCoroutine);
            redPopCoroutine = StartCoroutine(PopAnimation(redTeamScoreText));
        } else {
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
        Vector3 peakScale = Vector3.one * popScale;

        float t = 0f;
        while (t < popTime) {
            tf.localScale = Vector3.Lerp(baseScale, peakScale, popCurve.Evaluate(t / popTime));
            t += Time.deltaTime;
            yield return null;
        }

        tf.localScale = baseScale;
    }
}