using UnityEngine;
using System.Collections;

public class SoccerGoalTrigger : MonoBehaviour
{
    public PunTeams.Team team;
	
    void OnTriggerEnter(Collider other)
    {
        PlayerNetworkMover pnm = other.gameObject.GetComponent<PlayerNetworkMover>();
        if (pnm & pnm.hasBall)
        {
            SoccerGameManager.instance.AddGoal(team);
            pnm.hasBall = false;
        }
    }
}
