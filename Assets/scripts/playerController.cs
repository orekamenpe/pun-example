using UnityEngine;
using System.Collections;

public class playerController : MonoBehaviour {

	public float speed = 10.0f;
	private Rigidbody rgbody;
    private PhotonView photonView;

    void Start()
	{
        rgbody = GetComponent<Rigidbody>();
        photonView = GetComponent<PhotonView>();
	}
	
	// Update is called once per frame
	void Update () {
        if (photonView.isMine)
		InputMovement ();
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

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            stream.SendNext(rgbody.position);
        }
        else
        {
            rgbody.position = (Vector3)stream.ReceiveNext();
        }
    }
}
