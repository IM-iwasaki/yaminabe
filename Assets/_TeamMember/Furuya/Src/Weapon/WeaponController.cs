using UnityEngine;

//現在は使用していない
//制作進行上完全に使わないことが確定したら削除

public class WeaponController : MonoBehaviour {
    public WeaponData currentWeapon;
    float lastAttackTime;

    public Transform firePoint; // 発射点 or 判定基準点

    public void TryAttack() {
        if (currentWeapon == null) return;
        if (Time.time < lastAttackTime + currentWeapon.cooldown) return;
        lastAttackTime = Time.time;

        if (currentWeapon.type == WeaponType.Melee)
            AttackMelee();
        else if (currentWeapon.type == WeaponType.Gun)
            AttackRanged();
    }


    void AttackMelee() {
        // 簡単なオフライン判定（サーバー処理はNetworkWeaponで行う）
        Collider[] hits = Physics.OverlapSphere(firePoint.position, currentWeapon.range);
        foreach (var c in hits) {
            var hp = c.GetComponent<CharacterBase>();
            if (hp != null) {
                hp.TakeDamage(currentWeapon.damage);
                //if (currentWeapon.hitEffectPrefab)
                    //Instantiate(currentWeapon.hitEffectPrefab, c.transform.position, Quaternion.identity);
            }
        }
    }

    void AttackRanged() {
        if (currentWeapon.projectilePrefab == null) return;
        var proj = Instantiate(currentWeapon.projectilePrefab, firePoint.position, firePoint.rotation);
        var projRb = proj.GetComponent<Rigidbody>();
        if (projRb) projRb.velocity = firePoint.forward * currentWeapon.projectileSpeed;
    }
}
