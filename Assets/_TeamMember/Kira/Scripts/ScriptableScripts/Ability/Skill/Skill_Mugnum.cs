using UnityEngine;

[CreateAssetMenu(menuName = "Character/Skill/Mugnum_我慢")]
public class Skill_Mugnum : SkillBase {

    //
    //  スキル名：我慢
    //  タイプ　：自己強化型
    //  効果    ：ダメージを70％軽減する,移動速度を30％低下させる
    //　CT      ：30秒
    //

    public override void Activate(CharacterBase user) {
        user.DamageCut(30,5.0f);
        user.MoveSpeedBuff(0.7f, 5.0f);
    }
}
