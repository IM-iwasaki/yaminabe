using UnityEngine;
using Mirror;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class CaptureAreaPVE : NetworkBehaviour {

    [Header("スコアが増える間隔")]
    public float scorePerSecond = 1f;
    [Header("突破に必要なスコア")]
    public float targetScore = 8f;
    public Collider areaCollider;

    [Header("このエリア突破時に実行するイベント")]
    private List<PVEStageEvent> onClearedEvents = new();

    [SyncVar] private float currentScore = 0f;
    [SyncVar] private int currentPlayerCount = 0;
    [SyncVar] private int maxPlayerCount = 0;

    private float timer = 0f;

    private HashSet<CharacterBase> playersInArea = new();
    private bool cleared = false;

    private AreaClearCondition clearCondition;

    public float CurrentScore => currentScore;
    public float TargetScore => targetScore;
    public int CurrentPlayerCount => currentPlayerCount;
    public int MaxPlayerCount => maxPlayerCount;
    public AreaClearCondition ClearCondition => clearCondition;


    private void Awake() {
        if (areaCollider == null)
            areaCollider = GetComponent<Collider>();

        areaCollider.isTrigger = true;
    }

    [Server]
    public void Initialize(AreaSpawnPoint spawnPoint) {
        targetScore = spawnPoint.targetScore;
        clearCondition = spawnPoint.clearCondition;

        maxPlayerCount = ServerManager.instance.connectPlayer.Count;

        onClearedEvents.Clear();
        foreach (var evt in spawnPoint.events) {
            if (evt == null) continue;
            onClearedEvents.Add(evt);
        }
    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other) {
        var player = other.GetComponent<CharacterBase>();
        if (player != null) {
            playersInArea.Add(player);
            currentPlayerCount = playersInArea.Count;
        }
    }

    [ServerCallback]
    private void OnTriggerExit(Collider other) {
        var player = other.GetComponent<CharacterBase>();
        if (player != null) {
            playersInArea.Remove(player);
            currentPlayerCount = playersInArea.Count;
        }
    }

    [ServerCallback]
    private void Update() {
        if (cleared) return;
        if (!GameManager.Instance.IsGameRunning()) return;

        if (!CanIncreaseScore()) return;

        timer += Time.deltaTime;
        if (timer >= 1f) {
            timer = 0f;
            currentScore += scorePerSecond;

            if (currentScore >= targetScore) {
                CompleteArea();
            }
        }
    }

    /// <summary>
    /// 人数判定用
    /// </summary>
    [Server]
    private bool CanIncreaseScore() {
        if (playersInArea.Count == 0)
            return false;

        switch (clearCondition) {
            case AreaClearCondition.AnyPlayer:
                return true;

            case AreaClearCondition.AllPlayers:
                return playersInArea.Count >= ServerManager.instance.connectPlayer.Count;

            default:
                return false;
        }
    }

    [Server]
    private void CompleteArea() {
        if (cleared) return;
        cleared = true;

        RpcExecuteEvents();

        // 突破
        areaCollider.enabled = false;
    }

    [ClientRpc]
    private void RpcExecuteEvents() {
        var events = GetComponentsInChildren<PVEStageEvent>(true);

        foreach (var evt in events) {
            if (evt != null)
                evt.Execute();
        }
    }
}