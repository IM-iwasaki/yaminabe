using UnityEngine;
using System.Collections.Generic;
using Mirror;

/// <summary>
/// エリア制圧オブジェクト
/// 範囲内にいるプレイヤーのチームにスコアを加算
/// </summary>
[RequireComponent(typeof(Collider))]
public class CaptureArea : NetworkBehaviour {
    [Header("エリア設定")]
    public float scorePerSecond = 1f;        // 1秒ごとのスコア
    public Collider areaCollider;

    public HashSet<CharacterBase> playersInArea { get; private set; } = new ();// エリア内プレイヤー
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
        if (!GameManager.Instance.IsGameRunning()) return; // ← 追加
        if (playersInArea.Count == 0) return;

        // 全員同じチームかチェック
        int teamId = -1;
        foreach (var p in playersInArea) {
            teamId = p.parameter.TeamID;
            break; // 最初の要素のチームIDを取得したら抜ける
        }

        // タイマーでスコア加算
        timer += Time.deltaTime;
        if (timer >= 1f) {
            timer = 0f;
            RuleManager.Instance.OnCaptureProgress(teamId, scorePerSecond);
        }
    }
}