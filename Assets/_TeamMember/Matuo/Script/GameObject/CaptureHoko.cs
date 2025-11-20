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

    [SyncVar] private bool isHeld = false;
    [SyncVar] public NetworkIdentity holder;

    private Rigidbody rb;
    private Collider col;
    private float timer = 0f;

    private void Awake() {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnEnable() {
        GameManager.OnGameEnded += HandleGameEnd;
    }

    private void OnDisable() {
        GameManager.OnGameEnded -= HandleGameEnd;
    }

    /// <summary>
    /// ホコの追従とスコア加算（サーバー側のみ）
    /// </summary>
    [ServerCallback]
    private void Update() {
        if (isHeld && holder != null && holder != StageManager.Instance.netIdentity) {
            Vector3 targetPos = holder.transform.position + Vector3.up * holdHeight;
            transform.position = targetPos;
            transform.rotation = holder.transform.rotation;

            timer += Time.deltaTime;
            if (timer >= 1f) {
                timer = 0f;
                AddScoreToHolderTeam();
            }
        }
    }

    /// <summary>
    /// 衝突判定：プレイヤーがホコに触れたら取得
    /// </summary>
    [Server]
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
    [Server]
    public void TryPickup(NetworkIdentity player) {
        if (isHeld) return;

        isHeld = true;
        holder = player;
        rb.isKinematic = true;

        // 親子設定はサーバー側ではしないで位置追従のみ
        // クライアントに見えるように位置を同期
        RpcAttachToPlayer(player);
    }

    [ClientRpc]
    private void RpcAttachToPlayer(NetworkIdentity player) {
        holder = player;
    }

    /// <summary>
    /// ホコを落とす
    /// </summary>
    [Server]
    public void Drop() {
        if (!isHeld) return;

        holder = null;
        isHeld = false;
        rb.isKinematic = false;
        RpcDetachFromPlayer();
    }

    [ClientRpc]
    private void RpcDetachFromPlayer() {
        isHeld = false;                  // クライアント側でも追従停止
        holder = null;
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

    /// <summary>
    /// ゲーム終了時にホコを落とす
    /// </summary>
    [Server]
    private void HandleGameEnd() {
        Drop();
    }
}