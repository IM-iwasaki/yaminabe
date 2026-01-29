using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Mirror.BouncyCastle.Asn1.X509;

public class EffectHitbox : NetworkBehaviour {
    [SerializeField] private int damage = 10;
    [SerializeField] private int maxHitPerTarget = 2;
    private string ownerName;
    private int ID;

    // ‘ÎÛ‚²‚Æ‚Ìƒqƒbƒg‰ñ”
    private Dictionary<CharacterBase, int> hitCountMap = new Dictionary<CharacterBase, int>();

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
