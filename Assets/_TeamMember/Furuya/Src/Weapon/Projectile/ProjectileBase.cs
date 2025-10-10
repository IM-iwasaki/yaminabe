using UnityEngine;
using Mirror;

public class ProjectileBase : NetworkBehaviour {
    public float speed = 20f;
    public float lifeTime = 3f;
    public int damage = 10;
    private float lifeTimer;
    private GameObject owner;

    public void Init(GameObject shooter, float _speed, int _damage) {
        owner = shooter;
        speed = _speed;
        damage = _damage;
        lifeTimer = 0f;
    }

    void FixedUpdate() {
        if (!isServer) return; // サーバーのみ移動
        transform.position += transform.forward * speed * Time.fixedDeltaTime;

        lifeTimer += Time.fixedDeltaTime;
        if (lifeTimer >= lifeTime)
            NetworkServer.Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other) {
        if (!isServer) return;
        if (other.gameObject == owner) return;

        var target = other.GetComponent<CharacterBase>();
        if (target != null)
            target.TakeDamage(damage);

        NetworkServer.Destroy(gameObject);
    }
}