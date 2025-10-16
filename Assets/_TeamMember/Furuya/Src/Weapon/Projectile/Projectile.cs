using UnityEngine;
using Mirror;
using System.Collections;

public class Projectile : NetworkBehaviour {
    private int damage;
    private float speed;
    private float lifetime = 5f;
    private GameObject owner;
    private EffectType hitEffectType;
    private Rigidbody rb;

    [Server]
    public void Init(GameObject shooter, EffectType hitEffect, float _speed, int _damage) {
        owner = shooter;
        hitEffectType = hitEffect;
        speed = _speed;
        damage = _damage;

        if (rb == null) rb = GetComponent<Rigidbody>();

        StopAllCoroutines();
        StartCoroutine(AutoDisable()); // 生存時間で自動非アクティブ化
    }

    void FixedUpdate() {
        if (!isServer) return;
        transform.position += transform.forward * speed * Time.fixedDeltaTime;
    }

    void OnTriggerEnter(Collider other) {
        if (!isServer) return;
        if (other.gameObject == owner) return;

        var hp = other.GetComponent<CharacterBase>();
        if (hp != null) hp.TakeDamage(damage);

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
        yield return new WaitForSeconds(0.01f);
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
        if (prefab == null) {
            return;
        }

        var fx = EffectPoolManager.Instance.GetFromPool(prefab, pos, Quaternion.identity);
        EffectPoolManager.Instance.ReturnToPool(fx, 1.5f);
    }

}
