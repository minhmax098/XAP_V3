using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using EasyUI.Toast;

[RequireComponent(typeof(LoadingManager))]
[RequireComponent(typeof(PhotonConnectionManager))]
public class MeetingJoiningHandler : MonoBehaviourPunCallbacks
{
    public Button backBtn;
    public Button joinMeetingBtn;
    public Button resetMeetingCodeBtn;
    public InputField meetingCodeInputField;
    public GameObject inputMeetingCodeComponent;
    public GameObject meetingLobbyComponent;
    private int numberOfConnectionAttempt = 0;
    private int numberOfJoiningAttempt = 0;
    public Text lobbyMeetingCodeTxt;
    private bool isForcedDisconnected = false;
    void Start()
    {
        InitScreen();
        InitEvents();
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
        resetMeetingCodeBtn.onClick.AddListener(ResetMeetingCodeHandler);
        meetingCodeInputField.onValueChanged.AddListener(OnMeetingCodeChangedHandler);
        joinMeetingBtn.onClick.AddListener(JoinMeetingHandler);
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
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.NickName = PlayerPrefs.GetString("user_name");
        AllowJoinMeeting();
    }

    void DisallowJoinMeeting()
    {
        LoadingManager.Instance.ShowLoading(MeetingConfig.connecting);
    }

    void AllowJoinMeeting()
    {
        LoadingManager.Instance.HideLoading();
    }

    void OnMeetingCodeChangedHandler(string meetingCodeValue)
    {
        joinMeetingBtn.interactable = meetingCodeValue.Length == MeetingConfig.meetingCodeLength;
        resetMeetingCodeBtn.gameObject.SetActive(meetingCodeValue.Length > 0);
    }
    void BackScreenHandler()
    {
        isForcedDisconnected = true;
        StartCoroutine(PhotonConnectionManager.Instance.DisConnectPhotonServer(SceneConfig.home_user));
    }
    void ResetMeetingCodeHandler()
    {
        meetingCodeInputField.text = "";
    }

    void JoinMeetingHandler()
    {
        PhotonNetwork.JoinRoom(meetingCodeInputField.text);
        LoadingManager.Instance.ShowLoading(MeetingConfig.joinMeeting);
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LocalPlayer.MuteByHost(MeetingConfig.isMutedByHostDefault);
        PhotonNetwork.LocalPlayer.Mute();
        EnterLobbyMeeting();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        numberOfJoiningAttempt++;
        if (numberOfJoiningAttempt >= MeetingConfig.maxNumberOfJoiningAttempt)
        {
            numberOfJoiningAttempt = 0;
            LoadingManager.Instance.HideLoading();
            Toast.Show(MeetingConfig.invalidMeetingCodeMessage, MeetingConfig.longToastDuration);
        }
        else
        {
            JoinMeetingHandler();
        }
    }

    void EnterLobbyMeeting()
    {
        inputMeetingCodeComponent.SetActive(false);
        meetingLobbyComponent.SetActive(true);
        lobbyMeetingCodeTxt.text = lobbyMeetingCodeTxt.text + " " + meetingCodeInputField.text;
    }

    void InitMeetingJoining()
    {
        inputMeetingCodeComponent.SetActive(true);
        meetingLobbyComponent.SetActive(false);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        if (isForcedDisconnected)
        {
            return;
        }
        InitMeetingJoining();
        DisallowJoinMeeting();
        numberOfConnectionAttempt++;
        if (numberOfConnectionAttempt >= MeetingConfig.maxNumberOfConnecetionAttempt)
        {
            numberOfConnectionAttempt = 0;
            Toast.Show(MeetingConfig.errorConnectionMessage, MeetingConfig.toastDuration);
        }
        ConnectToMultiplayerServer();
    }
}