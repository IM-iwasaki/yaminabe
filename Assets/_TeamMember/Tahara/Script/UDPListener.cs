using UnityEngine;
using System.Net.Sockets;
using System.Text;
using System.Collections.Concurrent;
using System.Collections;
using System.Net;
/// <summary>
/// IPアドレスを定期的に受信する
/// </summary>
public class UDPListener : MonoBehaviour {
    /// <summary>
    /// 取り出せた時にだけ処理できる安全なキュー
    /// </summary>
    ConcurrentQueue<UdpMessage> messageQueue = new ConcurrentQueue<UdpMessage>();
    /// <summary>
    /// 受信するメッセージ
    /// </summary>
    [System.Serializable]
    public struct UdpMessage {
        public string ip;
        public int port;
        public string gameName;
        public string hostName;
    }
    /// <summary>
    /// タイトルシーンでIPアドレスが取得できたかどうかを判定する用変数
    /// </summary>
    public bool isGetIP = false;

    // Update is called once per frame
    void Update() {
        if (!TitleManager.instance) return;

        if (messageQueue.TryDequeue(out UdpMessage msg)) {
            TitleManager.instance.ipAddress = msg.ip;
            isGetIP = true;
        }
    }

    /// <summary>
    /// IPアドレスの受信を開始する
    /// </summary>
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
