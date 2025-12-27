using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class PlayerAgent : Agent
{
    private Rigidbody rb;
    private TeamManager teamManager;
    private GameManager gameManager;

    private Rigidbody ballRb;
    private Agent teammate;
    public Transform racketPivot;

    public int teamId;

    public float moveSpeed = 5f;
    public float rotationSpeed = 180f;
    float currentSwing = 0f;
    float maxSwingAngle = 60f;
    float swingSpeed = 300f;

    //Reward
    private const float outOfFieldPenalty = -0.3f;
    private const float ballPenalty = -0.5f;


    public override void OnEpisodeBegin()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        transform.localPosition = rb.position;
        transform.rotation = rb.rotation;

        currentSwing = 0f;
        racketPivot.localRotation = Quaternion.identity;

    }


    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(rb.linearVelocity);

        sensor.AddObservation(ballRb.transform.localPosition);
        sensor.AddObservation(ballRb.linearVelocity);

        sensor.AddObservation(teammate.transform.localPosition);
    }


    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];
        float rotate = actions.ContinuousActions[2];
        float swing = actions.ContinuousActions[3];

        // Movimento
        Vector3 move = new Vector3(moveX, 0, moveZ);
        rb.AddForce(move * moveSpeed, ForceMode.VelocityChange);

        // Rotazione
        transform.Rotate(Vector3.up * rotate * rotationSpeed * Time.fixedDeltaTime);

        // Swing
        HandleSwing(swing);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            AddReward(ballPenalty);
        }
        if (collision.gameObject.CompareTag("glass") || collision.gameObject.CompareTag("grid") || collision.gameObject.CompareTag("net"))
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
        Vector3 dir = (collision.transform.position - racketPivot.position).normalized;

        ballRb.AddForce(dir * 8f, ForceMode.VelocityChange);

        AddReward(0.1f);
    }




}
