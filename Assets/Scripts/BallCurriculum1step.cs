using UnityEngine;

public class BallCurriculum1step : MonoBehaviour
{
    public Rigidbody rb;
    public Player1step agent;

    private const float correctFieldReward = 1.5f;

    private const float outFieldPenalty = -0.35f;
    private const float groundPenalty = -0.05f;
    private const float gridPenalty = -0.35f;
    private const float bodyCollisionPenalty = -0.3f;

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("outField"))
        {
            //Debug.Log("out field");
            agent.AddReward(outFieldPenalty);
            agent.EndEpisode();
        }
        else if (other.gameObject.CompareTag("redGround"))
        {
            Debug.Log("red field");
            agent.AddReward(groundPenalty);
            agent.EndEpisode();
        } else if (other.gameObject.CompareTag("blueGround"))
        {
            Debug.Log("blue field");
            agent.AddReward(correctFieldReward);
            agent.EndEpisode();
        } 
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("grid"))
        {
            agent.AddReward(gridPenalty);
            agent.EndEpisode();
        }

        if (col.gameObject.CompareTag("Body"))
        {

            agent.AddReward(bodyCollisionPenalty);
        }
    }
}
