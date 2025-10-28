using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character/Skill/Soldier_積極攻勢")]
public class Skill_Soldier : SkillBase {

    //
    //  スキル名：積極攻勢
    //  タイプ　：自己強化型
    //  効果    ：10秒間、攻撃力が30％上昇する。(仮。本実装では攻撃系にする)
    //　CT      ：20秒
    //

    public override void Activate(CharacterBase user) {
        //攻撃力上昇開始
        user.AttackBuff(1.3f,10.0f);
        //デバッグログを出す
        Debug.Log("スキルを使用しました。");
    }
}
