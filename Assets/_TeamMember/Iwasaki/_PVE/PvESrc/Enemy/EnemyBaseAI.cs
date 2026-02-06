//using UnityEngine;
//using UnityEngine.AI;
//using Mirror;

///// <summary>
///// 敵AI（サーバー管理・NavMesh 追跡）
///// </summary>
//[RequireComponent(typeof(NavMeshAgent))]
//public class EnemyBaseAI : NetworkBehaviour {

//    private NavMeshAgent agent;      // NavMesh 移動制御
//    private Transform target;        // 追跡対象プレイヤー
//    private EnemyStatusBase status;  // ステータス管理（速度など）

//    void Awake() {
//        // 参照取得のみ（ここでは移動処理はしない）
//        agent = GetComponent<NavMeshAgent>();
//        status = GetComponent<EnemyStatusBase>();

//        // 念のため無効化（Server 開始時に有効化）
//        agent.enabled = false;
//    }

//    public override void OnStartServer() {
//        // サーバーでのみ NavMeshAgent を有効化
//        agent.enabled = true;

//        // NavMesh 上に補正配置
//        PlaceOnNavMesh();

//        // ステータスから移動速度を反映
//        if (status != null) {
//            agent.speed = status.GetMoveSpeed();
//        }

//        // 初回ターゲット探索
//        target = FindClosestPlayer();
//    }

//    /// <summary>
//    /// サーバーでのみ実行される Update
//    /// </summary>
//    [ServerCallback]
//    void Update() {
//        // NavMesh に乗っていない場合は処理しない（安全装置）
//        if (!agent.enabled || !agent.isOnNavMesh) return;

//        // ターゲットが消えたら再探索
//        if (target == null) {
//            target = FindClosestPlayer();
//            return;
//        }

//        // プレイヤーを追跡
//        agent.SetDestination(target.position);
//    }

//    /// <summary>
//    /// 一番近いプレイヤーを探す（サーバー）
//    /// </summary>
//    Transform FindClosestPlayer() {
//        float minDist = float.MaxValue;
//        Transform closest = null;

//        foreach (var conn in NetworkServer.connections.Values) {
//            if (conn.identity == null) continue;

//            float dist = Vector3.Distance(
//                transform.position,
//                conn.identity.transform.position
//            );

//            if (dist < minDist) {
//                minDist = dist;
//                closest = conn.identity.transform;
//            }
//        }

//        return closest;
//    }

//    /// <summary>
//    /// NavMesh 上に補正配置する
//    /// ・初期配置ズレ対策
//    /// ・スポーン事故防止
//    /// </summary>
//    void PlaceOnNavMesh() {
//        NavMeshHit hit;

//        if (NavMesh.SamplePosition(
//            transform.position,
//            out hit,
//            2.0f,
//            NavMesh.AllAreas)) {

//            transform.position = hit.position;
//        }
//        else {
//            Debug.LogError("NavMesh が見つかりません（敵初期位置不正）");
//        }
//    }
//}