using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Character/Skill/Chaser_ダブルマジック")]
public class Skill_Chaser : SkillBase {
    
    //
    //  スキル名：ダブルマジック
    //  タイプ　：攻撃強化特殊型
    //  効果    ：5秒の間、自身の攻撃が2段攻撃になる。
    //　CT      ：18秒
    //

    //使用後の経過時間を計測
    private float useTime = 0;
    //スキルの効果時間を定義
    private readonly float effectTime = 6.0f;
    //追加攻撃のインターバルを計測
    private float intervalTime = 0;
    //追加攻撃の発生ディレイ
    private float intervalDelay;

    public override void Activate(CharacterBase user) {
        //効果発動
        isSkillUse = true;
        //時間計測をリセット
        useTime = 0;
    }

    public override void SkillEffectUpdate(CharacterBase user) {
        intervalDelay = user.parameter.weaponController_main.weaponData.cooldown / 2;

        //使用中か確認、効果中は時間を計測
        if(isSkillUse) {
            useTime += Time.deltaTime;
            intervalTime += Time.deltaTime;

            //効果時間を過ぎたら効果を終了
            if (useTime >= effectTime) isSkillUse = false;

            //攻撃が入力された中かつインターバルが経過していたら
            if(user.input.AttackPressed && intervalTime >= intervalDelay) {
                //インターバルをリセット
                intervalTime = 0;
                //若干の遅延を入れて追加攻撃発動
                RequestExtraAttackWithDelay(intervalDelay, user);
            }

            //攻撃した瞬間にMP消費を相殺
            if (user.parameter.AttackTrigger) user.parameter.MP += 4;
        }       
    }

    //遅延をかけて追加攻撃開始の合図を送る
    public void RequestExtraAttackWithDelay(float delay,CharacterBase user){
        user.StartCoroutine(RequestExtraAttackRoutine(delay,user));
    }

    //遅延処理後に追加攻撃開始の合図を送る
    private IEnumerator RequestExtraAttackRoutine(float delay,CharacterBase user){
        yield return new WaitForSeconds(delay);

        // ここで追加攻撃を実行
        Vector3 shootDir = user.parameter.GetShootDirection();
        user.parameter.weaponController_main.CmdRequestExtraAttack(shootDir);        
    }
}
