using Mirror;
using System.Collections;
using UnityEngine;

/// <summary>
/// プレイヤーのアニメーション管理クラス
/// NetworkAnimator を使わず、状態同期で制御する
/// </summary>
public class CharacterAnimationController : NetworkBehaviour {

    // 現在使用中の Animator（Skin差し替えで更新される）
    public Animator anim = null;

    // ベースアニメーションレイヤー数
    [SerializeField]
    private int BaseAnimLayerCount = 2;

    // 移動アニメーション状態
    public enum MoveAnimState {
        Idle,
        RunF,
        RunB,
        RunL,
        RunR
    }

    // サーバーで管理し、全クライアントに同期される移動状態
    [SyncVar(hook = nameof(OnMoveAnimChanged))]
    private MoveAnimState moveState = MoveAnimState.Idle;

    /// <summary>
    /// ベースアニメーションレイヤーの重みを変更する（サーバー専用）
    /// </summary>
    [Server]
    public void ChangeBaseAnimationLayerWeight(int _layerIndex) {
        if (anim == null) return;

        for (int i = 0; i < BaseAnimLayerCount; i++) {
            anim.SetLayerWeight(i, i == _layerIndex ? 1.0f : 0.0f);
        }
    }

    /// <summary>
    /// 武器アニメーションレイヤーの重みを変更する（サーバー専用）
    /// </summary>
    [Server]
    public void ChangeWeaponAnimationLayerWeight(int _layerIndex) {
        if (anim == null) return;

        for (int i = 2, max = anim.layerCount; i < max; i++) {
            anim.SetLayerWeight(i, i == _layerIndex ? 1.0f : 0.0f);
        }
    }

    /// <summary>
    /// 入力値から移動アニメーションを制御する（クライアント→サーバー）
    /// </summary>
    [Command]
    public void ControllMoveAnimation(float _x, float _z) {
        moveState = CalcMoveState(_x, _z);
    }

    /// <summary>
    /// SyncVar の変更を全クライアントで受け取り、Animator に反映
    /// </summary>
    void OnMoveAnimChanged(MoveAnimState oldState, MoveAnimState newState) {
        ApplyMoveAnimation(newState);
    }

    /// <summary>
    /// 入力値から移動状態を算出する
    /// </summary>
    private MoveAnimState CalcMoveState(float x, float z) {
        if (x == 0 && z == 0) return MoveAnimState.Idle;
        if (z > 0) return MoveAnimState.RunF;
        if (z < 0) return MoveAnimState.RunB;
        if (x > 0) return MoveAnimState.RunR;
        return MoveAnimState.RunL;
    }

    /// <summary>
    /// Animator に移動アニメーションを反映する
    /// </summary>
    private void ApplyMoveAnimation(MoveAnimState state) {
        if (anim == null) return;

        anim.SetBool("RunF", state == MoveAnimState.RunF);
        anim.SetBool("RunB", state == MoveAnimState.RunB);
        anim.SetBool("RunL", state == MoveAnimState.RunL);
        anim.SetBool("RunR", state == MoveAnimState.RunR);
    }

    /// <summary>
    /// 移動アニメーションをリセットする（サーバー）
    /// </summary>
    [Command]
    public void CmdResetAnimation() {
        moveState = MoveAnimState.Idle;
    }

    /// <summary>
    /// 追加:タハラ　入力がなくなったらショットアニメーション終了
    /// </summary>
    [Command]
    public void StopShootAnim() {
        RpcStopShootAnim();
    }

    /// <summary>
    /// ショットアニメーション停止を全クライアントに反映
    /// </summary>
    [ClientRpc]
    private void RpcStopShootAnim() {
        if (anim == null) return;
        anim.SetBool("Shoot", false);
    }

    /// <summary>
    /// 死亡アニメーションを全クライアントで再生
    /// </summary>
    [ClientRpc]
    public void RpcDeadAnimation() {
        if (anim == null) return;
        anim.SetTrigger("Dead");
    }

    /// <summary>
    /// Skin変更時に Animator を差し替える
    /// NetworkAnimator は使わない
    /// </summary>
    public IEnumerator AddNetworkAnimator(GameObject _skin) {
        if (_skin == null) yield break;

        var animator = _skin.GetComponent<Animator>();
        if (animator == null) yield break;

        // Animator を即時差し替え
        anim = animator;

        yield break;
    }
}