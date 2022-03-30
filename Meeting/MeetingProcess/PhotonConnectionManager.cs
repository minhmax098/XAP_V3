using System;
using System.Collections;
using EasyUI.Toast;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using Hashtable = ExitGames.Client.Photon.Hashtable;

[RequireComponent(typeof(PhotonView))]
public class PhotonConnectionManager : MonoBehaviourPunCallbacks, IInRoomCallbacks
{
    private static PhotonConnectionManager instance;
    public static PhotonConnectionManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<PhotonConnectionManager>();
            }
            return instance;
        }
    }

    private PhotonView PV;

    void Start()
    {
        InitObjects();
    }

    void InitObjects()
    {
        PV = GetComponent<PhotonView>();
    }
    public void ConnectToMaster()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public IEnumerator DisConnectPhotonServer(string nextScene)
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
            while (PhotonNetwork.IsConnected)
            {
                yield return null;
            }
        }
        SceneManager.LoadScene(nextScene);
    }

    
    public object GetActionStatusValue(string key)
    {
        return PhotonNetwork.CurrentRoom.CustomProperties[key];
    }

    public void SetActionStatusValue(string key, object value)
    {
        Hashtable newProperties = new Hashtable() { { key, value } };
        PhotonNetwork.CurrentRoom.SetCustomProperties(newProperties, null, null);
    }

    public void OnHostLeftMeeting()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            CleanMeetingRoom();
        }
        MeetingExperienceManager.Instance.FinishMeetingByHost();
    }

    void CleanMeetingRoom()
    {
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.EmptyRoomTtl = 0;
        PhotonNetwork.CurrentRoom.PlayerTtl = 0;
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (otherPlayer.ActorNumber == (int)GetActionStatusValue(MeetingConfig.HOST_ACTOR_NUMBER_KEY))
        {
            OnHostLeftMeeting();
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        if (cause == DisconnectCause.ClientTimeout)
        {
            StartCoroutine(MeetingExperienceManager.Instance.OnLostConnection());
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        base.OnMasterClientSwitched(newMasterClient);
    }

    public void AssignPlayerMicroStateByHost(Player targetPlayer, bool isMute)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PV.RPC("SetPlayerMicroStateByHost", targetPlayer, isMute);
        }
    }

    public void ForceUpdateMicroStateByHost(bool isMute)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PV.RPC("SetPlayerMicroStateByHost", RpcTarget.All, isMute);
        }
    }

    public void SyncChildObjectSelection(string objectName)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PV.RPC("InitNewChildObject", RpcTarget.All, objectName);
        }
    }

    public void SyncProcess(string processName, bool newStatus, RpcTarget target = RpcTarget.All)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PV.RPC(processName, target, newStatus);
        }
    }

    public void SyncEmptyNodeTreeProcess()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PV.RPC("SyncEmptyNodeTree", RpcTarget.All);
        }
    }

    public Player[] GetActivePlayers()
    {
        Player[] activePlayers = Array.FindAll(PhotonNetwork.PlayerList, player => player.IsInactive == false);
        return activePlayers;
    }

    public bool IsMutedAllPlayers()
    {
        foreach (Player player in GetActivePlayers())
        {   
            if (player.IsMuted() == false)
            {
                return false;
            }
        }
        return true;
    }

    public bool IsActiveVoiceAllPlayers()
    {
        foreach (Player player in GetActivePlayers())
        {   
            if (player.IsMuted() == true)
            {
                return false;
            }
        }
        return true;
    }
}