using UnityEngine;

[CreateAssetMenu(menuName = "Character/Skill/Mugnum_我慢")]
public class Skill_Mugnum : SkillBase {

    //
    //  スキル名：我慢
    //  タイプ　：断続自己強化型
    //  効果    ：一瞬の間、受けるダメージを激減するが、
    //          　移動速度が減少する。
    //　CT      ：神速
    //

    public override void Activate(CharacterBase user) {
        user.DamageCut(25,0.3f);
        user.MoveSpeedBuff(0.5f, 0.3f);
    }
}
