using UnityEngine;
using Mirror;
using System.Collections;

[CreateAssetMenu(menuName = "Character/Passive/Bomberman_爆風回避")]
public class Passive_Bomberman : PassiveBase {

    [SerializeField] private int repeatCount = 5;
    [SerializeField] private float delay = 0.2f;

    public override void PassiveSetting() {
        //発動中でなかったら発動中の状態にする
        if (!isPassiveActive){
            isPassiveActive = true;
            //クールタイム計測をリセット
            coolTime = 15;
        }
    }

    public override void PassiveReflection(CharacterBase user) {
        //発動中でなかったらクールタイムを計測
        if (!isPassiveActive) {
            coolTime += Time.deltaTime;
            //クールタイムがクールダウン以上になったら発動中にする
            if (coolTime >= cooldown) {
                isPassiveActive = true;
                //クールタイム計測をリセット
                coolTime = 0;
            }
        }

        if (isPassiveActive && user.parameter.HP <= 0) {
            Vector3 attackDir = user.parameter.GetShootDirection();
            StartExtraAttackDelay(user, delay, repeatCount, attackDir);
            //発動状態を解除
            isPassiveActive = false;
        }

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

            float angle = Random.Range(-180f, 180f);
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
