using UnityEngine;

[CreateAssetMenu(menuName = "Character/Skill/Fighter_俊足")]
public class Skill_Fighter : SkillBase {

    //
    //  スキル名：俊足
    //  タイプ　：自己強化型
    //  効果    ：10秒間移動速度を上昇。
    //　CT      ：30秒
    //

    public override void Activate(CharacterBase user) {
        //移動速度上昇開始
        user.MoveSpeedBuff(2.0f, 10.0f);
    }
}

