using UnityEngine;
using Mirror;
using System.Collections;
using UnityEditor;

public class GrenadeBase : NetworkBehaviour {
    [Header("Sub Weapon Data")]
    public GrenadeData data;
    [SyncVar] private int ownerTeamID;

    private Rigidbody rb;
    private bool exploded;

    /// <summary>
    /// 初期化
    /// </summary>
    [Server]
    public void Init(SubWeaponData _data, int teamID, Vector3 direction) {
        // GrenadeDataとして扱える場合のみキャスト
        data = _data as GrenadeData;
        ownerTeamID = teamID;

        rb = GetComponent<Rigidbody>();
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // 投擲
        float arcHeight = _data.throwForce * 0.5f;
        rb.AddForce(direction.normalized * _data.throwForce + Vector3.up * arcHeight, ForceMode.VelocityChange);

        // 起爆タイマー（GrenadeDataの場合のみ）
        if (data != null)
            StartCoroutine(FuseRoutine(data.explosionDelay));
        else
            StartCoroutine(FuseRoutine(1.5f)); // fallback
    }

    /// <summary>
    /// 起爆処理
    /// </summary>
    [Server]
    private IEnumerator FuseRoutine(float delay) {
        yield return new WaitForSeconds(delay);
        Explode();
        ReturnToPool();
    }

    [Server]
    private void Explode() {
        if (exploded || data == null) return;
        exploded = true;

        Vector3 pos = transform.position;
        float radius = data.explosionRadius;

        int bombLayer = LayerMask.GetMask("Character");

        Collider[] hits = Physics.OverlapSphere(pos, radius, bombLayer);
        foreach (var c in hits) {
            var target = c.GetComponent<CharacterBase>();
            if (target == null) continue;

            if (!data.canDamageAllies && target.TeamID == ownerTeamID) continue;

            target.TakeDamage(data.damage);
        }

        RpcPlayExplosion(pos);

#if UNITY_EDITOR
        ExplosionDebugCircle.Create(pos, radius, Color.red, 0.5f);
#endif
    }

    [ClientRpc]
    private void RpcPlayExplosion(Vector3 pos) {
        if (data == null || data.useEffectType == EffectType.Default) return;

        GameObject prefab = WeaponPoolRegistry.Instance.GetHitEffect(data.useEffectType);
        if (prefab != null) {
            var fx = EffectPoolManager.Instance.GetFromPool(prefab, pos, Quaternion.identity);
            EffectPoolManager.Instance.ReturnToPool(fx, 1.5f);
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

    void OnDrawGizmosSelected() {
        if (data == null) return;
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, data.explosionRadius);
    }
}

#if UNITY_EDITOR

public class ExplosionDebugCircle : MonoBehaviour {
    private float radius;
    private Color color;
    private float duration;
    private float timer;

    public static void Create(Vector3 pos, float radius, Color color, float duration) {
        var obj = new GameObject("ExplosionDebugCircle");
        var circle = obj.AddComponent<ExplosionDebugCircle>();
        circle.radius = radius;
        circle.color = color;
        circle.duration = duration;
        obj.transform.position = pos;
    }

    private void Update() {
        timer += Time.deltaTime;
        if (timer >= duration) Destroy(gameObject);
    }

    private void OnDrawGizmos() {
        Gizmos.color = color;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
#endif