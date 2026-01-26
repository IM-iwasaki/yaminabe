using UnityEngine;
using UnityEngine.AI;
using Mirror;

/// <summary>
/// 敵AI（NavMesh 安全対応版）
/// </summary>
public class EnemyBaseAI : NetworkBehaviour {
    NavMeshAgent agent;

    void Awake() {
        agent = GetComponent<NavMeshAgent>();
    }

    public override void OnStartServer() {
        // サーバーでのみ有効化
        agent.enabled = true;

        // ★ ここが超重要
        PlaceOnNavMesh();
    }

    void Update() {
        if (!isServer) return;

        // NavMesh 上にいないなら何もしない
        if (!agent.isOnNavMesh) return;

        // ここで SetDestination してOK
    }

    /// <summary>
    /// 一番近い NavMesh 上の位置に補正する
    /// </summary>
    void PlaceOnNavMesh() {
        NavMeshHit hit;

        // 半径2m以内で NavMesh を探す
        if (NavMesh.SamplePosition(
            transform.position,
            out hit,
            2.0f,
            NavMesh.AllAreas)) {
            // NavMesh 上にワープ
            transform.position = hit.position;
        }
        else {
            Debug.LogError("NavMesh が見つかりません");
        }
    }
}
