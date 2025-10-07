using UnityEngine;

public class DrowGacha : MonoBehaviour
{
    public void OnClickGacha() {
        GachaSystem gacha = FindObjectOfType<GachaSystem>();
        gacha.PullSingle();
    }
}