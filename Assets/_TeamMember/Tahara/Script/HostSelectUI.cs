using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HostSelectUI : MonoBehaviour
{
    public Button selectHostButton;
    public bool isSelected = false;
    public UDPListener.UdpMessage selectedHost;
    [SerializeField]
    private Transform UIRoot;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="_host"></param>
    public void ShowHostList(List<UDPListener.UdpMessage> _host) {
        for(int i = 0 , max = _host.Count;i < max; i++) {
            //ボタン生成
            Button createdButton = Instantiate(selectHostButton, UIRoot);
            //生成したボタンにイベント登録
            createdButton.onClick.AddListener(() => OnHostButtonClicked(_host[i]));
            createdButton.GetComponentInChildren<TextMeshProUGUI>().text = _host[i].hostName;

        }
    }

    public void OnHostButtonClicked(UDPListener.UdpMessage _host) {
        selectedHost = _host;
        isSelected = true;
    }
}
