using UnityEngine;

public class Racket : MonoBehaviour
{
    public PlayerAgent agent;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            Debug.Log("racket collided with ball");
            agent.OnRacketHit(collision);
        }
    }
}
