using UnityEngine;
using System.Collections;

public class SoccerPlayerController : Photon.MonoBehaviour {

    public float speed = 10.0f;
    private Rigidbody rgbody;

    // Use this for initialization
    void Start ()
    {
        rgbody = GetComponent<Rigidbody>();
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (photonView.isMine)
        {
            InputMovement();
        }
    }

    void InputMovement()
    {
        if (Input.GetKey(KeyCode.W))
            rgbody.MovePosition(rgbody.position + transform.forward * speed * Time.deltaTime);

        if (Input.GetKey(KeyCode.S))
            rgbody.MovePosition(rgbody.position - transform.forward * speed * Time.deltaTime);


        if (Input.GetKey(KeyCode.D))
        {
            Quaternion deltaRotation = Quaternion.Euler(new Vector3(0,5,0));
            rgbody.MoveRotation(rgbody.rotation * deltaRotation);
        }

        if (Input.GetKey(KeyCode.A))
        {
            Quaternion deltaRotation = Quaternion.Euler(new Vector3(0,-5, 0));
            rgbody.MoveRotation(rgbody.rotation * deltaRotation);
        }

    }

}
