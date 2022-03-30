using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;


public class ARUIManager : MonoBehaviour
{
    private static ARUIManager instance;
    public static ARUIManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<ARUIManager>();
            }
            return instance;
        }
    }
    public GameObject UIComponentAR;
    public GameObject introText;
    public GameObject guideText;
    public GameObject arPointer;
    public bool IsStartAR { get; set; }
    public bool IsReadyToPlaceObject { get; set; }
    public bool IsReadyToControl { get; set; }
    public ARSession arSession;
    public static event Action OnARPlaceObject;
    void Start()
    {
        Screen.orientation = ScreenOrientation.Landscape;
        IsStartAR = false;
        RefreshStatusControl();
    }

    void Update()
    {
        if (ModeManager.Instance.Mode == ModeManager.MODE_EXPERIENCE.MODE_3D)
        {
            return;
        }
        if (Input.touchCount == 1)
        {
            if (IsReadyToPlaceObject & (!Helper.IsPointerOverUIObject()))
            {
                if (Input.GetTouch(0).phase == TouchPhase.Ended)
                {
                    PlaceObject();
                }
            }
        }
        AllowInteractingObject(ObjectManager.Instance.CurrentObject.activeSelf);
    }
    public void OnActivePointer(Pose pose)
    {
        if (!arPointer.activeSelf)
        {
            arPointer.SetActive(true);
        }
        arPointer.transform.position = pose.position;
        arPointer.transform.rotation = pose.rotation;
        IsReadyToPlaceObject = true;
        AllowPlacingObject(IsReadyToPlaceObject);
    }

    void AllowPlacingObject(bool _isReadyToPlaceObject)
    {
        guideText.SetActive(_isReadyToPlaceObject);
        introText.SetActive(!_isReadyToPlaceObject);
    }
    void PlaceObject()
    {
        OnARPlaceObject?.Invoke();
    }

    public void PlaceARObject()
    {   
        ObjectManager.Instance.InstantiateARObject(arPointer.transform.position, arPointer.transform.rotation);
    }
    void AllowInteractingObject(bool _isReadyToControl)
    {
        IsReadyToControl = _isReadyToControl;
        IsReadyToPlaceObject = !_isReadyToControl;
        IsStartAR = _isReadyToControl;

        ModeManager.Instance.UIComponentCommon.SetActive(_isReadyToControl);
        UIComponentAR.SetActive(!IsReadyToControl);
        arPointer.SetActive(!_isReadyToControl);
    }
    public void OnInactivePointer()
    {
        IsReadyToPlaceObject = false;
        AllowPlacingObject(IsReadyToPlaceObject);
        arPointer.SetActive(IsReadyToPlaceObject);
    }

    public void RefreshStatusControl()
    {
        IsReadyToPlaceObject = false;
        IsStartAR = false;
        IsReadyToControl = false;
        guideText.SetActive(false);
        introText.SetActive(true);
        UIComponentAR.SetActive(true);
        arPointer.SetActive(false);
        arSession.Reset();
    }
}
