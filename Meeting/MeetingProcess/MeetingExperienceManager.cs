using System.Linq.Expressions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using EasyUI.Toast;
using Photon.Realtime;
using UnityEngine.XR.ARFoundation;
using Hashtable = ExitGames.Client.Photon.Hashtable;


[RequireComponent(typeof(InteractionSetupForMember))]
[RequireComponent(typeof(PhotonConnectionManager))]
[RequireComponent(typeof(TouchManager))]
[RequireComponent(typeof(XRayManager))]
[RequireComponent(typeof(SeparateManager))]
[RequireComponent(typeof(LabelManager))]
[RequireComponent(typeof(PopupManager))]
[RequireComponent(typeof(MeetingMembersManager))]
public class MeetingExperienceManager : MonoBehaviourPunCallbacks, IInRoomCallbacks
{
    public GameObject interactionBtnsGroup;
    public Button btnSeparate;
    public Button btnLabel;
    public Button btnXray;
    public Button btnHold;
    public Button btnShowPopupExit;
    public Button btnClosePopupExit;
    public Button btnExitLesson;
    public Button btnCancelExitLesson;
    public Button btnShowGuideBoard;
    public Button btnExitGuideBoard;
    public Button btnAudio;
    public Button btnMenu;
    public Button btnStartTree;
    private bool isForcedDisconnected = false;
    private bool isFirstTimeJoining = true;
    
    public Button btnAR;
    public Button btnSwitch;
    public Button btnBacktoMode3D;

    private static MeetingExperienceManager instance;
    public static MeetingExperienceManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<MeetingExperienceManager>();
            }
            return instance;
        }
    }

    /**
        -------- REGISTER EVENTS --------
    */

    void Awake()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            SceneNameManager.setPrevScene(SceneConfig.lesson);
        }
        else
        {
            SceneNameManager.setPrevScene(SceneConfig.meetingJoining);
        }
    }
    public override void OnEnable()
    {
        base.OnEnable();
        MeetingObjectInstantiateObserve.onMeetingObjectInstantiate += OnMeetingObjectInstantiate;
        TouchManager.onSelectChildObject += OnSelectChildObject;
        TreeNodeManager.onClickNodeTree += OnClickNodeTree;
        ObjectManager.onChangeCurrentObject += OnChangeCurrentObject;
        ObjectManager.onResetObject += ResetMeetingObject;
        ARUIManager.OnARPlaceObject += OnARPlaceObject;
        MeetingCloudAnchorManager.onHostCloudAnchorFailed += OnHostCloudAnchorFailed;
        MeetingCloudAnchorManager.onResolveCloudAnchorFailed += OnResolveCloudAnchorFailed;
    }
    public override void OnDisable()
    {
        base.OnDisable();
        MeetingObjectInstantiateObserve.onMeetingObjectInstantiate -= OnMeetingObjectInstantiate;
        TouchManager.onSelectChildObject -= OnSelectChildObject;
        TreeNodeManager.onClickNodeTree -= OnClickNodeTree;
        ObjectManager.onChangeCurrentObject -= OnChangeCurrentObject;
        ObjectManager.onResetObject -= ResetMeetingObject;
        ARUIManager.OnARPlaceObject -= OnARPlaceObject;
        MeetingCloudAnchorManager.onHostCloudAnchorFailed -= OnHostCloudAnchorFailed;
        MeetingCloudAnchorManager.onResolveCloudAnchorFailed -= OnResolveCloudAnchorFailed;
    }

    /**
        -------- END REGISTER EVENTS --------
    */


    void Start()
    {
        SetupMode();
        InitLayoutScreen();
        InstantiateMeetingObject();
        InitEvents();
    }
    
    void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            TouchManager.Instance.HandleTouchInteraction();
        }
        SetupUIForMembers(PhotonNetwork.IsMasterClient);
        EnableFeature();
    }

    void SetupMode()
    {
        ModeManager.MODE_EXPERIENCE currentMode = (ModeManager.MODE_EXPERIENCE)PhotonConnectionManager.Instance.GetActionStatusValue(MeetingConfig.EXPERIENCE_MODE_KEY);
        ModeManager.Instance.CheckModeInitToView(currentMode);
    }
    void InitLayoutScreen()
    {
        Screen.orientation = ScreenOrientation.Landscape;
        StatusBarManager.statusBarState = StatusBarManager.States.Hidden;
        StatusBarManager.navigationBarState = StatusBarManager.States.Hidden;
    }
    
    void InstantiateMeetingObject()
    {
        ObjectManager.Instance.LoadDataOrgan();
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.InstantiateRoomObject(PathConfig.PRE_PATH_MODEL + ObjectManager.Instance.OriginOrganData.Name, Vector3.zero, Quaternion.identity);
        }
    }

    void EnableFeature()
    {
        if (ObjectManager.Instance.CurrentObject != null)
        {
            if (ObjectManager.Instance.CurrentObject.transform.childCount == 0)
            {
                btnHold.interactable = false;
                btnLabel.interactable = false;
                btnSeparate.interactable = false;
            }
            else
            {
                btnHold.interactable = true;
                btnLabel.interactable = true;
                btnSeparate.interactable = true;
            }

            if (ObjectManager.Instance.CurrentObject.GetComponent<AudioSource>() == null)
            {
                btnAudio.interactable = false;
            }
            else
            {
                btnAudio.interactable = true;
            }

            if (ModeManager.Instance.Mode == ModeManager.MODE_EXPERIENCE.MODE_AR)
            {
                btnAR.gameObject.SetActive(false);
                btnSwitch.gameObject.SetActive(true);
            }
            else if(ModeManager.Instance.Mode == ModeManager.MODE_EXPERIENCE.MODE_3D)
            {
                btnAR.gameObject.SetActive(true);
                btnSwitch.gameObject.SetActive(false);
            }   
        }
    }
    void InitEvents()
    {
        btnExitLesson.onClick.AddListener(ExitLesson);
        btnShowPopupExit.onClick.AddListener(ToggleExitConfirmationPopup);
        btnClosePopupExit.onClick.AddListener(ToggleExitConfirmationPopup);
        btnCancelExitLesson.onClick.AddListener(ToggleExitConfirmationPopup);

        btnXray.onClick.AddListener(HandleXRayView);
        btnSeparate.onClick.AddListener(HandleSeparation);
        btnHold.onClick.AddListener(HandleHoldAction);
        btnLabel.onClick.AddListener(HandleLabelView);
        btnShowGuideBoard.onClick.AddListener(HandleShowGuideBoard);
        btnExitGuideBoard.onClick.AddListener(HandleShowGuideBoard);
        btnStartTree.onClick.AddListener(BackToOriginOrgan);
        btnAR.onClick.AddListener(delegate { ToggleMode(ModeManager.MODE_EXPERIENCE.MODE_AR); });
        btnSwitch.onClick.AddListener(delegate { ToggleMode(ModeManager.MODE_EXPERIENCE.MODE_3D); });
        btnBacktoMode3D.onClick.AddListener(delegate { ToggleMode(ModeManager.MODE_EXPERIENCE.MODE_3D); });
    }

    void SetupUIForMembers(bool isHost)
    {
        interactionBtnsGroup.SetActive(isHost);
        // btnAudio.gameObject.SetActive(isHost);
        // btnMenu.gameObject.SetActive(isHost);
    }

    void OnMeetingObjectInstantiate(GameObject newObject)
    {
        ObjectManager.Instance.InitObject(newObject);
        if (isFirstTimeJoining)
        {
            isFirstTimeJoining = false;
            InteractionSetupForMember.Instance.InitInteractions();
        }
    }
    public void ExitLesson()
    {
        isForcedDisconnected = true;
        StartCoroutine(PhotonConnectionManager.Instance.DisConnectPhotonServer(SceneNameManager.prevScene));
    }

    void ToggleExitConfirmationPopup()
    {
        PopupManager.Instance.IsClickedExitLesson = !PopupManager.Instance.IsClickedExitLesson;
        if (PhotonNetwork.IsMasterClient)
        {
            PopupManager.Instance.SetContentForPopupExitLesson(MeetingConfig.finishMeetingTitle, MeetingConfig.finishMeetingContent, MeetingConfig.txtBtnFinishMeeting);
        }   
        else
        {
            PopupManager.Instance.SetContentForPopupExitLesson(MeetingConfig.leaveMeetingTitle, MeetingConfig.leaveMeetingContent, MeetingConfig.txtBtnLeaveMeeting);
        }
        PopupManager.Instance.ShowPopupExitLesson(PopupManager.Instance.IsClickedExitLesson);
    }

    public void FinishMeetingByHost()
    {
        isForcedDisconnected = true;
        StartCoroutine(FinishMeeting(SceneNameManager.prevScene));
    }

    IEnumerator FinishMeeting(string nextScene)
    {
        Toast.Show(MeetingConfig.hostLeftMessage, MeetingConfig.shortToastDuration);
        yield return new WaitForSeconds(MeetingConfig.shortToastDuration);
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
            while (PhotonNetwork.IsConnected)
            {
                yield return null;
            }
            SceneManager.LoadScene(nextScene);
        }
    }
    public IEnumerator OnLostConnection()
    {
        if (isForcedDisconnected)
        {
            yield break;
        }
        if (SceneManager.GetActiveScene().name == SceneConfig.meetingExperience)
        {
            Toast.Show(MeetingConfig.lostConnection, MeetingConfig.longToastDuration);
            yield return new WaitForSeconds(MeetingConfig.longToastDuration);
            SceneManager.LoadScene(SceneNameManager.prevScene);
        }
    }

    void HandleXRayView()
    {
        XRayManager.Instance.IsMakingXRay = !XRayManager.Instance.IsMakingXRay;
        PhotonConnectionManager.Instance.SetActionStatusValue(MeetingConfig.IS_MAKING_XRAY_KEY, XRayManager.Instance.IsMakingXRay);
    }

    void SyncXRayView(bool isMakingXRay)
    {
        XRayManager.Instance.HandleXRayView(isMakingXRay);
    }

    void HandleSeparation()
    {
        SeparateManager.Instance.IsSeparating = !SeparateManager.Instance.IsSeparating;
        PhotonConnectionManager.Instance.SetActionStatusValue(MeetingConfig.IS_SEPARATING_KEY, SeparateManager.Instance.IsSeparating);
    }

    void HandleHoldAction()
    {
        TouchManager.Instance.IsClickedHoldBtn = !TouchManager.Instance.IsClickedHoldBtn;
        PhotonConnectionManager.Instance.SetActionStatusValue(MeetingConfig.IS_CLICKED_HOLD_KEY, TouchManager.Instance.IsClickedHoldBtn);
    }

    void HandleLabelView()
    {
        LabelManager.Instance.IsShowingLabel = !LabelManager.Instance.IsShowingLabel;
        PhotonConnectionManager.Instance.SetActionStatusValue(MeetingConfig.IS_SHOWING_LABEL_KEY, LabelManager.Instance.IsShowingLabel);
    }

    void SyncLabelView(bool isSHowingLabel)
    {
        LabelManager.Instance.HandleLabelView(isSHowingLabel);
    }

    void HandleShowGuideBoard()
    {
        PopupManager.Instance.IsClickedGuideBoard = !PopupManager.Instance.IsClickedGuideBoard;
        PhotonConnectionManager.Instance.SetActionStatusValue(MeetingConfig.IS_CLICKED_GUIDE_BOARD_KEY, PopupManager.Instance.IsClickedGuideBoard);
    }

    void SyncGuideBoardView(bool isClickedGuideBoard)
    {
        PopupManager.Instance.ShowGuideBoard(isClickedGuideBoard);
    }

    void ToggleMode(ModeManager.MODE_EXPERIENCE mode)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonConnectionManager.Instance.SetActionStatusValue(MeetingConfig.EXPERIENCE_MODE_KEY, mode);
        }
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey(MeetingConfig.IS_MAKING_XRAY_KEY))
        {
            SyncXRayView((bool)propertiesThatChanged[MeetingConfig.IS_MAKING_XRAY_KEY]);
        }
        if (propertiesThatChanged.ContainsKey(MeetingConfig.IS_SEPARATING_KEY))
        {
            SeparateManager.Instance.HandleSeparate((bool)propertiesThatChanged[MeetingConfig.IS_SEPARATING_KEY]);
        }
        if (propertiesThatChanged.ContainsKey(MeetingConfig.IS_CLICKED_HOLD_KEY))
        {
            TouchManager.Instance.IsClickedHoldBtn = (bool)propertiesThatChanged[MeetingConfig.IS_CLICKED_HOLD_KEY];
        }
        if (propertiesThatChanged.ContainsKey(MeetingConfig.IS_SHOWING_LABEL_KEY))
        {
            SyncLabelView((bool)propertiesThatChanged[MeetingConfig.IS_SHOWING_LABEL_KEY]);
        }
        if (propertiesThatChanged.ContainsKey(MeetingConfig.IS_CLICKED_GUIDE_BOARD_KEY))
        {
            SyncGuideBoardView((bool)propertiesThatChanged[MeetingConfig.IS_CLICKED_GUIDE_BOARD_KEY]);
        }
        if (propertiesThatChanged.ContainsKey(MeetingConfig.EXPERIENCE_MODE_KEY))
        {
            ModeManager.Instance.ViewScreenWithMode((ModeManager.MODE_EXPERIENCE)propertiesThatChanged[MeetingConfig.EXPERIENCE_MODE_KEY]);
        }
    }

    void OnSelectChildObject(GameObject selectedObject)
    {
        OnResetStatusFeature();
        TreeNodeManager.Instance.DisplaySelectedObject(selectedObject, ObjectManager.Instance.CurrentObject);
        PhotonConnectionManager.Instance.SyncChildObjectSelection(selectedObject.name);
    }

    void OnResetStatusFeature()
    {
        PhotonConnectionManager.Instance.SetActionStatusValue(MeetingConfig.IS_SHOWING_LABEL_KEY, false);
        PhotonConnectionManager.Instance.SetActionStatusValue(MeetingConfig.IS_SEPARATING_KEY, false);
        SeparateManager.Instance.HandleSeparate(false);
        PhotonConnectionManager.Instance.SetActionStatusValue(MeetingConfig.IS_CLICKED_HOLD_KEY, false);
        MediaManager.Instance.StopMedia();
    }

    [PunRPC]
    void InitNewChildObject(string childObjectName)
    {
        ObjectManager.Instance.ChangeCurrentObject(GameObject.Find(childObjectName));
        TreeNodeManager.Instance.CreateChildNodeUI(childObjectName);
    }

    void OnClickNodeTree(String nodeName)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            TreeNodeManager.Instance.HandleClickNodeTree(nodeName);
        }
    }

    void BackToOriginOrgan()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            ResetMeetingObject();
        }
    }
    void ResetMeetingObject()
    {
        Vector3 positionObjectInAR = ObjectManager.Instance.OriginObject.transform.position;
        PhotonNetwork.Destroy(ObjectManager.Instance.OriginObject); 
        if (ObjectManager.Instance.OriginObject != null)
        {
            Destroy(ObjectManager.Instance.OriginObject);
        }
        if (ObjectManager.Instance.CurrentObject != null)
        {
            Destroy(ObjectManager.Instance.CurrentObject);
        }
        StartCoroutine(ResetOriginObject(positionObjectInAR));
    }
    IEnumerator ResetOriginObject(Vector3 positionObjectInAR)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.InstantiateRoomObject(PathConfig.PRE_PATH_MODEL + ObjectManager.Instance.OriginOrganData.Name, Vector3.zero, Quaternion.identity);
        }
        while (ObjectManager.Instance.OriginObject == null)
        {
            yield return null;
        }
        ObjectManager.Instance.OriginObject.SetActive(false);
        if (ModeManager.Instance.Mode == ModeManager.MODE_EXPERIENCE.MODE_AR)
        {
            if (ObjectManager.Instance.OriginObject.GetComponent<ARAnchor>() == null)
            {
                ObjectManager.Instance.OriginObject.AddComponent<ARAnchor>();
            }
            ObjectManager.Instance.OriginObject.transform.position = positionObjectInAR;
            ObjectManager.Instance.OriginObject.transform.localScale *= ModelConfig.scaleFactorInARMode;
        }
        ObjectManager.Instance.OriginObject.SetActive(true);
        OnResetStatusFeature();
        PhotonConnectionManager.Instance.SetActionStatusValue(MeetingConfig.IS_MAKING_XRAY_KEY, false);
        PhotonConnectionManager.Instance.SyncEmptyNodeTreeProcess();
    }

    [PunRPC]
    void SyncEmptyNodeTree()
    {
        TreeNodeManager.Instance.ClearAllNodeTree();
    }

    void OnChangeCurrentObject(string currentObjectName)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonConnectionManager.Instance.SetActionStatusValue(MeetingConfig.CURRENT_OBJECT_NAME_KEY, currentObjectName);
        }
    }

    void OnARPlaceObject(Vector3 position, Quaternion rotation)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            ARUIManager.Instance.PlaceARObject(position, rotation);
            MeetingCloudAnchorManager.Instance.HostCloudAnchor(ObjectManager.Instance.OriginObject.GetComponent<ARAnchor>());
        }
        else
        {
            Toast.Show(MeetingConfig.waitForHostPlaceObject, MeetingConfig.toastDuration);
        }
    }

    public void ShareCloudAnchorId(string cloudAnchorId)
    {
        Toast.Show(cloudAnchorId, MeetingConfig.longToastDuration);
    }

    public void OnHostCloudAnchorFailed(string message)
    {
        Toast.Show(message, MeetingConfig.longToastDuration);
    }

    public void ResolveCloudAnchorTransform(Transform cloudAnchorTransform)
    {
        Toast.Show(cloudAnchorTransform.ToString(), MeetingConfig.longToastDuration);
    }

    public void OnResolveCloudAnchorFailed(string message)
    {
        Toast.Show(message, MeetingConfig.longToastDuration);
    }
}
