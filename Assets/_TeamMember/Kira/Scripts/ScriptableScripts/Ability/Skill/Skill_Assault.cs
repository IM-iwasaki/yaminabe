using UnityEngine;

[CreateAssetMenu(menuName = "Character/Skill/Assault_迅速切込")]
public class Skill_Assault : SkillBase {

    //
    //  スキル名：迅速切込
    //  タイプ　：自己強化型
    //  効果    ：一瞬の間、移動速度を大幅に上昇。
    //　CT      ：14秒
    //

    public override void Activate(CharacterBase user) {
        //移動速度上昇開始
        user.MoveSpeedBuff(5.0f,0.3f);
        //デバッグログを出す
        Debug.Log("スキルを使用しました。");
    }
}
