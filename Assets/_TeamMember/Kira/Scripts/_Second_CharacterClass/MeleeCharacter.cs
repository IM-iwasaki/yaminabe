using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//
//  @file   Second_CharacterClass
//
class MeleeCharacter : CharacterBase {
    [Tooltip("インポートするステータスのScriptableObject")]
    [SerializeField]CharacterStatus InputStatus;
    //CharacterStatusをキャッシュ(ScriptableObjectを書き換えないための安全策)
    CharacterStatus RunTimeStatus;

    protected override void StatusInport() {
        RunTimeStatus = InputStatus;
        MaxHP = RunTimeStatus.MaxHP;
        HP = MaxHP;
        Attack = RunTimeStatus.Attack;
        MoveSpeed = RunTimeStatus.MoveSpeed;
        Debug.Log("MeleeCharacter.cs : StatusInportを実行しました。\nMaxHP:" + MaxHP + " Attack:" + Attack + " MoveSpeed:" + MoveSpeed);
    }

    protected override void StartAttack(PlayerConst.AttackType _type = PlayerConst.AttackType.Main) {       
    }

    protected override void StartUseSkill() {
    }

    // Start is called before the first frame update
    protected new void Start() {
        base.Start();
        StatusInport();
    }

    // Update is called once per frame
    void Update() {
        //if(!isLocalPlayer) return;

        MoveControl();
        JumpControl();
    }   
}
