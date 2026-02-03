using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(SphereCollider))]
public class EffectHitbox : NetworkBehaviour {

    [SerializeField] private int damage = 10;
    [SerializeField] private int maxHitPerTarget = 2;

    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float startRadius = 0.5f;
    [SerializeField] private float endRadius = 4f;

    private string ownerName;
    private int ID;

    private Vector3 forward;
    private float maxDistance;
    private float lifeTime;

    private Vector3 startPosition;
    private SphereCollider sphereCollider;

    private Dictionary<CharacterBase, int> hitCountMap = new();

    public override void OnStartServer() {
        sphereCollider = GetComponent<SphereCollider>();
        sphereCollider.isTrigger = true;
        hitCountMap.Clear();
    }

    [Server]
    public void Init(
        int _damage,
        string _ownerName,
        int _id,
        Vector3 _forward,
        float _maxDistance,
        float _lifeTime
    ) {
        damage = _damage;
        ownerName = _ownerName;
        ID = _id;

        forward = _forward.normalized;
        maxDistance = _maxDistance;
        lifeTime = _lifeTime;

        startPosition = transform.position;

        hitCountMap.Clear();
        sphereCollider.radius = startRadius;

        StartCoroutine(MoveAndExpand());
    }

    [Server]
    private IEnumerator MoveAndExpand() {
        float elapsed = 0f;

        while (true) {
            transform.position += forward * moveSpeed * Time.deltaTime;

            float traveled = Vector3.Distance(startPosition, transform.position);
            float t = traveled / maxDistance;

            sphereCollider.radius = Mathf.Lerp(startRadius, endRadius, t);

            if (traveled >= maxDistance)
                break;

            if (elapsed >= lifeTime)
                break;

            elapsed += Time.deltaTime;
            yield return null;
        }

        EffectPool.Instance.ReturnToPool(gameObject, 0.01f);
    }


    [ServerCallback]
    private void OnTriggerEnter(Collider other) {
        if (!other.TryGetComponent(out CharacterBase target))
            return;

        if (!hitCountMap.TryGetValue(target, out int hitCount))
            hitCount = 0;

        if (hitCount >= maxHitPerTarget)
            return;

        target.TakeDamage(damage, ownerName, ID);
        hitCountMap[target] = hitCount + 1;
    }


}
