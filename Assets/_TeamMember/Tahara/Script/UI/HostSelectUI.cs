using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HostSelectUI : MonoBehaviour {
    public UDPListener listener;
    public Button selectHostButton;
    public bool isSelected = false;
    public UDPListener.UdpMessage selectedHost;
    [SerializeField]
    private Transform UIRoot;

    private void Start() {
        listener.onHostUpdated += UpdateHost;
    }

    /// <summary>
    /// 見つけたホストを一覧で表示
    /// </summary>
    /// <param name="_host"></param>
    public void ShowHostList(List<UDPListener.UdpMessage> _host) {

        for (int i = 0, max = _host.Count; i < max; i++) {
            //ホストデータをキャプチャ
            var hostData = _host[i];
            //ボタン生成
            var buttonObj = Instantiate(selectHostButton, UIRoot);
            var hostBuuton = buttonObj.GetComponent<HostButton>();

            hostBuuton.Setup(hostData,OnHostButtonClicked);
                
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

    public void UpdateHost(UDPListener.UdpMessage updated) {
        foreach (Transform child in UIRoot) {
            var hostButton = child.GetComponent<HostButton>();
            if (hostButton.hostData.ip == updated.ip) {
                hostButton.hostData = updated;
                hostButton.RefreshUI();
            }
        }
    }

}
