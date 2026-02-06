using UnityEngine;
using Mirror;
using System.Collections;

/// <summary>
/// 魔法
/// </summary>
[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class MagicProjectile : NetworkBehaviour {
    private ProjectileType type = ProjectileType.Linear;
    private float speed = 20f;
    private float initialHeightSpeed = 5f;
    private int damage = 10;

    private Rigidbody rb;
    private GameObject owner;
    private string ownerName;
    private int ID;
    private EffectType hitEffectType;
    private bool initialized;
    [SerializeField]private float lifetime = 5f;

    /// <summary>
    /// 弾の初期化（発射時に呼ぶ）
    /// </summary>
    public void Init(GameObject shooter, string _name, int _ID, ProjectileType _type, EffectType hitEffect, float _speed, float _initialHeightSpeed, int _damage, Vector3 direction) {
        owner = shooter;
        ownerName = _name;
        ID = _ID;
        type = _type;
        hitEffectType = hitEffect;
        speed = _speed;
        initialHeightSpeed = _initialHeightSpeed;
        damage = _damage;

        if (rb == null) rb = GetComponent<Rigidbody>();

        if (rb != null) {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            if (type == ProjectileType.Parabola) {
                rb.useGravity = true;
                rb.velocity = direction * speed + Vector3.up * initialHeightSpeed;
            }
            else if (type == ProjectileType.GroundLine) {
                rb.useGravity = false;

                // 水平向き
                Vector3 forward = direction;
                forward.y = 0;
                forward.Normalize();

                // 床に吸着
                Vector3 pos = transform.position;
                if (Physics.Raycast(pos + Vector3.up, Vector3.down, out RaycastHit hit, 5f)) {
                    transform.position = hit.point;
                    transform.rotation = Quaternion.LookRotation(forward, hit.normal);
                }
                else {
                    transform.rotation = Quaternion.LookRotation(forward);
                }
            }
            else {
                rb.useGravity = false;
                rb.velocity = direction * speed;
            }
        }

        initialized = true;

        if (isServer) {
            StopAllCoroutines();
            StartCoroutine(AutoDisable()); // 自動で非アクティブ化
        }

        // エフェクト再初期化（必須）
        var particles = GetComponentsInChildren<ParticleSystem>(true);
        foreach (var ps in particles) {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.Play();
        }

    }

    void FixedUpdate() {
        if (!isServer) return;
        if (type == ProjectileType.Linear && rb == null)
            transform.position += transform.forward * speed * Time.fixedDeltaTime;
    }

    [ServerCallback]
    void OnTriggerEnter(Collider other) {
        if (!initialized || !isServer) return;
        if (other.gameObject == owner ||
            other.CompareTag("Magic")) return;

        // GroundLine は床に当たっても消さない
        if (type == ProjectileType.GroundLine) {
            // キャラ以外は無視
            if (!other.TryGetComponent(out CreatureBase target))
                return;

            // チーム判定
            if (target.parameter.TeamID != owner.GetComponent<CreatureBase>().parameter.TeamID
                || target.parameter.TeamID == -1) {
                target.TakeDamage(damage, ownerName, ID);
            }

            RpcPlayHitEffect(transform.position, hitEffectType);
            return; //消さない
        }

        // ---- 既存 Projectile 用 ----
        if (other.TryGetComponent(out CreatureBase targetNormal)) {
            if (targetNormal.parameter.TeamID != owner.GetComponent<CreatureBase>().parameter.TeamID
                || targetNormal.parameter.TeamID == -1) {
                targetNormal.TakeDamage(damage, ownerName, ID);
            }
        }

        RpcPlayHitEffect(transform.position, hitEffectType);
        Deactivate();
    }

    /// <summary>
    /// 自動で不可視
    /// </summary>
    /// <returns></returns>
    IEnumerator AutoDisable() {
        yield return new WaitForSeconds(lifetime);
        Deactivate();
    }

    /// <summary>
    /// 非アクティブ化
    /// </summary>
    [Server]
    private void Deactivate() {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        initialized = false;

        if (ProjectilePool.Instance != null)
            ProjectilePool.Instance.DespawnToPool(gameObject);
        else
            NetworkServer.Destroy(gameObject);
    }

    [Server]
    public void HideImmediately() {
        StartCoroutine(HideNextFrame());
    }

    [Server]
    private IEnumerator HideNextFrame() {
        yield return null; // 1フレーム待つ
        ProjectilePool.Instance.DespawnToPool(gameObject);
    }


    /// <summary>
    /// クライアントエフェクト表示
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="effectType"></param>
    [ClientRpc(includeOwner = true)]
    void RpcPlayHitEffect(Vector3 pos, EffectType effectType) {
        if (effectType == EffectType.Default) return;

        GameObject prefab = EffectPoolRegistry.Instance.GetHitEffect(effectType);
        if (prefab != null) {
            var fx = EffectPool.Instance.GetFromPool(prefab, pos, Quaternion.identity);
            fx.SetActive(true);
            EffectPool.Instance.ReturnToPool(fx, 3f);
        }
    }
}