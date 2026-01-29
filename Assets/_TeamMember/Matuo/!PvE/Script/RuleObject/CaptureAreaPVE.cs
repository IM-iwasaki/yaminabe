using UnityEngine;
using Mirror;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class CaptureAreaPVE : NetworkBehaviour {

    [Header("スコアが増える間隔")]
    public float scorePerSecond = 1f;
    [Header("突破に必要なスコア")]
    public float targetScore = 10f;
    public Collider areaCollider;

    [Header("このエリア突破時に実行するイベント")]
    private List<PVEStageEvent> onClearedEvents = new();

    private float currentScore = 0f;
    private float timer = 0f;

    private HashSet<CharacterBase> playersInArea = new();
    private bool cleared = false;

    private void Awake() {
        if (areaCollider == null)
            areaCollider = GetComponent<Collider>();

        areaCollider.isTrigger = true;
    }

    [Server]
    public void Initialize(AreaSpawnPoint spawnPoint) {
        targetScore = spawnPoint.targetScore;
        onClearedEvents.Clear();

        foreach (var prefab in spawnPoint.eventPrefabs) {
            if (prefab == null) continue;

            var evt = Instantiate(prefab, transform);
            NetworkServer.Spawn(evt.gameObject);
            onClearedEvents.Add(evt);
        }
    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other) {
        var player = other.GetComponent<CharacterBase>();
        if (player != null)
            playersInArea.Add(player);
    }

    [ServerCallback]
    private void OnTriggerExit(Collider other) {
        var player = other.GetComponent<CharacterBase>();
        if (player != null)
            playersInArea.Remove(player);
    }

    [ServerCallback]
    private void Update() {
        if (cleared) return;
        if (!GameManager.Instance.IsGameRunning()) return;
        if (playersInArea.Count == 0) return;

        timer += Time.deltaTime;
        if (timer >= 1f) {
            timer = 0f;
            currentScore += scorePerSecond;

            if (currentScore >= targetScore) {
                CompleteArea();
            }
        }
    }

    [Server]
    private void CompleteArea() {
        if (cleared) return;
        cleared = true;

        foreach (var e in onClearedEvents) {
            if (e != null)
                e.Execute();
        }

        // 突破済み表現（どれか選ぶ）
        areaCollider.enabled = false;
        // gameObject.SetActive(false);
        // NetworkServer.Destroy(gameObject);
    }
}