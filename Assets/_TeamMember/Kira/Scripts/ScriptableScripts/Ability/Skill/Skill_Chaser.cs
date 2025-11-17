using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName = "Character/Skill/Chaser_ダブルマジック")]
public class Skill_Chaser : SkillBase {
    
    //
    //  スキル名：ダブルマジック
    //  タイプ　：攻撃強化特殊型
    //  効果    ：6秒の間、自身の攻撃が2段攻撃になる。
    //　CT      ：18秒
    //

    //使用後の経過時間を計測
    private float UseTime = 0;
    //スキルの効果時間を定義
    private const float EffectTime = 6.0f;
    //追加攻撃のインターバルを計測
    private float IntervalTime = 0;

    public override void Activate(CharacterBase user) {
        //既に使用中か確認
        if (isSkillUse) return;
        //効果発動
        else isSkillUse = true;
    }

    public override void SkillEffectUpdate(CharacterBase user) {
        //使用中か確認、効果中は時間を計測
        if(isSkillUse) {
            UseTime += Time.deltaTime;
            IntervalTime += Time.deltaTime;

            //効果時間を過ぎたら効果を終了
            if (UseTime >= EffectTime) isSkillUse = false;

            //攻撃が入力された瞬間かつインターバルが経過していたら
            if(user.isAttackTrigger && IntervalTime >= user.weaponController_main.weaponData.cooldown) {
                //インターバルをリセット
                IntervalTime = 0;
                //追加攻撃発動
                user.weaponController_main.CmdRequestExtraAttack(user.firePoint.rotation.eulerAngles);
            }
        }
    }
}
