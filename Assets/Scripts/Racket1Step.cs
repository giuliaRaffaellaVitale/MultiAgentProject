using UnityEngine;

public class Racket1Step : MonoBehaviour
{
    public Player1step agent;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            agent.OnRacketHit(collision);
        }
    }
}
