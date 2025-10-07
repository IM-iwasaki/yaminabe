using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//
//  @file   Second_CharacterClass
//
class WizardBase : CharacterBase {
    //魔法職のみ：攻撃時に消費。時間経過で徐々に回復(攻撃中は回復しない)。レベルアップで最大MP(もしくは回復速度？)が上昇。
    protected int MP { get; private set; }
    protected int MaxMP { get; private set; }

    protected override void StatusInport() {
    }

    protected override void StartAttack() {
        
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
