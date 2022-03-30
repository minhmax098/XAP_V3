using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using EasyUI.Toast;

public class LoadingManager : MonoBehaviour
{
    public GameObject loadingPanel;
    private static LoadingManager instance;
    public static LoadingManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<LoadingManager>();
            }
            return instance;
        }
    }

    public void ShowLoading(string message)
    {
        loadingPanel.SetActive(true);
        loadingPanel.transform.GetChild(1).GetComponent<Text>().text = message;
    }

    public void HideLoading()
    {
        loadingPanel.SetActive(false);
    }
}