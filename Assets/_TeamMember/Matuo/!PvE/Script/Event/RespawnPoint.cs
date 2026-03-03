using UnityEngine;
using Mirror;

public class RespawnPoint : NetworkBehaviour {
    public static RespawnPoint Instance;

    private void Awake() {
        Instance = this;
    }

    [Server]
    public void MoveTo(Transform destination) {
        transform.position = destination.position;
        transform.rotation = destination.rotation;
    }
}