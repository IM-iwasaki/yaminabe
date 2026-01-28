using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HostButton : MonoBehaviour {
    public UDPListener.UdpMessage hostData;
    public TextMeshProUGUI label;
    public Button button;

    public void Setup(UDPListener.UdpMessage data, System.Action<UDPListener.UdpMessage> onClick) {
        hostData = data;
        label.text = data.hostName;

        RefreshColor();

        button.onClick.AddListener(() => onClick(hostData));
    }

    public void RefreshUI() {
        label.text = hostData.hostName;
        RefreshColor();
    }

    private void RefreshColor() {
        var colors = button.colors;
        colors.normalColor = hostData.gamePlaying ? Color.red : Color.white;
        button.colors = colors;
    }
}
