using UnityEngine;
using Mirror;

/// <summary>
/// 持ち運び型オブジェクト（保持中にスコア加算）
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class CaptureHoko : CaptureObjectBase {
    [Header("ホコ設定")]
    public float countSpeed = 1f;  // ホコ保持中のカウント進行速度

    [SyncVar] private bool isHeld = false;
    [SyncVar] private NetworkIdentity holder;
    private Rigidbody rb;

    protected override void Start() {
        base.Start();
        rb = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// プレイヤーがホコを拾う処理
    /// </summary>
    [Server]
    public void TryPickup(NetworkIdentity player) {
        if (isHeld) return;

        isHeld = true;
        holder = player;
        rb.isKinematic = true;
        transform.SetParent(player.transform);
        transform.localPosition = new Vector3(0, 1.2f, 0);

        ownerTeamId = player.GetComponent<PlayerTeamInfo>().teamId;
    }

    /// <summary>
    /// ホコを落とす処理
    /// </summary>
    [Server]
    public void Drop() {
        if (!isHeld) return;

        isHeld = false;
        holder = null;
        transform.SetParent(null);
        rb.isKinematic = false;
        ownerTeamId = -1;
    }

    /// <summary>
    /// ホコ保持中のカウント進行度計算
    /// </summary>
    protected override float CalculateProgress() {
        if (!isHeld || holder == null) return 0f;
        return countSpeed;
    }
}