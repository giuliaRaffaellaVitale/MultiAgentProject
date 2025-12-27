using System.Collections.Generic;
using UnityEngine;
using static BallController;

public class GameManager : MonoBehaviour
{
    public BallController ball;

    public List<PlayerAgent> redTeam;
    public List<PlayerAgent> blueTeam;
    public TeamManager teamManager;

    public Transform redServePosition;
    public Transform blueServePosition;

    public float resetDelay = 1.5f;
    private bool resetting = false;

    public enum Team { None, Red, Blue }
    private Team servingTeam = Team.Red;

    // Reward
    private const float winningReward = 1f;

    // Penalty
    private const float loserPenalty = -1f;
    private const float netTouchedPenalty = -0.5f;


    public void OnBallHitGround(Vector3 position)
    {
        if (resetting) return;

        Team losingTeam = position.x < 0 ? Team.Red : Team.Blue;
        Team winningTeam = losingTeam == Team.Red ? Team.Blue : Team.Red;

        AssignTeamReward(winningTeam, winningReward);
        AssignTeamReward(losingTeam, loserPenalty);

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

        AssignTeamReward(winner, winningReward);
        AssignTeamReward(loser, loserPenalty);

        EndRally();
    }

    public void OnNetTouched(Team team)
    {
        AssignTeamReward(team, netTouchedPenalty);
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

    void ResetRally()
    {
        Vector3 servePos = servingTeam == Team.Red
            ? redServePosition.position
            : blueServePosition.position;

        ball.ResetBall(servePos + Vector3.up * 0.5f);

        servingTeam = servingTeam == Team.Red ? Team.Blue : Team.Red;
        resetting = false;
    }

}
