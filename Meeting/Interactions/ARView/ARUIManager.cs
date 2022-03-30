using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

namespace MeetingModules
{
    public class ARUIManager : MonoBehaviour
    {
        private static ARUIManager instance;
        public static ARUIManager Instance
        {
            get
            {
                if (instance == null)
                    instance = FindObjectOfType<ARUIManager>();
                return instance;
            }
        }
        public GameObject introPanel;
        public GameObject introText;
        public GameObject guideText;
        public GameObject btnSwitch;
        public GameObject arPointer;
        public ARSession arSession;

        // Start is called before the first frame update
        void Start()
        {
            Screen.orientation = ScreenOrientation.Landscape;
        }

        void Update()
        {
            if (Input.touchCount == 1)
            {
                if (IsPlacingObject())
                    if (Input.GetTouch(0).phase == TouchPhase.Ended)
                    {
                        PlaceObject();
                    }
            }
        }

        public void InitUIAR()
        {
            arSession.Reset();
            AllowInteractingObject(false);
            AllowPlacingObject(false);
            arPointer.SetActive(false);
        }
        void AllowInteractingObject(bool isReadyToControl)
        {
            introPanel.SetActive(!isReadyToControl);
        }

        void AllowPlacingObject(bool isReadyToPlaceObject)
        {
            guideText.SetActive(isReadyToPlaceObject);
            introText.SetActive(!isReadyToPlaceObject);
        }
        public void OnInactivePointer()
        {
            AllowPlacingObject(false);
            arPointer.SetActive(false);
        }

        public void OnActivePointer(Pose pose)
        {
            AllowPlacingObject(true);
            arPointer.transform.position = pose.position;
            arPointer.transform.rotation = pose.rotation;
            if (!arPointer.activeInHierarchy)
            {
                arPointer.SetActive(true);
            }
        }
        void PlaceObject()
        {
            arPointer.SetActive(false);
            // ObjectManager.Instance.InstantiateARObject(arPointer.transform.position, arPointer.transform.rotation);
            AllowInteractingObject(true);
        }

        bool IsPlacingObject()
        {
            return guideText.activeInHierarchy;
        }
    }
}