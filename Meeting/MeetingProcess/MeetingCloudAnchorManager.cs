using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Google.XR.ARCoreExtensions;
using System;

public class MeetingCloudAnchorManager : MonoBehaviour
{
    private const int EXPIRE_TIME = 1; // day
    public ARAnchorManager arAnchorManager;
    public static event Action<string> onHostCloudAnchorFailed;
    public static event Action<string> onResolveCloudAnchorFailed;

    private static MeetingCloudAnchorManager instance;
    public static MeetingCloudAnchorManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<MeetingCloudAnchorManager>();
            }
            return instance;
        }   
    }

    public void HostCloudAnchor(ARAnchor localAnchor)
    {
        ARCloudAnchor arCloudAnchor = arAnchorManager.HostCloudAnchor(localAnchor, EXPIRE_TIME);
        if (arCloudAnchor != null)
        {
            StartCoroutine(CheckHostingAnchorProcess(arCloudAnchor));
        }
        else
        {
            onHostCloudAnchorFailed?.Invoke(MeetingConfig.unableToHostCloudAnchor);
        }
    }

    IEnumerator CheckHostingAnchorProcess(ARCloudAnchor currentARCloudAnchor)
    {
        while ((currentARCloudAnchor.cloudAnchorState != CloudAnchorState.Success))
        {
            if (currentARCloudAnchor.cloudAnchorState == CloudAnchorState.TaskInProgress)
            {
                yield return null;
            }
            else
            {
                onHostCloudAnchorFailed?.Invoke(MeetingConfig.GetAnchorStateMessage(currentARCloudAnchor.cloudAnchorState));
                yield break;
            }
        }
        MeetingExperienceManager.Instance.ShareCloudAnchorId(currentARCloudAnchor.cloudAnchorId);
    }

    public void ResolveCloudAnchor(string cloudAnchorId)
    {
        ARCloudAnchor arCloudAnchor = arAnchorManager.ResolveCloudAnchorId(cloudAnchorId);
        if (arCloudAnchor != null)
        {
            StartCoroutine(CheckResolvingAnchorProcess(arCloudAnchor));
        }
        else
        {
            onResolveCloudAnchorFailed?.Invoke(MeetingConfig.unableToResolveCloudAnchor);
        }
    }

    IEnumerator CheckResolvingAnchorProcess(ARCloudAnchor currentARCloudAnchor)
    {
        while ((currentARCloudAnchor.cloudAnchorState != CloudAnchorState.Success))
        {
            if (currentARCloudAnchor.cloudAnchorState == CloudAnchorState.TaskInProgress)
            {
                yield return null;
            }
            else
            {
                onResolveCloudAnchorFailed?.Invoke(MeetingConfig.GetAnchorStateMessage((currentARCloudAnchor.cloudAnchorState)));
                yield break;
            }
        }
        MeetingExperienceManager.Instance.ResolveCloudAnchorTransform(currentARCloudAnchor.transform);
    }
}
