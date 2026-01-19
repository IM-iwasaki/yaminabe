using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HostSelectUI : MonoBehaviour {
    public Button selectHostButton;
    public bool isSelected = false;
    public UDPListener.UdpMessage selectedHost;
    [SerializeField]
    private Transform UIRoot;

    /// <summary>
    /// 見つけたホストを一覧で表示
    /// </summary>
    /// <param name="_host"></param>
    public void ShowHostList(List<UDPListener.UdpMessage> _host) {

        for (int i = 0, max = _host.Count; i < max; i++) {
            //ホストデータをキャプチャ
            var hostData = _host[i];
            //ボタン生成
            Button createdButton = Instantiate(selectHostButton, UIRoot);
            if (hostData.gamePlaying) {
                ColorBlock buttonColor = createdButton.colors;
                buttonColor.selectedColor = buttonColor.normalColor = buttonColor.disabledColor = buttonColor.highlightedColor = Color.red;
                createdButton.colors = buttonColor;
            }
                

            //ボタンに表示するホストの名前を変更
            createdButton.GetComponentInChildren<TextMeshProUGUI>().text = hostData.hostName;
            //生成したボタンにイベント登録
            createdButton.onClick.AddListener(() => OnHostButtonClicked(hostData));
        }
    }

    public void ResetPanel() {
        for(int i = 0,max = UIRoot.childCount;i < max; i++) {
            Destroy(UIRoot.GetChild(i).gameObject);
        }
    }

    /// <summary>
    /// ボタンを押したした時の処理
    /// </summary>
    /// <param name="_host"></param>
    public void OnHostButtonClicked(UDPListener.UdpMessage _host) {
        //ゲームプレイ中なら参加できないように弾く
        if (_host.gamePlaying)
            return;
        selectedHost = _host;
        isSelected = true;
    }
}
