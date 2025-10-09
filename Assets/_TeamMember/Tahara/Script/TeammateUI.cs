using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TeammateUI : NetworkBehaviour { 

    [SerializeField]
    private Image weaponIcon = null;
    [SerializeField]
    private TextMeshProUGUI nameText = null;
    public void Initialize(NetworkIdentity _player) {
        if (!isLocalPlayer) return;
        nameText.text = _player.gameObject.GetComponent<CharacterBase>().name;
    }
}
