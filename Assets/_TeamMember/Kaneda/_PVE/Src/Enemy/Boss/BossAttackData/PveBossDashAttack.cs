using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName = "PveBoss/BossAttack/DashAttack")]
public class PveBossDashAttack : PveBossAttackData {

    [Header("突進する時間")]
    [SerializeField] private float dashDuration = 1.0f;
    [Header("突進するときの速度")]
    [SerializeField] private float dashSpeed = 25.0f;
    [Header("多段ヒット設定")]
    [SerializeField] private float hitInterval = 0.2f;


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
        //  Rigidbodyを取得
        Rigidbody rb = boss.GetComponent<Rigidbody>();
        if (rb == null) {
            boss.EndAttack();
            yield break;
        }

        float elapsed = 0f;
        float hitTimer = 0f;

        while (elapsed < dashDuration) {
            float delta = Time.deltaTime;
            elapsed += delta;
            hitTimer += delta;

            Vector3 nextPos =
                rb.position + dir * dashSpeed * delta;

            
            rb.MovePosition(nextPos);

            //  ヒット制御
            if (hitTimer >= hitInterval) {
                weapon.ServerRequestAttack(dir);
                hitTimer = 0f;
            }

            yield return null;
        }

        //  突進後停止処理
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // 物理を一旦止める
        rb.isKinematic = true;

        // 1フレーム待つ（重要）
        yield return null;

        // 通常状態に戻す
        rb.isKinematic = false;

        boss.EndAttack();
    }
}
