using UnityEngine;
using Mirror;

[RequireComponent(typeof(Rigidbody))]
/// <summary>
/// ホコオブジェクト
/// プレイヤーがホコを持っている間スコア加算
/// </summary>
public class CaptureHoko : CaptureObjectBase {
    [Header("ホコ設定")]
    public float countSpeed = 1f;

    [SyncVar] private bool isHeld = false;             // プレイヤーが保持しているか
    [SyncVar] private NetworkIdentity holder;          // ホコを持つプレイヤー
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

        // チームIDを CharacterBase から取得
        ownerTeamId = player.GetComponent<CharacterBase>().TeamID;
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
    /// ホコを持っている場合のみ加算
    /// </summary>
    /// <returns>ObjectManager に通知する進行度</returns>
    protected override float CalculateProgress() {
        if (!isHeld || holder == null) return 0f;
        return countSpeed;
    }
}