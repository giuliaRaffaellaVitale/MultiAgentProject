using NUnit.Framework;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using static GameManager;

public class PlayerAgent : Agent
{
    private Rigidbody rb;
    public Rigidbody ballRb;
    public Transform racketPivot;

    public PlayerAgent teammate;
    public List<PlayerAgent> opponents;
    public GameManager gameManager;

    public int teamId;
    public bool isTeamServing;
    public bool isPlayerServing;

    public float moveSpeed = 3f;
    public float rotationSpeed = 180f;
    float currentSwing = 0f;
    float maxSwingAngle = 60f;
    float swingSpeed = 300f;

    //Reward
    private const float racketHitReward = 0.2f;

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

        transform.rotation = rb.rotation;

        Transform spawn = gameManager.GetSpawnTransform(this);

        transform.position = spawn.position;
        transform.rotation = spawn.rotation;

        currentSwing = 0f;
        racketPivot.localRotation = Quaternion.identity;
    }


    public override void CollectObservations(VectorSensor sensor)
    {
        // Self
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(rb.linearVelocity);
        //sensor.AddObservation(transform.forward); //to be used later

        // Ball
        sensor.AddObservation(ballRb.position - transform.position);
        sensor.AddObservation(ballRb.linearVelocity);

        // Teammate
        sensor.AddObservation(teammate.transform.position - transform.position);
        sensor.AddObservation(teammate.rb.linearVelocity);

        // Opponents, later
        
        foreach (var opp in opponents)
        {
            sensor.AddObservation(opp.transform.position - transform.position);
            sensor.AddObservation(opp.rb.linearVelocity);
        }

        // Context
        sensor.AddObservation(isPlayerServing ? 1f : 0f);
        sensor.AddObservation(isTeamServing ? 1f : 0f); 
    }


    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];
        float rotate = actions.ContinuousActions[2];
        float swing = actions.ContinuousActions[3];

        if (gameManager.gameState == GameManager.GameState.Serve)
        {
            if (!isPlayerServing)
            {
                rb.linearVelocity = Vector3.zero;
                return;
            }

            transform.Rotate(Vector3.up * rotate * rotationSpeed * Time.fixedDeltaTime);
            HandleSwing(swing);

            return;
        }

        // Movement
        Vector3 move = new Vector3(moveX, 0, moveZ);
        rb.AddForce(move * moveSpeed, ForceMode.VelocityChange);

        // Rotation
        transform.Rotate(Vector3.up * rotate * rotationSpeed * Time.fixedDeltaTime);

        // Swing
        HandleSwing(swing);

        // Penality if too close to teammate
        float dist = Vector3.Distance(transform.position, teammate.transform.position);
        if (dist < 0.5f)
            AddReward(tooClosePenality);

        float inactivityTimer = 0f;

        // Inactivity penality
        if (rb.linearVelocity.magnitude < 0.1f)
            inactivityTimer += Time.fixedDeltaTime;
        else
            inactivityTimer = 0f;

        if (inactivityTimer > 1.5f)
            AddReward(inactivityPenality);
        
        // reward for moving towards the ball
        float distance = Vector3.Distance(transform.position, ballRb.position);
        AddReward(-distance * 0.001f);

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("courtNet") || collision.gameObject.CompareTag("net"))
        {
            AddReward(outOfFieldPenalty);
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

        //BallController ball = collision.gameObject.GetComponent<BallController>();
        /*
        if (gameManager.gameState == GameManager.GameState.Serve && isPlayerServing)
        {
            gameManager.StartRally();
        }
        */
        Vector3 dir = (ballRb.position - transform.position).normalized;
        dir.y = 0.2f;

        ballRb.linearVelocity = Vector3.zero;
        ballRb.AddForce(dir * 6f, ForceMode.VelocityChange);

        AddReward(racketHitReward);
    }

}
