using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Character/Skill/Soldier_積極攻勢")]
public class Skill_Soldier : SkillBase {

    //
    //  スキル名：全力突撃
    //  タイプ　：連続攻撃型
    //  効果    ：前方に突進しながら怒涛の連続攻撃を行う。
    //　CT      ：18秒
    //

    //前に移動する力の強さ
    readonly float forwardPower = 27.0f;
    //上に移動する力の強さ
    readonly float upPower = 3.5f;

    public override void Activate(CharacterBase user) {       
        Vector3 attackDir = user.parameter.GetShootDirection();
        StartExtraAttackDelay(user, 0.015f, 16, attackDir);
    }

    /// <summary>
    /// コルーチンの起動用関数
    /// </summary>
    public void StartExtraAttackDelay(CharacterBase user,float delay, int repeatCount, Vector3 dir) {
        user.StartCoroutine(ExtraAttackRoutine(user, delay, repeatCount, dir));
    }

    /// <summary>
    /// 連続攻撃用のコルーチン
    /// </summary>
    private IEnumerator ExtraAttackRoutine(CharacterBase user, float delay, int repeatCount, Vector3 dir) {
        for (int i = 0; i < repeatCount; i++) {
            yield return new WaitForSeconds(delay);
            //前方に力を加える
            user.rb.velocity = user.transform.forward * forwardPower + user.transform.up * upPower;
            //攻撃する
            ExtraAttack(dir,user);
        }
    }

    /// <summary>
    /// 追加攻撃のリクエスト
    /// </summary>
    private void ExtraAttack(Vector3 dir, CharacterBase user) {
        user.weaponController_main.CmdRequestExtraAttack(dir);
    }
}
