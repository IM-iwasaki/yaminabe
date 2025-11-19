using UnityEngine;
using Mirror;

/// <summary>
/// ホコオブジェクト
/// プレイヤーが持っている間、チームのスコアを加算する
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class CaptureHoko : NetworkBehaviour {
    [Header("ホコ設定")]
    public float scorePerSecond = 1f;       // 1秒ごとのスコア
    public float holdHeight = 1.2f;         // プレイヤー上のホコの位置
    public float dropHeightOffset = 0.5f;   // 落下時の少し上に置くオフセット

    [SyncVar] private bool isHeld = false;
    [SyncVar] public NetworkIdentity holder;

    private Rigidbody rb;
    private Collider col;
    private float timer = 0f;
    private Transform originalParent; // 元々の親を保持

    private void Awake() {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        col.isTrigger = true;

        // 元々の親を保存
        originalParent = transform.parent;
    }

    /// <summary>
    /// ホコの追従とスコア加算
    /// </summary>
    [ServerCallback]
    private void Update() {
        if (isHeld && holder != null) {
            // プレイヤーの頭上に追従
            transform.position = holder.transform.position + Vector3.up * holdHeight;
            transform.rotation = holder.transform.rotation;

            // スコア加算
            timer += Time.deltaTime;
            if (timer >= 1f) {
                timer = 0f;
                AddScoreToHolderTeam();
            }
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
    /// <param name="player">ホコを持つプレイヤーのNetworkIdentity</param>
    [Server]
    public void TryPickup(NetworkIdentity player) {
        if (isHeld) return;

        isHeld = true;
        holder = player;
        rb.isKinematic = true;
        transform.SetParent(player.transform);
        transform.localPosition = Vector3.up * holdHeight;
    }

    /// <summary>
    /// ホコを落とす
    /// 元の親に戻し、ステージ上に自然に置く
    /// </summary>
    [Server]
    public void Drop() {
        if (!isHeld) return;

        isHeld = false;
        holder = null;
        rb.isKinematic = false;

        RpcDetachFromPlayer(); // 全クライアントで親子解除
    }

    [ClientRpc]
    private void RpcDetachFromPlayer() {
        transform.SetParent(null);
    }

    /// <summary>
    /// チームにスコアを加算する
    /// </summary>
    [Server]
    private void AddScoreToHolderTeam() {
        if (holder == null) return;

        var player = holder.GetComponent<CharacterBase>();
        if (player == null) return;

        int teamId = player.TeamID;
        RuleManager.Instance.OnCaptureProgress(teamId, scorePerSecond);
    }

    private void OnEnable() {
        GameManager.OnGameEnded += HandleGameEnd;
    }

    private void OnDisable() {
        GameManager.OnGameEnded -= HandleGameEnd;
    }

    /// <summary>
    /// ゲーム終了時にホコを落とす
    /// </summary>
    [Server]
    private void HandleGameEnd() {
        Drop();
    }
}