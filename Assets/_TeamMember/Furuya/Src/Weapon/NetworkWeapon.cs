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
            case WeaponType.Magic: // 🧙 魔法武器
                ServerMagicAttack();
                break;
        }
    }

    bool CanAttack() {
        return weaponData != null && Time.time >= lastAttackTime + weaponData.cooldown;
    }

    // --- 近接 ---
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

        Rigidbody rb = proj.GetComponent<Rigidbody>();
        if (rb != null)
            rb.velocity = firePoint.forward * weaponData.projectileSpeed;
    }

    // --- 🧙 魔法 ---
    void ServerMagicAttack() {
        MagicWeaponData magicData = weaponData as MagicWeaponData;
        if (magicData == null || magicData.projectilePrefab == null) return;

        GameObject proj = Instantiate(magicData.projectilePrefab, firePoint.position, firePoint.rotation);
        NetworkServer.Spawn(proj);

        // MagicProjectile に初期化
        MagicProjectile projScript = proj.GetComponent<MagicProjectile>();
        if (projScript != null) {
            projScript.Init(
                gameObject,                         // 発射者
                magicData.magicType,           // 弾のタイプ（Linear / Parabola）
                magicData.projectileSpeed,          // 前方速度
                magicData.initialHeightSpeed,       // 上方向初速
                magicData.damage                    // ダメージ
            );
        }

        // 発射エフェクト
        if (magicData.muzzleFlashPrefab != null)
            RpcPlayMagicEffect(firePoint.position, firePoint.rotation);
    }

    [ClientRpc]
    void RpcSpawnHitEffect(Vector3 pos) {
        if (weaponData.hitEffectPrefab != null)
            Instantiate(weaponData.hitEffectPrefab, pos, Quaternion.identity);
    }

    [ClientRpc]
    void RpcPlayMagicEffect(Vector3 pos, Quaternion rot) {
        MagicWeaponData magicData = weaponData as MagicWeaponData;
        if (magicData != null && magicData.muzzleFlashPrefab != null)
            Instantiate(magicData.muzzleFlashPrefab, pos, rot);
    }

    bool IsValidTarget(GameObject obj) {
        return obj != gameObject; // 自分以外
    }
}
