using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using Photon.Pun;
using Photon.Realtime;

[RequireComponent(typeof(PhotonView))]
public class MeetingObjectInstantiateObserve : MonoBehaviour, IPunInstantiateMagicCallback
{
    public static event Action<GameObject> onMeetingObjectInstantiate;
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        onMeetingObjectInstantiate?.Invoke(this.gameObject);
    }
}
