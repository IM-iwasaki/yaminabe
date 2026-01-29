using Mirror;
using UnityEngine;

public class RemoveObjectEvent : PVEStageEvent {

    [SerializeField] private GameObject target;

    [Server]
    public override void Execute() {
        NetworkServer.Destroy(target);
    }
}