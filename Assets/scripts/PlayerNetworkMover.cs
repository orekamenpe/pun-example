using UnityEngine;
using System.Collections;

public class PlayerNetworkMover : Photon.MonoBehaviour 
{
    public delegate void Respawn(float time);
    public event Respawn RespawnMe;
    public delegate void SendMessage(string MessageOverlay);
    public event SendMessage SendNetworkMessage;

    Vector3 position;
    Quaternion rotation;
    float smoothing = 10f;

    bool initialLoad = true;

    void Start()
    {
        if(photonView.isMine)
        {
            GetComponent<Rigidbody> ().useGravity = true;
        }
        else
        {
            StartCoroutine("UpdateData");
        }
    }

    IEnumerator UpdateData () 
    {
        if(initialLoad)
        {
            initialLoad = false;
            transform.position = position;
            transform.rotation = rotation;
        }

        while(true)
        {
            if (position != Vector3.zero)
            {
                transform.position = Vector3.Lerp(transform.position, position, Time.deltaTime * smoothing);
                transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * smoothing);
            }

            yield return null;
        }
    }

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            position = (Vector3)stream.ReceiveNext();
            rotation = (Quaternion)stream.ReceiveNext();
        }
    }

}