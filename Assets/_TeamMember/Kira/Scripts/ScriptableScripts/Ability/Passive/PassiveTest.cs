using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character/その他/Passive_Test")]
public class PassiveTest : PassiveBase {
    public override void PassiveReflection(CharacterBase user) {
        //Debug.Log("パッシブが発動しています。");
    }
}
