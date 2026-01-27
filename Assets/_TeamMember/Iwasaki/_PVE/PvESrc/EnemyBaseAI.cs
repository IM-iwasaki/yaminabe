using UnityEngine;
using UnityEngine.AI;
using Mirror;

/// <summary>
/// 敵AI（サーバー管理・NavMesh追跡）
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyBaseAI : NetworkBehaviour {
    private NavMeshAgent agent;
    private Transform target;               // 追跡対象
    private EnemyStatus status;          // ステータス管理（NetworkBehaviour）

    void Awake() {
        // コンポーネント取得のみ（状態変更しない）
        agent = GetComponent<NavMeshAgent>();
        status = GetComponent<EnemyStatus>();
    }

    public override void OnStartServer() {
        // NavMeshAgent はサーバーでのみ有効
        agent.enabled = true;

        // NavMesh 上に強制配置
        PlaceOnNavMesh();

        // ステータスから移動速度を反映（サーバーのみ）
        if (status != null) {
            agent.speed = status.GetMoveSpeed();
        }

        // 初回ターゲット探索
        target = FindClosestPlayer();
    }

    /// <summary>
    /// サーバーでのみ実行される Update
    /// </summary>
    [ServerCallback]
    void Update() {
        // NavMesh に乗っていない場合は何もしない
        if (!agent.isOnNavMesh) return;

        // ターゲットが消えたら再探索
        if (target == null) {
            target = FindClosestPlayer();
            return;
        }

        // プレイヤーを追跡
        agent.SetDestination(target.position);
    }

    /// <summary>
    /// 一番近いプレイヤーを探す（サーバー）
    /// </summary>
    Transform FindClosestPlayer() {
        float minDist = float.MaxValue;
        Transform closest = null;

        foreach (var conn in NetworkServer.connections.Values) {
            if (conn.identity == null) continue;

            float dist = Vector3.Distance(
                transform.position,
                conn.identity.transform.position
            );

            if (dist < minDist) {
                minDist = dist;
                closest = conn.identity.transform;
            }
        }

        return closest;
    }

    /// <summary>
    /// NavMesh 上に補正配置
    /// </summary>
    void PlaceOnNavMesh() {
        NavMeshHit hit;

        if (NavMesh.SamplePosition(
            transform.position,
            out hit,
            2.0f,
            NavMesh.AllAreas)) {
            transform.position = hit.position;
        }
        else {
            Debug.LogError("NavMesh が見つかりません");
        }
    }
}
