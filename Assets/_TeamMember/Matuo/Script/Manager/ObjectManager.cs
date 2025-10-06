using UnityEngine;
using Mirror;
using System.Collections.Generic;

/// <summary>
/// CaptureObjectを管理
/// エリア・ホコ問わず統一してカウント進行度をRuleManagerに通知
/// </summary>
public class ObjectManager : NetworkSystemObject<ObjectManager> {
    private readonly List<CaptureObjectBase> captureObjects = new();
    private RuleManager ruleManager;

    public override void Initialize() {
        base.Initialize();
        ruleManager = FindAnyObjectByType<RuleManager>();
    }

    /// <summary>
    /// CaptureObjectを登録
    /// </summary>
    [Server]
    public void RegisterObject(CaptureObjectBase obj) {
        if (!captureObjects.Contains(obj))
            captureObjects.Add(obj);
    }

    /// <summary>
    /// CaptureObject制圧完了時
    /// </summary>
    [Server]
    public void NotifyCaptured(CaptureObjectBase obj, int teamId) {
        ruleManager?.OnObjectCaptured(obj, teamId);
    }

    /// <summary>
    /// CaptureObject進行度更新時
    /// </summary>
    [Server]
    public void NotifyCaptureProgress(int teamId, float amount) {
        ruleManager?.OnCaptureProgress(teamId, amount);
    }
}