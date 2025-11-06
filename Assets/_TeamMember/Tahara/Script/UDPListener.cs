using UnityEngine;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text;
using System.Collections.Concurrent;
using System;
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
    // Start is called before the first frame update
    void Start() {
        StartCoroutine(ReceiveMessageFromBroadcaster());
    }

    // Update is called once per frame
    void Update() {
        if (!TitleManager.instance) return;

        if (messageQueue.TryDequeue(out UdpMessage msg)) {

            TitleManager.instance.ipAddress = msg.ip;
        }
    }

    /// <summary>
    /// IPアドレスの定期受信
    /// </summary>
    /// <returns></returns>
    public IEnumerator ReceiveMessageFromBroadcaster() {
        UdpClient udpClient = new UdpClient(9876);
        while (true) {
            if (udpClient.Available > 0) {
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                byte[] result = udpClient.Receive(ref remoteEP);
                string json = Encoding.UTF8.GetString(result);
                UdpMessage message = JsonUtility.FromJson<UdpMessage>(json);

                messageQueue.Enqueue(message);

            }

            yield return null;
        }

    }
}
