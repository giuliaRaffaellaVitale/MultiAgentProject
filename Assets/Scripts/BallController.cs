using System;
using UnityEngine;
using static GameManager;

public class BallController : MonoBehaviour
{
    private Rigidbody ballRb;
    public GameManager gameManager;

    private int bounceCount = 0;

    public bool isServeBall = false;

    public enum CourtSide { None, Red, Blue }
    public CourtSide lastBounceSide = CourtSide.None;

    public Team lastTeamTouched = Team.None;


    private void Awake()
    {
        ballRb = GetComponent<Rigidbody>();
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("redGround"))
        {
            HandleGroundBounce(CourtSide.Red);
        }
        else if (collision.CompareTag("blueGround"))
        {
            HandleGroundBounce(CourtSide.Blue);
        }
        else if (collision.CompareTag("outField"))
        {
            gameManager.OnOutFieldTouched(lastBounceSide);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("redRacket"))
        {
            lastTeamTouched = Team.Red;
            bounceCount = 0; 
        }
        else if (collision.gameObject.CompareTag("blueRacket"))
        {
            lastTeamTouched = Team.Blue;
            bounceCount = 0; 
        }
        else if (collision.gameObject.CompareTag("courtNet"))
        {
            if (bounceCount == 1)
            {
                bounceCount++;
            } else
            {
                gameManager.OnNetTouched(lastTeamTouched);
            }
        } else if (collision.gameObject.CompareTag("net"))
        {
            gameManager.OnNetTouched(lastTeamTouched);
        }
    }

    public void StartServe()
    {
        ballRb.isKinematic = false;
        isServeBall = false;
        gameManager.gameState = GameState.Rally;
    }


    public void PlaceBall(Transform pos)
    {
        bounceCount = 0;
        ballRb.linearVelocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;

        ballRb.isKinematic = true;

        ballRb.position = 
            pos.position + 
            Vector3.forward * 0.3f + 
            Vector3.up * 0.8f;

        isServeBall = true;
    }

    void HandleGroundBounce(CourtSide side)
    {
        bounceCount++;
        lastBounceSide = side;

        Team expectedHitter = side == CourtSide.Red ? Team.Blue : Team.Red;

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
            gameManager.OnBounceExceed();
        }
    }


}
