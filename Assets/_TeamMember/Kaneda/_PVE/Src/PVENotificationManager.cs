using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PVENotificationManager : MonoBehaviour
{
    public static PVENotificationManager Instance;

    [SerializeField] private TextMeshProUGUI text;

    [Header("通知を表示する時間")]
    [SerializeField] private float isVisibleTime = 3.0f;

    private void Awake() {
        Instance = this;
        text.gameObject.SetActive(false);
    }

    /// <summary>
    /// 通知のテキストを受け取って表示する
    /// </summary>
    /// <param name="message"></param>
    public void SendNotificationMessage(string message) {
        text.gameObject.SetActive(true);

        text.SetText(message);
    }

}
