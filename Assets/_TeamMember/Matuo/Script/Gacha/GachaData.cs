using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ガチャのアイテムリストを保持するScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "GachaData", menuName = "Gacha/GachaData")]
public class GachaData : ScriptableObject {
    [Header("ガチャで排出されるアイテム一覧")]
    public List<GachaItem> items = new List<GachaItem>();
}