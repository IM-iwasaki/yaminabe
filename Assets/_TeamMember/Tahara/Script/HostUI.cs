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

    public static GameObject uiRootObject = null;
    [SyncVar(hook = nameof(ChangeRuleAndUI))]
    public int ruleIndex = 0;
    [SyncVar(hook = nameof(ChangeStageUI))]
    public int stageIndex = 0;
    public static bool isVisibleUI = false;

    [SerializeField]
    private List<string> ruleNames = null;
    [SerializeField]
    private Button gameStartButton = null;

    private void Start() {
        //スポーンさせる
        //NetworkServer.Spawn(gameObject);
        uiRootObject = GameObject.Find("Background");
        uiRootObject.SetActive(false);
        //ホストでなければ処理しない
        if (!isServer) return;
        if (GameSceneManager.Instance != null) {
            gameStartButton.onClick.AddListener(GameSceneManager.Instance.LoadGameSceneForAll);
        }
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

    private void ChangeRuleAndUI(int _oldValue, int _newValue) {
        int ruleCount = _newValue % ruleNames.Count;
        rule.text = ruleNames[Mathf.Abs(ruleCount)];
        RuleManager.Instance.currentRule = (GameRuleType)Mathf.Abs(ruleCount);
    }
    private void ChangeStageUI(int _oldValue, int _newValue) {
        stage.text = StageManager.Instance.stages[Mathf.Abs(_newValue % StageManager.Instance.stages.Count)].stageName;
    }
    /// <summary>
    /// ホストのUIの表示非表示を担当true->見える、false->見えない
    /// </summary>
    /// <param name="_isVisibleFlag"></param>
    public static void ShowOrHideUI(bool _isVisibleFlag) {
        if (_isVisibleFlag)
            uiRootObject.SetActive(true);
        else
            uiRootObject.SetActive(false);
    }


}
