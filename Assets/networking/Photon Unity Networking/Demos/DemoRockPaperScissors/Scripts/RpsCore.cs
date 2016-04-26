using System;
using System.Collections;
using Photon;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

// the Photon server assigns a ActorNumber (player.ID) to each player, beginning at 1
// for this game, we don't mind the actual number
// this game uses player 0 and 1, so clients need to figure out their number somehow


public class RpsCore : PunBehaviour, IPunTurnManagerCallbacks
{
    [SerializeField]
    private Text TurnText;

    [SerializeField]
    private Text TimeText;

    [SerializeField]
    private Text RemotePlayerText;

    [SerializeField]
    private Text LocalPlayerText;
    
    [SerializeField]
    private Image WinOrLossImage;


    [SerializeField]
    private Image localSelectionImage;
    public Hand localSelection;

    [SerializeField]
    private Image remoteSelectionImage;
    public Hand remoteSelection;


    [SerializeField]
    private Sprite SelectedRock;

    [SerializeField]
    private Sprite SelectedPaper;

    [SerializeField]
    private Sprite SelectedScissors;

    [SerializeField]
    private Sprite SpriteWin;

    [SerializeField]
    private Sprite SpriteLose;

    [SerializeField]
    private Sprite SpriteDraw;


    [SerializeField]
    private RectTransform DisconnectedPanel;

    private ResultType result;

    private PunTurnManager turnManager;
    public Hand randomHand;    // used to show remote player's "hand" while local player didn't select anything

    public enum Hand
    {
        None = 0,
        Rock,
        Paper,
        Scissors
    }

    public enum ResultType
    {
        None = 0,
        Draw,
        LocalWin,
        LocalLoss
    }

    public void Start()
    {
        this.turnManager = this.gameObject.AddComponent<PunTurnManager>();
        this.turnManager.TurnManagerListener = this;
        this.turnManager.TurnDuration = 5;
        

        this.localSelectionImage.gameObject.SetActive(false);
        this.remoteSelectionImage.gameObject.SetActive(false);
        this.StartCoroutine("CycleRemoteHandCoroutine");

        // when we play this scene in the editor (without the other scene), make sure we connect, too
        if (!PhotonNetwork.connected && !PhotonNetwork.connecting)
        {
            RpsDemoConnect dc = gameObject.AddComponent<RpsDemoConnect>();
            dc.ApplyUserIdAndConnect();
        }
    }

    public void Update()
    {
        // for debugging, it's useful to have a few actions tied to keys:
        if (Input.GetKeyUp(KeyCode.L))
        {
            PhotonNetwork.LeaveRoom();
        }
        if (Input.GetKeyUp(KeyCode.C))
        {
            PhotonNetwork.ConnectUsingSettings(null);
            PhotonHandler.StopFallbackSendAckThread();
        }

        // disable the "reconnect panel" if PUN is connected or connecting
        if (PhotonNetwork.connected && this.DisconnectedPanel.gameObject.GetActive())
        {
            this.DisconnectedPanel.gameObject.SetActive(false);
        }
        if (!PhotonNetwork.connected && !PhotonNetwork.connecting && !this.DisconnectedPanel.gameObject.GetActive())
        {
            this.DisconnectedPanel.gameObject.SetActive(true);
        }

        if (PhotonNetwork.inRoom)
        {
            if (this.TurnText != null)
            {
                this.TurnText.text = this.turnManager.Turn.ToString();
            }
            if (this.turnManager.Turn > 0 && this.TimeText != null)
            {
                float turnEnd = this.turnManager.GetRemainingSeconds();
                this.TimeText.text = turnEnd.ToString("F1") + " SECONDS";
            }
            this.UpdatePlayerTexts();


            // show local player's selected hand
            Sprite selected = SelectionToSprite(this.localSelection);
            if (selected != null)
            {
                this.localSelectionImage.gameObject.SetActive(true);
                this.localSelectionImage.sprite = selected;
            }

            // remote player's selection is only shown, when the turn is complete (finished by both)
            if (this.turnManager.IsCompletedByAll)
            {
                selected = SelectionToSprite(this.remoteSelection);
                if (selected != null)
                {
                    this.remoteSelectionImage.color = new Color(1,1,1,1);
                    this.remoteSelectionImage.sprite = selected;
                }
            }
            else
            {
                if (PhotonNetwork.room.playerCount < 2)
                {
                    this.remoteSelectionImage.color = new Color(1, 1, 1, 0);
                }

                // if the turn is not completed by all, we use a random image for the remote hand
                else if (this.turnManager.Turn > 0 && !this.turnManager.IsCompletedByAll)
                {
                    // alpha of the remote hand is used as indicator if the remote player "is active" and "made a turn"
                    PhotonPlayer remote = PhotonNetwork.player.GetNext();
                    float alpha = 0.5f;
                    if (this.turnManager.GetPlayerFinishedTurn(remote))
                    {
                        alpha = 1;
                    }
                    if (remote != null && remote.isInactive)
                    {
                        alpha = 0.1f;
                    }

                    this.remoteSelectionImage.color = new Color(1, 1, 1, alpha);
                    this.remoteSelectionImage.sprite = SelectionToSprite(randomHand);
                }
            }
        }
    }

    #region TurnManager Callbacks

    /// <summary>Called when a turn begins (Master Client set a new Turn number).</summary>
    public void OnTurnBegins(int turn)
    {
        Debug.Log("OnTurnBegins() turn: "+ turn);
        this.localSelection = Hand.None;
        this.remoteSelection = Hand.None;

        this.WinOrLossImage.gameObject.SetActive(false);

        this.localSelectionImage.gameObject.SetActive(false);
        this.remoteSelectionImage.gameObject.SetActive(true);
    }


    public void OnTurnCompleted(int obj)
    {
        Debug.Log("OnTurnCompleted: " + obj);

        this.CalculateWinAndLoss();
        this.UpdateScores();
        this.OnEndTurn();
    }


    // when a player moved (but did not finish the turn)
    public void OnPlayerMove(PhotonPlayer photonPlayer, int turn, object move)
    {
        Debug.Log("OnPlayerMove: " + photonPlayer + " turn: " + turn + " action: " + move);
        throw new NotImplementedException();
    }


    // when a player made the last/final move in a turn
    public void OnPlayerFinished(PhotonPlayer photonPlayer, int turn, object move)
    {
        Debug.Log("OnTurnFinished: " + photonPlayer + " turn: " + turn + " action: " + move);

        if (photonPlayer.isLocal)
        {
            this.localSelection = (Hand)(byte)move;
        }
        else
        {
            this.remoteSelection = (Hand)(byte)move;
        }
    }



    public void OnTurnTimeEnds(int obj)
    {
        Debug.Log("OnTurnTimeout");
    }

    private void UpdateScores()
    {
        if (this.result == ResultType.LocalWin)
        {
            PhotonNetwork.player.AddScore(1);   // this is an extension method for PhotonPlayer. you can see it's implementation
        }
    }

    #endregion

    #region Core Gameplay Methods

    
    /// <summary>Call to start the turn (only the Master Client will send this).</summary>
    public void StartTurn()
    {
        if (PhotonNetwork.isMasterClient)
        {
            this.turnManager.BeginTurn();
        }
    }


    public void MakeTurn(Hand selection)
    {
        this.turnManager.SendMove((byte)selection, true);
    }


    public void OnReceivedTurn()
    {
    }


    public void OnEndTurn()
    {
        this.StartCoroutine("ShowResultsBeginNextTurnCoroutine");
    }

    public IEnumerator ShowResultsBeginNextTurnCoroutine()
    {
        yield return new WaitForSeconds(1.5f);

        if (this.result == ResultType.Draw)
        {
            this.WinOrLossImage.sprite = this.SpriteDraw;
        }
        else
        {
            this.WinOrLossImage.sprite = this.result == ResultType.LocalWin ? this.SpriteWin : SpriteLose;
        }
        this.WinOrLossImage.gameObject.SetActive(true);

        yield return new WaitForSeconds(2.0f);
        this.StartTurn();
    }


    public void EndGame()
    {
    }

    private void CalculateWinAndLoss()
    {
        this.result = ResultType.Draw;
        if (this.localSelection == this.remoteSelection)
        {
            return;
        }
        
        if (this.localSelection == Hand.Rock)
        {
            this.result = (this.remoteSelection == Hand.Scissors) ? ResultType.LocalWin : ResultType.LocalLoss;
        }
        if (this.localSelection == Hand.Paper)
        {
            this.result = (this.remoteSelection == Hand.Rock) ? ResultType.LocalWin : ResultType.LocalLoss;
        }

        if (this.localSelection == Hand.Scissors)
        {
            this.result = (this.remoteSelection == Hand.Paper) ? ResultType.LocalWin : ResultType.LocalLoss;
        }
    }

    private Sprite SelectionToSprite(Hand hand)
    {
        switch (hand)
        {
            case Hand.None:
                break;
            case Hand.Rock:
                return this.SelectedRock;
            case Hand.Paper:
                return this.SelectedPaper;
            case Hand.Scissors:
                return this.SelectedScissors;
        }

        return null;
    }

    private void UpdatePlayerTexts()
    {
        PhotonPlayer remote = PhotonNetwork.player.GetNext();
        PhotonPlayer local = PhotonNetwork.player;

        if (remote != null)
        {
            // should be this format: "name        00"
            this.RemotePlayerText.text = remote.name + "        " + remote.GetScore().ToString("D2");
        }
        else
        {
            this.RemotePlayerText.text = "waiting for another player        00";
        }
        
        if (local != null)
        {
            // should be this format: "YOU   00"
            this.LocalPlayerText.text = "YOU   " + local.GetScore().ToString("D2");
        }
    }

    public IEnumerator CycleRemoteHandCoroutine()
    {
        while (true)
        {
            // cycle through available images
            this.randomHand = (Hand)Random.Range(1, 4);
            yield return new WaitForSeconds(0.5f);
        }
    }

    #endregion


    #region Handling Of Buttons

    public void OnClickRock()
    {
        this.MakeTurn(Hand.Rock);
    }

    public void OnClickPaper()
    {
       this.MakeTurn(Hand.Paper);
    }

    public void OnClickScissors()
    {
        this.MakeTurn(Hand.Scissors);
    }

    public void OnClickConnect()
    {
        PhotonNetwork.ConnectUsingSettings(null);
        PhotonHandler.StopFallbackSendAckThread();  // this is used in the demo to timeout in background!
    }
    
    public void OnClickReConnectAndRejoin()
    {
        PhotonNetwork.ReconnectAndRejoin();
        PhotonHandler.StopFallbackSendAckThread();  // this is used in the demo to timeout in background!
    }

    #endregion

    public override void OnLeftRoom()
    {
        Debug.Log("OnLeftRoom()");
    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.room.playerCount == 2)
        {
            if (this.turnManager.Turn == 0)
            {
                // when the room has two players, start the first turn (later on, joining players won't trigger a turn)
                this.StartTurn();
            }
        }
        else
        {
            Debug.Log("Waiting for another player");
        }
    }

    public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        Debug.Log("Other player arrived");

        if (PhotonNetwork.room.playerCount == 2)
        {
            if (this.turnManager.Turn == 0)
            {
                // when the room has two players, start the first turn (later on, joining players won't trigger a turn)
                this.StartTurn();
            }
        }
    }


    public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
    {
        Debug.Log("Other player disconnected! isInactive: " + otherPlayer.isInactive);
    }


    public override void OnConnectionFail(DisconnectCause cause)
    {
        this.DisconnectedPanel.gameObject.SetActive(true);
    }

}
