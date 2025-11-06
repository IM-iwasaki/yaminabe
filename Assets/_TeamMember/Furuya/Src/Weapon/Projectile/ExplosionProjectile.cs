using UnityEngine;
using Mirror;
using System.Collections;

public class ExplosionProjectile : NetworkBehaviour {
    private int damage;
    private float speed;
    private float radius;

    private Rigidbody rb;
    private GameObject owner;
    private string ownerName;
    private EffectType hitEffectType;
    private bool initialized;
    private float lifetime = 5f;

    protected bool exploded;

    public void Init(GameObject shooter, string _name, EffectType hitEffect, float _speed, int _damage, float _radius) {
        owner = shooter;
        ownerName = _name;
        hitEffectType = hitEffect;
        speed = _speed;
        damage = _damage;
        radius = _radius;

        exploded = false;
        initialized = true;

        rb = GetComponent<Rigidbody>();
        rb.velocity = transform.forward * speed;


        StartCoroutine(FuseRoutine(lifetime));
    }

    void FixedUpdate() {
        if (!isServer) return;
        if (!initialized) return;
        transform.position += transform.forward * speed * Time.fixedDeltaTime;
    }

    [ServerCallback]
    void OnTriggerEnter(Collider other) {
        if (!initialized || exploded || !isServer) return;

        // é©ï™é©êgÇÃî≠éÀå≥Ç…ÇÕìñÇΩÇÁÇ»Ç¢
        if (other.gameObject == owner) return;

        Explode();
    }

    [Server]
    private IEnumerator FuseRoutine(float delay) {
        yield return new WaitForSeconds(delay);
        Explode();
    }

    [ClientRpc(includeOwner = true)]
    protected void RpcPlayExplosion(Vector3 pos, EffectType effectType, float duration) {
        GameObject prefab = EffectPoolRegistry.Instance.GetHitEffect(effectType);
        if (prefab != null) {
            var fx = WeaponEffectPool.Instance.GetFromPool(prefab, pos, Quaternion.identity);
            fx.SetActive(true);
            WeaponEffectPool.Instance.ReturnToPool(fx, duration);
        }
    }

    [Server]
    protected virtual void Explode() {
        if (exploded) return;
        exploded = true;

        Vector3 pos = transform.position;
        int bombLayer = LayerMask.GetMask("Character");

        Collider[] hits = Physics.OverlapSphere(pos, radius, bombLayer);
        foreach (var c in hits) {
            var target = c.GetComponent<CharacterBase>();
            if (target == null) continue;

            target.TakeDamage(damage, ownerName);
        }

        AudioManager.Instance.CmdPlayWorldSE("Explode", transform.position);
        RpcPlayExplosion(pos, hitEffectType, 1.5f);

#if UNITY_EDITOR
        ExplosionDebugCircle.Create(pos, radius, Color.red, 0.5f);
#endif

        Deactivate();
    }

    [Server]
    private void Deactivate() {
        if (rb != null) {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        initialized = false;

        if (ProjectilePool.Instance != null)
            ProjectilePool.Instance.DespawnToPool(gameObject, 0.05f);
        else
            NetworkServer.Destroy(gameObject);
    }
}