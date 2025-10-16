using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

//
//  @file   Second_CharacterClass
//
class WizardCharacter : CharacterBase {
    [Tooltip("インポートするステータスのScriptableObject")]
    [SerializeField] CharacterStatus InputStatus;
    //CharacterStatusをキャッシュ(ScriptableObjectを書き換えないための安全策)
    private CharacterStatus RunTimeStatus;
    [Tooltip("使用するスキル")]
    private SkillBase[] EquippedSkills;
    [Tooltip("使用するパッシブ")]
    private PassiveBase[] EquippedPassives;

    //魔法職のみ：攻撃時に消費。時間経過で徐々に回復(攻撃中は回復しない)。レベルアップで最大MP(もしくは回復速度？)が上昇。
    protected int MP { get; private set; }
    protected int MaxMP { get; private set; }

    protected override void StatusInport() {
        if (InputStatus == null) {
            DefaultStatusInport();
            return;
        }

        RunTimeStatus = InputStatus;
        MaxHP = RunTimeStatus.MaxHP;
        HP = MaxHP;
        Attack = RunTimeStatus.Attack;
        MoveSpeed = RunTimeStatus.MoveSpeed;
        Debug.Log("MeleeCharacter.cs : StatusInportを実行しました。\nMaxHP:" + MaxHP + " Attack:" + Attack + " MoveSpeed:" + MoveSpeed);
        EquippedSkills = RunTimeStatus.Skills;
        /* xxx.Where() <= nullでないか確認する。 xxx.Select() <= 指定した変数を取り出す。 ※using System.Linq が必要。 */
        Debug.Log("MeleeCharacter.cs : スキルのインポートを行いました。\nインポートしたスキル: " + string.Join(", ", EquippedSkills.Where(i => i != null).Select(i => i.SkillName)));
        EquippedPassives = RunTimeStatus.Passives;
        Debug.Log("MeleeCharacter.cs : パッシブのインポートを行いました。\nインポートしたパッシブ: " + string.Join(", ", EquippedPassives.Where(i => i != null).Select(i => i.PassiveName)));
    }

    protected override void StartUseSkill() {
        EquippedSkills[0].Activate(gameObject);
    }

    // Start is called before the first frame update
    protected new void Awake() {
        base.Awake();
    }  

    // Update is called once per frame
    void Update() {
        MoveControl();
    }
}
