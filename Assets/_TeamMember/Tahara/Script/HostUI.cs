using Mirror;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HostUI : NetworkBehaviour {
    public static HostUI instance = null;
    [SerializeField, Header("表記するルール名")]
    private TextMeshProUGUI rule = null;
    [SerializeField, Header("表記するステージ名")]
    private TextMeshProUGUI stage = null;

    [SerializeField]
    private GameObject uiRootObject = null;
    [SyncVar(hook = nameof(ChangeRuleAndUI))]
    public int ruleIndex = 0;
    [SyncVar(hook = nameof(ChangeStageUI))]
    public int stageIndex = 0;
    [SyncVar(hook = nameof(ShowOrHideUI))]
    public bool isVisibleUI = false;

    [SerializeField]
    private List<string> ruleNames = null;
    [SerializeField]
    private Button gameStartButton = null;

    private void Start() {
        //スポーンさせる
        //NetworkServer.Spawn(gameObject);
        uiRootObject.SetActive(false);
        //ホストでなければ処理しない
        if (!isServer) return;
        if (GameSceneManager.Instance != null) {
            gameStartButton.onClick.AddListener(GameSceneManager.Instance.LoadGameSceneForAll);
        }

        instance = this;

        rule.text = ruleNames[ruleIndex];
        stage.text = StageManager.Instance.stages[stageIndex].stageName;
    }
    public void IncrementRuleIndex() {
        ruleIndex++;
    }
    public void DecrementRuleIndex() {
        ruleIndex--;
    }
    public void IncrementStageIndex() {
        stageIndex++;
    }
    public void DecrementStageIndex() {
        stageIndex--;
    }

    private void ChangeRuleAndUI(int _oldValue,int _newValue) {
        //if (_newValue < 1 || _newValue >= ruleNames.Count)
            ruleIndex = Mathf.Abs(_newValue % ruleNames.Count);
        
        rule.text = ruleNames[ruleIndex];
        RuleManager.Instance.currentRule = (GameRuleType)ruleIndex;
    }
    private void ChangeStageUI(int _oldValue, int _newValue) {
        //if (_newValue < 1 || _newValue >= StageManager.Instance.stages.Count)
            stageIndex = Mathf.Abs(_newValue % StageManager.Instance.stages.Count);


        stage.text = StageManager.Instance.stages[stageIndex].stageName;
    }
    //ホストUIを非表示にさせる
    public void ShowOrHideUI(bool _old, bool _isVisibleFlag) {
        if (_isVisibleFlag)
            uiRootObject.SetActive(true);
        else
            uiRootObject.SetActive(false);
    }

    
}
