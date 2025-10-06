using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//
//  @file   Second_CharacterClass
//
class MeleeBase : CharacterBase {
    //近接職のみ：攻撃速度が速い。レベルアップで攻撃速度がさらに上昇。
    protected int AttackSpeed { get; private set; }
    protected int MaxAttackSpeed { get; private set; }

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
