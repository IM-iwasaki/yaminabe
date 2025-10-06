using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// エリア制圧型オブジェクト
/// 一定時間同じチームのみが範囲にいると制圧完了
/// </summary>
public class CaptureArea : CaptureObjectBase {
    [Header("エリア設定")]
    public float captureTime = 3f;        // 制圧に必要な時間(仮で今は3秒)
    public Collider areaCollider;         // 制圧判定用コライダー

    private List<PlayerTeamInfo> playersInArea = new List<PlayerTeamInfo>();
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
    /// エリア制圧進行度を計算
    /// </summary>
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
        return Time.deltaTime; // ObjectManagerに渡すカウント進行度
    }
}