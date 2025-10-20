using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HostUI : MonoBehaviour {
    [SerializeField, Header("表記するルール名")]
    private TextMeshProUGUI rule = null;
    [SerializeField, Header("表記するステージ名")]
    private TextMeshProUGUI stage = null;

    public int ruleIndex { get; private set; } = 0;
    public int stageIndex { get; private set; } = 0;

    private int prevRuleIndex, prevStageIndex;
    [SerializeField]
    private List<string> ruleNames = null;
    [SerializeField]
    private List<string> stageNames = null;

    private void Update() {
        if (prevRuleIndex != ruleIndex)
            ChangeRuleUI();
        if (prevStageIndex != stageIndex)
            ChangeStageUI();

        RuleManager.Instance.currentRule = (GameRuleType)ruleIndex;

        //インデックスの更新
        prevRuleIndex = ruleIndex;
        prevStageIndex = stageIndex;
    }

    public void IncrementRuleIndex() {
        ruleIndex++;
    }
    public void DecrementRuleIndex() {
        ruleIndex--;
    }
    public void IncrementStageIndex() {
        ruleIndex++;
    }
    public void DecrementStageIndex() {
        ruleIndex--;
    }

    private void ChangeRuleUI() {
        rule.text = ruleNames[Mathf.Clamp(stageIndex, 0, ruleNames.Count)];
    }
    private void ChangeStageUI() {
        stage.text = stageNames[Mathf.Clamp(stageIndex, 0, stageNames.Count)];
    }


}
