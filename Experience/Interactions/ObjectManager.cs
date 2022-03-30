using System.Security;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.XR.ARFoundation;

public class ObjectManager : MonoBehaviour
{
    private static ObjectManager instance;
    public static ObjectManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<ObjectManager>();
            }
            return instance;
        }
    }

    public static event Action onInitOrganSuccessfully;
    public static event Action<string> onChangeCurrentObject;
    public static event Action onResetObject;

    public OrganInfor OriginOrganData { get; set; }
    public Material OriginOrganMaterial { get; set; }
    public GameObject OriginObject { get; set; }
    public List<Vector3> ListchildrenOfOriginPosition { get; set; }
    public GameObject CurrentObject { get; set; }
    public Vector3 OriginPosition { get; set; }
    public Quaternion OriginRotation { get; set; }
    public Vector3 OriginScale { get; set; }

    void Start() 
    {
        Screen.orientation = ScreenOrientation.LandscapeLeft;
    }

    public void LoadDataOrgan()
    {
        OriginOrganData = new OrganInfor("", "Brain_Demo", "");
    }

    public void InitOriginalExperience()
    {
        LoadDataOrgan();
        GameObject prefabMainOrgan = Resources.Load(PathConfig.PRE_PATH_MODEL + OriginOrganData.Name, typeof(GameObject)) as GameObject;
        GameObject objectInstance = Instantiate(prefabMainOrgan, prefabMainOrgan.transform.position, prefabMainOrgan.transform.rotation) as GameObject;
        InitObject(objectInstance);
    }

    public void InitObject(GameObject newObject)
    {   
        OriginObject = newObject;
        OriginOrganMaterial = OriginObject.GetComponent<Renderer>().materials[0];
        ChangeCurrentObject(OriginObject);
        OriginPosition = OriginObject.transform.position;
        OriginRotation = OriginObject.transform.rotation;
        OriginScale = OriginObject.transform.localScale;

        onInitOrganSuccessfully?.Invoke();
    }

    public void ChangeCurrentObject(GameObject newGameObject)
    {
        CurrentObject = newGameObject;
        ListchildrenOfOriginPosition = Helper.GetListchildrenOfOriginPosition(CurrentObject);
        onChangeCurrentObject?.Invoke(CurrentObject.name);
    }

    public void InstantiateARObject(Vector3 position, Quaternion rotation)
    {
        OriginObject.transform.position = position;
        OriginObject.transform.rotation = rotation;
        if (OriginObject.GetComponent<ARAnchor>() == null)
        {
            OriginObject.AddComponent<ARAnchor>();
        }
        OriginObject.transform.localScale *= ModelConfig.scaleFactorInARMode;
        OriginObject.SetActive(true);
    }

    public void Instantiate3DObject()
    {
        if (ARUIManager.Instance.IsStartAR)
        {
            onResetObject?.Invoke();
        }
        else
        {
            OriginObject.SetActive(true);
        }

        // Reset UI AR
        ARUIManager.Instance.RefreshStatusControl();
    }

    // public void Init3DObject
}
