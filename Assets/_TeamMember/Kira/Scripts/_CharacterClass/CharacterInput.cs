using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterInput : NetworkBehaviour {
    private CharacterBase core;
    [SerializeField] private InputActionAsset inputActions;

    public Vector2 MoveInput { get; private set; }
    public bool isJumpPressed { get; private set; }
    public bool AttackPressed { get; private set; }
    public bool AttackReleased { get; private set; }
    public bool AttackTriggered { get; private set; }

    public bool SkillTriggered;

    public bool InteractTriggered;

    private CharacterAnimationController characterAnimationController;

    public void Initialize(CharacterBase core) {
        this.core = core;
        characterAnimationController = GetComponent<CharacterAnimationController>();
        //コンテキストの登録
        var map = inputActions.FindActionMap("Player");
        foreach (var action in map.actions) {
            action.started += ctx => OnInputStarted(action.name, ctx);
            action.performed += ctx => OnInputPerformed(action.name, ctx);
            action.canceled += ctx => OnInputCanceled(action.name, ctx);
        }
        map.Enable();
    }

    private void LateUpdate() {
        AttackReleased = false;
        AttackTriggered = false;
        SkillTriggered = false;
        InteractTriggered = false;
        isJumpPressed = false;
    }

    /// <summary>
    /// 入力の共通ハンドラ
    /// </summary>
    private void OnInputStarted(string actionName, InputAction.CallbackContext ctx) {
        switch (actionName) {
            case "Jump":
                OnJump(ctx);
                break;
            case "Fire_Main":
                OnAttack(ctx);
                break;
            case "Fire_Sub":
                OnAttack(ctx);
                break;
            case "SubWeapon":
                core.weaponController_sub.TryUseSubWeapon();
                break;
            case "ShowHostUI":
                core.OnShowHostUI(ctx);
                break;
            case "CameraMenu":
                core.OnShowCameraMenu(ctx);
                break;
            case "Ready":
                core.OnReadyPlayer(ctx);
                break;
            case "SendMessage":
                core.OnSendMessage(ctx);
                break;
            case "SendStamp":
                core.OnSendStamp(ctx);
                break;
        }
    }
    private void OnInputPerformed(string actionName, InputAction.CallbackContext ctx) {
        switch (actionName) {
            case "Move":
                OnMove(ctx);
                break;
            case "Jump":
                OnJump(ctx);
                break;
            case "Fire_Main":
                OnAttack(ctx);
                break;
            case "Fire_Sub":
                OnAttack(ctx);
                break;
            case "Skill":
                OnSkill(ctx);
                break;
            case "Interact":
                OnInteract(ctx);
                break;
            case "Reload":
                OnReload(ctx);
                break;
        }
    }
    private void OnInputCanceled(string actionName, InputAction.CallbackContext ctx) {
        switch (actionName) {
            case "Move":
                MoveInput = Vector2.zero;
                characterAnimationController.CmdResetAnimation();
                break;
            case "Fire_Main":
            case "Fire_Sub":
                OnAttack(ctx);
                break;
        }
    }

    /// <summary>
    /// 移動
    /// </summary>
    public void OnMove(InputAction.CallbackContext ctx) {
        MoveInput = ctx.ReadValue<Vector2>();
        float moveX = MoveInput.x;
        float moveZ = MoveInput.y;
        //アニメーション管理
        characterAnimationController.ControllMoveAnimation(moveX, moveZ);
    }

    /// <summary>
    /// ジャンプ
    /// </summary>
    public void OnJump(InputAction.CallbackContext context) {
        // ボタンが押された瞬間だけ反応させる
        if (context.performed && core.parameter.IsGrounded) {
            isJumpPressed = true;
            bool isJumping = !core.parameter.IsGrounded;
            characterAnimationController.anim.SetBool("Jump", isJumping);
        }
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
                characterAnimationController.StopShootAnim();
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
    public void OnInteract(InputAction.CallbackContext ctx) {
        if (ctx.performed) InteractTriggered = true;
    }

    public void OnReload(InputAction.CallbackContext context) {
        if (context.performed && core.weaponController_main.ammo < core.weaponController_main.weaponData.maxAmmo) {
            core.weaponController_main.CmdReloadRequest();
        }
    }
}
