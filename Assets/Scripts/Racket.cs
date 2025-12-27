using UnityEngine;

public class Racket : MonoBehaviour
{
    public PlayerAgent agent;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            agent.OnRacketHit(collision);
        }
    }
}
