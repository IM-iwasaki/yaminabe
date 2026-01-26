using UnityEngine;
using UnityEngine.AI;
using Mirror;

/// <summary>
/// 敵AI（NavMesh 安全対応・追跡版）
/// </summary>
public class EnemyBaseAI : NetworkBehaviour {
    NavMeshAgent agent;
    Transform target; // 追いかける相手
    private EnemyBaseStatus status;

    void Awake() {
        agent = GetComponent<NavMeshAgent>();
        status = GetComponent<EnemyBaseStatus>();

        // ステータスの移動速度をNavMeshAgentに反映
        agent.speed = status.GetMoveSpeed();
    }

    public override void OnStartServer() {
        // サーバーでのみ有効化
        agent.enabled = true;

        // NavMesh 上に配置
        PlaceOnNavMesh();

        // 最初にターゲットを探す
        target = FindClosestPlayer();
    }

    void Update() {
        if (!isServer) return;

        // NavMesh 上にいないなら何もしない
        if (!agent.isOnNavMesh) return;

        // ターゲットがいなければ再探索
        if (target == null) {
            target = FindClosestPlayer();
            return;
        }

        // ★ これが「追いかける」正体
        agent.SetDestination(target.position);
    }

    /// <summary>
    /// 一番近いプレイヤーを探す
    /// </summary>
    Transform FindClosestPlayer() {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        float minDist = float.MaxValue;
        Transform closest = null;

        foreach (GameObject player in players) {
            float dist = Vector3.Distance(
                transform.position,
                player.transform.position
            );

            if (dist < minDist) {
                minDist = dist;
                closest = player.transform;
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
