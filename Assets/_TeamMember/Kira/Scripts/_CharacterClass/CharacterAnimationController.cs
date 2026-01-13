using Mirror;
using UnityEngine;

public class CharacterAnimationController : NetworkBehaviour {
    public Animator anim = null;
    private string currentAnimation;
    #region アニメーション関連

    /// <summary>
    /// アニメーターのレイヤー切り替え
    /// </summary>
    [Server]
    public void ChangeLayerWeight(int _layerIndex) {
        //ベースのレイヤーを飛ばし、引数と一致したレイヤーを使うようにする
        for (int i = 1, max = anim.layerCount - 1; i < max; i++) {
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
    #endregion

}
