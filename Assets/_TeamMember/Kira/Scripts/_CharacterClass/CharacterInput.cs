using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterInput : NetworkBehaviour {
    private CharacterBase core;
    private PlayerInput playerInput;

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

    public Vector2 lookInput;
    public bool isGamepad = false;
    public System.Action<bool> OnControlSchemeChanged;

    //最後にパッドを触った時間
    //(-999.0fであるのは、宣言時に初期値を設定しない場合に値が0になる点と、実装上
    // 0だと起動時にパッドを接続していなくてもパッドを使用していると誤認されてしまうため。)
    private float lastGamepadInputTime = -999.0f;

    //どれくらいパッドから入力がなかったら自動でキーボードマウスに戻るか。
    [SerializeField] private float gamepadTimeout = 2f;

    #region 初期化 / クリーンアップ

    public void Initialize(CharacterBase core) {
        // 自分のプレイヤー以外は入力を初期化しない
        if (!isLocalPlayer) return;

        if (inputInitialized) return;
        inputInitialized = true;

        this.core = core;
        animCon = GetComponent<CharacterAnimationController>();

        // PlayerInput取得
        playerInput = GetComponent<PlayerInput>();

        // PlayerInputからActionMap取得
        playerMap = playerInput.actions.FindActionMap("Player");

        foreach (var action in playerMap.actions) {
            action.started += OnActionStarted;
            action.performed += OnActionPerformed;
            action.canceled += OnActionCanceled;
        }

        playerMap.Enable();

        //明示的に初期化する。
        playerInput.SwitchCurrentControlScheme(
            "Keyboard&Mouse",
            Keyboard.current,Mouse.current
        );
        isGamepad = false;
        lastGamepadInputTime = -999f;
    }

    public override void OnStopClient() {
        CleanupInput();
    }

    private void OnDestroy() {
        CleanupInput();
    }

    private void CleanupInput() {
        if (!isLocalPlayer) return;
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

    private void Update() {
        //自分ではない、または入力が初期化されていない場合は帰る
        if (!isLocalPlayer || playerInput == null)
            return;

        //現在の入力デバイスがパッドだった場合入力値を読み込む
        bool gamepadInput =
        Gamepad.current != null &&
        (
            Gamepad.current.leftStick.ReadValue().sqrMagnitude > 0.01f ||
            Gamepad.current.rightStick.ReadValue().sqrMagnitude > 0.01f ||
            Gamepad.current.buttonSouth.isPressed
        );
        //最後にパッドを触った時間を計測
        //if (gamepadInput) lastGamepadInputTime = Time.time;

        if (gamepadInput) lastGamepadInputTime = Time.time;

        //直近にパッド入力があった場合有効化
        bool useGamepad =
            Gamepad.current != null && Time.time - lastGamepadInputTime < gamepadTimeout;

        // パッドに変更
        if (useGamepad && !isGamepad && Gamepad.current != null) {
            playerInput.SwitchCurrentControlScheme("Gamepad", Gamepad.current);
            isGamepad = true;

            OnControlSchemeChanged?.Invoke(true);
        }

        // キーボードに変更
        if (!useGamepad && isGamepad && Keyboard.current != null && Mouse.current != null) {
            playerInput.SwitchCurrentControlScheme("Keyboard&Mouse", Keyboard.current, Mouse.current);
            isGamepad = false;

            OnControlSchemeChanged?.Invoke(false);
        }
    }

    private void LateUpdate() {
        //押した瞬間・離した瞬間を管理する変数のリセット
        AttackReleased = false;
        AttackTriggered = false;
        SkillTriggered = false;
        InteractTriggered = false;
        isJumpPressed = false;

        if (core != null) core.parameter.attackTrigger = false;
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
        //死亡中は入力を受け付けない
        if (core.parameter.isDead || LoadingUI.instance.isLoading) return;
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
                core.weaponController_sub.TryUseSubWeapon(core.parameter.GetShootDirection());
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
        //死亡中は入力を受け付けない
        if (core.parameter.isDead || LoadingUI.instance.isLoading) return;
        switch (actionName) {
            case "Move":
                OnMove(ctx);
                break;
            case "Look":
                OnLook(ctx);
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
            case "Look":
                lookInput = Vector2.zero;
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
        MoveInput = ctx.ReadValue<Vector2>();

        float moveX = MoveInput.x;
        float moveZ = MoveInput.y;

        //死亡中は移動ベクトルを0にする
        if (core.parameter.isDead) MoveInput = Vector2.zero;

        animCon.ControllMoveAnimation(moveX, moveZ);
    }

    /// <summary>
    /// 入力アクションシステム
    /// </summary>
    public void OnLook(InputAction.CallbackContext ctx) {
        lookInput = ctx.ReadValue<Vector2>();
    }

    /// <summary>
    /// ジャンプ
    /// </summary>
    public void OnJump(InputAction.CallbackContext context) {
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
        //死亡していたらフラグを下して攻撃できなくする
        if (core.parameter.isDead || !isLocalPlayer) {
            AttackPressed = false;
            return;
        }

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