using UnityEngine;
using System.Collections;

public class SoccerGameManager : Photon.MonoBehaviour {

    public SoccerBallController soccerBall;
    public PlayerNetworkMover[] soccerPlayers;

    public static SoccerGameManager instance = null;              //Static instance of GameManager which allows it to be accessed by any other script.
    
    //Awake is always called before any Start functions
    void Awake()
    {
        //Check if instance already exists
        if (instance == null)

            //if not, set instance to this
            instance = this;

        //If instance already exists and it's not this:
        else if (instance != this)

            //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
            Destroy(gameObject);

        //Sets this to not be destroyed when reloading scene
        DontDestroyOnLoad(gameObject);
    }



    //Update is called every frame.
    void Update()
    {

    }

    [PunRPC]
    public void ResetGame()
    {
        soccerBall.resetGame();

        if (photonView.isMine)
        {
            photonView.RPC("ResetGame", PhotonTargets.Others, null);
        }
    }

    public void AddSoccerPlayer(PlayerNetworkMover pnm)
    {
        
    }

}
