using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterInput : NetworkBehaviour {
    private CharacterBase core;

    public Vector2 MoveInput { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool AttackPressed { get; private set; }
    public bool AttackReleased { get; private set; }
    public bool AttackTriggered { get; private set; }

    public bool SkillTriggered;

    public bool InteractTriggered;

    public void Initialize(CharacterBase core) {
        this.core = core;
    }

    private void LateUpdate() {
        AttackReleased = false;
        AttackTriggered = false;
        SkillTriggered = false;
        InteractTriggered = false;
        JumpPressed = false;
    }

    /// <summary>
    /// 移動
    /// </summary>
    public void OnMove(InputAction.CallbackContext ctx) {
        MoveInput = ctx.ReadValue<Vector2>();
        float moveX = MoveInput.x;
        float moveZ = MoveInput.y;
        //アニメーション管理
        //core.ControllMoveAnimation(moveX, moveZ);
    }

    /// <summary>
    /// ジャンプ
    /// </summary>
    public void OnJump(InputAction.CallbackContext ctx) {
        if (ctx.performed) JumpPressed = true;
    }

    public void OnAttack(InputAction.CallbackContext ctx) {
        //死亡していたら攻撃できない
        if (core.parameter.isDead || !isLocalPlayer) return;

        //入力タイプで分岐
        switch (ctx.phase) {
            //押した瞬間から
            case InputActionPhase.Started:
                AttackPressed = true;
                break;
            //離した瞬間まで
            case InputActionPhase.Canceled:
                AttackPressed = false;
                StopShootAnim();
                break;
            //押した瞬間
            case InputActionPhase.Performed:
                AttackTriggered = true;
                break;
        }
    }

    /// <summary>
    /// スキル
    /// </summary>
    public void OnSkill(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) SkillTriggered = true;
    }

    /// <summary>
    /// インタラクト
    /// </summary>
    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) InteractTriggered = true;
    }
}
