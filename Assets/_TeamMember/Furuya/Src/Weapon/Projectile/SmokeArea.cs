using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DoTエリア用　古谷
/// </summary>

public class SmokeArea : MonoBehaviour {

    private Dictionary<CreatureBase, float> timers = new();

    [Header("DoT Settings")]
    [SerializeField]private int damage = 10;
    [SerializeField] private float interval = 1f;
    [SerializeField] private string[] targetTags = { "Player", "Enemy" };

    bool IsTarget(GameObject obj) {
        foreach (var tag in targetTags)
            if (obj.CompareTag(tag))
                return true;

        return false;
    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other) {
        if (!IsTarget(other.gameObject)) return;

        var target = other.GetComponent<CreatureBase>();
        if (target == null) return;

        // 味方無効
        //if (target.teamID == ownerTeamID) return;

        target.TakeDamage(damage, "Smoke", -1);
    }


    [ServerCallback]
    private void OnTriggerStay(Collider other) {
        if (!IsTarget(other.gameObject)) return;

        var target = other.GetComponent<CreatureBase>();
        if (target == null) return;

        //if (target.teamID == ownerTeamID) return;

        if (!timers.ContainsKey(target))
            timers[target] = 0f;

        timers[target] += Time.deltaTime;

        if (timers[target] < interval) return;

        timers[target] = 0f;

        target.TakeDamage(damage, "Smoke", -1);
    }


    [ServerCallback]
    private void OnTriggerExit(Collider other) {
        var target = other.GetComponent<CreatureBase>();
        if (target != null)
            timers.Remove(target);
    }
}
