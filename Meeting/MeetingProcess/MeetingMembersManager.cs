using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.Unity;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

[RequireComponent(typeof(VoiceConnection))]
[RequireComponent(typeof(PhotonConnectionManager))]
[RequireComponent(typeof(PhotonView))]
public class MeetingMembersManager : MonoBehaviourPunCallbacks
{
    public GameObject memberList;
    public Text meetingCode;
    public Text numberOfMember;
    public  Button btnToggleMeetingMember;
    public Animator toggleMeetingMemberAnimator;
    public Text presenterName;
    public Text presenterTitle;
    public PlayerVoiceManager presenterVoiceManager;
    public VoiceConnection voiceConnection;
    public Button btnUpdateAllMicroState;
    public Text txtMuteAll;
    public Image allMicroState;
    public PlayerVoiceManager yourVoiceManager;
    public Text txtMuteYou;
    public bool IsMutingAll {get; set; } = true;

    public GameObject hostControlPanel;
    public GameObject clientControlPanel;
    private static MeetingMembersManager instance;
    public static MeetingMembersManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<MeetingMembersManager>();
            }
            return instance;
        }
    }

    void Start()
    {
        InitMemberListPanel();
        InitEvents();
    }

    void Update()
    {
        UpdateControlUI();
        UpdateVoiceControl();
    }

    void UpdateVoiceControl()
    {
        if (PhotonConnectionManager.Instance.IsMutedAllPlayers())
        {
            UpdateUIForAllMicroStates(true);
        }
        else if (PhotonConnectionManager.Instance.IsActiveVoiceAllPlayers())
        {
            UpdateUIForAllMicroStates(false);
        }
    }

    void UpdateControlUI()
    {
        hostControlPanel.SetActive(PhotonNetwork.IsMasterClient);
        clientControlPanel.SetActive(!PhotonNetwork.IsMasterClient);
    }

    void InitEvents()
    {
        btnToggleMeetingMember.onClick.AddListener(ToggleMeetingMemberPanel);
        btnUpdateAllMicroState.onClick.AddListener(UpdateAllMicroStatesByHost);
    }

    void ToggleMeetingMemberPanel()
    {
        toggleMeetingMemberAnimator.SetBool(AnimatorConfig.isShowMeetingMemberList, !toggleMeetingMemberAnimator.GetBool(AnimatorConfig.isShowMeetingMemberList));
    }

    public void InitMemberListPanel()
    {
        meetingCode.text = PhotonNetwork.CurrentRoom.Name;
        numberOfMember.text = PhotonNetwork.PlayerList.Length.ToString();
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player.IsMasterClient)
            {
                SetPresenterInfo(player);
            }
            if (player.IsLocal)
            {
                SetYourVoiceManager(player);
            }
            SetPlayerInfo(player);
        }
    }

    void SetYourVoiceManager(Player player)
    {
        yourVoiceManager.SetPlayer(player);
    }

    void SetPresenterInfo(Player player)
    {
        presenterName.text = player.NickName;
        presenterTitle.text = MeetingConfig.organizerName + (player.IsLocal ? MeetingConfig.localPlayerTitle : "");
        presenterVoiceManager.SetPlayer(player);
    }

    void SetPlayerInfo(Player player)
    {
        GameObject playerResource = Instantiate(Resources.Load(PathConfig.MEETING_MEMBER_PATH) as GameObject);
        PlayerManager playerManager = playerResource.GetComponent<PlayerManager>() as PlayerManager;
        playerManager.SetPlayer(player);
        playerResource.transform.SetParent(memberList.transform);
        playerResource.transform.localScale = Vector3.one;
    }

    void RemovePlayerInfo(Player player)
    {   
        foreach (Transform memberTransform in memberList.transform)
        {
            if (memberTransform.GetComponent<PlayerManager>().GetPlayer().ActorNumber == player.ActorNumber)
            {
                Destroy(memberTransform.gameObject);
                return;
            }
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        SetPlayerInfo(newPlayer);
        numberOfMember.text = PhotonConnectionManager.Instance.GetActivePlayers().Length.ToString();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        RemovePlayerInfo(otherPlayer);
        numberOfMember.text = PhotonConnectionManager.Instance.GetActivePlayers().Length.ToString();
    }

    public void SetPlayerVoice(bool isMute, Player targetPlayer)
    {
        if (targetPlayer == PhotonNetwork.LocalPlayer)
        {
            SetPlayerMicroState(isMute);
        }
        else if (PhotonNetwork.IsMasterClient)
        {
            PhotonConnectionManager.Instance.AssignPlayerMicroStateByHost(targetPlayer, isMute);
        }
    }

    void UpdateAllMicroStatesByHost()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            IsMutingAll = !IsMutingAll;
            PhotonConnectionManager.Instance.SetActionStatusValue(MeetingConfig.IS_MUTING_ALL_KEY, IsMutingAll);
            PhotonConnectionManager.Instance.ForceUpdateMicroStateByHost(IsMutingAll);
        }
    }

    public void UpdateUIForAllMicroStates(bool currentMicroState)
    {
        IsMutingAll = currentMicroState;
        if (IsMutingAll)
        {
            txtMuteAll.text = MeetingConfig.unmuteAll;
            allMicroState.sprite = Resources.Load<Sprite>(PathConfig.MICRO_OFF_IMAGE);
        }
        else
        {
            txtMuteAll.text = MeetingConfig.muteAll;
            allMicroState.sprite = Resources.Load<Sprite>(PathConfig.MICRO_ON_IMAGE);
        }
    }

    [PunRPC]
    public void SetPlayerMicroStateByHost(bool isMute)
    {
        SetPlayerMicroState(isMute);
        PhotonNetwork.LocalPlayer.MuteByHost(isMute);
    }

    [PunRPC]
    public void SetPlayerMicroState(bool isMute)
    {
        if (isMute)
        {
            PhotonNetwork.LocalPlayer.Mute();
        }
        else
        {
            PhotonNetwork.LocalPlayer.Unmute();
        }
        voiceConnection.PrimaryRecorder.IsRecording = !isMute;
        voiceConnection.PrimaryRecorder.TransmitEnabled = !isMute;
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        base.OnRoomPropertiesUpdate(propertiesThatChanged);
        if (propertiesThatChanged.ContainsKey(MeetingConfig.IS_MUTING_ALL_KEY))
        {
            UpdateUIForAllMicroStates((bool)propertiesThatChanged[MeetingConfig.IS_MUTING_ALL_KEY]);
        }
    }

    public void UpdateUIForMyMicro(bool isMute)
    {
        txtMuteYou.text = isMute ? MeetingConfig.unmuteYou : MeetingConfig.muteYou;
    }
}
