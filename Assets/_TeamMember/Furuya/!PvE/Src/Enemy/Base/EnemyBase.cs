using Mirror;
using UnityEngine;
using UnityEngine.AI;

public abstract class EnemyBase : NetworkBehaviour {

    [SerializeField] protected EnemyData enemyData;

    protected NavMeshAgent agent;
    protected Transform target;

    public override void OnStartServer() {
        agent = GetComponent<NavMeshAgent>();
        ApplyStatus();
        OnInitialize();
    }

    [ServerCallback]
    protected virtual void Update() {
        OnUpdateAI();
    }

    [Server]
    protected virtual void ApplyStatus() {
        agent.speed = enemyData.moveSpeed;
    }

    /// <summary>
    /// 初期化拡張用
    /// </summary>
    [Server]
    protected virtual void OnInitialize() { }

    /// <summary>
    /// 毎フレームのAI更新
    /// </summary>
    [Server]
    protected virtual void OnUpdateAI() { }

    /// <summary>
    /// 攻撃処理
    /// </summary>
    [Server]
    protected virtual void OnAttack() { }
}
