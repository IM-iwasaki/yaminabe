using UnityEngine;
using Mirror;
using Mirror.Examples.Tanks;

public class NetworkWeapon : NetworkBehaviour {
    public WeaponData weaponData;
    public Transform firePoint;

    [Command] // クライアントからサーバーへ「攻撃して！」を送る
    public void CmdRequestAttack() {
        if (!CanAttack()) return;
        lastAttackTime = Time.time;
        if (weaponData.type == WeaponType.Melee)
            ServerMeleeAttack();
        else if (weaponData.type == WeaponType.Gun)
            ServerRangedAttack();
    }

    float lastAttackTime;
    bool CanAttack() => Time.time >= lastAttackTime + weaponData.cooldown;

    void ServerMeleeAttack() {
        Collider[] hits = Physics.OverlapSphere(firePoint.position, weaponData.range);
        foreach (var c in hits) {
            var hp = c.GetComponent<CharacterBase>();
            if (hp != null && IsValidTarget(hp.gameObject)) {
                hp.TakeDamage(weaponData.damage); // サーバー側でダメージ適用
                RpcSpawnHitEffect(c.transform.position);
            }
        }
    }

    void ServerRangedAttack() {
        var proj = Instantiate(weaponData.projectilePrefab, firePoint.position, firePoint.rotation);
        var netObj = proj.GetComponent<NetworkIdentity>();
        NetworkServer.Spawn(proj); // 全クライアントにプロジェクトル生成を通知
        var pb = proj.GetComponent<Projectile>();
        if (pb != null) pb.Init(weaponData.damage, weaponData.projectileSpeed);
    }

    bool IsValidTarget(GameObject target) {
        // チーム判定や味方判定をここで行う
        return true;
    }

    [ClientRpc]
    void RpcSpawnHitEffect(Vector3 pos) {
        if (weaponData.hitEffectPrefab) Instantiate(weaponData.hitEffectPrefab, pos, Quaternion.identity);
    }
}
