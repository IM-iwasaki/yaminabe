using Mirror;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [SerializeField]
    private GameObject bullet = null;
    private void Awake() {
        var net = GetComponent<NetworkTransformHybrid>();
        net.syncDirection = SyncDirection.ServerToClient;
    }
    private void Update() {
        if (isLocalPlayer) {
            if (Input.GetKeyDown(KeyCode.Space)) {
                CmdShootBullet();
            }
        }
    }

    private void FixedUpdate() {
        if (isLocalPlayer) {
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");

            CmdPlayerMove(x, z);
        }
    }
    [Command]
    void CmdPlayerMove(float _x,float _z) {
        Vector3 v = new Vector3(_x, 0, _z) * 5;
        GetComponent<Rigidbody>().AddForce(v);
    }

    [Command]
    void CmdShootBullet() {
        GameObject obj = Instantiate(bullet,transform);
        NetworkServer.Spawn(obj);
    }
}
