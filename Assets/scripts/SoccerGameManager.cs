using UnityEngine;
using System.Collections;

public class SoccerGameManager : Photon.MonoBehaviour {

    public SoccerBallController soccerBall;

    public static SoccerGameManager instance = null;              //Static instance of GameManager which allows it to be accessed by any other script.

    int localScore = 0;
    int visitorScore = 0;

    public TextMesh Score;

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
    public void AddGoal(PunTeams.Team team)
    {
        if (team == PunTeams.Team.blue)
        {
            localScore++;
        }
        else
        {
            visitorScore++;
        }

        RestoreGame();

        photonView.RPC("AddGoal", PhotonTargets.Others, (int)team);
    }

        
    public void ResetGame()
    {
        photonView.RPC("NewGame", PhotonTargets.All, null);
    }

    [PunRPC]
    void RestoreGame()
    {
        soccerBall.resetGame();

        RespawnPlayersPosition();
    }

    [PunRPC]
    void NewGame()
    {
        soccerBall.resetGame();

        localScore = 0;
        visitorScore = 0;

        RespawnPlayersPosition();
    }

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            stream.SendNext(localScore);
            stream.SendNext(visitorScore);
        }
        else
        {
            localScore = (int)stream.ReceiveNext();
            visitorScore = (int)stream.ReceiveNext();
        }

        printScore();
    }

    void printScore()
    {
        Score.text = localScore + " - " + visitorScore;
    }

    void RespawnPlayersPosition()
    {
        // TODO: respawn player to initial position
    }
}
