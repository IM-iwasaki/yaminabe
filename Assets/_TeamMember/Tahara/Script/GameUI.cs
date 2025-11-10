using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 各クライアントに配置するロードで壊されないUI
/// </summary>
public class GameUI : MonoBehaviour
{
    public static GameUI instance = null;
    private void Awake() {
        DontDestroyOnLoad(gameObject);
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }
}
