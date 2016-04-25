using UnityEngine;
using System.Collections;

public class playerController : Photon.MonoBehaviour {

	public float speed = 10.0f;
	private Rigidbody rgbody;
    private Renderer render;


    private float lastSynchronizationTime = 0f;
    private float syncDelay = 0f;
    private float syncTime = 0f;
    private Vector3 syncStartPosition = Vector3.zero;
    private Vector3 syncEndPosition = Vector3.zero;

    void Start()
	{
        rgbody = GetComponent<Rigidbody>();
        render = GetComponent<Renderer>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (photonView.isMine)
        {
            InputMovement();
            InputColorChange();
        }
        else
        {
            SyncedMovement();
        }
		
	}

	void InputMovement()
	{
		if (Input.GetKey(KeyCode.W))
            rgbody.MovePosition (rgbody.position + Vector3.forward * speed * Time.deltaTime);

		if (Input.GetKey(KeyCode.S))
            rgbody.MovePosition(rgbody.position - Vector3.forward * speed * Time.deltaTime);

		if (Input.GetKey(KeyCode.D))
            rgbody.MovePosition(rgbody.position + Vector3.right * speed * Time.deltaTime);

		if (Input.GetKey(KeyCode.A))
            rgbody.MovePosition(rgbody.position - Vector3.right * speed * Time.deltaTime);
	}

    private void SyncedMovement()
    {
        syncTime += Time.deltaTime;
        rgbody.position = Vector3.Lerp(syncStartPosition, syncEndPosition, syncTime / syncDelay);
    }

    private void InputColorChange()
    {
        if (Input.GetKeyDown(KeyCode.R))
            ChangeColorTo(new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)));
    }

    [PunRPC]
    void ChangeColorTo(Vector3 color)
    {
        render.material.color = new Color(color.x, color.y, color.z, 1f);

        if (photonView.isMine)
            photonView.RPC("ChangeColorTo", PhotonTargets.OthersBuffered, color);
    }

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            stream.SendNext(rgbody.position);
            stream.SendNext(rgbody.velocity);
        }
        else
        {
            Vector3 syncPosition = (Vector3)stream.ReceiveNext();
            Vector3 syncVelocity = (Vector3)stream.ReceiveNext();

            syncTime = 0f;
            syncDelay = Time.time - lastSynchronizationTime;
            lastSynchronizationTime = Time.time;

            syncEndPosition = syncPosition + syncVelocity * syncDelay;
            syncStartPosition = rgbody.position;
        }
    }
}
