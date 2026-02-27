using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DoTエリア用　古谷
/// </summary>

public class DoTArea : NetworkBehaviour {
    [SyncVar] private int ownerTeamID;
    private int ID;
    private string ownerName;
    private EffectType hitEffectType;
    public float lifetime = 5f;
    private Rigidbody rb;
    private float speed = 20f;
    private bool initialized;

    private Dictionary<CreatureBase, float> timers = new();

    [Header("DoT Settings")]
    private int damage = 10;
    [SerializeField] private float interval = 1f;
    [SerializeField] private string[] targetTags = { "Player", "Enemy" };

    bool IsTarget(GameObject obj) {
        foreach (var tag in targetTags)
            if (obj.CompareTag(tag))
                return true;

        return false;
    }

    /// <summary>
    /// 弾の初期化（発射時に呼ぶ）
    /// </summary>
    public void Init(int _ownerTeamID, string _name, int _ID, EffectType hitEffect, float _speed, int _damage) {
        ownerTeamID = _ownerTeamID;
        ownerName = _name;
        ID = _ID;
        hitEffectType = hitEffect;
        speed = _speed;
        damage = _damage;

        if (rb == null) rb = GetComponent<Rigidbody>();

        if (rb != null) {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            rb.useGravity = false;
        }

        initialized = true;

        if (isServer) {
            StopAllCoroutines();
            StartCoroutine(AutoDisable()); // 自動で非アクティブ化
        }
    }

    void FixedUpdate() {
        if (!isServer || !initialized) return;

        if (Physics.Raycast(transform.position + Vector3.up * 0.5f,
                            Vector3.down,
                            out RaycastHit hit,
                            2f)) {
            Vector3 groundForward =
                Vector3.ProjectOnPlane(transform.forward, hit.normal).normalized;
            if (groundForward.sqrMagnitude < 0.001f)
                groundForward = Vector3.Cross(hit.normal, transform.right);
            transform.rotation =
                Quaternion.LookRotation(groundForward, hit.normal);
            transform.position += groundForward * speed * Time.fixedDeltaTime;
        }
    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other) {
        if (!initialized || !isServer) return;
        if (!IsTarget(other.gameObject)) return;

        var target = other.GetComponent<CreatureBase>();
        if (target == null) return;

        // 味方無効
        if (target.teamID == ownerTeamID) return;

        target.TakeDamage(damage, ownerName, ID);
        RpcPlayHitEffect(target.transform.position, hitEffectType);
    }


    [ServerCallback]
    private void OnTriggerStay(Collider other) {
        if (!initialized || !isServer) return;
        if (!IsTarget(other.gameObject)) return;

        var target = other.GetComponent<CreatureBase>();
        if (target == null) return;

        if (target.teamID == ownerTeamID) return;

        if (!timers.ContainsKey(target))
            timers[target] = 0f;

        timers[target] += Time.deltaTime;

        if (timers[target] < interval) return;

        timers[target] = 0f;

        target.TakeDamage(damage, ownerName, ID);
        RpcPlayHitEffect(target.transform.position, hitEffectType);
    }


    [ServerCallback]
    private void OnTriggerExit(Collider other) {
        var target = other.GetComponent<CreatureBase>();
        if (target != null)
            timers.Remove(target);
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
        timers.Clear();

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

        GameObject prefab = EffectPoolRegistry.Instance.GetHitEffect(effectType);
        if (prefab != null) {
            var fx = EffectPool.Instance.GetFromPool(prefab, pos, Quaternion.identity);
            fx.SetActive(true);
            EffectPool.Instance.ReturnToPool(fx, 1.5f);
        }
    }
}
