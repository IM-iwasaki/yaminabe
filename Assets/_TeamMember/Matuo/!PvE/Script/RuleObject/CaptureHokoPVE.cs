using Mirror;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// PVE用ホコ
/// ・プレイヤーは同時に1つしか持てない
/// ・CollectPointに入ったら回収済みになり拾えない
/// </summary>
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class CaptureHokoPVE : NetworkBehaviour {

    [Header("ホコ設定")]
    public float holdHeight = 1.2f;

    // 今誰が持っているか
    [SyncVar]
    private NetworkIdentity holder;

    // CollectPointに納品されたか
    [SyncVar]
    private bool isCollected = false;

    private Collider col;
    private Rigidbody rb;

    /// <summary>
    /// サーバー上で誰がホコを持っているかを一元管理
    /// </summary>
    private static Dictionary<NetworkIdentity, CaptureHokoPVE> holderMap = new();

    private void Awake() {
        col = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();
        col.isTrigger = true;
    }

    public override void OnStopServer() {
        base.OnStopServer();
        if (holder != null && holderMap.ContainsKey(holder))
            holderMap.Remove(holder);
    }

    private void Update() {
        if (!isServer) return;

        if (holder != null) {
            Vector3 pos = holder.transform.position + Vector3.up * holdHeight;
            transform.position = pos;
            transform.rotation = holder.transform.rotation;
        }
    }

    /// <summary>
    /// 今拾える状態か？
    /// </summary>
    [Server]
    public bool CanBePickedUp() {
        return !isCollected && holder == null;
    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other) {
        if (!CanBePickedUp()) return;

        var player = other.GetComponent<CharacterBase>();
        if (player == null || player.netIdentity == null) return;

        // すでに別のホコを持っているなら拾えない
        if (holderMap.ContainsKey(player.netIdentity))
            return;

        Pickup(player.netIdentity);
    }

    /// <summary>
    /// ホコを拾う
    /// </summary>
    [Server]
    private void Pickup(NetworkIdentity player) {
        holder = player;
        holderMap[player] = this;
        rb.isKinematic = true;
    }

    /// <summary>
    /// ホコを落とす（CollectPoint納品時にも使用）
    /// </summary>
    [Server]
    private void Drop() {
        if (holder == null) return;

        holderMap.Remove(holder);
        holder = null;
        rb.isKinematic = false;
    }

    /// <summary>
    /// CollectPointに納品された
    /// </summary>
    [Server]
    public void MarkCollected() {
        if (isCollected) return;

        isCollected = true;

        // 持っていたら解除
        Drop();

        // 完全に拾えなくする
        col.enabled = false;
        rb.isKinematic = true;
    }
}