using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TeammateUI : NetworkBehaviour { 

    //武器のアイコン
    [SerializeField]
    private Image weaponIcon = null;
    //プレイヤーの名前用UI
    [SerializeField]
    private TextMeshProUGUI nameText = null;

    /// <summary>
    /// 初期化関数
    /// </summary>
    /// <param name="_player"></param>
    public void Initialize(NetworkIdentity _player) {
        if (!_player.isLocalPlayer) return;
        nameText.text = _player.gameObject.name;
        Color teamColor = Color.white;
        switch (_player.GetComponent<GeneralCharacter>().TeamID) {
            case 0:
                teamColor = Color.red;
                break;
            case 1:
                teamColor = Color.blue;
                break;
            default:
                break;
        }
        weaponIcon.color = teamColor;
    }
}
