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

    protected override void StartAttack(PlayerConst.AttackType _type = PlayerConst.AttackType.Main) {
        if (weaponController == null) return;

        // 武器が攻撃可能かチェックしてサーバー命令を送る
        Vector3 shootDir = GetShootDirection();
        weaponController.CmdRequestAttack(shootDir);      
    }

    protected override void StartUseSkill() {

    }

    // Start is called before the first frame update
    protected new void Start() {
        base.Start();
    }

    // Update is called once per frame
    void Update() {
        MoveControl();
    }    
}
