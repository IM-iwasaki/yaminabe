using UnityEngine;
using Mirror;
using System.Collections;
using UnityEditor;

/// <summary>
/// グレネードの基礎データ
/// </summary>
public class GrenadeBase : NetworkBehaviour {
    [SyncVar] private int ownerTeamID;
    private string ownerName;

    private Rigidbody rb;
    protected bool exploded;

    // GrenadeDataから分離したパラメータ
    private float explosionRadius;
    private int damage;
    private bool canDamageAllies;
    protected EffectType effectType;
    private float explosionDelay = 1.5f;

    [Server]
    public void Init(int teamID, string _name, Vector3 direction, float throwForce, float projectileSpeed, float explosionRadius, int damage, bool canDamageAllies, EffectType hitEffect, float delay = 1.5f) {
        ownerTeamID = teamID;
        ownerName = _name;
        this.explosionRadius = explosionRadius;
        this.damage = damage;
        this.canDamageAllies = canDamageAllies;
        this.effectType = hitEffect;
        this.explosionDelay = delay;

        rb = GetComponent<Rigidbody>();
        rb.velocity = direction.normalized * projectileSpeed; // 初速を設定
        rb.angularVelocity = Vector3.zero;

        Vector3 arcForce = direction.normalized * throwForce + Vector3.up * (throwForce * 0.5f);
        rb.AddForce(arcForce, ForceMode.VelocityChange); // 放物線を描く力を追加

        StartCoroutine(FuseRoutine(explosionDelay));
    }

    /// <summary>
    /// 爆発まで待機
    /// </summary>
    /// <param name="delay"></param>
    /// <returns></returns>
    [Server]
    private IEnumerator FuseRoutine(float delay) {
        yield return new WaitForSeconds(delay);
        Explode();
        ReturnToPool();
    }

    /// <summary>
    /// 爆発処理
    /// </summary>
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

            target.TakeDamage(damage, ownerName);
        }

        AudioManager.Instance.CmdPlayWorldSE("Explode", transform.position);
        RpcPlayExplosion(pos, effectType, 1.5f);

#if UNITY_EDITOR
        ExplosionDebugCircle.Create(pos, explosionRadius, Color.red, 0.5f);
#endif
    }

    /// <summary>
    /// クライアントで爆発エフェクト表示
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="effectType"></param>
    /// <param name="duration"></param>
    [ClientRpc(includeOwner = true)]
    protected void RpcPlayExplosion(Vector3 pos, EffectType effectType, float duration) {
        GameObject prefab = EffectPoolRegistry.Instance.GetHitEffect(effectType);
        if (prefab != null) {
            var fx = EffectPool.Instance.GetFromPool(prefab, pos, Quaternion.identity);
            fx.SetActive(true);
            EffectPool.Instance.ReturnToPool(fx, duration);
        }
    }

    /// <summary>
    /// プールに戻す
    /// </summary>
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