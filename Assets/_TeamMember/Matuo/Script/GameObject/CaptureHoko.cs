using UnityEngine;
using Mirror;

[RequireComponent(typeof(Rigidbody))]
public class CaptureHoko : CaptureObjectBase {
    [Header("ホコ設定")]
    public float countSpeed = 1f;

    [SyncVar] private bool isHeld = false;
    [SyncVar] private NetworkIdentity holder;
    private Rigidbody rb;

    /// <summary>
    /// 初期化処理
    /// </summary>
    protected override void Start() {
        base.Start();
        rb = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// プレイヤーがホコを拾う
    /// </summary>
    /// <param name="player">ホコ餅プレイヤー</param>
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
    /// ホコを落とす
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
    /// カウント計算
    /// </summary>
    /// <returns>加算するカウント</returns>
    protected override float CalculateProgress() {
        if (!isHeld || holder == null) return 0f;
        return countSpeed;
    }
}