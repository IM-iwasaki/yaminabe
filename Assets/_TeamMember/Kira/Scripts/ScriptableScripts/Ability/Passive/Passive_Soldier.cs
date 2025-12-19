using UnityEngine;

[CreateAssetMenu(menuName = "Character/Passive/Soldier_実戦投入")]
public class Passive_Soldier : PassiveBase {

    //
    // パッシブ名　：実戦投入
    // タイプ      ：HP発動型
    // 効果        ：移動中は受けるダメージを軽減。
    //

    public override void PassiveReflection(CharacterBase user) {
        //移動中か検証、移動中であれば発動。
        if (user.parameter.isMoving) isPassiveActive = true;
        else isPassiveActive = false;

        //効果中はダメージを軽減。
        if (isPassiveActive) user.DamageRatio = 80;
        else user.DamageRatio = 100;        
    }
}
