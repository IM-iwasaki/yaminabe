using UnityEngine;
using UnityEngine.AI;
using Mirror;
using System.Collections;

/// <summary>
/// 敵AI（サーバー管理・NavMesh追跡）
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyBaseAI : NetworkBehaviour {
    private NavMeshAgent agent;
    private Transform target;               // 追跡対象
    private EnemyStatusBase status;          // ステータス管理

    void Awake() {
        // 参照取得のみ（ここでは有効化しない）
        agent = GetComponent<NavMeshAgent>();
        status = GetComponent<EnemyStatusBase>();

        // NavMesh 完成前に動かないよう無効化
        agent.enabled = false;
    }

    public override void OnStartServer() {
        // サーバーで NavMesh 完成待ちを開始
        StartCoroutine(WaitForNavMeshAndInitialize());
    }

    /// <summary>
    /// NavMesh が使用可能になるまで待ってから初期化する
    /// </summary>
    IEnumerator WaitForNavMeshAndInitialize() {
        // NavMesh が生成され、かつ自分の足元に存在するまで待つ
        while (!NavMesh.SamplePosition(
            transform.position,
            out _,
            2.0f,
            NavMesh.AllAreas)) {
            yield return null; // 1フレーム待機
        }

        // NavMesh が確認できたら Agent を有効化
        agent.enabled = true;

        // NavMesh 上に補正配置
        PlaceOnNavMesh();

        // ステータス反映
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
        // 初期化前 or NavMesh から外れている場合は何もしない
        if (!agent.enabled || !agent.isOnNavMesh) return;

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
