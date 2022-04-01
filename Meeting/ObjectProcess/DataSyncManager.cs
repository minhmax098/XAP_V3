using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class DataSyncManager : MonoBehaviourPun, IPunObservable
    {
        public bool isSynchronizedPosition = true;
        public bool isSynchronizedRotation = true;
        public bool isSynchronizedScale = true;
        public bool isSynchronizedDisplay = true;
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                SendData(stream, transform);
            }
            else
            {
                ReceiveData(stream, transform);
            }
        }

        private void SendData(PhotonStream stream, Transform currentTransform)
        {
            if (currentTransform.gameObject.tag == TagConfig.LABEL_TAG)
            {
                return;
            }
            if (isSynchronizedPosition)
            {
                if (ModeManager.Instance.Mode == ModeManager.MODE_EXPERIENCE.MODE_3D)
                {
                    stream.SendNext(currentTransform.localPosition);
                }
            }
            if (isSynchronizedRotation)
            {
                stream.SendNext(currentTransform.localRotation);
            }
            if (isSynchronizedScale)
            {
                stream.SendNext(currentTransform.localScale);
            }
            if (isSynchronizedDisplay)
            {
                stream.SendNext(currentTransform.gameObject.activeSelf);
            }
            for (int i = 0; i < currentTransform.childCount; i++)
            {
                if (currentTransform.GetChild(i) != null)
                {
                    SendData(stream, currentTransform.GetChild(i));
                }
            }
        }

        private void ReceiveData(PhotonStream stream, Transform currentTransform)
        {
            if (currentTransform.gameObject.tag == TagConfig.LABEL_TAG)
            {
                return;
            }
            if (isSynchronizedPosition)
            {
                if (ModeManager.Instance.Mode == ModeManager.MODE_EXPERIENCE.MODE_3D)
                {
                    currentTransform.localPosition = (Vector3)stream.ReceiveNext();
                }
            }
            if (isSynchronizedRotation)
            {
                currentTransform.localRotation = (Quaternion)stream.ReceiveNext();
            }
            if (isSynchronizedScale)
            {
                currentTransform.localScale = (Vector3)stream.ReceiveNext();
            }
            if (isSynchronizedDisplay)
            {
                currentTransform.gameObject.SetActive((bool)stream.ReceiveNext());
            }
            for (int i = 0; i < currentTransform.childCount; i++)
            {
                if (currentTransform.GetChild(i) != null)
                {
                    ReceiveData(stream, currentTransform.GetChild(i));
                }
            }
        }
    }