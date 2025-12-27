using UnityEngine;
using static GameManager;

public class BallController : MonoBehaviour
{
    private Rigidbody ballRb;
    private GameManager gameManager;

    private int bounceCount = 0;

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
        if (collision.gameObject.CompareTag("ground"))
        {
            bounceCount++;
            if (bounceCount > 1)
            {
                gameManager.OnBounceExceed();
            }

        } else if (collision.gameObject.CompareTag("outField"))
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

    public void ResetBall(Vector3 position)
    {
        bounceCount = 0;
        ballRb.linearVelocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;
        transform.position = position;
    }

}
