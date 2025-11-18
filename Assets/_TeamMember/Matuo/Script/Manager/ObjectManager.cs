using UnityEngine;
using Mirror;
using System.Collections.Generic;

/// <summary>
/// オブジェクト管理
/// CaptureObjectBaseの登録・進行度・キル通知をルールマネージャへ中継
/// </summary>
public class ObjectManager : NetworkSystemObject<ObjectManager> {
    private readonly List<CaptureObjectBase> captureObjects = new();
    private RuleManager ruleManager;

    public override void Initialize() {
        base.Initialize();
        ruleManager = RuleManager.Instance;
    }

    /// <summary>
    /// オブジェクトを登録
    /// </summary>
    [Server]
    public void RegisterObject(CaptureObjectBase obj) {
        if (!captureObjects.Contains(obj))
            captureObjects.Add(obj);
    }

    /// <summary>
    /// 制圧完了通知
    /// </summary>
    [Server]
    public void NotifyCaptured(CaptureObjectBase obj, int teamId) {
        ruleManager?.OnObjectCaptured(obj, teamId);
    }

    /// <summary>
    /// カウント通知
    /// </summary>
    [Server]
    public void NotifyCaptureProgress(int teamId, float amount) {
        ruleManager?.OnCaptureProgress(teamId, amount);
    }

    /// <summary>
    /// キル通知 (デスマッチ用)
    /// </summary>
    //[Server]
    //public void NotifyKill(int teamId, int kills) {
    //    ruleManager?.OnTeamKill(teamId, kills);
    //}
}