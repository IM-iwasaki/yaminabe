using UnityEngine;
using Mirror;
using System.Collections.Generic;

/// <summary>
/// PvE用エリア制圧オブジェクト
/// プレイヤーがいるとスコア加算、敵は無視
/// </summary>
[RequireComponent(typeof(Collider))]
public class CaptureAreaPVE : NetworkBehaviour {
    [Header("エリア設定")]
    public float scorePerSecond = 1f;
    public Collider areaCollider;

    private HashSet<CharacterBase> playersInArea = new();
    private float timer = 0f;

    private void Awake() {
        if (areaCollider == null)
            areaCollider = GetComponent<Collider>();

        areaCollider.isTrigger = true;
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
        if (!GameManager.Instance.IsGameRunning()) return;
        if (playersInArea.Count == 0) return;

        timer += Time.deltaTime;
        if (timer >= 1f) {
            timer = 0f;
            int teamId = 0; // プレイヤー固定チーム
            RuleManager.Instance.OnCaptureProgress(teamId, scorePerSecond);
        }
    }
}