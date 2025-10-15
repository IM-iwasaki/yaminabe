using UnityEngine;
using Mirror;
using System.Collections;

public class MagicProjectile : NetworkBehaviour {
    private ProjectileType type = ProjectileType.Linear;
    private float speed = 20f;
    private float initialHeightSpeed = 5f;
    private int damage = 10;

    private Rigidbody rb;
    private GameObject owner;
    private EffectType hitEffectType;
    private float lifetime = 5f; // 最大生存時間

    /// <summary>
    /// 弾の初期化（発射時に呼ぶ）
    /// </summary>
    public void Init(GameObject shooter, ProjectileType _type, EffectType hitEffect, float _speed, float _initialHeightSpeed, int _damage, Vector3 direction) {
        owner = shooter;
        type = _type;
        hitEffectType = hitEffect;
        speed = _speed;
        initialHeightSpeed = _initialHeightSpeed;
        damage = _damage;

        if (rb == null) rb = GetComponent<Rigidbody>();

        if (rb != null) {
            if (type == ProjectileType.Parabola) {
                rb.useGravity = true;
                rb.velocity = direction * speed + Vector3.up * initialHeightSpeed;
            }
            else {
                rb.useGravity = false;
                rb.velocity = direction * speed;
            }
        }

        StopAllCoroutines();
        StartCoroutine(AutoDisable()); // 生存時間で自動非アクティブ化
    }

    void FixedUpdate() {
        if (!isServer) return;
        if (type == ProjectileType.Linear && rb == null)
            transform.position += transform.forward * speed * Time.fixedDeltaTime;
    }

    [ServerCallback]
    void OnTriggerEnter(Collider other) {
        if (other.gameObject == owner) return;

        if (other.TryGetComponent(out CharacterBase target))
            target.TakeDamage(damage);

        RpcPlayHitEffect(transform.position, hitEffectType);
        StartCoroutine(DisableProjectile());
    }

    /// <summary>
    /// 時間経過で非アクティブ化
    /// </summary>
    IEnumerator AutoDisable() {
        yield return new WaitForSeconds(lifetime);
        SetInactive();
    }

    /// <summary>
    /// ヒット時に非アクティブ化
    /// </summary>
    IEnumerator DisableProjectile() {
        yield return new WaitForSeconds(0.05f);
        SetInactive();
    }

    [Server]
    void SetInactive() {
        if (rb != null) rb.velocity = Vector3.zero;
        gameObject.SetActive(false);
    }

    [ClientRpc]
    void RpcPlayHitEffect(Vector3 pos, EffectType effectType) {
        GameObject prefab = WeaponPoolRegistry.Instance.GetHitEffect(effectType);
        if (prefab != null) {
            var fx = EffectPoolManager.Instance.GetFromPool(prefab, pos, Quaternion.identity);
            EffectPoolManager.Instance.ReturnToPool(fx, 1.5f);
        }
    }
}