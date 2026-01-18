using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.UIElements;
using static GM1vs1;

public class P1vs1 : Agent
{
    private Rigidbody rb;
    public Rigidbody ballRb;
    public Transform racketPivot;

    public P1vs1 opponent;
    public GM1vs1 gameManager;

    public Transform servingPosition;

    public int teamId;
    public bool isPlayerServing;

    public float moveSpeed = 0.2f;
    private float lastDistanceToBall;

    //Reward
    private const float racketHitReward = 2f;
    private const float goodTimingReward = 0.30f;
    private const float movementReward = 0.1f;

    //Penality
    private const float tooClosePenality = -0.1f;
    private const float inactivityPenality = -0.05f;

    public void SetIsServing(bool player)
    {
        isPlayerServing = player;
    }

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
    }


    public override void OnEpisodeBegin()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        transform.position = gameManager.GetPlayerSpawnTransform(this).position;

        lastDistanceToBall = Vector3.Distance(rb.position, ballRb.position);
    }


    public override void CollectObservations(VectorSensor sensor)
    {
        // Self
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(rb.linearVelocity);

        // Ball
        sensor.AddObservation(ballRb.position - transform.position);
        sensor.AddObservation(ballRb.linearVelocity);

        // Context
        sensor.AddObservation(isPlayerServing ? 1f : 0f);

        // Opponent 
        sensor.AddObservation(opponent.transform.position - transform.position);
        sensor.AddObservation(opponent.rb.linearVelocity);
    }


    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];
        float swing = actions.ContinuousActions[3];

        // Movement
        Vector3 move = new Vector3(moveX, 0, moveZ);
        rb.AddForce(move * moveSpeed, ForceMode.VelocityChange);

        // Swing
        HandleSwing(swing);

        // reward for moving towards the ball
        bool mustReceive =
        gameManager.ball.isReceivable &&
        gameManager.ball.expectedReceiver ==
            (teamId == 0 ? Team1vs1.Red : Team1vs1.Blue);

        if (mustReceive)
        {
            float currentDist = Vector3.Distance(rb.position, ballRb.position);
            float delta = lastDistanceToBall - currentDist;

            AddReward(delta * movementReward); // reward if the agent moves towards the ball

            lastDistanceToBall = currentDist;

            if (rb.linearVelocity.magnitude < 0.1f)
            {
                AddReward(inactivityPenality);
            }
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("net") || collision.gameObject.CompareTag("grid") || collision.gameObject.CompareTag("courtNet"))
        {
            AddReward(tooClosePenality);
        }
    }

    void HandleSwing(float swingInput)
    {
        if (swingInput > 0.5f)
        {
            racketPivot.localRotation = Quaternion.Euler(-30f, 0, 0);
        }
        else
            racketPivot.localRotation = Quaternion.identity;
    }

    public void OnRacketHit(Collision collision)
    {
        float height = collision.gameObject.GetComponent<Rigidbody>().position.y;

        if (height > -0.63f && height < 1f)
        {
            AddReward(goodTimingReward); // good timing
        }

        Vector3 dir = (ballRb.position - rb.position).normalized;
        dir.y = 0.3f;

        ballRb.linearVelocity = Vector3.zero;
        ballRb.AddForce(dir * 8f, ForceMode.VelocityChange);

        AddReward(racketHitReward);
    }

    public void ResetDistanceToBall()
    {
        lastDistanceToBall = Vector3.Distance(rb.position, ballRb.position);
    }
}
