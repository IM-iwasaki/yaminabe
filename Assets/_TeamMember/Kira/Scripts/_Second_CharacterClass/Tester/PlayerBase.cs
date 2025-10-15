using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//
//  @file   Second_CharacterClass_Test
//
class PlayerBase : CharacterBase {
    [Tooltip("インポートするステータスのScriptableObject")]
    [SerializeField] CharacterStatus InputStatus;
    //CharacterStatusをキャッシュ(ScriptableObjectを書き換えないための安全策)
    CharacterStatus RunTimeStatus;

    protected override void StatusInport() {
        //InputStatusがnullだったら警告を出してデフォルト値で初期化
        if(InputStatus == null) {
            Debug.LogWarning("InputStatusがnullになっています。デフォルトの値で初期化を行います。");
            DefaultStatusInport();
            return;
        }

        RunTimeStatus = InputStatus;
        MaxHP = RunTimeStatus.MaxHP;
        HP = MaxHP;
        Attack = RunTimeStatus.Attack;
        MoveSpeed = RunTimeStatus.MoveSpeed;
        Debug.Log("MeleeCharacter.cs : StatusInportを実行しました。\nMaxHP:" + MaxHP + " Attack:" + Attack + " MoveSpeed:" + MoveSpeed);
    }

    protected override void StartUseSkill() {

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
