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
    public float pickupCooldown = 3.0f;     // Drop 後に再度拾えるまでの時間

    [SyncVar(hook = nameof(OnHolderChanged))]
    public NetworkIdentity holder;

    private Rigidbody rb;
    private Collider col;
    private float scoreTimer = 0f;
    private bool canBePickedUp = true;

    private void Awake() {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    public override void OnStartServer() {
        base.OnStartServer();
        GameManager.Instance.RegisterHoko(this);
    }

    /// <summary>
    /// SyncVar hook：holder の変化に応じて見た目を更新
    /// </summary>
    private void OnHolderChanged(NetworkIdentity oldHolder, NetworkIdentity newHolder) {
        if (newHolder != null) {
            rb.isKinematic = true;
        } else {
            rb.isKinematic = false;
        }
    }

    /// <summary>
    /// ホコの追従とスコア加算（サーバー側のみ）
    /// </summary>
    [Server]
    private void Update() {
        if (holder != null) {
            Vector3 targetPos = holder.transform.position + Vector3.up * holdHeight;
            transform.position = targetPos;
            transform.rotation = holder.transform.rotation;

            RpcUpdateHokoPosition(targetPos, holder.transform.rotation);

            scoreTimer += Time.deltaTime;
            if (scoreTimer >= 1f) {
                scoreTimer = 0f;
                AddScoreToHolderTeam();
            }
        }
    }

    [ClientRpc]
    private void RpcUpdateHokoPosition(Vector3 position, Quaternion rotation) {
        transform.position = position;
        transform.rotation = rotation;
    }

    /// <summary>
    /// 衝突判定：プレイヤーがホコに触れたら取得
    /// </summary>
    [Server]
    private void OnTriggerEnter(Collider other) {
        if (holder != null || !canBePickedUp) return;

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
        if (holder != null) return;

        holder = player;
        scoreTimer = 0f; // スコア加算タイマーリセット
    }

    /// <summary>
    /// ホコを落とす
    /// </summary>
    [Server]
    public void Drop() {
        if (holder == null) return;

        holder = null;
        canBePickedUp = false;
        Invoke(nameof(EnablePickup), pickupCooldown);
    }

    [Server]
    private void EnablePickup() {
        canBePickedUp = true;
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
    /// ゲーム終了時にホコ落とす
    /// </summary>
    [Server]
    public void HandleGameEnd() {
        Drop();
    }
}