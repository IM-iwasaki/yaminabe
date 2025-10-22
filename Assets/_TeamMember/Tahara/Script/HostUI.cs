using Mirror;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HostUI : NetworkBehaviour {
    [SerializeField, Header("表記するルール名")]
    private TextMeshProUGUI rule = null;
    [SerializeField, Header("表記するステージ名")]
    private TextMeshProUGUI stage = null;

    [SerializeField]
    private GameObject unVisibleUIFromClient = null;
    public int ruleIndex { get; private set; } = 0;
    public int stageIndex { get; private set; } = 0;

    private int prevRuleIndex, prevStageIndex;
    [SerializeField]
    private List<string> ruleNames = null;
    [SerializeField]
    private Button gameStartButton = null;

    private void Start() {
        ////スポーンさせる
        //NetworkServer.Spawn(gameObject);
        if (!isServer) {
            unVisibleUIFromClient.SetActive(false);
        }
        if (GameSceneManager.Instance != null) {
            gameStartButton.onClick.AddListener(GameSceneManager.Instance.LoadGameSceneForAll);
        }
    }
    private void Update() {
        if (!isServer) return;
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
        if (ruleIndex >= ruleNames.Count)
            ruleIndex = 0;
    }
    public void DecrementRuleIndex() {
        ruleIndex--;
        if (ruleIndex < 0)
            ruleIndex = ruleNames.Count - 1;
    }
    public void IncrementStageIndex() {
        stageIndex++;
        if (stageIndex >= StageManager.Instance.stages.Count - 1)
            stageIndex = 0;

    }
    public void DecrementStageIndex() {
        stageIndex--;
        if (stageIndex < 0)
            stageIndex = StageManager.Instance.stages.Count - 1;
    }

    private void ChangeRuleUI() {
        rule.text = ruleNames[ruleIndex];
    }
    private void ChangeStageUI() {
        stage.text = StageManager.Instance.stages[stageIndex].stageName;
    }


}
