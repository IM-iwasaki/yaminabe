using Mirror;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Host用UI
/// </summary>
public class HostUI : NetworkBehaviour {
    [SerializeField, Header("表記するルール名")]
    private TextMeshProUGUI rule = null;
    [SerializeField, Header("表記するステージ名")] 
    private TextMeshProUGUI stage = null;
    /// <summary>
    /// 親オブジェクト
    /// </summary>
    public static GameObject uiRootObject = null;
    /// <summary>
    /// ルールを変更する用のインデックス
    /// </summary>
    [SyncVar(hook = nameof(ChangeRuleAndUI))]
    public int ruleIndex = 0;
    /// <summary>
    /// ステージを変更する用のインデックス
    /// </summary>
    [SyncVar(hook = nameof(ChangeStageUI))]
    public int stageIndex = 0;
    /// <summary>
    /// UIを見せるかどうか
    /// </summary>
    public static bool isVisibleUI = false;

    /// <summary>
    /// ルールの名前のリスト
    /// </summary>
    [SerializeField]
    private List<string> ruleNames = null;
    /// <summary>
    /// ゲーム開始ボタン
    /// </summary>
    [SerializeField]
    private Button gameStartButton = null;

    private void Start() {
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

    /// <summary>
    /// ルール用インデックス増加関数
    /// </summary>
    public void IncrementRuleIndex() {
        ruleIndex++;
    }

    /// <summary>
    /// ルール用インデックス減少関数
    /// </summary>
    public void DecrementRuleIndex() {
        ruleIndex--;
    }

    /// <summary>
    /// ステージ用インデックス増加関数
    /// </summary>
    public void IncrementStageIndex() {
        stageIndex++;
    }

    //ステージ用インデックス減少関数
    public void DecrementStageIndex() {
        stageIndex--;
    }
    /// <summary>
    /// hookで呼ばれるルール用インデックスに変更があった時に発火する関数
    /// </summary>
    /// <param name="_oldValue"></param>
    /// <param name="_newValue"></param>
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
    public static void ShowOrHideUI() {
        isVisibleUI = !isVisibleUI;
        uiRootObject.SetActive(isVisibleUI);
        Cursor.lockState = isVisibleUI ? CursorLockMode.None : CursorLockMode.Locked ;
    }


}
