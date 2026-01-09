using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static BallController;

public class GameManager : MonoBehaviour
{
    public BallController ball;
    public List<PlayerAgent> redTeam;
    public List<PlayerAgent> blueTeam;
    public TeamManager teamManager;

    public Transform redServePosition;
    public Transform redNotServePosition1;
    public Transform redNotServePosition2;
    public Transform blueServePosition;
    public Transform blueNotServePosition1;
    public Transform blueNotServePosition2;

    public float resetDelay = 0.5f;
    private bool resetting = false;

    private float maxServeTime = 20f;
    private float serveTimer;
    private int servingIndex = 0; 


    public enum Team { None, Red, Blue }
    private Team servingTeam = Team.Red;

    public enum ServingPlayer {  Player1R, Player2R, Player1B, Player2B };
    private ServingPlayer servingPlayer = ServingPlayer.Player1R;

    public enum GameState { Serve, Rally }
    public GameState gameState = GameState.Serve;

    // Reward
    private const float winningReward = 1f;
    private const float validBounceReward = 0.1f;

    // Penalty
    private const float loserPenalty = -1f;
    private const float netTouchedPenalty = -0.5f;
    private const float invalidBouncePenalty = -0.2f;
    private const float serveTimeExceededPenalty = 0.01f;

    void Start()
    {
        /*redTeam[0].SetIsServing(true, true, redServePosition);
        redTeam[1].SetIsServing(true, false, redNotServePosition2);
        blueTeam[0].SetIsServing (false, false, blueNotServePosition1);
        blueTeam[1].SetIsServing(false, false, blueNotServePosition2);

        ball.PlaceBall(redServePosition);
        ball.StartServe();*/
        SetServe(servingTeam);
    }

    void Update()
    {
        if (gameState == GameState.Serve)
        {
            serveTimer += Time.deltaTime;

            if (serveTimer > maxServeTime)
            {
                // fallo di servizio

                AssignTeamReward(servingTeam, serveTimeExceededPenalty);

                EndRally();
            }
        }
    }


    public Transform GetSpawnTransform(PlayerAgent agent)
    {
        if (redTeam.Contains(agent))
        {
            if(servingTeam == Team.Red)
            {
                if (agent.isPlayerServing)
                    return redServePosition;
                else 
                    return redNotServePosition2;
            }
            else
            {
                if (redTeam[0] == agent)
                    return redNotServePosition1;
                else
                    return redNotServePosition2;
            }
        } 
        else 
        {
            if (servingTeam == Team.Blue)
            {
                if (agent.isPlayerServing)
                    return blueServePosition;
                else
                    return blueNotServePosition2;
            }
            else
            {
                if (blueTeam[0] == agent)
                    return blueNotServePosition1;
                else
                    return blueNotServePosition2;
            }
        }
    }

    public void StartRally()
    {
        //ball.StartServe();
        gameState = GameState.Rally;
    }

    public void OnBallHitGround(Vector3 position)
    {
        if (resetting) return;

        Team losingTeam = position.x < 0 ? Team.Red : Team.Blue;
        Team winningTeam = losingTeam == Team.Red ? Team.Blue : Team.Red;

        AssignTeamReward(winningTeam, winningReward);
        AssignTeamReward(losingTeam, loserPenalty);

        EndRally();
    }

    public void ValidBounce(Team team)
    {
        AssignTeamReward(team, validBounceReward);
    }

    public void InvalidBounce(Team team)
    {
        AssignTeamReward(team, invalidBouncePenalty);

        EndRally();    
    }

    public void OnBounceExceed()
    {
        Debug.Log("bounce exceed");
        if (ball.lastBounceSide == CourtSide.Red)
        {
            // rimbalza 2 volte nel campo rosso → punto BLU
            AssignTeamReward(Team.Blue, winningReward);
            AssignTeamReward(Team.Red, loserPenalty);
        }
        else if (ball.lastBounceSide == CourtSide.Blue)
        {
            // rimbalza 2 volte nel campo blu → punto ROSSO
            AssignTeamReward(Team.Red, winningReward);
            AssignTeamReward(Team.Blue, loserPenalty);
        }

        EndRally();
    }

    public void OnOutFieldTouched(CourtSide side)
    {
        // se esce dal campo rosso → punto blu
        Team winner = side == CourtSide.Red ? Team.Blue : Team.Red;
        Team loser = winner == Team.Red ? Team.Blue : Team.Red;

        servingTeam = winner;

        AssignTeamReward(winner, winningReward);
        AssignTeamReward(loser, loserPenalty);

        EndRally();
    }

    public void OnNetTouched(Team team)
    {
        AssignTeamReward(team, netTouchedPenalty);

        EndRally();
    }

    void AssignTeamReward(Team team, float reward)
    {
        var agents = team == Team.Red ? redTeam : blueTeam;
        foreach (var agent in agents)
        {
            agent.AddReward(reward);
        }
    }

    void EndRally()
    {
        resetting = true;

        servingTeam = servingTeam == Team.Red ? Team.Blue : Team.Red;

        SetServe(servingTeam);

        foreach (var agent in redTeam)
            agent.EndEpisode();

        foreach (var agent in blueTeam)
            agent.EndEpisode();

        //Invoke(nameof(ResetRally), resetDelay);
        ResetRally();
    }


    void SetServe(Team team)
    {
        servingIndex = 1 - servingIndex; // alterna 0 ↔ 1

        foreach (var p in redTeam)
            p.SetIsServing(team == Team.Red, false);

        foreach (var p in blueTeam)
            p.SetIsServing(team == Team.Blue, false);

        if (team == Team.Red)
            redTeam[servingIndex].SetIsServing(true, true);
        else
            blueTeam[servingIndex].SetIsServing(true, true);
    }


    void ResetRally()
    {

        //PlayerAgent agent = GetCurrentServingPlayer();
        
        serveTimer = 0f;
        //ball.PlaceBall(servingTeam);
        //ball.StartServe();

        gameState = GameState.Serve;
        resetting = false;
    }

}
