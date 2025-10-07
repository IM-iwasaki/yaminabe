using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//
//  @file   Second_CharacterClass_Test
//
class PlayerBase : CharacterBase {

    protected override void StartAttack() {
        if (weaponController == null) return;

        // 武器が攻撃可能かチェックしてサーバー命令を送る
        weaponController.CmdRequestAttack();
    }

    // Start is called before the first frame update
    protected new void Start() {
        base.Start();        
    }

    // Update is called once per frame
    void Update() {
        MoveControl();
        LookControl();
    }

}
