using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// ガチャから出てくるアイテムのデータ
/// </summary>
[CreateAssetMenu(fileName = "GachaData", menuName = "Gacha/GachaData")]
public class GachaData : ScriptableObject {
    public List<GachaItem> items;
}