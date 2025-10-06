using UnityEngine;
using Mirror;

public class MagicWeapon : WeaponBase {
    [SerializeField] Transform firePoint;

    [Server]
    protected override void ServerAttack() {
        var proj = Instantiate(data.projectilePrefab, firePoint.position, firePoint.rotation);
        NetworkServer.Spawn(proj);

        var projectile = proj.GetComponent<Projectile>();
        projectile.Init(data.damage, data.projectileSpeed);
    }
}
