using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class InteractionSetupForMember : MonoBehaviour
{
    private static InteractionSetupForMember instance;
    public static InteractionSetupForMember Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<InteractionSetupForMember>();
            }
            return instance;
        }
    }

    public void InitInteractions()
    {
        InitTouchManager();
        InitSeparateManager();
        InitXRayManager();
        InitLabelManager();
        InitGuideBoardManager();
        InitVoiceManager();
        InitCurrentObject();
    }
    void InitCurrentObject()
    {
        string currentObjectName = (string)PhotonConnectionManager.Instance.GetActionStatusValue(MeetingConfig.CURRENT_OBJECT_NAME_KEY);
        if (!string.IsNullOrEmpty(currentObjectName))
        {
            ObjectManager.Instance.ChangeCurrentObject(GameObject.Find(currentObjectName));
            if (ObjectManager.Instance.CurrentObject.name != ObjectManager.Instance.OriginObject.name)
            {
                TreeNodeManager.Instance.CreateChildNodeUI(currentObjectName);
            }
        }
    }

    void InitVoiceManager()
    {
        bool isMuting = (bool)PhotonConnectionManager.Instance.GetActionStatusValue(MeetingConfig.IS_MUTING_ALL_KEY);
        MeetingMembersManager.Instance.IsMutingAll = isMuting;
        MeetingMembersManager.Instance.SetPlayerMicroState(true);
        PhotonNetwork.LocalPlayer.MuteByHost(isMuting);
    }

    void InitTouchManager()
    {
        TouchManager.Instance.IsClickedHoldBtn = (bool)PhotonConnectionManager.Instance.GetActionStatusValue(MeetingConfig.IS_CLICKED_HOLD_KEY);
    }
    void InitSeparateManager()
    {
        SeparateManager.Instance.IsSeparating = (bool)PhotonConnectionManager.Instance.GetActionStatusValue(MeetingConfig.IS_SEPARATING_KEY);
        if (SeparateManager.Instance.IsSeparating)
        {
            SeparateManager.Instance.HandleSeparate(SeparateManager.Instance.IsSeparating);
        }
    }

    void InitXRayManager()
    {
        XRayManager.Instance.IsMakingXRay = (bool)PhotonConnectionManager.Instance.GetActionStatusValue(MeetingConfig.IS_MAKING_XRAY_KEY);
        if (XRayManager.Instance.IsMakingXRay)
        {
            XRayManager.Instance.HandleXRayView(XRayManager.Instance.IsMakingXRay);
        }
    }

    void InitLabelManager()
    {
        LabelManager.Instance.IsShowingLabel = (bool)PhotonConnectionManager.Instance.GetActionStatusValue(MeetingConfig.IS_SHOWING_LABEL_KEY);
        if (LabelManager.Instance.IsShowingLabel)
        {
            LabelManager.Instance.HandleLabelView(LabelManager.Instance.IsShowingLabel);
        }
    }

    void InitGuideBoardManager()
    {
        PopupManager.Instance.IsClickedGuideBoard = (bool)PhotonConnectionManager.Instance.GetActionStatusValue(MeetingConfig.IS_CLICKED_GUIDE_BOARD_KEY);
        if (PopupManager.Instance.IsClickedGuideBoard)
        {
            PopupManager.Instance.ShowGuideBoard(PopupManager.Instance.IsClickedGuideBoard);
        }
    }
}
