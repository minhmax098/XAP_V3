using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviourPunCallbacks
{
    private Player player;
    public Text playerName;
    public PlayerVoiceManager playerVoiceManager;

    public void SetPlayer(Player playerObject)
    {
        player = playerObject;
        playerVoiceManager.SetPlayer(playerObject);
        InitPlayer();
    }
    private void InitPlayer()
    {
        SetPlayerName();
    }

    public Player GetPlayer()
    {
        return player;
    }

    private void SetPlayerName()
    {
        if (player == null)
        {
            return;
        }
        string name = player.NickName;
        int acceptedLength = MeetingConfig.maxLengthOfPlayerName;
        if (player.IsLocal) 
        {
            acceptedLength = acceptedLength -  MeetingConfig.localPlayerTitleLength;
        }
        else if (player.IsOrganizer())
        {
            acceptedLength = acceptedLength - MeetingConfig.organizerNameLength;
        }
        if (name.Length > acceptedLength)
        {
            name = name.Substring(0, acceptedLength - MeetingConfig.dotLength) + MeetingConfig.dot;
        }
        if (player.IsLocal) 
        {
            name += MeetingConfig.localPlayerTitle;
        }
        else if (player.IsOrganizer())
        {
            name += MeetingConfig.organizerName;
        }
        playerName.text = name;
    }
}
