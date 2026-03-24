using UnityEngine;

[CreateAssetMenu(menuName = "Character/Skill/Mugnum_我慢")]
public class Skill_Mugnum : SkillBase {

    //
    //  スキル名：緊急回復
    //  タイプ　：停止回復型
    //  効果    ：短い間移動不能になるが
    //            HPをすばやく全回復する。(CT:神速)
    //　CT      ：神速
    //

    public override void Activate(CharacterBase user) {
        //user.MoveSpeedBuff(0.0001f, 1.0f);
        user.CmdUseSkill_MoveSpeed(0.0001f, 1.0f);
        user.CmdHealCharacter(1.0f, 0.2f);
    }
}
