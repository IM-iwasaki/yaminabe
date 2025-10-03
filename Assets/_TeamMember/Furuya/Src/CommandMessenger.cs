using Mirror;
using UnityEngine;

public class CommandMessenger : NetworkBehaviour {
    private CharacterBase status;

    private void Awake() {
        status = GetComponent<CharacterBase>();
    }

    private void Update() {
        if (!isLocalPlayer) return;

        HandleInput();
    }

    private void HandleInput() {
        // 移動入力
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        if (h != 0 || v != 0) {
            CmdMove(new Vector3(h, 0, v));
        }

        // 攻撃入力
        if (Input.GetButtonDown("Fire1")) {
            CmdAttack();
        }
    }

    [Command]
    private void CmdMove(Vector3 direction) {
        // サーバーで物理処理
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.MovePosition(rb.position + direction * status.speed * Time.fixedDeltaTime);

        // 移動アニメーションなど
        RpcPlayMoveAnimation(direction);
    }

    [Command]
    private void CmdAttack() {
        // 攻撃処理（ダメージ判定など）
        status.TakeDamage(10);

        // 全クライアントに攻撃エフェクト通知
        RpcPlayAttackEffect();
    }

    [ClientRpc]
    private void RpcPlayMoveAnimation(Vector3 direction) {
        // クライアント側アニメーション再生
    }

    [ClientRpc]
    private void RpcPlayAttackEffect() {
        // 攻撃エフェクト再生
    }
}
