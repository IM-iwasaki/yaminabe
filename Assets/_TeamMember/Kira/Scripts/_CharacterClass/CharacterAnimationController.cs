using Mirror;
using System.Collections;
using UnityEngine;

/// <summary>
/// プレイヤーのアニメーション管理クラス
/// </summary>
public class CharacterAnimationController : NetworkBehaviour {
    //扱うアニメーター
    public Animator anim = null;
    //ベースアニメーションレイヤーの数
    [SerializeField]
    private int BaseAnimLayerCount = 2;
    //現在のアニメーションの文字列
    private string currentAnimation;

    [Server]
    public void ChangeBaseAnimationLayerWeight(int _layerIndex) {
        if (anim == null) return;
        if (anim.Equals(null)) return;

        for (int i = 0; i < BaseAnimLayerCount; i++) {
            anim.SetLayerWeight(i, i == _layerIndex ? 1.0f : 0.0f);
        }
    }

    /// <summary>
    /// アニメーターのレイヤー切り替え
    /// </summary>
    [Server]
    public void ChangeWeaponAnimationLayerWeight(int _layerIndex) {
        if (anim == null) return;
        if (anim.Equals(null)) return;
        //ベースのレイヤーを飛ばし、引数と一致したレイヤーを使うようにする
        for (int i = 2, max = anim.layerCount; i < max; i++) {
            anim.SetLayerWeight(i, i == _layerIndex ? 1.0f : 0.0f);
        }
    }

    /// <summary>
    /// 移動アニメーションの管理
    /// </summary>
    /// <param name="_x"></param>
    /// <param name="_z"></param>
    [Command]
    public void ControllMoveAnimation(float _x, float _z) {
        ResetRunAnimation();
        //斜め入力の場合
        if (_x != 0 && _z != 0) {
            anim.SetBool("RunL", false);
            anim.SetBool("RunR", false);
            if (_z > 0) {
                currentAnimation = "RunF";
            }
            if (_z < 0) {
                currentAnimation = "RunB";
            }
            anim.SetBool(currentAnimation, true);
            return;

        }

        if (_x > 0 && _z == 0) {
            currentAnimation = "RunR";
        }
        if (_x < 0 && _z == 0) {
            currentAnimation = "RunL";
        }
        if (_x == 0 && _z > 0) {
            currentAnimation = "RunF";
        }
        if (_x == 0 && _z < 0) {
            currentAnimation = "RunB";
        }
        anim.SetBool(currentAnimation, true);
    }

    /// <summary>
    /// 移動アニメーションのリセット
    /// </summary>
    private void ResetRunAnimation() {
        anim.SetBool("RunF", false);
        anim.SetBool("RunR", false);
        anim.SetBool("RunL", false);
        anim.SetBool("RunB", false);

        currentAnimation = null;
    }

    [Command]
    public void CmdResetAnimation() {
        ResetRunAnimation();
    }

    /// <summary>
    /// 追加:タハラ　入力がなくなったらショットアニメーション終了
    /// </summary>
    [Command]
    public void StopShootAnim() {
        //アニメーション終了
        anim.SetBool("Shoot", false);
    }

    /// <summary>
    /// NetworkAnimatorを使用した結果
    /// ローカルでの変更によってアニメーション変更がかかるため制作
    /// </summary>
    [ClientRpc]
    public void RpcDeadAnimation() {
        anim.SetTrigger("Dead");
    }

    public IEnumerator AddNetworkAnimator(GameObject _skin) {
        if (!isLocalPlayer) yield break;
#if false
        var animator = _skin.GetComponent<Animator>();
        if (animator == null) yield break;
        anim = animator;
        yield return null;
        if (_skin == null) yield break;
        var networkAnim = _skin.AddComponent<NetworkAnimator>();
        yield return null;

        networkAnim.syncDirection = SyncDirection.ClientToServer;


#else
        // ① Animator を取得
        var animator = _skin.GetComponent<Animator>();
        if (animator == null) yield break;

        // ② NetworkIdentity の子階層に入ったことを保証するため 1 フレーム待つ
        yield return null;

        // ③ Animator の初期化待ち
        yield return null;

        // ④ NetworkAnimator を追加（この時点で NetworkIdentity が見つかる）
        var networkAnim = _skin.AddComponent<NetworkAnimator>();

        // ⑤ Awake → Initialize 完了待ち
        yield return null;

        // ⑥ animator をセット
        networkAnim.animator = animator;
        networkAnim.syncDirection = SyncDirection.ClientToServer;

        anim = animator;
#endif
    }
}
