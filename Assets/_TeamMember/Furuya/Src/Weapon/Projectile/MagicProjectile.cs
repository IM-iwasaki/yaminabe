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
    private EffectType hitEffectType;
    private bool initialized;
    private float lifetime = 5f;

    /// <summary>
    /// 弾の初期化（発射時に呼ぶ）
    /// </summary>
    public void Init(GameObject shooter, string _name, ProjectileType _type, EffectType hitEffect, float _speed, float _initialHeightSpeed, int _damage, Vector3 direction) {
        owner = shooter;
        ownerName = _name;
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
    }

    void FixedUpdate() {
        if (!isServer) return;
        if (type == ProjectileType.Linear && rb == null)
            transform.position += transform.forward * speed * Time.fixedDeltaTime;
    }

    [ServerCallback]
    void OnTriggerEnter(Collider other) {
        if (!initialized || !isServer) return;
        if (other.gameObject == owner) return;

        if (other.TryGetComponent(out CharacterBase target))
            target.TakeDamage(damage, ownerName);


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
            EffectPool.Instance.ReturnToPool(fx, 1.5f);
        }
    }
}