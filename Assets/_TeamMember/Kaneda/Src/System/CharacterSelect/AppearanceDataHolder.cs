using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// プレイヤーの見た目データだけを保持するクラス
/// </summary>
public class AppearanceDataHolder : MonoBehaviour
{
    //  インスタンス化
    public static AppearanceDataHolder instance;

    // サーバー側が全プレイヤーの見た目を保持
    public Dictionary<uint, (int characterNo, int skinNo)> states = new Dictionary<uint, (int, int)>();

    // クライアント側で netId → GameObject を保持
    public Dictionary<uint, GameObject> clientPlayers = new Dictionary<uint, GameObject>();

    private void Awake() {
        if (instance != null && instance != this) {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject); // データだけ常駐
    }
}
