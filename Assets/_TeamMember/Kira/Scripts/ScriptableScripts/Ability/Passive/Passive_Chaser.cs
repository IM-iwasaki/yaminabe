using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Character/Skill/Chaser_マジックチェイン")]
public class Passive_Chaser : PassiveBase {

    //
    // パッシブ名　：マジックチェイン
    // タイプ      ：攻撃持続強化型
    // 効果        ：攻撃を行うたびにスキルのCTがごくわずかに短縮される。
    //               攻撃を行った回数に応じて効果が上昇する。(最大50回まで)
    //               また、スキルの使用中はMPが減らなくなる。
    //

    //攻撃のインターバルを計測
    private float intervalTime = 0;

    //CT短縮の効果の大きさ
    private readonly float CTAcceleration = 0.01f;
    //パッシブ蓄積数の最大数
    private readonly int passiveMaxChains = 50;

    public override void PassiveSetting() {
        passiveChains = 0;
        intervalTime = 0;
    }

    public override void PassiveReflection(CharacterBase user) {
        intervalTime += Time.deltaTime;

        //攻撃した瞬間にインターバルが経過していたら
        if (user.input.AttackPressed && intervalTime >= user.weaponController_main.weaponData.cooldown) {
            //チェインは最大50個まで、最大でなければチェインを蓄積
            if(passiveChains < passiveMaxChains){
                passiveChains++;
            }
            //インターバルリセット
            intervalTime = 0;

            //チェインの多さに応じてスキルCTを短縮
            user.parameter.skillAfterTime += (CTAcceleration * passiveChains);
            //スキルCTが最大だったら補正
            float skillCooldown = user.GetComponent<GeneralCharacter>().parameter.equippedSkills[0].cooldown;
            if(user.parameter.skillAfterTime >= skillCooldown) {
                user.parameter.skillAfterTime = skillCooldown;
            }
        }
    }
}
