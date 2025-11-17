using UnityEngine;
using Mirror;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))] // コライダーが必要
/// <summary>
/// ホコオブジェクト
/// プレイヤーがホコを持っている間スコア加算
/// </summary>
public class CaptureHoko : CaptureObjectBase {
    [Header("ホコ設定")]
    public float countSpeed = 1f;

    [SyncVar] private bool isHeld = false;             // プレイヤーが保持しているか
    [SyncVar] public NetworkIdentity holder;          // ホコを持つプレイヤー
    private Rigidbody rb;
    [SerializeField] private Collider hokoCollider;

    /// <summary>
    /// 初期化処理
    /// </summary>
    protected override void Start() {
        base.Start();
        rb = GetComponent<Rigidbody>();
        hokoCollider = GetComponent<Collider>();
        hokoCollider.isTrigger = true;
    }

    [ServerCallback]
    protected new void Update() {
        if (isHeld && holder != null) {
            transform.position = holder.transform.position + new Vector3(0, 1.2f, 0);
            transform.rotation = holder.transform.rotation;
        }
    }

    /// <summary>
    /// 衝突判定
    /// プレイヤーが当たったら拾う処理を呼び出す
    /// </summary>
    /// <param name="other">衝突したコライダー</param>
    [ServerCallback]
    private void OnTriggerEnter(Collider other) {
        if (isHeld) return;
        var player = other.GetComponent<CharacterBase>();
        if (player != null && player.netIdentity != null) {
            TryPickup(player.netIdentity);
        }
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
    /// <returns>ObjectManager に通知するカウント</returns>
    protected override float CalculateProgress() {
        if (!isHeld || holder == null) return 0f;
        return countSpeed;
    }
}