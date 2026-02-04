using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PveBossMoveController : NetworkBehaviour
{

    [Header("回転速度")]
    [SerializeField] private float rotateSpeed = 10.0f;

    //  移動可能かどうか
    private bool canMove = false;

    /// <summary>
    /// ターゲットに向かって移動する
    /// </summary>
    /// <param name="target"></param>
    /// <param name="moveSpeed"></param>
    [Server]
    public void MoveToTarget(Transform target, float moveSpeed) {
        //  移動不可、またはターゲットが無ければスキップ
        if (!canMove || target == null) return;
        //  ターゲットの座標取得、y座標固定
        Vector3 targetPos = target.position;
        targetPos.y = transform.position.y;

        //  移動処理
        BossMove(targetPos, moveSpeed);

        //  回転処理
        BossRotate(targetPos, rotateSpeed);
    }

    /// <summary>
    /// 移動処理
    /// </summary>
    /// <param name="targetPos"></param>
    /// <param name="moveSpeed"></param>
    private void BossMove(Vector3 targetPos, float moveSpeed) {
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPos,
            moveSpeed * Time.deltaTime);
    }
    
    /// <summary>
    /// 回転処理
    /// </summary>
    /// <param name="targetPos"></param>
    /// <param name="rotateSpeed"></param>
    private void BossRotate(Vector3 targetPos, float rotateSpeed) {
        Vector3 dir = targetPos - transform.position;
        dir.y = 0f;
        if(dir.sqrMagnitude > 0.001f) {
            Quaternion rot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                rot,
                rotateSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// 移動を停止させる
    /// </summary>
    [Server]
    public void Stop() {
        canMove = false;
    }
    /// <summary>
    /// 移動を再開させる
    /// </summary>
    [Server]
    public void Resume() {
        canMove = true;
    }

}
