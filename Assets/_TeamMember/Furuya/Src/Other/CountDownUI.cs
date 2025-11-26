using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ゲーム開始前のカウントダウン用
/// </summary>
public class CountdownUI : MonoBehaviour {
    public static CountdownUI Instance { get; private set; }

    [SerializeField] private Image countdownImage;  // 表示用Image
    [SerializeField] private Sprite[] numberSprites; // 0〜9までの数字スプライト
    [SerializeField] private Sprite goSprite;        // GO!用スプライト
    [SerializeField] private GameObject uiRoot;

    private void Awake() {
        Instance = this;
        uiRoot.SetActive(false);
    }

    /// <summary>
    /// カウントダウン開始
    /// </summary>
    public void StartCountdown(int seconds) {
        StopAllCoroutines();
        StartCoroutine(CountdownCoroutine(seconds));
    }

    private IEnumerator CountdownCoroutine(int seconds) {
        uiRoot.SetActive(true);

        int remaining = seconds;
        while (remaining > 0) {
            if (remaining - 1 < numberSprites.Length)
                countdownImage.sprite = numberSprites[remaining - 1]; // 配列は0始まり
            else
                countdownImage.sprite = numberSprites[0]; // 念のため

            yield return new WaitForSeconds(1f);
            remaining--;
        }

        // GO!表示
        countdownImage.sprite = goSprite;
        yield return new WaitForSeconds(1f);

        uiRoot.SetActive(false);
    }
}
