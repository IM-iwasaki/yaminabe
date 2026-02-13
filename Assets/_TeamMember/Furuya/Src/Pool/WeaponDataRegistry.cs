using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 名前保存用
/// </summary>
public interface IWeaponInfo {
    int WeaponID { get; }
    string WeaponName { get; }
}

public class WeaponDataRegistry : MonoBehaviour {
    [Header("登録する武器データ（メイン＋サブ問わず）")]
    [SerializeField]
    private List<ScriptableObject> allWeaponData = new List<ScriptableObject>();

    private static Dictionary<int, IWeaponInfo> weaponDict = new Dictionary<int, IWeaponInfo>();

    void Awake() {
        RegisterAll();
    }

    /// <summary>
    /// 内容確認
    /// </summary>
    private void RegisterAll() {
        weaponDict.Clear();

        foreach (var obj in allWeaponData) {
            if (obj is not IWeaponInfo weaponInfo)
                continue;

            if (weaponDict.ContainsKey(weaponInfo.WeaponID)) {
                Debug.LogWarning(
                    $"WeaponDataRegistry: 重複したID '{weaponInfo.WeaponID}' が存在します。"
                );
                continue;
            }

            weaponDict[weaponInfo.WeaponID] = weaponInfo;
        }

        Debug.Log($"WeaponDataRegistry: {weaponDict.Count} 件登録しました。");
    }


    // --- WeaponDataを直接取得するメソッド ---
    public static WeaponData GetWeapon(int weaponID) {
        if (weaponDict.TryGetValue(weaponID, out var info)
            && info is WeaponData weapon) {
            return weapon;
        }

        Debug.LogWarning(
            $"WeaponDataRegistry: ID {weaponID} に対応する WeaponData が見つかりません。"
        );

        return null;
    }

    // --- SubWeaponDataを直接取得するメソッド ---
    public static SubWeaponData GetSubWeapon(int weaponID) {
        if (weaponDict.TryGetValue(weaponID, out var info)
            && info is SubWeaponData sub) {
            return sub;
        }

        return null;
    }
}
