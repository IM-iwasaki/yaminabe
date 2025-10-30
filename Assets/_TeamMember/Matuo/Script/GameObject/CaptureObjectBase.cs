using UnityEngine;
using Mirror;

/// <summary>
/// CaptureObject系共通基底
/// エリア・ホコのカウント計算を共通化
/// </summary>
public abstract class CaptureObjectBase : NetworkBehaviour {
    [SyncVar] protected int ownerTeamId = -1;   // 制圧したチーム
    protected ObjectManager objectManager;

    protected virtual void Start() {
        objectManager = FindAnyObjectByType<ObjectManager>();
        if (isServer)
            objectManager?.RegisterObject(this);
    }

    /// <summary>
    /// カウント計算（派生クラスで実装）
    /// </summary>
    protected abstract float CalculateProgress();

    /// <summary>
    /// 毎フレーム進行度通知
    /// </summary>
    protected virtual void Update() {
        if (!isServer) return;

        float progress = CalculateProgress();
        if (progress > 0f && ownerTeamId >= 0) {
            objectManager?.NotifyCaptureProgress(ownerTeamId, progress);
        }
    }

    /// <summary>
    /// 制圧完了通知
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