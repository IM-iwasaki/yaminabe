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
    [Header("ボスのフェーズ")]
    [SerializeField] private bool isPhase = false;

    //  攻撃クールタイムを計るタイマー
    private float attackTimer;

    //  参照コンポーネント
    private PveBossSearchController search;
    private PveBossMoveController move;
    private PveBossAttackController attack;

    //  現在のターゲット
    private Transform targetPlayer;

    /// <summary>
    /// 初期化、コンポーネントを取得
    /// </summary>
    void Awake() {
        status = GetComponent<EnemyStatusBase>();
        search = GetComponent<PveBossSearchController>();
        move = GetComponent<PveBossMoveController>();
        attack = GetComponent<PveBossAttackController>();
    }

    private void Update() {
        //  server以外で処理しない
        if (!isServer) return;
        //  攻撃状態以外でタイマーを進める
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
