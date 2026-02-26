using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingUI : MonoBehaviour
{
    [SerializeField]
    private Slider loadingUI;
    [SerializeField]
    private TextMeshProUGUI loadingPersent;
    // Start is called before the first frame update
    public IEnumerator LoadingCorutine() {
        loadingUI.value = 0.0f;
        while (loadingUI.value < 100) {
            loadingUI.value += 0.033f;
            loadingPersent.text = loadingUI.value.ToString("F1") + "%";
            yield return null;
        }
        loadingPersent.text = "100%";
    }
}
