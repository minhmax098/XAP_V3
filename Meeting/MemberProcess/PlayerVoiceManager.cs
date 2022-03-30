using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerVoiceManager : MonoBehaviourPunCallbacks
{
    private Player player;
    public Image microImage;
    private Button btnMicro;

    // Start is called before the first frame update
    void Start()
    {
        InitUI();
        InitEvents();
    }

    void InitUI()
    {
        if (this.gameObject.GetComponent<Button>() == null)
        {
            this.gameObject.AddComponent<Button>();
        }
        btnMicro = this.gameObject.GetComponent<Button>();
    }

    public void SetPlayer(Player playerObject)
    {
        player = playerObject;
        UpdateMicroUI(player.IsMuted());
    }

    void InitEvents()
    {
        btnMicro.onClick.AddListener(ToggleMicro);
    }

    void ToggleMicro()
    {
        if (player == null)
        {
            return;
        }
        if ((player.IsLocal && !player.IsMutedByHost()) || PhotonNetwork.IsMasterClient)
        {
            bool isMutingMicro = player.IsMuted();
            if (isMutingMicro)
            {
                player.Unmute();
            }
            else
            {
                player.Mute();
            }
            MeetingMembersManager.Instance.SetPlayerVoice(!isMutingMicro, player);
        }
    }

    public void UpdateMicroUI(bool isMute)
    {
        if (isMute)
        {
            microImage.sprite = Resources.Load<Sprite>(PathConfig.MICRO_OFF_IMAGE);
        }
        else
        {
            microImage.sprite = Resources.Load<Sprite>(PathConfig.MICRO_ON_IMAGE);
        }
        if (PhotonNetwork.LocalPlayer == player)
        {
            MeetingMembersManager.Instance.UpdateUIForMyMicro(isMute);
        }
    }
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (targetPlayer != null && targetPlayer == player)
        {
            if (changedProps.ContainsKey(MeetingConfig.IS_MUTE_KEY))
            {
                UpdateMicroUI((bool)changedProps[MeetingConfig.IS_MUTE_KEY]);
            }
        }
    }
}
