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

        // ゼロベクトルチェック
        if (dir.sqrMagnitude < 0.0001f) {
            dir = boss.transform.forward;
        } else {
            dir.Normalize();
        }

        boss.StartCoroutine(MagicCoroutine(boss, weapon, dir));
    }

    private IEnumerator MagicCoroutine(
    PveBossController boss,
    EnemyWeaponController weapon,
    Vector3 dir) {
        Vector3 fixedDir = dir;

        yield return new WaitForSecondsRealtime(0.25f);

        Vector3 dirLeft = Quaternion.AngleAxis(-45f, Vector3.up) * fixedDir;
        Vector3 dirRight = Quaternion.AngleAxis(45f, Vector3.up) * fixedDir;

        weapon.ServerRequestSkill(fixedDir);
        weapon.ServerRequestSkill(dirLeft);
        weapon.ServerRequestSkill(dirRight);

        yield return new WaitForSecondsRealtime(1.5f);

        //  攻撃終了
        boss.EndAttack();
    }

}
