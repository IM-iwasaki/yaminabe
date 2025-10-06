using Mirror;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(NetworkIdentity))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Rigidbody))]

abstract class CharacterBase : NetworkBehaviour {
    //現在の体力
    public int HP{ get; private set; }
    //最大の体力
    public int MaxHP { get; private set; }
    //基礎攻撃力
    public int Attack { get; private set; }
    //移動速度
    public int MoveSpeed { get; private set; }

    //移動を要求する方向
    protected Vector2 MoveInput { get; private set; }
    //実際に移動する方向
    public Vector3 MoveDirection { get; set; }

    //視点を要求する方向
    public Vector2 LookInput { get; private set; }

    //プレイヤーの状態
    protected bool IsDead { get; private set; } = false;

    //コンポーネント情報
    protected new Rigidbody rigidbody;

    //次派生クラスで定義
    //近接
    [SerializeField] protected int MaxAttackSpeed { get; private set; }
    //魔法
    [SerializeField] protected int MP { get; private set; }
    [SerializeField] protected int MaxMP { get; private set; }
    //弾倉
    [SerializeField] protected int Magazine { get; private set; }
    [SerializeField] protected int MaxMagazine { get; private set; }

    //スタン、怯み(硬直する,カメラ以外操作無効化)

    protected void Start() {
         rigidbody = GetComponent<Rigidbody>();
    }

    //入力系を取る。

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

    //当たり判定関係
    protected void OnTriggerStay(Collider _collider) {
        if (_collider.CompareTag("Item")) {
            //アイテム使用キー入力入れる
            ItemBase item = _collider.GetComponent<ItemBase>();
            //仮。挙動確認。
            item.Use(gameObject);
        }
    }

    //攻撃関数
    abstract protected void StartAttack();

    //移動関数
    abstract protected void MoveControl();

    //被弾関数
    public void TakeDamage(int _damage) {
        //ダメージが0以下だったら帰る
        if (_damage <= 0) return;
        //HPの減算処理
        HP -= _damage;
        //HPが0以下になったらisDeadを真にする
        if (HP <= 0) IsDead = true;
    }
}
