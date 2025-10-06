using UnityEngine;
using Mirror;

public class Projectile : NetworkBehaviour {
    int damage;
    float speed;
    float life = 5f;

    [Server]
    public void Init(int dmg, float spd) {
        damage = dmg;
        speed = spd;
        Invoke(nameof(DestroySelf), life);
    }

    void FixedUpdate() {
        if (!isServer) return;
        transform.position += transform.forward * speed * Time.fixedDeltaTime;
    }

    [ServerCallback]
    void OnTriggerEnter(Collider other) {
        var hp = other.GetComponent<CharacterBase>();
        if (hp != null) hp.TakeDamage(damage);
        DestroySelf();
    }

    [Server]
    void DestroySelf() => NetworkServer.Destroy(gameObject);
}
