using UnityEngine;
using Mirror;

public class HeavyWeapon : WeaponBase {
    [SerializeField] GameObject explosionProjectile;
    [SerializeField] Transform firePoint;

    [Server]
    protected override void ServerAttack() {
        var proj = Instantiate(explosionProjectile, firePoint.position, firePoint.rotation);
        NetworkServer.Spawn(proj);
        var p = proj.GetComponent<ExplosionProjectile>();
        p.Init(data.damage);
    }
}
