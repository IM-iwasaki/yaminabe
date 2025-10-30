using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EffectData", menuName = "Effect/EffectData")]
public class EffectData : ScriptableObject 
{
    [Header("登録されているエフェクト一覧")]
    public List<EffectInfo> effectInfos = new();

    [System.Serializable]
    public class EffectInfo {
        [Header("エフェクト名")]
        public string name;
        [Header("エフェクトプレハブ")]
        public GameObject effect;
    }

}
