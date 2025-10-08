using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 全部デバッグ用クラス
/// </summary>

public class DemoPlayer : NetworkBehaviour {
    [SerializeField]
    private GameObject bullet = null;
    [SerializeField]
    private Transform fire = null;
    [SerializeField]
    private Camera playerCamera = null;
    public int TeamID = -1;
    
    [SerializeField, SyncVar(hook = nameof(ChangedHP))]
    private int HP = 100;
    [SerializeField]
    TextMeshProUGUI text = null;
    [SerializeField]
    private Canvas playerUI = null;
    [SerializeField]
    Slider slider = null;
    private float timer = 0;
    private float interval = 1;

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
            playerUI.gameObject.SetActive(false);
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
    private void Update() {
        timer += Time.deltaTime;
        
        if(HP > 0 && timer > interval) {
            HP -= 1;
            timer = 0;
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
    private void ChangedHP(int _old,int _new) {
        if (isLocalPlayer) {
            text.text = _new.ToString();
            slider.value = HP;
        }
       
    }
}
