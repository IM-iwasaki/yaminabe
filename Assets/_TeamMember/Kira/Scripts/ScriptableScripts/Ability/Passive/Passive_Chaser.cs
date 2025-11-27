using JetBrains.Annotations;
using UnityEngine;

[CreateAssetMenu(menuName = "Character/Skill/Chaser_マジックチェイン")]
public class Passive_Chaser : PassiveBase {

    //
    // パッシブ名　：マジックチェイン
    // タイプ      ：攻撃持続強化型
    // 効果        ：攻撃を行うたびにスキルのCTがごくわずかに短縮される。
    //               攻撃を行った回数に応じて効果が上昇する。(最大10回まで)
    //

    //パッシブの蓄積数
    private int Chains = 0;
    //攻撃のインターバルを計測
    private float IntervalTime = 0;

    public override void PassiveSetting(CharacterBase user) {
        Chains = 0;
        IntervalTime = 0;
    }

    public override void PassiveReflection(CharacterBase user) {
        IntervalTime += Time.deltaTime;
        //リロード中は発動しない
        if (user.isReloading) return;

        //攻撃した瞬間にインターバルが経過していたら
        if (user.isAttackPressed && IntervalTime >= user.weaponController_main.weaponData.cooldown) {
            //チェインは最大10個まで、最大でなければチェインを蓄積
            if(Chains < 10)Chains++;
            //インターバルリセット
            IntervalTime = 0;

            //チェインの多さに応じてスキルCTを短縮
            user.skillAfterTime += (0.06f * Chains);
            //スキルCTが最大だったら補正
            float skillCooldown = user.GetComponent<GeneralCharacter>().equippedSkills[0].cooldown;
            if(user.skillAfterTime >= skillCooldown) {
                user.skillAfterTime = skillCooldown;
            }
        }
    }
}
