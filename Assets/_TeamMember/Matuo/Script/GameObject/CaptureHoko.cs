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

    private bool isActive = true;

    [Header("スコア距離設定")]
    public float spawnBlockDistance = 40f; // この距離以内ならカウント停止
    public float fastDistance = 30f;         // 敵陣に近いと高速
    public float fastMultiplier = 1.5f;

    [Header("UI")]
    public Canvas warningCanvas;
    public float blinkSpeed = 1f;

    private CanvasGroup warningCanvasGroup;
    private bool showNearSpawnWarning = false;

    [Header("リセット設定")]
    public float resetTime = 30f; // 放置されたら戻る時間
    private float dropTimer = 0f;
    private bool isDropped = false;

    private void Awake() {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        col.isTrigger = true;

        if (warningCanvas != null) {
            warningCanvasGroup = warningCanvas.GetComponent<CanvasGroup>();
        }
    }

    public override void OnStartServer() {
        base.OnStartServer();
        GameManager.Instance.RegisterHoko(this);
    }

    /// <summary>
    /// holderの変化に応じて見た目を更新
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
        if (!isActive) return;
        if (!GameManager.Instance.IsGameRunning()) return;

        if (holder != null) {
            Vector3 targetPos = holder.transform.position + Vector3.up * holdHeight;
            transform.position = targetPos;
            transform.rotation = holder.transform.rotation;

            RpcUpdateHokoPosition(targetPos, holder.transform.rotation);

            var player = holder.GetComponent<CharacterBase>();
            if (player == null) return;

            bool nearSpawn = IsNearOwnSpawn(player);

            var conn = player.connectionToClient;
            if (conn != null) {
                TargetSetNearSpawnWarning(conn, nearSpawn);
            }

            float multiplier = GetScoreMultiplier(player);

            scoreTimer += Time.deltaTime * multiplier;

            if (scoreTimer >= 1f) {
                scoreTimer = 0f;
                AddScoreToHolderTeam();
            }
        }

        // ホコが落ちてるときのリセット処理
        if (holder == null && isDropped) {
            dropTimer += Time.deltaTime;

            if (dropTimer >= resetTime) {
                ResetToCenter();
            }
        }
    }

    /// <summary>
    /// クライアント側UI処理
    /// </summary>
    private void LateUpdate() {
        UpdateWarningUI();
    }

    private void UpdateWarningUI() {
        if (warningCanvasGroup == null) return;

        if (!showNearSpawnWarning) {
            if (warningCanvas.gameObject.activeSelf)
                warningCanvas.gameObject.SetActive(false);
            return;
        }

        if (!warningCanvas.gameObject.activeSelf)
            warningCanvas.gameObject.SetActive(true);

        warningCanvasGroup.alpha = Mathf.PingPong(Time.time * blinkSpeed, 1f);
    }

    [ClientRpc]
    private void RpcUpdateHokoPosition(Vector3 position, Quaternion rotation) {
        transform.position = position;
        transform.rotation = rotation;
    }

    /// <summary>
    /// プレイヤーがホコに触れたら取得
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
        scoreTimer = 0f;

        isDropped = false;
        dropTimer = 0f;

        AudioManager.Instance.CmdPlayWorldSE("Hoko", transform.position);

        // 移動速度を下げる
        var param = player.GetComponent<CharacterParameter>();
        if (param != null) {
            param.speedMultiplier = 0.7f;
        }
    }

    /// <summary>
    /// ホコを落とす
    /// </summary>
    [Server]
    public void Drop() {
        if (holder == null) return;

        var player = holder.GetComponent<CharacterBase>();
        if (player != null && player.connectionToClient != null) {
            TargetSetNearSpawnWarning(player.connectionToClient, false);
        }

        var param = holder.GetComponent<CharacterParameter>();
        if (param != null) {
            param.speedMultiplier = 1f;
        }

        holder = null;
        isDropped = true;
        dropTimer = 0f;
        canBePickedUp = false;
        Invoke(nameof(EnablePickup), pickupCooldown);
        RuleManager.Instance.NotifyObjectStateChanged();
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

        if (IsNearOwnSpawn(player))
            return;

        int teamId = player.parameter.TeamID;
        RuleManager.Instance.OnCaptureProgress(teamId, scorePerSecond);
    }

    /// <summary>
    /// 現在ホコを持っているチームIDを返す
    /// 所持者がいなければ -1
    /// </summary>
    public int GetHolderTeam() {
        if (holder == null)
            return -1;

        var player = holder.GetComponent<CharacterBase>();
        if (player == null)
            return -1;

        return player.parameter.TeamID;
    }

    /// <summary>
    /// 敵陣との距離でスコア倍率を決める
    /// </summary>
    [Server]
    private float GetScoreMultiplier(CharacterBase player) {
        TeamData.TeamColor myTeam = player.parameter.TeamID == 0 ? TeamData.TeamColor.Red : TeamData.TeamColor.Blue;

        TeamData.TeamColor enemyTeam = myTeam == TeamData.TeamColor.Red ? TeamData.TeamColor.Blue : TeamData.TeamColor.Red;

        var enemySpawns = StageManager.Instance.GetTeamSpawnPoints(enemyTeam);

        float enemyDist = float.MaxValue;

        foreach (var sp in enemySpawns) {
            if (sp == null) continue;

            float d = Vector3.Distance(transform.position, sp.position);
            if (d < enemyDist) enemyDist = d;
        }

        // 敵陣に近いと倍率アップ
        if (enemyDist < fastDistance)
            return fastMultiplier;

        return 1f;
    }

    /// <summary>
    /// ホコを持っているプレイヤーが自陣のリスポーン地点に近いか
    /// </summary>
    [Server]
    private bool IsNearOwnSpawn(CharacterBase player) {

        var spawnPoints = StageManager.Instance.GetTeamSpawnPoints(
            player.parameter.TeamID == 0 ? TeamData.TeamColor.Red : TeamData.TeamColor.Blue
        );

        foreach (var sp in spawnPoints) {
            if (sp == null) continue;

            float dist = Vector3.Distance(transform.position, sp.position);

            if (dist < spawnBlockDistance) {
                return true;
            }
        }

        return false;
    }
    /// <summary>
    /// 警告UI用
    /// </summary>
    [TargetRpc]
    private void TargetSetNearSpawnWarning(NetworkConnection target, bool active) {
        if (holder == null || !holder.isLocalPlayer) {
            showNearSpawnWarning = false;
            return;
        }

        showNearSpawnWarning = active;
    }

    [Server]
    private void ResetToCenter() {
        isDropped = false;
        dropTimer = 0f;

        Vector3 centerPos = new Vector3(0f, 2f, 0f);

        transform.position = centerPos;
        transform.rotation = Quaternion.identity;

        RpcUpdateHokoPosition(centerPos, Quaternion.identity);

        canBePickedUp = true;

        RuleManager.Instance.NotifyObjectStateChanged();
    }
}