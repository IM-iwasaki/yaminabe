using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character/Passive/_Test")]
public class PassiveTest : PassiveBase {
    public override void PassiveReflection(GameObject user) {
        Debug.Log("パッシブが発動しています。");
    }
}
