using UnityEngine;
using System.Collections.Generic;

public class JsonToBinaryConverter : MonoBehaviour {
    [TextArea(4, 20)]
    public string jsonInput;

    [ContextMenu("Convert JSON to Binary")]
    public void Convert() {
        // JSON → PlayerData に変換
        PlayerData data = JsonUtility.FromJson<PlayerData>(jsonInput);

        // items が null の可能性に備える
        if (data.items == null)
            data.items = new List<string>();

        // バイナリとして保存
        PlayerSaveData.Save(data);

        Debug.Log("JSON からバイナリへの変換が完了しました！");
        Debug.Log("保存先: " + PlayerSaveData.filePath);
    }
}