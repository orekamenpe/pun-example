using UnityEngine;
using System.Collections;

public class playerController : MonoBehaviour {

	public float speed = 10.0f;
	private Rigidbody rigidbody;

	void Start()
	{
		rigidbody = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {
		InputMovement ();
	}

	void InputMovement()
	{
		if (Input.GetKey(KeyCode.W))
			rigidbody.MovePosition (rigidbody.position + Vector3.forward * speed * Time.deltaTime);

		if (Input.GetKey(KeyCode.S))
			rigidbody.MovePosition(rigidbody.position - Vector3.forward * speed * Time.deltaTime);

		if (Input.GetKey(KeyCode.D))
			rigidbody.MovePosition(rigidbody.position + Vector3.right * speed * Time.deltaTime);

		if (Input.GetKey(KeyCode.A))
			rigidbody.MovePosition(rigidbody.position - Vector3.right * speed * Time.deltaTime);
	}
}
