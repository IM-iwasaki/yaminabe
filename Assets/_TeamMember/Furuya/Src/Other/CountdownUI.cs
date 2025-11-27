using Mirror;
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

    void Start() {
        // メッセージ受信登録（クライアント）
        NetworkClient.RegisterHandler<CountdownMessage>(OnCountdownMessage);
    }

    void OnCountdownMessage(CountdownMessage msg) {
        Debug.Log("[Client] Countdown received: " + msg.seconds);
        StartCoroutine(CountdownCoroutine(msg.seconds));
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
