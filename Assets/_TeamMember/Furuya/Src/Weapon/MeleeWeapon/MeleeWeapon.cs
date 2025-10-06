using UnityEngine;
using Mirror;

public class MeleeWeapon : WeaponBase {
    [SerializeField] Transform attackOrigin;

    [Server]
    protected override void ServerAttack() {
        Collider[] hits = Physics.OverlapSphere(attackOrigin.position, data.range);
        foreach (var hit in hits) {
            var hp = hit.GetComponent<CharacterBase>();
            if (hp != null && hp.gameObject != gameObject) {
                hp.TakeDamage(data.damage);
            }
        }
    }
}
