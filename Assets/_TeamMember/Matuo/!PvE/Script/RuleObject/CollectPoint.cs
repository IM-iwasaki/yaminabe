using Mirror;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ホコ集約ポイント（PVE用）
/// </summary>
[RequireComponent(typeof(Collider))]
public class CollectPoint : NetworkBehaviour {
    // このポイントで必要なホコの数
    [Header("必要ホコ数")]
    public int requiredCount = 3;

    // 規定数のホコが集まったときに実行するイベント
    [Header("クリア時イベント")]
    [SerializeField]
    private List<PVEStageEvent> onCompletedEvents = new();

    // 現在このポイントに納品されたホコの数
    private int collectedCount = 0;

    // すでにクリア済みかどうか
    private bool completed = false;

    // 集約ポイントの当たり判定
    private Collider col;

    private void Awake() {
        col = GetComponent<Collider>();

        // プレイヤーやホコが出入りできるようにトリガーにする
        col.isTrigger = true;
    }

    /// <summary>
    /// ホコが集約ポイントに入ったときの処理
    /// </summary>
    [ServerCallback]
    private void OnTriggerEnter(Collider other) {
        // すでにクリア済みなら何もしない
        if (completed) return;

        // PVE用ホコかどうかを判定
        var hoko = other.GetComponent<CaptureHokoPVE>();
        if (hoko == null) return;

        // すでに回収済み、または拾えない状態のホコは無視
        if (!hoko.CanBePickedUp()) return;

        CollectHoko(hoko);
    }

    /// <summary>
    /// ホコを1つ回収する
    /// ・ホコは回収済み状態になり再取得不可
    /// </summary>
    [Server]
    private void CollectHoko(CaptureHokoPVE hoko) {
        // ホコを回収済みとしてマーク
        hoko.MarkCollected();

        collectedCount++;

        // 規定数に達したらクリア処理
        if (collectedCount >= requiredCount) {
            Complete();
        }
    }

    /// <summary>
    /// ホコが集まったときの処理
    /// </summary>
    [Server]
    private void Complete() {
        if (completed) return;
        completed = true;

        foreach (var evt in onCompletedEvents) {
            if (evt != null)
                evt.Execute();
        }
    }
}