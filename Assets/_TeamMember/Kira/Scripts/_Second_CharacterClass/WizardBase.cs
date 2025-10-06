using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//
//  @file   Second_CharacterClass
//
class WizardBase : CharacterBase {
    //–‚–@
    protected int MP { get; private set; }
    protected int MaxMP { get; private set; }

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
