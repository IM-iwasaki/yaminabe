using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HostUI : MonoBehaviour
{
    [SerializeField,Header("表記するルール名")]
    private TextMeshProUGUI ruleName = null;
    [SerializeField,Header("表記するステージ名")]
    private TextMeshProUGUI stageName = null;

    private int ruleIndex = 0;
    private int stageIndex = 0;

    private int prevRuleIndex,prevStageIndex;

    private void Update() {
        //if (prevRuleIndex != ruleIndex)
            //ChangeRuleUI();
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

    //public void;
   

}
