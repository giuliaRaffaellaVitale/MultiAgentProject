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

    public int teamId;
    public bool isTeamServing;
    public bool isPlayerServing;

    public float moveSpeed = 2f;
    public float rotationSpeed = 180f;

    //Reward
    private const float racketHitReward = 0.5f;

    //Penality
    private const float outOfFieldPenalty = -0.3f;
    private const float ballPenalty = -0.5f;
    private const float tooClosePenality = -0.05f;
    private const float inactivityPenality = -0.005f;

    public void SetIsServing(bool team, bool player)
    {
        isTeamServing = team;
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

        transform.position = redServingPosition.position;

        ballRb.linearVelocity = Vector3.zero;
        ballRb.position = ballRedServing.position;
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
    }


    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];
        float rotate = actions.ContinuousActions[2];
        float swing = actions.ContinuousActions[3];

        // Movement
        Vector3 move = new Vector3(moveX, 0, moveZ);
        rb.AddForce(move * moveSpeed, ForceMode.VelocityChange);

        // Swing
        HandleSwing(swing);

        // reward for moving towards the ball
        float distance = Vector3.Distance(rb.position, ballRb.position);
        AddReward(-distance * 0.001f);

    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("net") || collision.gameObject.CompareTag("grid"))
        {
            AddReward(tooClosePenality);
        }
    }

    void HandleSwing(float swingInput)
    {
        if (swingInput > 0.5f)
            racketPivot.localRotation = Quaternion.Euler(-60f, 0, 0);
        else
            racketPivot.localRotation = Quaternion.identity;
    }

    public void OnRacketHit(Collision collision)
    {
        Vector3 dir = (ballRb.position - rb.position).normalized;
        dir.y = 0.2f;

        ballRb.linearVelocity = Vector3.zero;
        ballRb.AddForce(dir * 6f, ForceMode.VelocityChange);

        AddReward(racketHitReward);
    }

}
