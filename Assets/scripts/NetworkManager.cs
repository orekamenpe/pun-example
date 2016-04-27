using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class NetworkManager : MonoBehaviour 
{
    [SerializeField] Text connectionText;
    [SerializeField] Transform[] spawnPoints;

    [SerializeField] GameObject serverWindow;
    [SerializeField] InputField username;
    [SerializeField] InputField roomName;
    [SerializeField] InputField roomList;
    [SerializeField] InputField messageWindow;

    GameObject player;
    Queue<string> messages;
    const int messageCount = 6;
    PhotonView photonView;


    void Start () 
    {
        photonView = GetComponent<PhotonView> ();
        messages = new Queue<string> (messageCount);

        PhotonNetwork.logLevel = PhotonLogLevel.Full;
        PhotonNetwork.ConnectUsingSettings ("1.0");
        StartCoroutine ("UpdateConnectionString");
    }

    IEnumerator UpdateConnectionString () 
    {
        while(true)
        {
            connectionText.text = PhotonNetwork.connectionStateDetailed.ToString ();
            yield return null;
        }
    }

    void OnJoinedLobby()
    {
        serverWindow.SetActive (true);

        // random Name
        username.text = "Guest" + Random.Range(1, 9999);
        if (roomList.text != "")
        {
            roomName.text = roomList.text;
        }
        else
        {
            roomName.text = "room1";
        }
    }

    void OnReceivedRoomListUpdate()
    {
        roomList.text = "";
        RoomInfo[] rooms = PhotonNetwork.GetRoomList ();
        foreach(RoomInfo room in rooms)
            roomList.text += room.name + "\n";
    }

    public void JoinRoom()
    {
        PhotonNetwork.player.name = username.text;
        RoomOptions roomOptions = new RoomOptions(){ isVisible = true, maxPlayers = 10 };
        PhotonNetwork.JoinOrCreateRoom (roomName.text, roomOptions, TypedLobby.Default);
    }

    void OnJoinedRoom()
    {
        serverWindow.SetActive (false);
        StopCoroutine ("UpdateConnectionString");
        connectionText.text = "";
        StartSpawnProcess (0f);
    }

    void StartSpawnProcess(float respawnTime)
    {
        StartCoroutine ("SpawnPlayer", respawnTime);
    }

    IEnumerator SpawnPlayer(float respawnTime)
    {
        yield return new WaitForSeconds(respawnTime);

        int index = Random.Range (0, spawnPoints.Length);
        player = PhotonNetwork.Instantiate("SoccerPlayer", spawnPoints[index].position, spawnPoints[index].rotation, 0);
        player.GetComponent<PlayerNetworkMover> ().RespawnMe += StartSpawnProcess;
        player.GetComponent<PlayerNetworkMover> ().SendNetworkMessage += AddMessage;

        AddMessage ("Spawned player: " + PhotonNetwork.player.name);
    }

    void AddMessage(string message)
    {
        photonView.RPC ("AddMessage_RPC", PhotonTargets.All, message);
    }

    [PunRPC]
    void AddMessage_RPC(string message)
    {
        messages.Enqueue (message);
        if(messages.Count > messageCount)
            messages.Dequeue();

        messageWindow.text = "";
        foreach(string m in messages)
            messageWindow.text += m + "\n";
    }
}