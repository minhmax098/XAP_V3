using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class TouchManager : MonoBehaviour
{
    private static TouchManager instance;
    public static TouchManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<TouchManager>();
            }
            return instance;
        }
    }

    public static event Action onResetStatusFeature;
    public static event Action<GameObject> onSelectChildObject;

    const float ROTATION_RATE = 0.08f;
    const float LONG_TOUCH_THRESHOLD = 1f;
    const float ROTATION_SPEED = 0.5f;
    float touchDuration = 0.0f;
    Touch touch;
    Touch touchZero;
    Touch touchOne;
    float originDelta;
    Vector3 originScale;

    Vector3 originLabelScale = new Vector3(1f, 1f, 1f);
    float currentDelta;
    float scaleFactor;
    Vector2 originPosition;
    bool isMovingByLongTouch = false;
    bool isLongTouch = false;
    Vector3 originScaleSelected;
    GameObject currentSelectedObject;
    private Vector3 mOffset;
    private float mZCoord;
    public GameObject btnHold;
    private bool isClickedHoldBtn;
    public bool IsClickedHoldBtn 
    { 
        get
        {
            return isClickedHoldBtn;
        }
        set
        {
            isClickedHoldBtn = value;
            btnHold.GetComponent<Image>().sprite = isClickedHoldBtn ? Resources.Load<Sprite>(PathConfig.HOLD_CLICKED_IMAGE) : Resources.Load<Sprite>(PathConfig.HOLD_UNCLICK_IMAGE);
        }
    }

    public void HandleTouchInteraction()
    {
        if (ObjectManager.Instance.CurrentObject == null)
        {
            return;
        }
        if (PopupManager.Instance.IsHavingPopup() || Input.touchCount < 1 || ObjectManager.Instance.OriginObject == null)
        {
            return;
        }
        if (Input.touchCount == 3)
        {
            if (Input.GetTouch(0).phase == TouchPhase.Moved && Input.GetTouch(1).phase == TouchPhase.Moved && Input.GetTouch(2).phase == TouchPhase.Moved)
            {
                HandleSimultaneousThreeTouch(Input.GetTouch(1));
            }
        }
        else if (Input.touchCount == 2)
        {
            touchZero = Input.GetTouch(0);
            touchOne = Input.GetTouch(1);
            HandleSimultaneousTouch(touchZero, touchOne);
        }
        else if (Input.touchCount == 1)
        {
            touch = Input.GetTouch(0);
            if (touch.tapCount == 1)
            {
                HandleSingleTouch(touch);
            }
            else if (touch.tapCount == 2)
            {
                touch = Input.touches[0];
                if (touch.phase == TouchPhase.Ended)
                {
                    HandleDoupleTouch(touch);
                }
            }
        }
    }
    private void HandleSingleTouch(Touch touch)
    {
        switch (touch.phase)
        {
            case TouchPhase.Began:
                {
                    currentSelectedObject = Helper.GetChildOrganOnTouchByTag(touch.position);
                    isMovingByLongTouch = currentSelectedObject != null;
                    if (currentSelectedObject != null)
                    {
                        mZCoord = Camera.main.WorldToScreenPoint(currentSelectedObject.transform.position).z;
                        mOffset = currentSelectedObject.transform.position - GetTouchPositionAsWorldPoint(touch);
                    }
                    break;
                }

            case TouchPhase.Stationary:
                {
                    if (isMovingByLongTouch && !isLongTouch && IsClickedHoldBtn)
                    {
                        touchDuration += Time.deltaTime;
                        if (touchDuration > LONG_TOUCH_THRESHOLD)
                        {
                            OnLongTouchInvoke();
                        }
                    }
                    break;
                }
            case TouchPhase.Moved:
                {
                    if (isLongTouch)
                    {
                        Drag(touch, currentSelectedObject);
                    }
                    else
                    {
                        Rotate(touch);
                    }
                    break;
                }
            case TouchPhase.Ended:
                {
                    ResetLongTouch();
                    break;
                }

            case TouchPhase.Canceled:
                {
                    ResetLongTouch();
                    break;
                }
        }
    }

    void ResetLongTouch()
    {
        touchDuration = 0f;
        isLongTouch = false;
        isMovingByLongTouch = false;
        currentSelectedObject = null;
    }

    void OnLongTouchInvoke()
    {
        StartCoroutine(HightLightObject());
        isLongTouch = true;
    }

    IEnumerator HightLightObject()
    {
        originScaleSelected = currentSelectedObject.transform.localScale;
        currentSelectedObject.transform.localScale = originScaleSelected * 1.5f;
        yield return new WaitForSeconds(0.12f);
        currentSelectedObject.transform.localScale = originScaleSelected;
    }
    private void Rotate(Touch touch)
    {
        if (ModeManager.Instance.Mode == ModeManager.MODE_EXPERIENCE.MODE_AR)
        {
            ObjectManager.Instance.OriginObject.transform.rotation *= Quaternion.Euler(new Vector3(0, -touch.deltaPosition.x * ROTATION_SPEED, 0));
        }
        else if (ModeManager.Instance.Mode == ModeManager.MODE_EXPERIENCE.MODE_3D)
        {
            ObjectManager.Instance.OriginObject.transform.Rotate(touch.deltaPosition.y * ROTATION_RATE, -touch.deltaPosition.x * ROTATION_RATE, 0, Space.World);
        }
    }

    private Vector3 GetTouchPositionAsWorldPoint(Touch touch)
    {
        Vector3 touchPoint = touch.position;
        touchPoint.z = mZCoord;
        return Camera.main.ScreenToWorldPoint(touchPoint);
    }

    private void Drag(Touch touch, GameObject obj)
    {
        if (obj != null)
        {
            obj.transform.position = GetTouchPositionAsWorldPoint(touch) + mOffset;
        }
    }

    private async void HandleSimultaneousTouch(Touch touchZero, Touch touchOne)
    {
        if (touchZero.phase == TouchPhase.Began || touchOne.phase == TouchPhase.Began)
        {
            originDelta = Vector2.Distance(touchZero.position, touchOne.position);
            originScale = ObjectManager.Instance.OriginObject.transform.localScale;
            
            if (LabelManager.Instance.listLabelObjects.Count > 0)
            {
                originLabelScale = LabelManager.Instance.listLabelObjects[0].transform.localScale;
            }            
        }
        else if (touchZero.phase == TouchPhase.Moved || touchOne.phase == TouchPhase.Moved)
        {
            currentDelta = Vector2.Distance(touchZero.position, touchOne.position);
            scaleFactor = currentDelta / originDelta;
            
            ObjectManager.Instance.OriginObject.transform.localScale = originScale * scaleFactor;
            Debug.Log("Scale Factor: " + scaleFactor);   
            
            foreach (GameObject obj in LabelManager.Instance.listLabelObjects)
            {
                obj.transform.localScale = originLabelScale / scaleFactor;
            }
        }
    }

    private void HandleDoupleTouch(Touch touch)
    {
        GameObject selectedObject = Helper.GetChildOrganOnTouchByTag(touch.position);

        if (selectedObject == null || selectedObject == ObjectManager.Instance.OriginObject || ObjectManager.Instance.CurrentObject.transform.childCount < 1)
        {
            return;
        }
        onSelectChildObject?.Invoke(selectedObject);
    }

    private void HandleSimultaneousThreeTouch(Touch touch)
    {
        Drag(touch, ObjectManager.Instance.OriginObject);
    }
}
