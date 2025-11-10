using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 各クライアントに配置するロードで壊されないUI
/// </summary>
public class GameUI : MonoBehaviour
{
    private void Awake() {
        DontDestroyOnLoad(gameObject);
    }
}
