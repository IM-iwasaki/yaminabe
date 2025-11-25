using TMPro;
using UnityEngine;

//スキル表示用クラス
public class SkillDisplayer : MonoBehaviour {
    public static SkillDisplayer Instance { get; private set; }

    [SerializeField]private TextMeshPro skillTxt;
    [SerializeField]private TextMeshPro passiveTxt;

    void Awake() {
        Instance = this;
    }

    public void SetSkillUI(string _skillName, string _skillTxt, string _passiveName, string _passiveTxt) {
        skillTxt.text =
        $"Skill : {_skillName}\n{_skillTxt}";

        passiveTxt.text =
        $"Passive : {_passiveName}\n{_passiveTxt}";
    }
}