using UnityEngine;
using System.Collections;

public class SoccerBallController : MonoBehaviour {

    private SphereCollider spCollider;

    GameObject player;

	// Use this for initialization
	void Start ()
    {
        spCollider = GetComponent<SphereCollider>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnTriggerEnter(Collider other)
    {
        SoccerPlayerController spc = other.gameObject.GetComponent<SoccerPlayerController>();
        if (spc)
        {
            other.gameObject.GetComponent<PlayerNetworkMover>().takeBall();
        }
    }

    public void holdBall(GameObject player)
    {
        spCollider.enabled = false;
        this.transform.parent = player.transform;
        this.transform.localPosition = new Vector3(0f, -0.5f, 1f);
    }

    public void resetGame()
    {
        this.transform.parent = null;
        this.transform.localPosition = new Vector3(0f, 0.5f, 0f);
        spCollider.enabled = true;
    }
}
