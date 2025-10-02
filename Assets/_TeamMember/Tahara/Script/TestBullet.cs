using Mirror;
using UnityEngine;

public class TestBullet : NetworkBehaviour
{
    private void Awake() {
        var net = GetComponent<NetworkTransformHybrid>();
        net.syncDirection = SyncDirection.ServerToClient;
    }

    private void Update() {

        if (!isServer) return;
            if (transform.position.z >= 10)
                NetworkServer.Destroy(gameObject);

            transform.position += Vector3.forward * Time.deltaTime;
        
        
    }
}
