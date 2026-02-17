using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PveBoss/BossAttack/MagicAttack")]
public class PveBossMagicAttack : PveBossAttackData {

    public override void StartAttack(
        EnemyWeaponController weapon, 
        PveBossController boss, 
        Transform target) {

        //  引数が無効なら終わらせる
        if (weapon == null || boss == null || target == null) {
            boss.EndAttack();
            return;
        }

        // ターゲット方向を取得（Y固定）
        Vector3 dir = target.position - boss.transform.position;
        dir.y = 0f;
        dir.Normalize();

        // 向きを先に合わせる（見た目の安定）
        boss.transform.rotation = Quaternion.LookRotation(dir);

        boss.StartCoroutine(MagicCoroutine(boss, weapon, dir));

    }

    private IEnumerator MagicCoroutine(
    PveBossController boss,
    EnemyWeaponController weapon,
    Vector3 dir) {

        yield return new WaitForSecondsRealtime(0.25f);

        // 左45度
        Vector3 dirLeft = Quaternion.AngleAxis(-45f, Vector3.up) * dir;

        // 右45度
        Vector3 dirRight = Quaternion.AngleAxis(45f, Vector3.up) * dir;

        //  三方向に攻撃を飛ばす
        weapon.ServerRequestAttack(dir);
        weapon.ServerRequestAttack(dirLeft);
        weapon.ServerRequestAttack(dirRight);

        yield return new WaitForSecondsRealtime(1.5f);

        //  攻撃終了
        boss.EndAttack();
    }

}
