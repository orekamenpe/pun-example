using UnityEngine;
using System.Collections;

public class PlayerNetworkMover : Photon.MonoBehaviour 
{
    public delegate void Respawn(float time);
    public event Respawn RespawnMe;
    public delegate void SendMessage(string MessageOverlay);
    public event SendMessage SendNetworkMessage;

    Vector3 position = new Vector3(0,6,6);
    Quaternion rotation = Quaternion.identity;
    float smoothing = 5f;
    public bool hasBall = false;

    Rigidbody rgbody;

    private float lastSynchronizationTime = 0f;
    private float syncDelay = 0f;
    private float syncTime = 0f;
    private Vector3 syncStartPosition = Vector3.zero;
    private Vector3 syncEndPosition = Vector3.zero;

    bool initialLoad = true;

    void Awake()
    {
        rgbody = GetComponent<Rigidbody>();
    }

    void Start()
    {
        
        if (photonView.isMine)
        {
            rgbody.useGravity = true;
            GetComponent<SoccerPlayerController>().changeTeam(PhotonNetwork.player.GetTeam());
        }
        else
        {
            GetComponent<SoccerPlayerController>().changeTeam(PhotonNetwork.otherPlayers[0].GetTeam());
            StartCoroutine("UpdateData");
        }
    }

    IEnumerator UpdateData () 
    {
        if(initialLoad)
        {
            initialLoad = false;
            rgbody.position = position;
            transform.rotation = rotation;
        }

        while(true)
        {
            syncTime += Time.deltaTime;
            rgbody.position = Vector3.Lerp(syncStartPosition, syncEndPosition, syncTime / syncDelay);
            transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * smoothing);

            yield return null;
        }
    }

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            stream.SendNext(rgbody.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            syncEndPosition = (Vector3)stream.ReceiveNext();
            rotation = (Quaternion)stream.ReceiveNext();

            syncStartPosition = rgbody.position;

            syncTime = 0f;
            syncDelay = Time.time - lastSynchronizationTime;
            lastSynchronizationTime = Time.time;
        }
    }


    [PunRPC]
    public void TakeBall()
    {
        SoccerGameManager.instance.soccerBall.HoldBall(this.gameObject);
        hasBall = true;

        if (photonView.isMine)
        {
            photonView.RPC("TakeBall", PhotonTargets.OthersBuffered, null );
        }   
    }
}