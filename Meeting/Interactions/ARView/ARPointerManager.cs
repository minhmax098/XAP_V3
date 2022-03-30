using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace MeetingModules
{
    public class ARPointerManager : MonoBehaviour
    {
        public ARRaycastManager raycastManager;
        List<ARRaycastHit> hitList = new List<ARRaycastHit>();
        Vector2 centerOfScreen;
        public GameObject pointer;

        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
            if (!ObjectManager.Instance.CurrentObject.activeInHierarchy)
            {
                DetectPointer();
            }
        }
        void DetectPointer()
        {
            centerOfScreen = new Vector2(Screen.width / 2, Screen.height / 2);
            raycastManager.Raycast(centerOfScreen, hitList, TrackableType.Planes);
            if (hitList.Count > 0)
            {
                ARUIManager.Instance.OnActivePointer(hitList[0].pose);
            }
            else
            {
                ARUIManager.Instance.OnInactivePointer();
            }
        }
    }
}