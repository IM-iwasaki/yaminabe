using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character/Skill/_Test")]
public class SkillTset : SkillBase {
    public override void Activate(CharacterBase user) {
        Debug.Log("スキル実行関数が呼ばれました。");
    }
}
