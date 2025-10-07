using Mirror;
using UnityEngine;

public class DemoPlayer : NetworkBehaviour {
    [SerializeField]
    private GameObject bullet = null;
    [SerializeField]
    private Transform fire = null;
    [SerializeField]
    private Camera playerCamera = null;
    public int TeamID;
    private void Awake() {
        var net = GetComponent<NetworkTransformHybrid>();
        net.syncDirection = SyncDirection.ServerToClient;
       
    }
    private void Start() {
        if (isLocalPlayer) {
            playerCamera.gameObject.SetActive(true);
        }
        else {
            playerCamera.gameObject.SetActive(false);
        }
    }

    public override void OnStartLocalPlayer() {
        //base.OnStartLocalPlayer();
        if (isLocalPlayer) {
            playerCamera.gameObject.SetActive(true);
        }
        else {
            playerCamera.gameObject.SetActive(false);
        }
    }

    private void FixedUpdate() {
        if (isLocalPlayer) {
            if (Input.GetKeyDown(KeyCode.Space)) {
                CmdShootBullet();
            }

            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");

            CmdPlayerMove(x, z);
        }
        

    }
    [Command]
    void CmdPlayerMove(float _x, float _z) {
        Vector3 v = new Vector3(_x, 0, _z) * 5;
        GetComponent<Rigidbody>().AddForce(v);
    }

    [Command]
    void CmdShootBullet() {
        GameObject obj = Instantiate(bullet, fire.position, fire.rotation);
        NetworkServer.Spawn(obj, connectionToClient);
    }
}
