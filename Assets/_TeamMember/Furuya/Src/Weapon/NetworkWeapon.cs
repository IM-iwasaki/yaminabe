using UnityEngine;
using Mirror;
using System.Collections;

public class NetworkWeapon : NetworkBehaviour {
    public WeaponData weaponData;
    public Transform firePoint;
    float lastAttackTime;

    [Command]
    public void CmdRequestAttack() {
        if (!CanAttack()) return;
        lastAttackTime = Time.time;

        switch (weaponData.type) {
            case WeaponType.Melee:
                ServerMeleeAttack();
                break;
            case WeaponType.Gun:
                ServerRangedAttack();
                break;
        }
    }

    bool CanAttack() {
        return weaponData != null && Time.time >= lastAttackTime + weaponData.cooldown;
    }

    // ==========
    // 近接攻撃
    // ==========
    [Server]
    void ServerMeleeAttack() {
        Collider[] hits = Physics.OverlapSphere(firePoint.position, weaponData.range);
        RpcShowMeleeRange(firePoint.position, weaponData.range);

        foreach (var c in hits) {
            var hp = c.GetComponent<CharacterBase>();
            if (hp != null && IsValidTarget(hp.gameObject)) {
                hp.TakeDamage(weaponData.damage);
                RpcSpawnHitEffect(c.transform.position);
            }
        }
    }

    // ==========
    // 遠距離攻撃
    // ==========
    [Server]
    void ServerRangedAttack() {
        if (weaponData.projectilePrefab == null) return;

        // プールから取得
        var proj = EffectPoolManager.Instance.GetFromPool(
            weaponData.projectilePrefab,
            firePoint.position,
            firePoint.rotation
        );

        var projComp = proj.GetComponent<ProjectileBase>();
        if (projComp != null)
            projComp.Init(gameObject, weaponData.projectileSpeed, weaponData.damage);

        // Mirrorの同期を保つため、Spawn時にNetworkServer.Spawnを呼ぶ必要あり
        if (!proj.TryGetComponent<NetworkIdentity>(out var netId))
            proj.AddComponent<NetworkIdentity>();

        if (!proj.activeInHierarchy)
            proj.SetActive(true);

        NetworkServer.Spawn(proj);
        RpcSpawnMuzzleEffect(firePoint.position, firePoint.rotation);
    }

    // ==========
    // RPC通知
    // ==========
    [ClientRpc]
    void RpcSpawnHitEffect(Vector3 pos) {
        if (weaponData.hitEffectPrefab == null) return;
        var effect = EffectPoolManager.Instance.GetFromPool(weaponData.hitEffectPrefab, pos, Quaternion.identity);
        EffectPoolManager.Instance.ReturnToPool(effect, 1.5f);
    }

    [ClientRpc]
    void RpcSpawnMuzzleEffect(Vector3 pos, Quaternion rot) {
        if (weaponData.muzzleFlashPrefab == null) return;
        var effect = EffectPoolManager.Instance.GetFromPool(weaponData.muzzleFlashPrefab, pos, rot);
        EffectPoolManager.Instance.ReturnToPool(effect, 1.5f);
    }

    [ClientRpc]
    void RpcShowMeleeRange(Vector3 center, float range) {
        StartCoroutine(ShowSphereDebug(center, range, 0.2f));
    }

    IEnumerator ShowSphereDebug(Vector3 center, float radius, float time) {
        float elapsed = 0f;
        while (elapsed < time) {
            elapsed += Time.deltaTime;
            DebugDrawSphere(center, radius, Color.red);
            yield return null;
        }
    }

    void DebugDrawSphere(Vector3 center, float radius, Color color) {
        int segments = 20;
        for (int i = 0; i < segments; i++) {
            float angle1 = (i / (float) segments) * Mathf.PI * 2;
            float angle2 = ((i + 1) / (float) segments) * Mathf.PI * 2;
            Vector3 p1 = center + new Vector3(Mathf.Cos(angle1), 0, Mathf.Sin(angle1)) * radius;
            Vector3 p2 = center + new Vector3(Mathf.Cos(angle2), 0, Mathf.Sin(angle2)) * radius;
            Debug.DrawLine(p1, p2, color, 0.02f);
        }
    }

    bool IsValidTarget(GameObject obj) {
        return obj != gameObject;
    }
}
