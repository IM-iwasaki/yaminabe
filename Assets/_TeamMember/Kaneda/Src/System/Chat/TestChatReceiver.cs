using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestChatReceiver : MonoBehaviour
{
    [SerializeField] Sprite[] sprites = null;

    private int randStamp = 0;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow)) {
            randStamp = Random.Range(0,sprites.Length);
            ChatManager.instance.AddStamp(sprites[randStamp], "Player");
        }

        if (Input.GetKeyDown(KeyCode.DownArrow)) {
            ChatManager.instance.AddSystemMessage("System Test");
        }
    }
}
