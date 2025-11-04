using UnityEngine;

/// <summary>
/// アイテムの基底クラス
/// - 武器・消費アイテム共通の処理をまとめる
/// </summary>
public abstract class ItemBase : MonoBehaviour {

    [Header("アイテム名")]
    public string itemName; // アイテムの名前

    [Header("説明文")]
    [TextArea]
    public string description; // 説明文

    [Header("アイテムのアイコン")]
    public Sprite icon; // UI用アイコン（必要なら）

    [Header("取得時破棄するかどうか（デフォ破棄）")]
    public bool canDestroy = true;

    //  アイテムを毎秒どれだけ回転させるか
    private Vector3 weaponRotation = new Vector3(0, 50f, 0);
    private void Update() {
        //  オブジェクトを回転させる
        transform.Rotate(weaponRotation * Time.deltaTime);
    }

    /// <summary>
    /// アイテムを使用する処理
    /// プレイヤー処理に依存 → コメントで枠だけ用意
    /// </summary>
    /// <param name="player">使用対象のプレイヤー</param>
    public abstract void Use(GameObject player);
}
