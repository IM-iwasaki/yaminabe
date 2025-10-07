using UnityEngine;
using System.Collections.Generic;
using Mirror.Examples.MultipleMatch;

/// <summary>
/// エリア制圧オブジェクト
/// 範囲内に同じチームが一定時間いると制圧
/// </summary>
public class CaptureArea : CaptureObjectBase {
    [Header("エリア設定")]
    public float captureTime = 3f;
    public Collider areaCollider;

    private List<PlayerTeamInfo> playersInArea = new();
    private float captureProgress = 0f;

    private void OnTriggerEnter(Collider other) {
        var player = other.GetComponent<PlayerTeamInfo>();
        if (player != null && !playersInArea.Contains(player))
            playersInArea.Add(player);
    }

    private void OnTriggerExit(Collider other) {
        var player = other.GetComponent<PlayerTeamInfo>();
        if (player != null)
            playersInArea.Remove(player);
    }

    /// <summary>
    /// カウント計算
    /// </summary>
    /// <returns>加算するカウント</returns>
    protected override float CalculateProgress() {
        if (playersInArea.Count == 0) return 0f;

        int teamId = playersInArea[0].teamId;
        bool sameTeam = playersInArea.TrueForAll(p => p.teamId == teamId);
        if (!sameTeam) return 0f;

        captureProgress += Time.deltaTime;
        if (captureProgress >= captureTime) {
            NotifyCaptured(teamId);
            captureProgress = 0f;
        }

        ownerTeamId = teamId;
        return Time.deltaTime;
    }
}