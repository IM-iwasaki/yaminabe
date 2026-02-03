using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PveBossController : NetworkBehaviour
{
    /// <summary>
    /// ボスの行動状態
    /// </summary>
    public enum BossState {
        Idle,
        Move,
        Attack
    }
    public BossState State { get; private set; }

    //  ボスのステータス
    private EnemyStatusBase status;

    [Header("ボスの攻撃クールタイム")]
    [SerializeField] private float attackCooltime = 3.0f;
    [Header("ボスの索敵範囲")]
    [SerializeField] private float searchRadius = 3.0f;

    //  攻撃クールタイムを計るタイマー
    private float attackTimer;

    /// <summary>
    /// 初期化、コンポーネントを取得
    /// </summary>
    void Awake() {
        status = GetComponent<EnemyStatusBase>();
    }

    private void Update() {
        if (!isServer) return;

        if(State != BossState.Attack) {
            attackTimer += Time.deltaTime;
        }
    }

    /// <summary>
    /// ボスの行動状態を更新
    /// </summary>
    /// <param name="state"></param>
    [Server]
    public void ChageState(BossState state) {
        State = state;
    }

}
