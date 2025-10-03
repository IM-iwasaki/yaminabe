using Mirror;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(NetworkIdentity))]
[RequireComponent(typeof(Rigidbody))]

abstract class CharacterBase : NetworkBehaviour {
    //現在の体力
    [SerializeField] protected int HP;
    [SerializeField] protected int maxHP;
    [SerializeField] protected int Attack;
    [SerializeField] protected int moveSpeed;

    //次派生クラスで定義
    //近接
    [SerializeField] protected int maxAttackSpeed;
    //魔法
    [SerializeField] protected int MP;
    [SerializeField] protected int maxMP;
    //弾倉
    [SerializeField] protected int magazine;
    [SerializeField] protected int maxMagazine;

    //スタン、怯み(硬直する,カメラ以外操作無効化)

    protected void Awake() {
        //SpawnManager.Instance.    
    }

    //入力系を取る。

    


    protected void OnTriggerStay(Collider collider) {
        if (collider.CompareTag("Item")) {
            //アイテム使用キー入力入れる
            ItemBase item = collider.GetComponent<ItemBase>();
            //仮。挙動確認。
            item.Use(gameObject);
        }
    }

    //被弾関数
    protected void TakeDamage(int _damage) {
        HP -= _damage;
    }
}
