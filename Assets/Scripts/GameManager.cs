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
    public Transform redNotServePosition;
    public Transform blueServePosition;
    public Transform blueNotServePosition;

    public float resetDelay = 1.5f;
    private bool resetting = false;

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

    void Start()
    {
        redTeam[0].SetIsServing(true, true, redServePosition);
        redTeam[1].SetIsServing(true, false, redNotServePosition);
        foreach (var agent in blueTeam)
        {
            agent.SetIsServing(false, false, null);
        }

        ball.PlaceBall(redServePosition);
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

        foreach (var agent in redTeam)
            agent.EndEpisode();

        foreach (var agent in blueTeam)
            agent.EndEpisode();

        Invoke(nameof(ResetRally), resetDelay);
    }

    void SetServe(Team servingTeam)
    {
        if (servingTeam == Team.Red)
        {
            // alternance of servings to balance training
            servingPlayer = servingPlayer == ServingPlayer.Player1R ? ServingPlayer.Player2R : ServingPlayer.Player1R;
            if (servingPlayer == ServingPlayer.Player2R)
            {
                redTeam[1].SetIsServing(true, true, redServePosition);
                redTeam[0].SetIsServing(true, false, redNotServePosition);
                foreach (var agent in blueTeam)
                {
                    agent.SetIsServing(false, false, null);
                }
            }
            else
            {
                redTeam[0].SetIsServing(true, true, redServePosition);
                redTeam[1].SetIsServing(true, false, redNotServePosition);
                foreach (var agent in blueTeam)
                {
                    agent.SetIsServing(false, false, null);
                }
            }
        }
        else
        {
            // alternance of servings to balance training
            servingPlayer = servingPlayer == ServingPlayer.Player1B ? ServingPlayer.Player2B : ServingPlayer.Player1B;
            if (servingPlayer == ServingPlayer.Player2B)
            {
                blueTeam[1].SetIsServing(true, true, blueServePosition);
                blueTeam[0].SetIsServing(true, false, blueNotServePosition);
                foreach (var agent in redTeam)
                {
                    agent.SetIsServing(false, false, null);
                }
            }
            else
            {
                blueTeam[0].SetIsServing(true, true, blueServePosition);
                blueTeam[1].SetIsServing(true, false, blueNotServePosition);
                foreach (var agent in redTeam)
                {
                    agent.SetIsServing(false, false, null);
                }
            }
        }

    }

    void ResetRally()
    {
        Vector3 servePos = servingTeam == Team.Red
            ? redServePosition.position
            : blueServePosition.position;

        servingTeam = servingTeam == Team.Red ? Team.Blue : Team.Red;

        SetServe(servingTeam);

        var servingPosition = servingTeam == Team.Red ? redServePosition : blueServePosition;
        
        ball.PlaceBall(servingPosition);

        gameState = GameState.Serve;
        resetting = false;
    }

}
