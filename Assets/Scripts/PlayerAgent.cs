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
    private const float tooClosePenality = -0.01f;
    private const float inactivityPenality = -0.005f;

    public void SetIsServing(bool team, bool player, Transform pos)
    {
        isPlayerServing = player;
        if (team)
        {
            rb.position = pos.position;
        } else
        {
            rb.position = rb.position;
        }
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

        currentSwing = 0f;
        racketPivot.localRotation = Quaternion.identity;
    }


    public override void CollectObservations(VectorSensor sensor)
    {
        // Self
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(rb.linearVelocity);
        sensor.AddObservation(transform.forward);

        // Ball
        sensor.AddObservation(ballRb.position - transform.position);
        sensor.AddObservation(ballRb.linearVelocity);

        // Teammate
        sensor.AddObservation(teammate.transform.position - transform.position);
        sensor.AddObservation(teammate.rb.linearVelocity);

        // Opponents
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
            AddReward(-0.01f);

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
        {
            currentSwing += swingSpeed * Time.fixedDeltaTime;
        }
        else
        {
            currentSwing -= swingSpeed * Time.fixedDeltaTime;
        }

        currentSwing = Mathf.Clamp(currentSwing, -maxSwingAngle, maxSwingAngle);
        racketPivot.localRotation = Quaternion.Euler(currentSwing, 0, 0);
    }

    public void OnRacketHit(Collision collision)
    {

        Rigidbody ballRb = collision.rigidbody;

        Vector3 dir = transform.forward + Vector3.up * 0.2f;
        dir.Normalize();

        ballRb.AddForce(dir * 8f, ForceMode.VelocityChange);

        AddReward(racketHitReward);
    }

}
