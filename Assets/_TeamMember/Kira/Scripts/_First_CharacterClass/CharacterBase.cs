using Mirror;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(NetworkIdentity))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Rigidbody))]

//
// @flie    First_CharacterClass
//
abstract class CharacterBase : NetworkBehaviour {
    //現在の体力
    [SyncVar]public int HP;
    //最大の体力
    public int MaxHP { get; private set; }
    //基礎攻撃力
    public int Attack { get; private set; }
    //移動速度
    public int MoveSpeed { get; private set; } = 5;
    //持っている武器の文字列
    public string CurrentWeapon { get; private set; }
    //所属チームの番号
    [SyncVar]public int TeamID;

    //移動を要求する方向
    protected Vector2 MoveInput;
    //実際に移動する方向
    public Vector3 MoveDirection { get; private set; }

    //視点を要求する方向
    protected Vector2 LookInput { get; private set; }
    //向いている方向
    public Vector3 LookDirection { get; private set; }

    //プレイヤーの状態
    protected bool IsDead { get; private set; } = false;

    //コンポーネント情報
    protected new Rigidbody rigidbody;

    //スタン、怯み(硬直する,カメラ以外操作無効化)


    //武器の使用するため
    [SerializeField] protected WeaponController weaponController;

    protected void Start() {
        rigidbody = GetComponent<Rigidbody>();

    }

    #region 入力受付

    //移動入力を受け付けるコンテキスト
    public void OnMove(InputAction.CallbackContext context) {
        MoveInput = context.ReadValue<Vector2>();
    }
    //視点入力を受け付けるコンテキスト
    public void OnLook(InputAction.CallbackContext context) {
        LookInput = context.ReadValue<Vector2>();
    }
    //攻撃入力を受け付けるコンテキスト
    public void OnAttack(InputAction.CallbackContext context) {
        if (context.performed) StartAttack();
    }
    //インタラクト入力を受け付けるコンテキスト
    public void OnInteract(InputAction.CallbackContext context) {
        if (context.performed) Interact();
    }

    #endregion

    #region 入力実行関数

    //攻撃関数
    abstract protected void StartAttack();

    //移動関数
    protected void MoveControl(){
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

    //視点移動関数
    protected void LookControl() {

    }

    //インタラクト関数
    protected void Interact() {
    }

    #endregion

    //当たり判定関係
    protected void OnTriggerStay(Collider _collider) {
        if (_collider.CompareTag("Item")) {
            //アイテム使用キー入力入れる
            ItemBase item = _collider.GetComponent<ItemBase>();
            //仮。挙動確認。
            item.Use(gameObject);
        }
    }

    //被弾関数
    [Server]public void TakeDamage(int _damage) {
        //ダメージが0以下だったら帰る
        if (_damage <= 0) return;
        //HPの減算処理
        HP -= _damage;
        //HPが0以下になったらisDeadを真にする
        if (HP <= 0) IsDead = true;
    }
}
