using UnityEngine;
using Mirror;
using static UnityEngine.UI.GridLayoutGroup;

public class Projectile : NetworkBehaviour {
    int damage;
    float speed;
    float life = 5f;
    private GameObject owner;
    private EffectType hitEffectType;

    [Server]
    public void Init(GameObject shooter, float _speed, int _damage) {
        owner = shooter;
        speed = _speed;
        damage = _damage;
        Invoke(nameof(DestroySelf), life);
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
        DestroySelf();
    }

    [Server]
    void DestroySelf() => NetworkServer.Destroy(gameObject);

    [ClientRpc]
    void RpcPlayHitEffect(Vector3 pos, EffectType effectType) {
        GameObject prefab = WeaponPoolRegistry.Instance.GetHitEffect(effectType);
        if (prefab != null) {
            var fx = EffectPoolManager.Instance.GetFromPool(prefab, pos, Quaternion.identity);
            EffectPoolManager.Instance.ReturnToPool(fx, 1.5f);
        }
    }
}
