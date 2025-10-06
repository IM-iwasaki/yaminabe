using UnityEngine;
using Mirror;

public class ExplosionProjectile : NetworkBehaviour {
    [SerializeField] float radius = 3f;
    [SerializeField] float delay = 1.5f;
    int damage;

    [Server]
    public void Init(int dmg) {
        damage = dmg;
        Invoke(nameof(Explode), delay);
    }

    [Server]
    void Explode() {
        Collider[] hits = Physics.OverlapSphere(transform.position, radius);
        foreach (var c in hits) {
            var hp = c.GetComponent<CharacterBase>();
            if (hp != null) hp.TakeDamage(damage);
        }
        NetworkServer.Destroy(gameObject);
    }
}
