using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PveBossAttackController : NetworkBehaviour
{
    //  攻撃中かどうか
    public bool isAttacking {  get; private set; }

    private PveBossController boss;
    private EnemyWeaponController weapon;

    [Header("攻撃データリスト(現在最大2個)\n1.メイン攻撃\n2.スキル攻撃")]
    [SerializeField] private List<PveBossAttackData> bossAttack = new();

    private void Awake() {
        boss = GetComponent<PveBossController>();
        weapon = GetComponent<EnemyWeaponController>();
    }

    [Server]
    public bool TryAttack(Transform target) {
        //  攻撃中なら無視
        if(isAttacking) return false;

        //  ターゲットがいなければ無効
        if(target == null) return false;
        //  攻撃開始
        isAttacking = true;
        //  攻撃を抽選、使用
        RandomDrawAttack();

        return true;
    }

    /// <summary>
    /// 攻撃のランダム抽選
    /// </summary>
    private void RandomDrawAttack() {
        //  乱数を取得
        int rand = Random.Range(0, bossAttack.Count);
        //  抽選された攻撃を使用
        bossAttack[1].StartAttack(weapon, boss, boss.currentTarget);
    }

    /// <summary>
    /// 攻撃フラグを下す
    /// </summary>
    [Server]
    public void EndAttack() {
        isAttacking = false;
    }

}
