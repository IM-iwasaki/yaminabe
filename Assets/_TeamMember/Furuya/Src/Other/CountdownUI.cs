using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CountdownUI : MonoBehaviour {
    public static CountdownUI Instance { get; private set; }

    [SerializeField] private Image countdownImage;
    [SerializeField] private Sprite[] numberSprites; // 3,2,1
    [SerializeField] private Sprite goSprite;       // GO!
    [SerializeField] private GameObject uiRoot;

    private void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        uiRoot.SetActive(false);
    }

    public IEnumerator CountdownCoroutine(int seconds) {
        uiRoot.SetActive(true);

        int remaining = seconds;
        while (remaining > 0) {
            Debug.Log("[UI] remaining=" + remaining + " sprite=" + (countdownImage.sprite ? countdownImage.sprite.name : "null"));

            countdownImage.sprite = numberSprites[Mathf.Clamp(remaining - 1, 0, numberSprites.Length - 1)];
            yield return new WaitForSeconds(1f);
            remaining--;
        }

        countdownImage.sprite = goSprite;
        yield return new WaitForSeconds(1f);

        uiRoot.SetActive(false);
    }
}
