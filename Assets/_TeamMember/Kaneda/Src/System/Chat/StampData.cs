using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StampData", menuName = "ScriptableObject/Stamp/StampData")]
public class StampData : ScriptableObject
{
    [Header("登録されているスタンプ一覧")]
    public List<StampInfo> stampInfos = new();

    [System.Serializable]
    public class StampInfo {
        [Header("スタンプ名")]
        public string stampName;
        [Header("スタンプ画像")]
        public Sprite stampImage;
    }
}
