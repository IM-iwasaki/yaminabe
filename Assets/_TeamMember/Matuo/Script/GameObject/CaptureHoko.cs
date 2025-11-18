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
    [SyncVar] public NetworkIdentity holder;  // 現在ホコを持っているプレイヤー

    private Rigidbody rb;
    private Collider col;
    private float timer = 0f;

    private void Awake() {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        col.isTrigger = true;
    }

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
    /// <param name="player">ホコ餅プレイヤー</param>
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
    /// </summary>
    [Server]
    public void Drop() {
        if (!isHeld) return;

        isHeld = false;
        holder = null;
        transform.SetParent(null);
        rb.isKinematic = false;
    }

    /// <summary>
    /// ゲーム終了時にホコを落とす用
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

    [Server]
    private void HandleGameEnd() {
        Drop();
    }
}