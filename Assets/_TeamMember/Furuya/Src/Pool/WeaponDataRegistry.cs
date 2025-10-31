using System.Collections.Generic;
using UnityEngine;

public interface IWeaponInfo {
    string WeaponName { get; }
}

public class WeaponDataRegistry : MonoBehaviour {
    [Header("登録する武器データ（メイン＋サブ問わず）")]
    [SerializeField]
    private List<ScriptableObject> allWeaponData = new List<ScriptableObject>();

    private static Dictionary<string, IWeaponInfo> weaponDict = new Dictionary<string, IWeaponInfo>();

    void Awake() {
        RegisterAll();
    }

    private void RegisterAll() {
        weaponDict.Clear();

        foreach (var obj in allWeaponData) {
            if (obj is not IWeaponInfo weaponInfo)
                continue;

            if (string.IsNullOrEmpty(weaponInfo.WeaponName))
                continue;

            if (weaponDict.ContainsKey(weaponInfo.WeaponName)) {
                Debug.LogWarning($"WeaponDataRegistry: 重複した武器名 '{weaponInfo.WeaponName}' が存在します。");
                continue;
            }

            weaponDict[weaponInfo.WeaponName] = weaponInfo;
        }

        Debug.Log($"WeaponDataRegistry: {weaponDict.Count} 件のWeaponData/SubWeaponDataを登録しました。");
    }

    // --- WeaponDataを直接取得するメソッド ---
    public static WeaponData GetWeapon(string weaponName) {
        if (weaponDict.TryGetValue(weaponName, out var info) && info is WeaponData weapon) {
            Debug.LogWarning($"{weapon} を取得した");

            return weapon;
        }

        Debug.LogWarning($"WeaponDataRegistry: '{weaponName}' に対応する WeaponData が見つかりません。");
        return null;
    }

    // --- SubWeaponDataを直接取得するメソッド ---
    public static SubWeaponData GetSubWeapon(string weaponName) {
        if (weaponDict.TryGetValue(weaponName, out var info) && info is SubWeaponData sub)
            return sub;

        Debug.LogWarning($"WeaponDataRegistry: '{weaponName}' に対応する SubWeaponData が見つかりません。");
        return null;
    }

    // --- 共通インターフェースアクセス（型不問） ---
    public static IWeaponInfo Get(string weaponName) {
        weaponDict.TryGetValue(weaponName, out var info);
        return info;
    }
}
