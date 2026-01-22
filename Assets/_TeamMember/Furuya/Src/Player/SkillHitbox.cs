using UnityEngine;
using Mirror;
using System.Collections.Generic;

public class SkillHitbox : NetworkBehaviour {

    Transform owner;
    int attackerTeam;
    int damage;
    string attackerName;
    int attackerID;

    HashSet<CharacterBase> hitTargets = new();

    public void Initialize(
        Transform _owner,
        int _team,
        int _damage,
        string _name,
        int _ID
    ) {
        owner = _owner;
        attackerTeam = _team;
        damage = _damage;
        attackerName = _name;
        attackerID = _ID;
    }

    void Update() {
        if (owner == null) return;

        transform.position =
            owner.position + owner.forward * 1.0f;
        transform.rotation = owner.rotation;
    }

    [ServerCallback]
    void OnTriggerEnter(Collider other) {

        CharacterBase target = other.GetComponent<CharacterBase>();
        if (target == null) return;

        if (target.parameter.TeamID == attackerTeam) return;

        // “¯ˆê‘ÎÛ‚Ö‚Ì‘½’i–h~
        if (hitTargets.Contains(target)) return;
        hitTargets.Add(target);

        target.TakeDamage(damage, attackerName, attackerID);
    }
}
