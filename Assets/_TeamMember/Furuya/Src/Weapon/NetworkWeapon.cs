using UnityEngine;
using Mirror;

public class NetworkWeapon : NetworkBehaviour {
    public WeaponData weaponData;
    public Transform firePoint;
    float lastAttackTime;

    [Command] // クライアント → サーバーへ攻撃リクエスト
    public void CmdRequestAttack(Vector3 direction) {
        if (!CanAttack()) return;
        lastAttackTime = Time.time;

        switch (weaponData.type) {
            case WeaponType.Melee:
                ServerMeleeAttack();
                break;
            case WeaponType.Gun:
                //ServerRangedAttack(direction);
                break;
            case WeaponType.Magic:
                ServerMagicAttack(direction);
                break;
        }
    }

    bool CanAttack() {
        return weaponData != null && Time.time >= lastAttackTime + weaponData.cooldown;
    }

    public void SetWeaponData(WeaponData data) {
        weaponData = data;
    }

    // --- 近接攻撃 ---
    void ServerMeleeAttack() {
        Collider[] hits = Physics.OverlapSphere(firePoint.position, weaponData.range);
        foreach (var c in hits) {
            var hp = c.GetComponent<CharacterBase>();
            if (hp != null && IsValidTarget(hp.gameObject)) {
                hp.TakeDamage(weaponData.damage);
                RpcSpawnHitEffect(c.transform.position, weaponData.hitEffectType);
            }
        }
    }

    // --- 銃撃 ---
    void ServerRangedAttack() {
        if (weaponData.projectilePrefab == null) return;

        GameObject proj = Instantiate(weaponData.projectilePrefab, firePoint.position, firePoint.rotation);
        NetworkServer.Spawn(proj);

        Rigidbody rb = proj.GetComponent<Rigidbody>();
        if (rb != null)
            rb.velocity = firePoint.forward * weaponData.projectileSpeed;

        RpcPlayMuzzleFlash(firePoint.position, weaponData.muzzleFlashType);
    }

    // --- 魔法攻撃 ---
    void ServerMagicAttack(Vector3 direction) {
        MagicWeaponData magicData = weaponData as MagicWeaponData;
        if (magicData == null || magicData.projectilePrefab == null) return;

        GameObject proj = Instantiate(magicData.projectilePrefab, firePoint.position, Quaternion.LookRotation(direction));
        NetworkServer.Spawn(proj);

        MagicProjectile projScript = proj.GetComponent<MagicProjectile>();
        if (projScript != null) {
            projScript.Init(
                gameObject,
                magicData.magicType,
                magicData.projectileSpeed,
                magicData.initialHeightSpeed,
                magicData.damage,
                direction
            );
        }

        // 発射エフェクト
        RpcPlayMuzzleFlash(firePoint.position, magicData.muzzleFlashType);
    }

    // --- クライアントでヒットエフェクト再生 ---
    [ClientRpc]
    void RpcSpawnHitEffect(Vector3 pos, EffectType type) {
        GameObject prefab = WeaponPoolRegistry.Instance.GetHitEffect(type);
        if (prefab != null) {
            var fx = EffectPoolManager.Instance.GetFromPool(prefab, pos, Quaternion.identity);
            EffectPoolManager.Instance.ReturnToPool(fx, 1.5f);
        }
    }

    // --- クライアントでマズルフラッシュ再生 ---
    [ClientRpc]
    void RpcPlayMuzzleFlash(Vector3 pos, EffectType type) {
        GameObject prefab = WeaponPoolRegistry.Instance.GetMuzzleFlash(type);
        if (prefab != null) {
            var fx = EffectPoolManager.Instance.GetFromPool(prefab, pos, Quaternion.identity);
            EffectPoolManager.Instance.ReturnToPool(fx, 0.8f);
        }
    }

    bool IsValidTarget(GameObject obj) {
        return obj != gameObject; // 自分以外を対象
    }
}
