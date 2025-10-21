using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// ステージとルール選択用
/// </summary>
public class RuleSelectUI : MonoBehaviour {
    [Header("ステージ選択")]
    public TMP_Text stageNameText;
    public Button stageLeftButton;
    public Button stageRightButton;

    [Header("ルール選択")]
    public TMP_Text ruleNameText;
    public Button ruleLeftButton;
    public Button ruleRightButton;

    [Header("開始ボタン")]
    public Button startButton;

    private void Start() {
        // ボタンイベント登録
        stageLeftButton.onClick.AddListener(OnStageLeft);
        stageRightButton.onClick.AddListener(OnStageRight);
        ruleLeftButton.onClick.AddListener(OnRuleLeft);
        ruleRightButton.onClick.AddListener(OnRuleRight);
        startButton.onClick.AddListener(OnStartButtonClicked);

        UpdateStageDisplay();
        UpdateRuleDisplay();
    }

    private void OnStageLeft() {
        StageManager.Instance.SelectPreviousStage();
        UpdateStageDisplay();
    }

    private void OnStageRight() {
        StageManager.Instance.SelectNextStage();
        UpdateStageDisplay();
    }

    private void UpdateStageDisplay() {
        if (stageNameText != null)
            stageNameText.text = StageManager.Instance.GetCurrentStageName();
    }

    private GameRuleType[] rules = (GameRuleType[])System.Enum.GetValues(typeof(GameRuleType));
    private int currentRuleIndex = 0;

    private void OnRuleLeft() {
        currentRuleIndex = (currentRuleIndex - 1 + rules.Length) % rules.Length;
        UpdateRuleDisplay();
    }

    private void OnRuleRight() {
        currentRuleIndex = (currentRuleIndex + 1) % rules.Length;
        UpdateRuleDisplay();
    }

    private void UpdateRuleDisplay() {
        if (ruleNameText != null)
            ruleNameText.text = rules[currentRuleIndex].ToString();
    }

    private void OnStartButtonClicked() {
        // 選択中のルールとステージでゲーム開始
        GameManager.Instance.StartGame(rules[currentRuleIndex], StageManager.Instance.GetCurrentStageIndex());
    }
}