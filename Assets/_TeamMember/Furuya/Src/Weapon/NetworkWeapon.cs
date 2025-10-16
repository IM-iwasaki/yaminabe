using UnityEngine;
using Mirror;
using Mirror.BouncyCastle.Asn1.Pkcs;

public class NetworkWeapon : NetworkBehaviour {
    public WeaponData weaponData;
    public Transform firePoint;
    float lastAttackTime;

    // --- 攻撃リクエスト ---
    [Command]
    public void CmdRequestAttack(Vector3 direction) {
        if (!CanAttack()) return;
        lastAttackTime = Time.time;

        switch (weaponData.type) {
            case WeaponType.Melee:
                ServerMeleeAttack();
                break;
            case WeaponType.Gun:
                ServerGunAttack(direction);
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

    // --- 銃撃処理（TPSレティクル方向） ---
    void ServerGunAttack(Vector3 direction) {
        if (weaponData.projectilePrefab == null) return;

        // 弾生成
        GameObject proj = ProjectilePool.Instance.GetFromPool(
            weaponData.projectilePrefab,
            firePoint.position,
            Quaternion.LookRotation(direction)
        );

        if (proj.TryGetComponent(out Projectile projScript)) {
            projScript.Init(
                gameObject,
                weaponData.hitEffectType,
                weaponData.projectileSpeed,
                weaponData.damage
            );
        }

        // 弾速を与える
        if (proj.TryGetComponent(out Rigidbody rb))
            rb.velocity = direction * weaponData.projectileSpeed;

        // エフェクト
        RpcPlayMuzzleFlash(firePoint.position, weaponData.muzzleFlashType);
    }

    // --- 魔法攻撃 ---
    void ServerMagicAttack(Vector3 direction) {
        if (weaponData is not MagicWeaponData magicData || magicData.projectilePrefab == null)
            return;

        GameObject proj = ProjectilePool.Instance.GetFromPool(
            magicData.projectilePrefab,
            firePoint.position,
            Quaternion.LookRotation(direction)
        );

        if (proj.TryGetComponent(out MagicProjectile projScript)) {
            projScript.Init(
                gameObject,
                magicData.magicType,
                magicData.hitEffectType,
                magicData.projectileSpeed,
                magicData.initialHeightSpeed,
                magicData.damage,
                direction
            );
        }

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
        return obj != gameObject; // 自分以外
    }
}
