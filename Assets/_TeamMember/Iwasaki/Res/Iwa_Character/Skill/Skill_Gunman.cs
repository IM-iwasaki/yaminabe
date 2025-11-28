using UnityEngine;

[CreateAssetMenu(menuName = "Character/Skill/Gunman_ガンマンソウル")]
public class Skill_Gunman : SkillBase {

    //
    //  スキル名：ガンマンソウル
    //  タイプ　：自己回復
    //  効果    ：回復する
    //　CT      ：30秒
    //

    public override void Activate(CharacterBase user) {
        //回復上昇開始
        user.Heal(0.4f, 3.0f);
    }
}
