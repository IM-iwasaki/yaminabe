using UnityEngine;
using Mirror;

/// <summary>
/// ƒvƒŒƒCƒ„[í“¬ˆ—
/// </summary>
public class PlayerCombat : NetworkBehaviour {
    [SyncVar] public int teamId = 0;

    /// <summary>
    /// “G‚ğ“|‚µ‚½‚Æ‚«‚Ìˆ—
    /// </summary>
    /// <param name="target">“|‚µ‚½‘Šè</param>
    [Server]
    public void OnKill(NetworkIdentity target) {
        var objectManager = FindAnyObjectByType<ObjectManager>();
        objectManager?.NotifyKill(teamId, 1);
    }
}