using System;
using Unity.MLAgents;
using UnityEngine;
using static GameManager;
using static GM1vs1;

public class BallController : MonoBehaviour
{
    private Rigidbody ballRb;
    public GameManager gm;
    public GM1vs1 gameManager;

    private int bounceCount = 0;

    public bool isServeBall = false;
    public bool isReceivable = false;

    private float maxTime = 20f;
    public float timer = 0f;
    private int receptionRed = 0;
    private int receptionBlue = 0;

    public Transform BallRedServing;
    public Transform BallBlueServing;

    public enum CourtSide { None, Red, Blue }
    public CourtSide lastBounceSide = CourtSide.None;

    public Team1vs1 lastTeamTouched = Team1vs1.None;

    public Team1vs1 expectedReceiver = Team1vs1.None;

    // penalty to the opponent when the agent take the ball
    private const float opponentPenalty = -0.01f;
    private const float bodyCollisionPenalty = -0.3f;

    // reward for good reception
    private const float receptionReward = 0.5f;

    private void Awake()
    {
        ballRb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer > maxTime)
        {
            timer = 0f;
            Debug.Log("exceeded time");
            gameManager.EndRally();
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        timer = 0f;
        receptionBlue = 0;
        receptionRed = 0;

        if (collision.CompareTag("redGround"))
        {
            //Debug.Log("red ground touched");
            HandleGroundBounce(CourtSide.Red);
        }
        else if (collision.CompareTag("blueGround"))
        {
            //Debug.Log("blue ground touched");
            HandleGroundBounce(CourtSide.Blue);
        }
        else if (collision.CompareTag("outField"))
        {
            //Debug.Log("outside ground touched");
            isReceivable = false;
            gameManager.OnOutFieldTouched(lastTeamTouched, isServeBall);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        timer = 0f;
        if (collision.gameObject.CompareTag("redRacket"))
        {
            lastTeamTouched = Team1vs1.Red;
            expectedReceiver = Team1vs1.Blue;
            receptionRed++;

            if (isReceivable)
            {
                gameManager.AssignReward(lastTeamTouched, receptionReward*receptionRed*0.05f);
                Debug.Log("reception red "+receptionRed);
            }
                
            bounceCount = 0; 
            isServeBall = false;
            isReceivable = true;

            gameManager.blueTeam.ResetDistanceToBall();
            gameManager.AssignReward(expectedReceiver, opponentPenalty);
        }
        else if (collision.gameObject.CompareTag("blueRacket"))
        {
            lastTeamTouched = Team1vs1.Blue;
            expectedReceiver = Team1vs1.Red;
            receptionBlue++;

            if (isReceivable)
            {
                gameManager.AssignReward(lastTeamTouched, receptionReward*receptionBlue*0.5f);
                Debug.Log("reception blue "+receptionBlue);
            }

            bounceCount = 0; 
            isServeBall = false;
            isReceivable = true;

            gameManager.redTeam.ResetDistanceToBall();
            gameManager.AssignReward(expectedReceiver, opponentPenalty);
        }
        if (collision.gameObject.CompareTag("courtNet"))
        {
            receptionBlue = 0;
            receptionRed = 0;
            isReceivable = false;

            if (bounceCount == 1)
            {
                bounceCount++;
            } else
            {
                bounceCount = 0;
                gameManager.OnOutFieldTouched(lastTeamTouched, isServeBall);
            }
        }
        if (collision.gameObject.CompareTag("net"))
        {
            receptionBlue = 0;
            receptionRed = 0;
            isReceivable = false;
            bounceCount = 0;

            gameManager.OnOutFieldTouched(lastTeamTouched, isServeBall);
        }
        if (collision.gameObject.CompareTag("Body"))
        {
            gameManager.AssignReward(lastTeamTouched, bodyCollisionPenalty);
        }
    }
/*
    public void StartServe()
    {
        ballRb.isKinematic = false;
        isServeBall = false;
        //gameManager.gameState = GameState.Rally;
    }


    public void PlaceBall(Team servingTeam)
    {
        bounceCount = 0;

        ballRb.isKinematic = true;

        if (servingTeam == Team.Red)
            ballRb.position = BallRedServing.position;
        else
            ballRb.position = BallBlueServing.position;

        isServeBall = true;
    }
*/
    void HandleGroundBounce(CourtSide lastBounceSide)
    {
        if (lastTeamTouched == Team1vs1.None)
        {
            isReceivable = false;
            gameManager.InvalidBounce(Team1vs1.None);
            Debug.Log("None");
            return;
        }

        CourtSide expectedSide = lastTeamTouched == Team1vs1.Red ? CourtSide.Blue : CourtSide.Red;

        if (lastBounceSide != expectedSide)
        {
            isReceivable = false;
            gameManager.InvalidBounce(lastTeamTouched);
        }

        bounceCount++;
        
        if (bounceCount == 1)
        {
            gameManager.ValidBounce(lastTeamTouched);
        }
        else if (bounceCount >= 2)
        {
            isReceivable = false;
            gameManager.OnBounceExceed(lastBounceSide);
        } 
    }


}
