using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// キャラクターの入力を管理
/// </summary>
public class CharacterInput : MonoBehaviour {

    CharacterBase player;
    [SerializeField] private InputActionAsset inputActions;
    //移動を要求する方向
    public Vector2 MoveInput { get ; private set; }


    /// <summary>
    /// 初期化(Baseが呼び出す。)
    /// </summary>
    /// <param name="_linkTarget">参照する対象</param>
    public void Initialize(CharacterBase _linkTarget) {
        player = _linkTarget;

        //コンテキストの登録
        var map = inputActions.FindActionMap("Player");
        foreach (var action in map.actions) {
            action.started += ctx => OnInputStarted(action.name, ctx);
            action.performed += ctx => OnInputPerformed(action.name, ctx);
            action.canceled += ctx => OnInputCanceled(action.name, ctx);
        }
        map.Enable();
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
                player.HandleAttack(ctx);
                break;
            case "Fire_Sub":
                player.HandleAttack(ctx);
                break;
            case "SubWeapon":
                player.paramater.weaponController_sub.TryUseSubWeapon();
                break;
            case "ShowHostUI":
                OnShowHostUI(ctx);
                break;
            case "CameraMenu":
                OnShowCameraMenu(ctx);
                break;
            case "Ready":
                OnReadyPlayer(ctx);
                break;
            case "SendMessage":
                OnSendMessage(ctx);
                break;
            case "SendStamp":
                OnSendStamp(ctx);
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
                player.HandleAttack(ctx);
                break;
            case "Fire_Sub":
                player.HandleAttack(ctx);
                break;
            case "Skill":
                OnUseSkill(ctx);
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
                player.CmdResetAnimation();
                break;
            case "Fire_Main":
            case "Fire_Sub":
                player.HandleAttack(ctx);
                break;
        }
    }

    /// <summary>
    /// 移動
    /// </summary>
    public void OnMove(InputAction.CallbackContext context) {
        MoveInput = context.ReadValue<Vector2>();
        float moveX = MoveInput.x;
        float moveZ = MoveInput.y;
        //アニメーション管理
        player.ControllMoveAnimation(moveX, moveZ);

    }

    /// <summary>
    /// ジャンプ
    /// </summary>
    public void OnJump(InputAction.CallbackContext context) {
        // ボタンが押された瞬間だけ反応させる
        if (context.performed && player.paramater.IsGrounded) {
            player.paramater.IsJumpPressed = true;
            bool isJumping = !player.paramater.IsGrounded;
            player.anim.SetBool("Jump", isJumping);
        }
    }
    /// <summary>
    /// スキル
    /// </summary
    public void OnUseSkill(InputAction.CallbackContext context) {
        if (context.performed)
            player.paramater.StartUseSkill();
    }
    /// <summary>
    /// インタラクト
    /// </summary>
    public void OnInteract(InputAction.CallbackContext context) {
        if (context.performed) player.Interact();
    }
    /// <summary>
    /// リロード
    /// </summary>
    public void OnReload(InputAction.CallbackContext context) {
        if (context.performed && player.paramater.weaponController_main.ammo < player.paramater.weaponController_main.weaponData.maxAmmo) {
            player.paramater.weaponController_main.CmdReloadRequest();
        }
    }
    /// <summary>
    /// 追加:タハラ UI表示
    /// </summary>
    public void OnShowHostUI(InputAction.CallbackContext context) {
        if (!player.isServer || !player.isLocalPlayer || SceneManager.GetActiveScene().name == "GameScene") return;
        if (context.started) {
            if (player.CameraMenu.isOpen)
                player.CameraMenu.ToggleMenu();
            HostUI.ShowOrHideUI();
        }
    }

    public void OnShowCameraMenu(InputAction.CallbackContext context) {
        if (!player.isLocalPlayer)
            return;
        if (context.started) {
            if (HostUI.isVisibleUI) {
                HostUI.ShowOrHideUI();
            }

            player.CameraMenu.ToggleMenu();
        }
    }
    /// <summary>
    /// 追加:タハラ プレイヤーの準備状態切り替え
    /// </summary>
    /// <param name="context"></param>
    public void OnReadyPlayer(InputAction.CallbackContext context) {
        if (!player.isLocalPlayer || SceneManager.GetActiveScene().name == "GameScene") return;
        //内部の準備状態を更新
        if (context.started) {
            if (!player.isServer)
            player.CmdChangePlayerReady();
        else {
            player.paramater.ready = !player.paramater.ready;
                ChatManager.instance.CmdSendSystemMessage(player.paramater.PlayerName + " ready :  " + player.paramater.ready);
            }
        }
    }

    /// <summary>
    /// 追加:タハラ チャット送信
    /// </summary>
    public void OnSendMessage(InputAction.CallbackContext context) {
        if (!player.isLocalPlayer)
            return;
        //チャット送信
        var key = context.control.name;
        string sendMessage;
        switch (key) {
            case "upArrow":
                sendMessage = "?";
                break;
            case "leftArrow":
                sendMessage = "ggEZ";
                break;
            case "rightArrow":
                sendMessage = "WTF";
                break;
            default:
                sendMessage = "4649";
                break;
        }
        ChatManager.instance.CmdSendSystemMessage(player.paramater.PlayerName + ":" + sendMessage);
    }

    /// <summary>
    /// 追加:タハラ スタンプ送信
    /// </summary>
    public void OnSendStamp(InputAction.CallbackContext context) {
        if (!player.isLocalPlayer) return;
        //チャット送信
        if (context.started) {
            int stampIndex = Random.Range(0, 4);
            ChatManager.instance.CmdSendStamp(stampIndex, player.paramater.PlayerName);
        }
    }
}
