using Mirror;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;

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
    private Animator animator;

    private Transform target;              // 現在のターゲット（プレイヤー）

    [Header("多段ヒット設定")]
    [SerializeField] private float hitInterval = 0.5f;
    private float hitTimer = 0.0f;

    [Header("攻撃判定用レイヤー")]
    [SerializeField] private LayerMask wallLayer = default; // PVEWall を指定

    [Header("攻撃距離設定")]
    [SerializeField] private float attackRange = 2.0f;

    [Header("突進攻撃設定")]
    [SerializeField] private float dashChance = 0.2f;
    [SerializeField] private float dashDuration = 1.5f;
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashHitInterval = 0.2f;

    private bool isDashing = false;

    /// <summary>
    /// サーバー開始時の初期化
    /// </summary>
    public override void OnStartServer() {

        // コンポーネント取得
        agent = GetComponent<NavMeshAgent>();
        weapon = GetComponent<EnemyWeaponController>();
        status = GetComponent<EnemyStatusBase>();
        animator = GetComponent<Animator>();

        var data = status.statusData;

        // NavMeshAgent 設定
        agent.speed = data.moveSpeed;
        agent.acceleration = data.acceleration;
        agent.angularSpeed = data.angularSpeed;
        agent.stoppingDistance = data.stoppingDistance;

        // 定期的にターゲットを探す
        InvokeRepeating(nameof(SearchTarget), 0f, searchInterval);
    }

    /// <summary>
    /// 毎フレームのAI更新（サーバーのみ）
    /// </summary>
    [ServerCallback]
    private void Update() {

        if (target == null)
            return;

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance > attackRange) {

            // 突進中なら何もしない
            if (isDashing) return;

            // ランダムで突進開始
            if (Random.value < dashChance * Time.deltaTime) {
                StartCoroutine(DashCoroutine());
                return;
            }

            agent.isStopped = false;
            agent.SetDestination(target.position);
        } else {
            // 攻撃距離内
            if (!Physics.Linecast(transform.position, target.position, wallLayer)) {
                agent.isStopped = true;

                Vector3 direction = (target.position - transform.position).normalized;

                hitTimer += Time.deltaTime;
                if (hitTimer >= hitInterval) {
                    hitTimer = 0.0f;
                    weapon.ServerRequestAttack(direction);
                    animator.SetTrigger("Attack");
                }
            } else {
                agent.isStopped = false;
                agent.SetDestination(target.position);
            }
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

    /// <summary>
    /// 突進攻撃
    /// </summary>
    /// <returns></returns>
    private IEnumerator DashCoroutine() {

        isDashing = true;

        agent.isStopped = true;
        agent.updatePosition = false;
        agent.updateRotation = false;

        Vector3 dir = (target.position - transform.position);
        dir.y = 0f;
        dir.Normalize();

        transform.rotation = Quaternion.LookRotation(dir);

        float elapsed = 0f;
        float hitTimer = 0f;

        while (elapsed < dashDuration) {

            float delta = Time.deltaTime;
            elapsed += delta;
            hitTimer += delta;

            float moveDistance = dashSpeed * delta;

            // 壁チェック
            if (Physics.Raycast(transform.position, dir, moveDistance, wallLayer)) {
                // 壁に当たるので突進終了
                break;
            }

            Vector3 nextPos = transform.position + dir * moveDistance;

            // NavMesh外チェック
            NavMeshHit moveHit;
            if (NavMesh.SamplePosition(nextPos, out moveHit, 1.0f, NavMesh.AllAreas)) {
                transform.position = moveHit.position;
            } else {
                // NavMesh外に出るので突進終了
                break;
            }

            // 多段ヒット
            if (hitTimer >= dashHitInterval) {
                weapon.ServerRequestAttack(dir);
                hitTimer = 0f;
            }

            yield return null;
        }

        // NavMeshに安全に戻す
        NavMeshHit endHit;
        if (NavMesh.SamplePosition(transform.position, out endHit, 2.0f, NavMesh.AllAreas)) {
            agent.Warp(endHit.position);
        } else {
            Debug.LogWarning("NavMeshに戻れない");
        }

        agent.updatePosition = true;
        agent.updateRotation = true;
        agent.isStopped = false;

        isDashing = false;
    }
}