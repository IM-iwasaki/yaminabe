using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;
using static TeamData;

[RequireComponent(typeof(NetworkIdentity))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Rigidbody))]

//
// @flie    First_CharacterClass
//
abstract class CharacterBase : NetworkBehaviour {
    #region ～変数宣言～

    #region ～ステータス～
    //現在の体力
    [SyncVar]public int HP;
    //最大の体力
    public int MaxHP { get; protected set; }
    //基礎攻撃力
    public int Attack { get; protected set; }
    //移動速度
    public int MoveSpeed { get; protected set; } = 5;
    //持っている武器の文字列
    public string CurrentWeapon { get; protected set; }
    //所属チームの番号
    [SyncVar]public int TeamID;

    #endregion

    #region ～Vector系統変数～

    //移動を要求する方向
    protected Vector2 MoveInput;
    //実際に移動する方向
    public Vector3 MoveDirection { get; private set; }
    //視点を要求する方向
    protected Vector2 LookInput { get; private set; }
    //向いている方向
    public Vector3 LookDirection { get; private set; }

    //リスポーン地点
    public Vector3 RespownPosition { get; protected set; }

    #endregion

    #region ～状態管理・コンポーネント変数～

    //プレイヤーの状態
    protected bool IsDead { get; private set; } = false;

    //コンポーネント情報
    protected new Rigidbody rigidbody;

    #endregion

    //武器を使用するため
    [SerializeField] protected NetworkWeapon weaponController;

    //スタン、怯み(硬直する,カメラ以外操作無効化)

    #endregion

    protected void Start() {
        rigidbody = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// ステータスのインポート
    /// </summary>
    protected abstract void StatusInport();

    /// <summary>
    /// 当たり判定関係
    /// </summary>
    protected void OnTriggerStay(Collider _collider) {
        if (isLocalPlayer) {
            if (_collider.CompareTag("Item")) {
                //アイテム使用キー入力入れる
                ItemBase item = _collider.GetComponent<ItemBase>();
                //仮。挙動確認。
                item.Use(gameObject);
            }
            if (_collider.CompareTag("RedTeam")) {
                CmdJoinTeam(netIdentity, teamColor.Red);
            }
            if (_collider.CompareTag("BlueTeam")) {
                CmdJoinTeam(netIdentity, teamColor.Blue);
            }
        }
    }

    /// <summary>
    /// 被弾・死亡判定関数
    /// </summary>
    [Command]public void TakeDamage(int _damage) {
        //ダメージが0以下だったら帰る
        if (_damage <= 0)
            return;
        //HPの減算処理
        HP -= _damage;
        //HPが0以下になったらisDeadを真にする
        if (HP <= 0)
            IsDead = true;
    }

    /// <summary>
    /// リスポーン関数
    /// </summary>
    [Command]public void Respown() {
        //死んでいなかったら即抜け
        if (!IsDead)
            return;

        //死亡状態解除
        IsDead = false;
        //リスポーン地点に移動させる
        transform.position = RespownPosition;
        //リスポーン時に向きをリセットしたほうがいいかも。その場合ここに書く。

    }

    /// <summary>
    /// チーム参加処理(TeamIDを更新)
    /// </summary>
    [Command]public void CmdJoinTeam(NetworkIdentity _player, teamColor _color) {
        CharacterBase player = _player.GetComponent<CharacterBase>();
        int currentTeam = player.TeamID;
        int newTeam = (int)_color;

        //加入しようとしてるチームが埋まっていたら
        if (ServerManager.instance.teams[(newTeam)].teamPlayerList.Count >= TEAMMATE_MAX) {
            Debug.Log("チームの人数が最大です！");
            return;
        }
        //既に同じチームに入っていたら
        if (newTeam == currentTeam) {
            Debug.Log("今そのチームにいます!");
            return;
        }
        //新たなチームに加入する時
        //今加入しているチームから抜けてIDをリセット
        ServerManager.instance.teams[_player.GetComponent<CharacterBase>().TeamID].teamPlayerList.Remove(_player);
        player.TeamID = -1;
        //新しいチームに加入
        ServerManager.instance.teams[newTeam].teamPlayerList.Add(_player);
        player.TeamID = newTeam;
        //ログを表示
        Debug.Log(_player.ToString() + "は" + newTeam + "番目のチームに加入しました！");
    }

    #region 入力受付・入力実行関数

    /// <summary>
    /// 移動
    /// </summary>
    public void OnMove(InputAction.CallbackContext context) {
        MoveInput = context.ReadValue<Vector2>();
    }
    /// <summary>
    /// 視点
    /// </summary>
    public void OnLook(InputAction.CallbackContext context) {
        LookInput = context.ReadValue<Vector2>();
    }
    /// <summary>
    /// メイン攻撃
    /// </summary
    public void OnAttack_Main(InputAction.CallbackContext context) {
        if (context.performed) StartAttack(PlayerConst.AttackType.Main);
    }
    /// <summary>
    /// サブ攻撃
    /// </summary
    public void OnAttack_Sub(InputAction.CallbackContext context) {
        if (context.performed) StartAttack(PlayerConst.AttackType.Sub);
    }
    /// <summary>
    /// スキル
    /// </summary
    public void OnUseSkill(InputAction.CallbackContext context) {
        if (context.performed)
            StartUseSkill();
    }
    /// <summary>
    /// インタラクト
    /// </summary>
    public void OnInteract(InputAction.CallbackContext context) {
        if (context.performed) Interact();
    }

    /// <summary>
    /// 移動関数
    /// </summary>
    protected void MoveControl() {
        //カメラの向きを取得
        Transform cameraTransform = Camera.main.transform;
        //進行方向のベクトルを取得
        Vector3 forward = cameraTransform.forward;
        forward.y = 0f;
        forward.Normalize();
        //右方向のベクトルを取得
        Vector3 right = cameraTransform.right;
        right.y = 0f;
        right.Normalize();
        //2つのベクトルを合成
        MoveDirection = forward * MoveInput.y + right * MoveInput.x;

        // カメラの向いている方向をプレイヤーの正面に
        Vector3 aimForward = forward; // 水平面だけを考慮
        if (aimForward != Vector3.zero) {
            Quaternion targetRot = Quaternion.LookRotation(aimForward);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, PlayerConst.TURN_SPEED * Time.deltaTime);
        }

        // 移動方向にキャラクターを向ける
        //Quaternion targetRotation = Quaternion.LookRotation(MoveDirection);
        //transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);

        rigidbody.velocity = new Vector3(MoveDirection.x * MoveSpeed, rigidbody.velocity.y, MoveDirection.z * MoveSpeed);
    }

    /// <summary>
    /// 攻撃関数
    /// </summary>
    abstract protected void StartAttack(PlayerConst.AttackType _type = PlayerConst.AttackType.Main);

    //スキル使用関数
    abstract protected void StartUseSkill();

    //インタラクト関数
    protected void Interact() {
    }

    #endregion    
}
