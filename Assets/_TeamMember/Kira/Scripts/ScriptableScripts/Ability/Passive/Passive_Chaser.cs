using UnityEngine;

[CreateAssetMenu(menuName = "Character/Skill/Chaser_マジックチェイン")]
public class Passive_Chaser : PassiveBase {

    //
    // パッシブ名　：マジックチェイン
    // タイプ      ：攻撃持続強化型
    // 効果        ：攻撃を行うたびにスキルのCTがごくわずかに短縮される。
    //               攻撃を行った回数に応じて効果が上昇する。(最大10回まで)
    //

    //攻撃のインターバルを計測
    private float intervalTime = 0;

    public override void PassiveSetting(CharacterBase user) {
        passiveChains = 0;
        intervalTime = 0;
    }

    public override void PassiveReflection(CharacterBase user) {
        intervalTime += Time.deltaTime;

        //攻撃した瞬間にインターバルが経過していたら
        if (user.isAttackPressed && intervalTime >= user.weaponController_main.weaponData.cooldown) {
            //チェインは最大50個まで、最大でなければチェインを蓄積
            if(passiveChains < 50){
                passiveChains++;
            }
            //インターバルリセット
            intervalTime = 0;

            //チェインの多さに応じてスキルCTを短縮
            user.skillAfterTime += (0.01f * passiveChains);
            //スキルCTが最大だったら補正
            float skillCooldown = user.GetComponent<GeneralCharacter>().equippedSkills[0].cooldown;
            if(user.skillAfterTime >= skillCooldown) {
                user.skillAfterTime = skillCooldown;
            }
        }
    }
}
