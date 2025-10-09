using UnityEngine;

/// <summary>
/// ÉKÉ`ÉÉà¯Ç≠óp(âºÇ≈çÏÇ¡ÇΩÇæÇØ)
/// </summary>
public class DrowGacha : MonoBehaviour
{  
    public void OnClickSingleGacha() {
        GachaSystem gacha = FindObjectOfType<GachaSystem>();
        gacha.PullSingle();
    }
    public void OnClickMultipleGacha() {
        // âºÇ≈10òA
        GachaSystem gacha = FindObjectOfType<GachaSystem>();
        gacha.PullMultiple(10);
    }
}