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

    private CharacterAnimationController animCon;
    private InputActionMap playerMap;
    private bool inputInitialized;

    #region 初期化 / クリーンアップ

    public void Initialize(CharacterBase core) {
        if (inputInitialized) return;
        inputInitialized = true;

        this.core = core;
        animCon = GetComponent<CharacterAnimationController>();

        playerMap = inputActions.FindActionMap("Player");

        foreach (var action in playerMap.actions) {
            action.started += OnActionStarted;
            action.performed += OnActionPerformed;
            action.canceled += OnActionCanceled;
        }

        playerMap.Enable();
    }

    public override void OnStopClient() {
        CleanupInput();
    }

    private void OnDestroy() {
        CleanupInput();
    }

    private void CleanupInput() {
        if (!inputInitialized || playerMap == null) return;

        foreach (var action in playerMap.actions) {
            action.started -= OnActionStarted;
            action.performed -= OnActionPerformed;
            action.canceled -= OnActionCanceled;
        }

        playerMap.Disable();
        inputInitialized = false;
    }

    #endregion

    private void LateUpdate() {
        //押した瞬間・離した瞬間を管理する変数のリセット
        AttackReleased = false;
        AttackTriggered = false;
        SkillTriggered = false;
        InteractTriggered = false;
        isJumpPressed = false;

        if (core != null)
            core.parameter.AttackTrigger = false;
    }

    #region InputSystem 共通ハンドラ

    private void OnActionStarted(InputAction.CallbackContext ctx) {
        OnInputStarted(ctx.action.name, ctx);
    }

    private void OnActionPerformed(InputAction.CallbackContext ctx) {
        OnInputPerformed(ctx.action.name, ctx);
    }

    private void OnActionCanceled(InputAction.CallbackContext ctx) {
        OnInputCanceled(ctx.action.name, ctx);
    }

    #endregion

    #region 入力処理

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
                animCon.CmdResetAnimation();
                break;
            case "Fire_Main":
            case "Fire_Sub":
                OnAttack(ctx);
                break;
        }
    }

    #endregion

    #region 各種入力

    /// <summary>
    /// 移動入力
    /// </summary>
    public void OnMove(InputAction.CallbackContext ctx) {
        if (!core.parameter.canMove) return;

        MoveInput = ctx.ReadValue<Vector2>();

        float moveX = MoveInput.x;
        float moveZ = MoveInput.y;
        animCon.ControllMoveAnimation(moveX, moveZ);
    }

    /// <summary>
    /// ジャンプ
    /// </summary>
    public void OnJump(InputAction.CallbackContext context) {
        //TODO:ホコを持っていたら弾く

        // ボタンが押された瞬間だけ反応させる
        if (context.performed && core.parameter.IsGrounded) {
            isJumpPressed = true;
            bool isJumping = !core.parameter.IsGrounded;
            animCon.anim.SetBool("Jump", isJumping);
        }
    }

    /// <summary>
    /// 攻撃入力
    /// </summary>
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
                animCon.StopShootAnim();
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
    public void OnSkill(InputAction.CallbackContext ctx) {
        if (ctx.performed) SkillTriggered = true;
    }

    /// <summary>
    /// インタラクト
    /// </summary>
    public void OnInteract(InputAction.CallbackContext ctx) {
        if (ctx.performed) InteractTriggered = true;
    }

    /// <summary>
    /// リロード
    /// </summary>
    /// <param name="context"></param>
    public void OnReload(InputAction.CallbackContext context) {
        if (context.performed &&
            core.weaponController_main.ammo < core.weaponController_main.weaponData.maxAmmo) {
            core.weaponController_main.CmdReloadRequest();
        }
    }
    #endregion
}