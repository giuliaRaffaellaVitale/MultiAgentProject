using System.Threading;
using UnityEngine;
using static BallController;
using static GM1vs1;

public class GM1vs1 : MonoBehaviour
{
    public BallController ball;
    public P1vs1 redTeam;
    public P1vs1 blueTeam;

    public Transform redServePosition;
    public Transform blueServePosition;
    public Rigidbody ballRb;

    public Transform ballServeRed;
    public Transform ballServeBlue;

    public float resetDelay = 0.5f;
    private bool resetting = false;
    private int count = 0;

    public enum Team1vs1 { None, Red, Blue }
    private Team1vs1 servingTeam = Team1vs1.Red;

    public enum ServingPlayer { Player1R, Player1B };
    private ServingPlayer servingPlayer = ServingPlayer.Player1R;

    public enum GameState1vs1 { Serve, Rally }
    public GameState1vs1 gameState = GameState1vs1.Serve;

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
        SetServe(servingTeam);
        Time.timeScale = 2f;
    }

    public void SetRally()
    {
        gameState = GameState1vs1.Rally;
        ball.isServeBall = false;
    }


    public Transform GetPlayerSpawnTransform(P1vs1 agent)
    {
        if (redTeam == agent)
        {
            return redServePosition;
        }
        else
        {
            return blueServePosition;
        }
    }

    public Transform GetBallSpawnTransform()
    {
        ball.isServeBall = true;
        if (servingTeam == Team1vs1.Red)
        {
            return ballServeRed;
        }
        else
        {
            return ballServeBlue;
        }
    }


    public void ValidBounce(Team1vs1 team)
    {
        AssignReward(team, validBounceReward);
    }

    public void InvalidBounce(Team1vs1 team)
    {
        AssignReward(team, invalidBouncePenalty);
        
        EndRally();
    }

    public void OnBounceExceed(CourtSide side)
    {
        if (side == CourtSide.Red)
        {
            // rimbalza 2 volte nel campo rosso ? punto BLU
            AssignReward(Team1vs1.Blue, winningReward);
            AssignReward(Team1vs1.Red, loserPenalty);
        }
        else if (ball.lastBounceSide == CourtSide.Blue)
        {
            // rimbalza 2 volte nel campo blu ? punto ROSSO
            AssignReward(Team1vs1.Red, winningReward);
            AssignReward(Team1vs1.Blue, loserPenalty);
        }

        EndRally();
    }

    public void OnOutFieldTouched(Team1vs1 team, bool isServeBall)
    {
        Team1vs1 winner;
        Team1vs1 loser;

        if (isServeBall)
        {
            loser = servingTeam;
        } else
        {
            loser = team;
        }

        winner = loser == Team1vs1.Red ? Team1vs1.Blue : Team1vs1.Red;

        AssignReward(winner, winningReward);
        AssignReward(loser, loserPenalty);

        EndRally();
    }

    public void AssignReward(Team1vs1 team, float reward)
    {
        var agent = team == Team1vs1.Red ? redTeam : blueTeam;
        agent.AddReward(reward);

    }

    public void EndRally()
    {
        resetting = true;

        servingTeam = servingTeam == Team1vs1.Red ? Team1vs1.Blue : Team1vs1.Red;

        SetServe(servingTeam);

        ResetRally();

        count++;
        
        redTeam.EndEpisode();
        blueTeam.EndEpisode();

        ballRb.isKinematic = false;
        //Invoke(nameof(ResetRally), resetDelay);

    }


    void SetServe(Team1vs1 team)
    {
        redTeam.SetIsServing(team == Team1vs1.Red);

        blueTeam.SetIsServing(team == Team1vs1.Blue);
    }


    void ResetRally()
    {
        gameState = GameState1vs1.Serve;

        ballRb.linearVelocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;

        ballRb.isKinematic = true;  

        ballRb.transform.position = GetBallSpawnTransform().position;
        ball.timer = 0f;

        resetting = false;
    }
}
