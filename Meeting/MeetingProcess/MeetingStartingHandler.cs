using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using EasyUI.Toast;
using UnityEngine.SceneManagement;
using Hashtable = ExitGames.Client.Photon.Hashtable;

[RequireComponent(typeof(LoadingManager))]
[RequireComponent(typeof(PhotonConnectionManager))]
public class MeetingStartingHandler : MonoBehaviourPunCallbacks
{
    public Button backBtn;
    public Button copyMeetingCodeBtn;
    public Button launchMeetingBtn;
    public Text meetingCodeTxt;
    private int numberOfConnectionAttempt = 0;
    private int numberOfStartingAttempt = 0;
    private bool isForcedDisconnected = false;
    void Start()
    {
        InitScreen();
        InitEvents();
        GenerateMeetingCode();
        ConnectToMultiplayerServer();
    }
    // Update is called once per frame
    void Update()
    {
    }
    void InitScreen()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        StatusBarManager.statusBarState = StatusBarManager.States.TranslucentOverContent;
        StatusBarManager.navigationBarState = StatusBarManager.States.Hidden;
    }
    void InitEvents()
    {
        backBtn.onClick.AddListener(BackScreenHandler);
        copyMeetingCodeBtn.onClick.AddListener(CopyMeetingCodeHandler);
        launchMeetingBtn.onClick.AddListener(StartMeetingRoom);
    }
    void BackScreenHandler()
    {
        isForcedDisconnected = true;
        StartCoroutine(PhotonConnectionManager.Instance.DisConnectPhotonServer(SceneConfig.lesson));
    }
    void CopyMeetingCodeHandler()
    {
        Helper.CopyToClipboard(meetingCodeTxt.text);
        Toast.Show(MeetingConfig.successCopyMeetingCodeMessage, 2f);
    }
    void GenerateMeetingCode()
    {
        meetingCodeTxt.text = GetMeetingCodeFromServer();
    }
    string GetMeetingCodeFromServer()
    {
        // Neet call API
        return (Random.Range(100000000, 999999999)).ToString();
    }
    void ConnectToMultiplayerServer()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.AutomaticallySyncScene = false;
        PhotonNetwork.NickName = PlayerPrefs.GetString("user_name");
        CreateMeetingRoom();
    }
    void CreateMeetingRoom()
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.PlayerTtl = MeetingConfig.meetingPlayerTTL;
        roomOptions.EmptyRoomTtl = MeetingConfig.meetingRoomEmptyTTL;
        PhotonNetwork.JoinOrCreateRoom(meetingCodeTxt.text, roomOptions, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        InitRoomProperties();
        AllowStartMeeting();
        PhotonNetwork.LocalPlayer.SetOrganizer();
        PhotonNetwork.LocalPlayer.MuteByHost(MeetingConfig.isMutedByHostDefault);
        PhotonNetwork.LocalPlayer.Mute();
    }

    void InitRoomProperties()
    {
        Hashtable roomProperties = new Hashtable();
        roomProperties.Add(MeetingConfig.IS_MAKING_XRAY_KEY, false);
        roomProperties.Add(MeetingConfig.IS_SEPARATING_KEY, false);
        roomProperties.Add(MeetingConfig.IS_SHOWING_LABEL_KEY, false);
        roomProperties.Add(MeetingConfig.IS_CLICKED_HOLD_KEY, false);
        roomProperties.Add(MeetingConfig.IS_CLICKED_GUIDE_BOARD_KEY, false);
        roomProperties.Add(MeetingConfig.HOST_ACTOR_NUMBER_KEY, PhotonNetwork.LocalPlayer.ActorNumber);
        roomProperties.Add(MeetingConfig.IS_MUTING_ALL_KEY, true);
        roomProperties.Add(MeetingConfig.CURRENT_OBJECT_NAME_KEY, "");
        roomProperties.Add(MeetingConfig.EXPERIENCE_MODE_KEY, ModeManager.MODE_EXPERIENCE.MODE_3D);
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties, null, null);
    }

    void AllowStartMeeting()
    {
        launchMeetingBtn.interactable = true;
        LoadingManager.Instance.HideLoading();
    }

    void DisallowStartMeeting()
    {
        launchMeetingBtn.interactable = false;
        LoadingManager.Instance.ShowLoading(MeetingConfig.connecting);
    }
    void StartMeetingRoom()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        LoadingManager.Instance.ShowLoading(MeetingConfig.startingMeeting);
        PhotonNetwork.LoadLevel(SceneConfig.meetingExperience);
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        if (isForcedDisconnected)
        {
            return;
        }
        DisallowStartMeeting();
        numberOfConnectionAttempt++;
        if (numberOfConnectionAttempt >= MeetingConfig.maxNumberOfConnecetionAttempt)
        {
            numberOfConnectionAttempt = 0;
            Toast.Show(MeetingConfig.errorConnectionMessage, MeetingConfig.toastDuration);
        }
        ConnectToMultiplayerServer();
    }
}

