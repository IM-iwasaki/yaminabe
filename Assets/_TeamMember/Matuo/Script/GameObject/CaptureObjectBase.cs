using UnityEngine;
using Mirror;

/// <summary>
/// CaptureObject系の共通基底クラス
/// 保持中/制圧中の進行度を計算し、ObjectManagerへ通知
/// </summary>
public abstract class CaptureObjectBase : NetworkBehaviour {
    [SyncVar] protected int ownerTeamId = -1; // 現在制圧しているチーム
    protected ObjectManager objectManager;

    protected virtual void Start() {
        objectManager = FindAnyObjectByType<ObjectManager>();
        if (isServer) {
            objectManager?.RegisterObject(this);
        }
    }

    /// <summary>
    /// 派生クラスごとに進行度を計算する
    /// </summary>
    protected abstract float CalculateProgress();

    /// <summary>
    /// 毎フレーム進行度をObjectManagerに通知
    /// </summary>
    protected virtual void Update() {
        if (!isServer) return;

        float progress = CalculateProgress();
        if (progress > 0f && ownerTeamId >= 0) {
            objectManager?.NotifyCaptureProgress(ownerTeamId, progress * Time.deltaTime);
        }
    }

    /// <summary>
    /// オブジェクト制圧完了時に呼ばれる
    /// </summary>
    [Server]
    protected void NotifyCaptured(int teamId) {
        ownerTeamId = teamId;
        objectManager?.NotifyCaptured(this, teamId);
    }

    /// <summary>
    /// 現在制圧しているチーム
    /// </summary>
    public int GetOwnerTeam() => ownerTeamId;
}