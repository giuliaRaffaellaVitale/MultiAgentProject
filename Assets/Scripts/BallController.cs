using System;
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
    public Transform BallRedServing;
    public Transform BallBlueServing;

    public enum CourtSide { None, Red, Blue }
    public CourtSide lastBounceSide = CourtSide.None;

    public Team1vs1 lastTeamTouched = Team1vs1.None;


    private void Awake()
    {
        ballRb = GetComponent<Rigidbody>();
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("redGround"))
        {
            Debug.Log("red ground touched");
            HandleGroundBounce(CourtSide.Red);
        }
        else if (collision.CompareTag("blueGround"))
        {
            Debug.Log("blue ground touched");
            HandleGroundBounce(CourtSide.Blue);
        }
        else if (collision.CompareTag("outField"))
        {
            Debug.Log("outside ground touched");
            gameManager.OnOutFieldTouched(lastTeamTouched, isServeBall);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("redRacket"))
        {
            lastTeamTouched = Team1vs1.Red;
            bounceCount = 0; 
            isServeBall = false;
        }
        else if (collision.gameObject.CompareTag("blueRacket"))
        {
            lastTeamTouched = Team1vs1.Blue;
            bounceCount = 0; 
            isServeBall = false;
        }
        if (collision.gameObject.CompareTag("courtNet"))
        {
            if (bounceCount == 0)
            {
                bounceCount++;
            } else
            {
                gameManager.OnOutFieldTouched(lastTeamTouched, isServeBall);
            }
        }
        if (collision.gameObject.CompareTag("net"))
        {
            gameManager.OnOutFieldTouched(lastTeamTouched, isServeBall);
        }
    }

    public void StartServe()
    {
        ballRb.isKinematic = false;
        isServeBall = false;
        //gameManager.gameState = GameState.Rally;
    }


    public void PlaceBall(Team servingTeam)
    {
        bounceCount = 0;
        //ballRb.linearVelocity = Vector3.zero;
        //ballRb.angularVelocity = Vector3.zero;

        ballRb.isKinematic = true;

        /*
        Transform racket = agent.racketPivot;

        Vector3 spawnPos =
            racket.position +
            racket.forward * 0.25f +   // davanti alla racchetta
            racket.up * 0.1f +         // leggermente sopra
            racket.right * 0.05f;       // leggermente verso destra
        */

        if (servingTeam == Team.Red)
            ballRb.position = BallRedServing.position;
        else
            ballRb.position = BallBlueServing.position;

        isServeBall = true;
    }

    void HandleGroundBounce(CourtSide side)
    {
        bounceCount++;
        Debug.Log("bounce: "+bounceCount);

        lastBounceSide = side;

        Team1vs1 expectedHitter = side == CourtSide.Red ? Team1vs1.Blue : Team1vs1.Red;

        if (lastTeamTouched != expectedHitter)
        {
            gameManager.InvalidBounce(lastTeamTouched);
            return;
        }
        if (bounceCount == 1)
        {
            gameManager.ValidBounce(lastTeamTouched);
        }
        else if (bounceCount >= 2)
        {
            Debug.Log("bounce exceeded");
            gameManager.OnBounceExceed();
        } 
    }


}
