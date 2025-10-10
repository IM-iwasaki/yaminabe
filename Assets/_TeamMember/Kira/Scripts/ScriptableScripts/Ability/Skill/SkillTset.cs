using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character/Skill/_Test")]
public class SkillTset : SkillBase {
    public override void Activate(GameObject user) {
        Debug.Log("スキル実行関数が呼ばれました。");
    }
}
