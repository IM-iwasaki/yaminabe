using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TeammateUI : NetworkBehaviour { 

    [SerializeField]
    private Image weaponIcon = null;
    [SerializeField]
    private TextMeshProUGUI nameText = null;
    public void Initialize(NetworkIdentity _player) {
        if (!_player.isLocalPlayer) return;
        nameText.text = _player.gameObject.name;
        Color teamColor = Color.white;
        switch (_player.GetComponent<DemoPlayer>().TeamID) {
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
