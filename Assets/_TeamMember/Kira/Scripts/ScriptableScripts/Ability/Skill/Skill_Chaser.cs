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
        if (IsSkillUse) {
            Debug.Log("スキルの効果中です。");
            return;
        }
        //効果発動
        else IsSkillUse = true;
    }

    public override void SkillEffectUpdate(CharacterBase user) {
        //使用中か確認、効果中は時間を計測
        if(IsSkillUse) {
            UseTime += Time.deltaTime;
            IntervalTime += Time.deltaTime;

            //効果時間を過ぎたら効果を終了
            if (UseTime >= EffectTime) IsSkillUse = false;

            //攻撃が入力されていてかつインターバルが経過していたら
            if(user.IsAttackTrigger && IntervalTime >= user.FireInterval) {
                //インターバルをリセット
                IntervalTime = 0;
                //追加攻撃発動
                user.StartAttack();
            }
        }
    }
}
