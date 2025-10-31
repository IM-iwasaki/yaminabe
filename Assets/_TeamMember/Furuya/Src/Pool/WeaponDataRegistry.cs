using System.Collections.Generic;
using UnityEngine;

public class WeaponDataRegistry : MonoBehaviour {
    [SerializeField]
    private List<WeaponData> weaponDataList = new List<WeaponData>();

    private static Dictionary<string, WeaponData> weaponDict = new Dictionary<string, WeaponData>();

    void Awake() {
        RegisterAll();
    }

    private void RegisterAll() {
        weaponDict.Clear();

        foreach (var data in weaponDataList) {
            if (data == null || string.IsNullOrEmpty(data.weaponName))
                continue;

            if (weaponDict.ContainsKey(data.weaponName)) {
                Debug.LogWarning($"WeaponDataRegistry: 重複した武器名 '{data.weaponName}' が存在します。");
                continue;
            }

            weaponDict[data.weaponName] = data;
        }

        Debug.Log($"WeaponDataRegistry: {weaponDict.Count} 件のWeaponDataを登録しました。");
    }

    public static WeaponData Get(string weaponName) {
        if (weaponDict.TryGetValue(weaponName, out var data))
            return data;

        Debug.LogWarning($"WeaponDataRegistry: '{weaponName}' に一致するデータが見つかりません。");
        return null;
    }
}
