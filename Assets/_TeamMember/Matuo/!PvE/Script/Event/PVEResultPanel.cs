using UnityEngine;
using Mirror;
using TMPro;

public class PVEResultPanel : MonoBehaviour {




    [SerializeField] private TextMeshProUGUI pvewinnerText;

    // 演出設定
    [SerializeField] private float textDelay = 0.4f;     // 表示遅延
    [SerializeField] private float popTime = 0.35f;      // ドン時間
    [SerializeField] private float pulseAmount = 0.05f;  // 鼓動幅
    [SerializeField] private float pulseSpeed = 2f;      // 鼓動速度
    [SerializeField] private float delay = 0.3f;      // 表示遅延
    [SerializeField] private float animTime = 0.5f;   // アニメ時間

    private void Start() {
        if (pvewinnerText != null) {
            // 初期状態（小さく＆回転）
            pvewinnerText.transform.localScale = Vector3.zero;
            pvewinnerText.transform.rotation = Quaternion.Euler(0, 0, -360f);

            StartCoroutine(RotatePopAnimation());
        
        }
    }



    private System.Collections.IEnumerator RotatePopAnimation() {
        yield return new WaitForSeconds(delay);

        float time = 0f;

        while (time < animTime) {
            time += Time.deltaTime;
            float t = time / animTime;

            // イージング（減速）
            t = Mathf.Sin(t * Mathf.PI * 0.5f);

            // スケール 0 → 1.3
            float scale = Mathf.Lerp(0f, 1.3f, t);
            pvewinnerText.transform.localScale = new Vector3(scale, scale, 1f);

            // 回転 -360 → 0
            float angle = Mathf.Lerp(-360f, 0f, t);
            pvewinnerText.transform.rotation = Quaternion.Euler(0, 0, angle);

            yield return null;
        }

        // ===== 少し跳ねる（ドン感） =====
        pvewinnerText.transform.localScale = Vector3.one * 0.9f;
        yield return new WaitForSeconds(0.05f);
        pvewinnerText.transform.localScale = Vector3.one;

        // 回転を完全に戻す
        pvewinnerText.transform.rotation = Quaternion.identity;

        // ===== 鼓動開始 =====
        StartCoroutine(TextPulse());

    }


    

    // 鼓動アニメーション
    private System.Collections.IEnumerator TextPulse() {
        while (true) {
            float scale = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
            pvewinnerText.transform.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }
    }




    public void OnClickNextStage() {
        if (!NetworkServer.active) return;

        RuleManager.Instance?.Initialize();
        GameManager.Instance.EndGame();

        // 次のステージへ
        GameSceneManager.Instance.LoadPvESceneForAll();
        PlayerListManager.Instance.ResetAllScores();
    }

    public void OnClickReturnLobby() {
        if (!NetworkServer.active) return;

        RuleManager.Instance?.Initialize();
        GameManager.Instance.EndGame();
        //プレイヤーの状態を戻す
        ServerManager.instance.ResetCharacterStatus();
        GameSceneManager.Instance.LoadLobbySceneForAll();
        PlayerListManager.Instance.ResetAllScores();
    }
}