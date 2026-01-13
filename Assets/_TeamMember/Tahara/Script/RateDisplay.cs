using TMPro;
using UnityEngine;

public class RateDisplay : MonoBehaviour
{
    public static RateDisplay instance;

    [SerializeField]
    private TextMeshPro rateValueText;
    private PlayerData data;
    private void Awake() {
        Initialize();
        ChangeRateUI();
    }

    private void Initialize() {
        instance = this;
        data = PlayerSaveData.Load();
    }

    public void ChangeRateUI() {
        rateValueText.text = data.currentRate.ToString();
    }
}
