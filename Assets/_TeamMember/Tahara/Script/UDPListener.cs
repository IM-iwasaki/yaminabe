using UnityEngine;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text;
using System.Collections.Concurrent;
using System;

public class UDPListener : MonoBehaviour {
    ConcurrentQueue<UdpMessage> messageQueue = new ConcurrentQueue<UdpMessage>();
    [System.Serializable]
    public class UdpMessage {
        public string ip;
        public int port;
        public string gameName;
        public string hostName;
    }
    // Start is called before the first frame update
    void Start() {
        Task.Run(ReceiveMessageFromBroadcaster);
    }

    // Update is called once per frame
    void Update() {
        Debug.Log("çXêVÇµÇƒÇ‹Ç∑");
        while (messageQueue.TryDequeue(out UdpMessage msg)) {
            TitleManager.instance.SetIPAddress(msg.ip);
            Debug.Log(TitleManager.instance.ipAddress);
        }
    }

    public async Task ReceiveMessageFromBroadcaster() {
        UdpClient udpClient = new UdpClient(9876);
        try {
            while (true) {
                UdpReceiveResult result = await udpClient.ReceiveAsync();
                string json = Encoding.UTF8.GetString(result.Buffer);
                UdpMessage message = JsonUtility.FromJson<UdpMessage>(json);

                messageQueue.Enqueue(message);

            }

        }
        catch (Exception error) {
            Debug.LogError(error.Message);
        }
        finally {
            udpClient.Close();
        }

    }
}
