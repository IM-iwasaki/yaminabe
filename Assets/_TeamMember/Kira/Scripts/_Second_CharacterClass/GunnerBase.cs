using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//
//  @file   Second_CharacterClass
//
class GunnerBase : CharacterBase {
    //間接職のみ：攻撃するたびに弾薬を消費、空になるとリロードが必要。レベルアップで最大弾容量が増加。
    protected int Magazine { get; private set; }
    protected int MaxMagazine { get; private set; }

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
