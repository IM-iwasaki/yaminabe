using UnityEngine;
using Mirror;

[RequireComponent(typeof(Collider))]
public class CaptureHokoPVE : NetworkBehaviour {
    [Header("追従速度")]
    public float followSpeed = 5f;

    [SyncVar(hook = nameof(OnHolderChanged))]
    public NetworkIdentity holder;

    private Rigidbody rb;
    private Collider col;

    private void Awake() {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnHolderChanged(NetworkIdentity oldHolder, NetworkIdentity newHolder) {
        rb.isKinematic = newHolder != null;
    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other) {
        if (holder != null) return;

        var player = other.GetComponent<CharacterBase>();
        //if (player != null && !player.HasHoko()) { // プレイヤーは一度に1個のみ
        //    TryPickup(player.netIdentity);
        //}
    }

    [Server]
    public void TryPickup(NetworkIdentity player) {
        if (holder != null) return;
        holder = player;

        var p = player.GetComponent<CharacterBase>();
        //p.SetHoldingHoko(true);
    }

    [ServerCallback]
    private void Update() {
        if (holder == null || !GameManager.Instance.IsGameRunning()) return;

        var player = holder.GetComponent<CharacterBase>();
        transform.position = Vector3.Lerp(transform.position, player.transform.position + Vector3.up * 1.2f, followSpeed * Time.deltaTime);
        transform.rotation = player.transform.rotation;
    }

    [Server]
    public void Drop() {
        if (holder == null) return;

        var player = holder.GetComponent<CharacterBase>();
        //player.SetHoldingHoko(false);

        holder = null;
    }
}