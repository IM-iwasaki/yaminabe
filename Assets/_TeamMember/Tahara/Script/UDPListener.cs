using UnityEngine;
using System.Net.Sockets;
using System.Text;
using System.Collections.Concurrent;
using System.Collections;
using System.Net;

public class UDPListener : MonoBehaviour {
    ConcurrentQueue<UdpMessage> messageQueue = new ConcurrentQueue<UdpMessage>();
    [System.Serializable]
    public struct UdpMessage {
        public string ip;
        public int port;
        public string gameName;
        public string hostName;
    }

    public bool isGetIP = false;

    // Update is called once per frame
    void Update() {
        if (!TitleManager.instance) return;

        if (messageQueue.TryDequeue(out UdpMessage msg)) {
            TitleManager.instance.ipAddress = msg.ip;
            isGetIP = true;
        }
    }

    public void StartReceiveIP() {
        StartCoroutine(ReceiveMessageFromBroadcaster());
    }

    /// <summary>
    /// IPアドレスの定期受信
    /// </summary>
    /// <returns></returns>
    public IEnumerator ReceiveMessageFromBroadcaster() {
        IPEndPoint localEP = new IPEndPoint(IPAddress.Any, 9876);
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.SetSocketOption(SocketOptionLevel.Socket,SocketOptionName.ReuseAddress,true);
        socket.Bind(localEP);

        UdpClient udpClient = new UdpClient();
        udpClient.Client = socket;
        while (true) {
            if (udpClient.Available > 0) {
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                byte[] result = udpClient.Receive(ref remoteEP);
                string json = Encoding.UTF8.GetString(result);
                UdpMessage message = JsonUtility.FromJson<UdpMessage>(json);
                //キューに追加
                messageQueue.Enqueue(message);   
            }
            yield return null;
        }

    }
}
