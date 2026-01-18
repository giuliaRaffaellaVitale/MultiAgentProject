using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class Player1step : Agent
{
    private Rigidbody rb;
    public Rigidbody ballRb;
    public Transform racketPivot;

    public Transform redServingPosition;
    public Transform ballRedServing;
    public Transform blueField;

    private const float moveSpeed = 1f;
    public float rotationSpeed = 180f;
    private float lastDistanceToBall;

    //Reward
    private const float racketHitReward = 1f;
    private const float goodTimingReward = 0.1f;
    private const float movementReward = 0.01f;

    //Penality
    private const float tooClosePenality = -0.05f;
    

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
    }


    public override void OnEpisodeBegin()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        transform.position = redServingPosition.position;

        ballRb.linearVelocity = Vector3.zero;
        ballRb.position = ballRedServing.position;

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

        // Phase 1, neutral observation to use the same observation size of phase 2.
        for (int i = 0; i < 7; i++)
        {
            sensor.AddObservation(0f);
        }
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

        float currentDist = Vector3.Distance(rb.position, ballRb.position);
        float delta = lastDistanceToBall - currentDist;
        lastDistanceToBall = currentDist;

        AddReward(delta * movementReward); // reward if the agent moves towards the ball
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
        //fase 2
        float height = collision.gameObject.GetComponent<Rigidbody>().position.y;

        if (height > -0.63f && height < 1f)
        {
            AddReward(goodTimingReward); // good timing
        }

        Vector3 dir = (ballRb.position - rb.position).normalized;
        dir.y = 0.3f;

        Vector3 toTarget = (blueField.position - ballRb.position).normalized;
        float alignment = Vector3.Dot(dir.normalized, toTarget);
        AddReward(movementReward * alignment);

        ballRb.linearVelocity = Vector3.zero;
        ballRb.AddForce(dir * 8f, ForceMode.VelocityChange);

        AddReward(racketHitReward);
    }

}
