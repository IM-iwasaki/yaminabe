using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character/Skill/Bomberman_爆弾狂投")]
public class Skill_Bomberman : SkillBase {

    //
    //  スキル名：爆弾狂投
    //  タイプ　：攻撃型
    //  効果    ：沢山爆弾投げる
    //　CT      ：?秒
    //

    [SerializeField]private int repeatCount = 3;
    [SerializeField]private float delay = 0.2f;

    public override void Activate(CharacterBase user) {
        Vector3 attackDir = user.parameter.GetShootDirection();
        StartExtraAttackDelay(user, delay, repeatCount, attackDir);
    }

    /// <summary>
    /// コルーチンの起動用関数
    /// </summary>
    public void StartExtraAttackDelay(CharacterBase user, float delay, int repeatCount, Vector3 dir) {
        user.StartCoroutine(ExtraAttackRoutine(user, delay, repeatCount, dir));
    }

    /// <summary>
    /// 連続攻撃用のコルーチン
    /// </summary>
    private IEnumerator ExtraAttackRoutine(CharacterBase user, float delay, int repeatCount, Vector3 dir) {
        for (int i = 0; i < repeatCount; i++) {

            float angle = Random.Range(-30f, 30f);
            Vector3 randomDir = Quaternion.Euler(0f, angle, 0f) * dir;

            yield return new WaitForSeconds(delay);
            //攻撃する
            ExtraAttack(randomDir, user);
        }
        //追加攻撃が終わったら攻撃モーションを終わらせる
        user.animCon.StopShootAnim();
    }

    /// <summary>
    /// 追加攻撃のリクエスト
    /// </summary>
    private void ExtraAttack(Vector3 dir, CharacterBase user) {
        user.weaponController_main.CmdRequestExtraAttack(dir);
    }

}
