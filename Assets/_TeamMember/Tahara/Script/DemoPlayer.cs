using Mirror;
using System.Collections.Generic;
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
    private int HP;
    [SerializeField]
    private const int MaxHP = 100;
    [SerializeField]
    public PlayerUIManager playerUI = null;
    private void Awake() {
        var net = GetComponent<NetworkTransformHybrid>();
        net.syncDirection = SyncDirection.ServerToClient;
        HP = MaxHP;
        playerCamera.enabled = false;
    }
    //private void Start() {
    //    if (isLocalPlayer) {
    //        playerCamera.gameObject.SetActive(true);
    //    }
    //    else {
    //        playerCamera.gameObject.SetActive(false);
    //        uiManager.gameObject.SetActive(false);
    //    }
    //}

    public override void OnStartLocalPlayer() {
        //base.OnStartLocalPlayer();
        if(isLocalPlayer) {
            playerCamera.enabled = true;
            playerCamera.tag = "MainCamera";
            //UIを生成(実際には見えてない)
            GameObject canvas = GameObject.Find("GameUI");
            PlayerUIManager uiM = Instantiate(playerUI, canvas.transform);
            playerUI = uiM.GetComponent<PlayerUIManager>();
        }
    }

    public override void OnStopLocalPlayer() {

        playerUI.ResetTeammateUI();
    }

    private void Update() {
        if (!isLocalPlayer) return;
        if (Input.GetKeyDown(KeyCode.LeftShift))
            CmdTestHPRemove();
        if (Input.GetKeyDown(KeyCode.R))
            CmdTestHPReset();
        if (Input.GetKeyDown(KeyCode.Return))
            CmdRequestRandomTeam();

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
    private void ChangedHP(int _oldHP, int _newHP) {
        if (isLocalPlayer) {
            playerUI.ChangHPUI(MaxHP,_newHP);
        }

    }
    [Command]
    private void CmdTestHPRemove() {
        HP -= 20;
    }
    [Command]
    private void CmdTestHPReset() {
        HP = MaxHP;
    }

    [Command]
    private void CmdRequestRandomTeam() {
        ServerManager.instance.RandomTeamDecide();
    }


    [TargetRpc]
    public void TargetSendTeamList(NetworkConnection target, uint[] teammateIds) {
        foreach (uint id in teammateIds) {
            if (NetworkClient.spawned.TryGetValue(id, out var teammate)) {
                playerUI.ResetTeammateUI();
                playerUI.CreateTeammateUI(teammate);
            }
        }
    }


}
