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
    public BossState bossState { get; private set; }

    //  ボスのステータス
    private EnemyStatusBase status;

    [Header("ボスの攻撃クールタイム")]
    [SerializeField] private float attackCooltime = 3.0f;
    [Header("ボスのフェーズ")]
    [SerializeField] private bool isPhase = false;

    //  攻撃クールタイムを計るタイマー
    private float attackTimer = 0;

    //  参照コンポーネント
    private PveBossSearchController search;
    private PveBossMoveController move;
    private PveBossAttackController attack;

    //  現在のターゲット
    public Transform currentTarget { get; private set; }

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

        //  攻撃中は何もしない
        if (bossState == BossState.Attack) return;

        //  攻撃状態以外でタイマーを進める
        attackTimer += Time.deltaTime;

        //  ターゲットの候補をリストに格納
        List<Transform> targets = search.GetTargets();
        //  ターゲット候補がいなければIdle
        if (targets.Count == 0) {
            currentTarget = null;
            ChangeState(BossState.Idle);
            return;
        }
        //  ターゲットが未設定または無効化されていたら再抽選
        if(currentTarget == null || !targets.Contains(currentTarget)) {
            currentTarget = SelectRandomTarget(targets);
        }

        //  攻撃可能判定を取る
        bool canAttack = CanAttack(attackTimer, bossState, currentTarget);
        //  攻撃可能判定がTrueなら攻撃、Falseなら移動
        if (canAttack) {
            Debug.Log("Bossの攻撃");
            ChangeState(BossState.Attack);
            attackTimer = 0;
            attack.TryAttack(currentTarget);
        }
        else {
            ChangeState(BossState.Move);
            move.MoveToTarget(currentTarget, status.statusData.moveSpeed);
        }
    }

    /// <summary>
    /// ボスの行動状態を更新
    /// </summary>
    /// <param name="state"></param>
    [Server]
    public void ChangeState(BossState state) {
        //  同じなら変更しない
        if(bossState == state) return;
        //  状態更新
        bossState = state;
        //  変更した際に攻撃中なら移動を止める
        switch (bossState) {
            case BossState.Attack:
                move.Stop();
                break;
            case BossState.Move:
                move.Resume();
                break;
        }
    }

    /// <summary>
    /// ターゲットを抽選する
    /// </summary>
    /// <returns></returns>
    private Transform SelectRandomTarget(List<Transform> targets) {
        int index = Random.Range(0, targets.Count);
        return targets[index];
    }

    /// <summary>
    /// 攻撃可能かどうかを判定する
    /// </summary>
    /// <param name="timer"></param>
    /// <param name="state"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    private bool CanAttack(float timer, BossState state,Transform target) {
        if(timer >= attackCooltime && 
            state != BossState.Attack && 
            target != null) {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 攻撃が終わったら呼ぶ
    /// </summary>
    public void EndAttack() {
        attack.EndAttack();
        move.Resume();
        ChangeState(BossState.Idle);
    }

}
