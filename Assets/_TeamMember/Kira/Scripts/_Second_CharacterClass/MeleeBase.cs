using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//
//  @file   Second_CharacterClass
//
class MeleeBase : CharacterBase {
    //ãﬂê⁄
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
