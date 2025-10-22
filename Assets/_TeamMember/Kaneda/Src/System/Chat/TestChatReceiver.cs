using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestChatReceiver : MonoBehaviour
{
    [SerializeField] private StampData stampData;

    private int randStamp = 0;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow)) {
            randStamp = Random.Range(0,stampData.stampInfos.Count);
            ChatManager.instance.CmdSendStamp(randStamp, "Player");
        }

        if (Input.GetKeyDown(KeyCode.DownArrow)) {
            ChatManager.instance.CmdSendSystemMessage("System Test");
        }
    }
}
