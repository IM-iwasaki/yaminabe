using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterInput : NetworkBehaviour {
    private CharacterBase core;

    public Vector2 MoveInput { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool AttackPressed { get; private set; }
    public bool AttackReleased { get; private set; }

    public bool SkillTriggered;

    public bool InteractTriggered;

    public void Initialize(CharacterBase core) {
        this.core = core;
    }

    private void LateUpdate() {
        AttackReleased = false;
        SkillTriggered = false;
        InteractTriggered = false;
        JumpPressed = false;
    }

    /// <summary>
    /// 移動
    /// </summary>
    public void OnMove(InputAction.CallbackContext ctx) {
        MoveInput = ctx.ReadValue<Vector2>();
    }

    /// <summary>
    /// ジャンプ
    /// </summary>
    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) JumpPressed = true;
    }

    public void OnAttack(InputAction.CallbackContext ctx)
    {
        if (ctx.started) AttackPressed = true;
        if (ctx.canceled) {
            AttackPressed = false;
            AttackReleased = true;
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
