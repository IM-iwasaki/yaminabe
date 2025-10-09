using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

//
//  @file   Second_CharacterClass
//
class MeleeCharacter : CharacterBase {
    [Tooltip("インポートするステータスのScriptableObject")]
    [SerializeField]CharacterStatus InputStatus;
    //CharacterStatusをキャッシュ(ScriptableObjectを書き換えないための安全策)
    private CharacterStatus RunTimeStatus;
    [Tooltip("使用するスキル")]
    private SkillBase[] EquippedSkills;

    protected override void StatusInport() {
        RunTimeStatus = InputStatus;
        MaxHP = RunTimeStatus.MaxHP;
        HP = MaxHP;
        Attack = RunTimeStatus.Attack;
        MoveSpeed = RunTimeStatus.MoveSpeed;
        Debug.Log("MeleeCharacter.cs : StatusInportを実行しました。\nMaxHP:" + MaxHP + " Attack:" + Attack + " MoveSpeed:" + MoveSpeed);
        EquippedSkills = RunTimeStatus.Skills;
        /* xxx.Where() <= nullでないか確認する。 xxx.Select() <= 指定した変数を取り出す。 ※using System.Linq が必要。 */
        Debug.Log("MeleeCharacter.cs : スキルのインポートを行いました。\nインポートしたスキル: " + string.Join(", ", EquippedSkills.Where(i => i != null).Select(i => i.SkillName)));
    }

    protected override void StartAttack(PlayerConst.AttackType _type = PlayerConst.AttackType.Main) {       
    }

    protected override void StartUseSkill() {
        EquippedSkills[0].Activate(gameObject);
    }

    // Start is called before the first frame update
    protected new void Start() {
        base.Start();
        StatusInport();
    }

    // Update is called once per frame
    void Update() {
        if(!isLocalPlayer) return;

        MoveControl();
        JumpControl();
    }   

    
}
