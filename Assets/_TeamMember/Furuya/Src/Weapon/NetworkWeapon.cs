using UnityEngine;
using Mirror;

public class NetworkWeapon : NetworkBehaviour {
    public WeaponData weaponData;
    public Transform firePoint;
    float lastAttackTime;

    [Command] // クライアント → サーバーへ攻撃リクエスト
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
            case WeaponType.Magic:
                ServerMagicAttack();
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
                RpcSpawnHitEffect(c.transform.position);
            }
        }
    }

    // --- 銃撃 ---
    void ServerRangedAttack() {
        if (weaponData.projectilePrefab == null) return;

        GameObject proj = Instantiate(weaponData.projectilePrefab, firePoint.position, firePoint.rotation);
        NetworkServer.Spawn(proj);

        // Rigidbodyで前進させる
        Rigidbody rb = proj.GetComponent<Rigidbody>();
        if (rb != null)
            rb.velocity = firePoint.forward * weaponData.projectileSpeed;

        // 発射エフェクト再生
        RpcPlayMuzzleFlash(firePoint.position, firePoint.rotation);
    }

    // --- 魔法攻撃 ---
    void ServerMagicAttack() {
        MagicWeaponData magicData = weaponData as MagicWeaponData;
        if (magicData == null || magicData.projectilePrefab == null) return;

        GameObject proj = Instantiate(magicData.projectilePrefab, firePoint.position, firePoint.rotation);
        NetworkServer.Spawn(proj);

        // 弾の初期化
        MagicProjectile projScript = proj.GetComponent<MagicProjectile>();
        if (projScript != null) {
            projScript.Init(
                gameObject,
                magicData,
                magicData.magicType,
                magicData.projectileSpeed,
                magicData.initialHeightSpeed,
                magicData.damage
            );
        }

        // 発射エフェクト再生
        RpcPlayMagicEffect(firePoint.position, firePoint.rotation);
    }

    // --- クライアントでヒットエフェクト再生（プール対応） ---
    [ClientRpc]
    void RpcSpawnHitEffect(Vector3 pos) {
        if (weaponData.hitEffectPrefab == null) return;

        var fx = EffectPoolManager.Instance.GetFromPool(weaponData.hitEffectPrefab, pos, Quaternion.identity);
        EffectPoolManager.Instance.ReturnToPool(fx, 1.5f);
    }

    // --- 通常銃のマズルフラッシュ ---
    [ClientRpc]
    void RpcPlayMuzzleFlash(Vector3 pos, Quaternion rot) {
        if (weaponData.muzzleFlashPrefab == null) return;

        var fx = EffectPoolManager.Instance.GetFromPool(weaponData.muzzleFlashPrefab, pos, rot);
        EffectPoolManager.Instance.ReturnToPool(fx, 0.8f);
    }

    // --- 魔法用マズルフラッシュ ---
    [ClientRpc]
    void RpcPlayMagicEffect(Vector3 pos, Quaternion rot) {
        MagicWeaponData magicData = weaponData as MagicWeaponData;
        if (magicData == null || magicData.muzzleFlashPrefab == null) return;

        var fx = EffectPoolManager.Instance.GetFromPool(magicData.muzzleFlashPrefab, pos, rot);
        EffectPoolManager.Instance.ReturnToPool(fx, 1.2f);
    }

    bool IsValidTarget(GameObject obj) {
        return obj != gameObject; // 自分以外を対象
    }
}
