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

    public HashSet<CharacterBase> playersInArea { get; private set; } = new();// エリア内プレイヤー
    private float scoreTimer = 0f;

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
        RuleManager.Instance.NotifyObjectStateChanged();
    }

    [Server]
    private void RemoveDeadPlayers() {
        playersInArea.RemoveWhere(p => p == null || p.parameter == null || p.parameter.isDead);
    }

    [ServerCallback]
    private void Update() {
        if (!GameManager.Instance.IsGameRunning()) return;

        RemoveDeadPlayers();

        if (playersInArea.Count == 0) return;

        int? firstTeam = null;
        bool multipleTeams = false;

        foreach (var p in playersInArea) {
            int team = p.parameter.TeamID;

            if (firstTeam == null)
                firstTeam = team;
            else if (firstTeam != team) {
                multipleTeams = true;
                break;
            }
        }

        // 両チームいるなら止める
        if (multipleTeams || firstTeam == null) {
            scoreTimer = 0f;
            return;
        }

        // 同じチーム人数
        int count = playersInArea.Count;

        // 倍率
        float multiplier = Mathf.Pow(1.5f, count - 1);

        scoreTimer += Time.deltaTime * multiplier;

        while (scoreTimer >= 1f) {
            scoreTimer -= 1f;
            RuleManager.Instance.OnCaptureProgress(firstTeam.Value, scorePerSecond);
        }
    }

    /// <summary>
    /// 現在このエリアを制圧しているチームを返す
    /// 誰もいない場合 -1
    /// </summary>
    public int GetControllingTeam() {
        int? firstTeam = null;

        foreach (var p in playersInArea) {
            int team = p.parameter.TeamID;

            if (firstTeam == null)
                firstTeam = team;
            else if (firstTeam != team)
                return -1; // 両チームいる
        }

        return firstTeam ?? -1;
    }
}