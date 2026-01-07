using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(menuName = "ScriptableObject/SubWeapons/Item")]
public abstract class ItemData : SubWeaponData {
    [Header("BaseData")]
    public ItemType itemType;
}
