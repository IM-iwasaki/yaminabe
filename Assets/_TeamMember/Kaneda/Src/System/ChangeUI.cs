using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIInputIconChanger : MonoBehaviour {
    [SerializeField] CharacterInput input;
    [SerializeField] Image targetImage;

    [SerializeField] Sprite keyboardSprite;
    [SerializeField] Sprite gamepadSprite;

    void Start() {
        foreach (var ci in FindObjectsOfType<CharacterInput>()) {
            if (ci.isLocalPlayer) {
                input = ci;
                break;
            }
        }

        if (input != null) {
            input.OnControlSchemeChanged += ChangeIcon;
            ChangeIcon(input.isGamepad);
        }
    }

    void Update() {
        // まだ取得できていない場合だけ探す
        if (input == null) {
            foreach (var ci in FindObjectsOfType<CharacterInput>()) {
                if (ci.isLocalPlayer) {
                    input = ci;
                    input.OnControlSchemeChanged += ChangeIcon;

                    ChangeIcon(input.isGamepad);
                    break;
                }
            }
        }
    }

    void ChangeIcon(bool isGamepad) {
        targetImage.sprite = isGamepad ? gamepadSprite : keyboardSprite;
    }
}