using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//
//  @file   Second_CharacterClass_Test
//
class PlayerBase : CharacterBase {
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
