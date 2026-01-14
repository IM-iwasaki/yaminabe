using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HostSelectUI : MonoBehaviour
{
    public TextMeshPro selectHostButton;
    public bool isSelected = false;
    public UDPListener.UdpMessage selectedHost;
    [SerializeField]
    private Transform UIRoot;

    public void ShowHostList(List<UDPListener.UdpMessage> _host) {
        for(int i = 0 , max = _host.Count;i < max; i++) {
            //ƒ{ƒ^ƒ“¶¬
            Instantiate(selectHostButton, UIRoot);
        }
    }

    public void OnHostButtonClicked(UDPListener.UdpMessage _host) {
        selectedHost = _host;
        isSelected = true;
    }
}
