using Mirror;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 敵ベース
/// ここでするのは索敵と攻撃リクエストのみ
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyBase : NetworkBehaviour {

    [Header("AI設定")]
    public float searchInterval = 1.0f;   // ターゲット探索間隔

    private NavMeshAgent agent;            // NavMeshAgent
    private EnemyWeaponController weapon;  // 武器コントローラー
    private EnemyStatusBase status;        // 敵ステータス

    private Transform target;              // 現在のターゲット（プレイヤー）

    /// <summary>
    /// サーバー開始時の初期化
    /// </summary>
    public override void OnStartServer() {

        // コンポーネント取得
        agent = GetComponent<NavMeshAgent>();
        weapon = GetComponent<EnemyWeaponController>();
        status = GetComponent<EnemyStatusBase>();

        var data = status.statusData;

        // NavMeshAgent 設定
        agent.speed = data.moveSpeed;                  // 移動速度
        agent.acceleration = data.acceleration;        // 加速
        agent.angularSpeed = data.angularSpeed;        // 旋回速度
        agent.stoppingDistance = data.stoppingDistance;// 攻撃開始距離

        // 定期的にターゲットを探す
        InvokeRepeating(nameof(SearchTarget), 0f, searchInterval);
    }

    /// <summary>
    /// 毎フレームのAI更新（サーバーのみ）
    /// </summary>
    [ServerCallback]
private void Update() {

    // status や parameter が null の場合もガード
    if (target == null)
        return;

    float distance = Vector3.Distance(transform.position, target.position);

    if (distance > agent.stoppingDistance) {
        agent.isStopped = false;
        agent.SetDestination(target.position);
    }
    else {
        agent.isStopped = true;
        Vector3 direction = (target.position - transform.position).normalized;
        weapon.ServerRequestAttack(direction);
    }
}


    /// <summary>
    /// 一番近い生存プレイヤーを探す
    /// </summary>
    [Server]
    private void SearchTarget() {

        float minDistance = float.MaxValue;
        Transform nearest = null;

        // 接続中プレイヤーを全探索
        foreach (var conn in NetworkServer.connections.Values) {
            if (conn.identity == null) continue;

            var player = conn.identity.GetComponent<CharacterBase>();
            if (player == null || player.parameter.isDead) continue;

            float dist = Vector3.Distance(
                transform.position,
                player.transform.position
            );

            if (dist < minDistance) {
                minDistance = dist;
                nearest = player.transform;
            }
        }

        target = nearest;
    }
}