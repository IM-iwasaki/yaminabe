using UnityEngine;

[CreateAssetMenu(menuName = "Character/その他/StatusBase(基本的に作らないで)")]
public class StatusBase : ScriptableObject {
    [Header("全てのキャラクターステータスの基礎ステータス")]
    //基礎最大体力
    public int MaxHP = 100;
    //基礎攻撃力
    public int Attack = 10;
    //基礎移動速度
    public int MoveSpeed = 3;
}
