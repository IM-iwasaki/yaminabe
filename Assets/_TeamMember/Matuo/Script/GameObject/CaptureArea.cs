using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// エリア制圧オブジェクト
/// 範囲内に同じチームが一定時間いると制圧
/// </summary>
public class CaptureArea : CaptureObjectBase {
    [Header("エリア設定")]
    public float captureTime = 3f;           // 制圧に必要な時間
    public Collider areaCollider;
    [SerializeField]
    private List<CharacterBase> playersInArea = new(); // エリア内プレイヤーリスト
    private float captureProgress = 0f;               // 現在の制圧進行時間

    /// <summary>
    /// プレイヤーがエリアに入った時にリストに追加
    /// </summary>
    /// <param name="other">衝突したコライダー</param>
    private void OnTriggerEnter(Collider other) {
        var player = other.GetComponent<CharacterBase>();
        if (player != null && !playersInArea.Contains(player))
            playersInArea.Add(player);
    }

    /// <summary>
    /// プレイヤーがエリアから出た時にリストから削除
    /// </summary>
    /// <param name="other">衝突したコライダー</param>
    private void OnTriggerExit(Collider other) {
        var player = other.GetComponent<CharacterBase>();
        if (player != null)
            playersInArea.Remove(player);
    }

    /// <summary>
    /// 制圧進行度を計算
    /// 同じチームのプレイヤーだけがいる場合に加算
    /// </summary>
    /// <returns>ObjectManager に通知する進行度</returns>
    protected override float CalculateProgress() {
        if (playersInArea.Count == 0) return 0f;

        // エリア内の全員が同じチームか確認
        int teamId = playersInArea[0].TeamID;
        bool sameTeam = playersInArea.TrueForAll(p => p.TeamID == teamId);
        if (!sameTeam) return 0f;

        // 制圧進行
        captureProgress += Time.deltaTime;
        if (captureProgress >= captureTime) {
            NotifyCaptured(teamId);
            captureProgress = 0f;
        }

        ownerTeamId = teamId;
        return Time.deltaTime;
    }
}