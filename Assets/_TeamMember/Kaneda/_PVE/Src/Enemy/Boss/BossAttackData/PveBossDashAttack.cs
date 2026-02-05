using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PveBoss/BossAttack/DashAttack")]
public class PveBossDashAttack : PveBossAttackData {

    [Header("ダッシュする距離")]
    [SerializeField] private float dashDistance = 10.0f;
    [Header("ダッシュするときの速度")]
    [SerializeField] private float dashSpeed = 10.0f;
    [Header("多段ヒット設定")]
    [SerializeField] private float hitInterval = 0.3f;

    /// <summary>
    /// 攻撃を実際に処理する
    /// </summary>
    /// <param name="weapon"></param>
    /// <param name="boss"></param>
    /// <param name="target"></param>
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

        // 突進移動を開始（移動処理はControllerに任せる）
        boss.StartCoroutine(DashCoroutine(boss, weapon, dir));
    }

    /// <summary>
    /// 突進処理
    /// </summary>
    /// <param name="boss"></param>
    /// <param name="weapon"></param>
    /// <param name="dir"></param>
    /// <returns></returns>
    private IEnumerator DashCoroutine(
    PveBossController boss,
    EnemyWeaponController weapon,
    Vector3 dir) {
        //  距離を取得
        Vector3 start = boss.transform.position;
        Vector3 end = start + dir * dashDistance;

        float t = 0f;
        float duration = dashDistance / dashSpeed;

        float hitTime = 0f;

        while (t < duration) {
            t += Time.deltaTime;
            hitTime += Time.deltaTime;
            //  移動
            boss.transform.position = Vector3.Lerp(start, end, t / duration);
            //  攻撃がヒットするか
            if (hitTime >= hitInterval) {
                //  ヒットタイマーリセット
                hitTime = 0f;
                // 突進後に攻撃（ヒット判定）
                weapon.ServerRequestAttack(dir);
            }

            yield return null;
        }

        // 攻撃終了
        boss.EndAttack();
    }
}
