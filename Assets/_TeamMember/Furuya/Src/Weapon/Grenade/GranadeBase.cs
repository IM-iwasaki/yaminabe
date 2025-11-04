using UnityEngine;
using Mirror;
using System.Collections;
using UnityEditor;

public class GrenadeBase : NetworkBehaviour {
    [SyncVar] private int ownerTeamID;

    private Rigidbody rb;
    protected bool exploded;

    // GrenadeDataÇ©ÇÁï™ó£ÇµÇΩÉpÉâÉÅÅ[É^
    private float explosionRadius;
    private int damage;
    private bool canDamageAllies;
    protected EffectType effectType;
    private float explosionDelay = 1.5f;

    [Server]
    public void Init(int teamID, Vector3 direction, float throwForce, float projectileSpeed, float explosionRadius, int damage, bool canDamageAllies, EffectType hitEffect, float delay = 1.5f) {
        ownerTeamID = teamID;
        this.explosionRadius = explosionRadius;
        this.damage = damage;
        this.canDamageAllies = canDamageAllies;
        this.effectType = hitEffect;
        this.explosionDelay = delay;

        rb = GetComponent<Rigidbody>();
        rb.velocity = direction.normalized * projectileSpeed; // èâë¨Çê›íË
        rb.angularVelocity = Vector3.zero;

        Vector3 arcForce = direction.normalized * throwForce + Vector3.up * (throwForce * 0.5f);
        rb.AddForce(arcForce, ForceMode.VelocityChange); // ï˙ï®ê¸Çï`Ç≠óÕÇí«â¡

        StartCoroutine(FuseRoutine(explosionDelay));
    }

    [Server]
    private IEnumerator FuseRoutine(float delay) {
        yield return new WaitForSeconds(delay);
        Explode();
        ReturnToPool();
    }

    [Server]
    protected virtual void Explode() {
        if (exploded) return;
        exploded = true;

        Vector3 pos = transform.position;
        int bombLayer = LayerMask.GetMask("Character");

        Collider[] hits = Physics.OverlapSphere(pos, explosionRadius, bombLayer);
        foreach (var c in hits) {
            var target = c.GetComponent<CharacterBase>();
            if (target == null) continue;
            if (!canDamageAllies && target.TeamID == ownerTeamID) continue;

            target.TakeDamage(damage);
        }

        RpcPlayExplosion(pos, effectType, 1.5f);

#if UNITY_EDITOR
        ExplosionDebugCircle.Create(pos, explosionRadius, Color.red, 0.5f);
#endif
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
    private void ReturnToPool() {
        if (rb != null) {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        exploded = false;

        if (ProjectilePool.Instance != null)
            ProjectilePool.Instance.DespawnToPool(gameObject, 0.05f);
        else
            NetworkServer.Destroy(gameObject);
    }
}