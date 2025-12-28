using UnityEngine;
using static GameManager;

public class BallController : MonoBehaviour
{
    private Rigidbody ballRb;
    private GameManager gameManager;

    private int bounceCount = 0;

    public bool isServeBall = false;

    public enum CourtSide { None, Red, Blue }
    public CourtSide lastBounceSide = CourtSide.None;

    public Team lastTeamTouched = Team.None;


    private void Awake()
    {
        ballRb = GetComponent<Rigidbody>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("redGround"))
            lastBounceSide = CourtSide.Red;

        if (other.CompareTag("blueGround"))
            lastBounceSide = CourtSide.Blue;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("redRacket"))
        {
            lastTeamTouched = Team.Red;
        }
        if (collision.gameObject.CompareTag("blueRacket"))
        {
            lastTeamTouched = Team.Blue;
        }
        if (collision.gameObject.CompareTag("redGround") )
        {
            if (lastTeamTouched == Team.Blue)
            {
                bounceCount++;
                if (bounceCount == 1)
                {
                    gameManager.ValidBounce(lastTeamTouched);
                }
                if (bounceCount > 1)
                {
                    gameManager.OnBounceExceed();
                }
            }
            else
            {
                gameManager.InvalidBounce(lastTeamTouched);
            }
        }
        if (collision.gameObject.CompareTag("blueGround"))
        {
            if (lastTeamTouched == Team.Red)
            {
                bounceCount++;
                if (bounceCount == 1)
                {
                    gameManager.ValidBounce(lastTeamTouched);
                }
                if (bounceCount > 1)
                {
                    gameManager.OnBounceExceed();
                }
            }
            else
            {
                gameManager.InvalidBounce(lastTeamTouched);
            }
        }
        else if (collision.gameObject.CompareTag("outField"))
        {
            gameManager.OnOutFieldTouched(lastBounceSide);

        } else if (collision.gameObject.CompareTag("courtNet"))
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

}
