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
        if (!GameManager.Instance.IsGameRunning()) return;
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
            timer = 0f;
            return;
        }

        // 単独チームのみ加算
        timer += Time.deltaTime;
        if (timer >= 1f) {
            timer = 0f;
            RuleManager.Instance.OnCaptureProgress(firstTeam.Value, scorePerSecond);
        }
    }
}