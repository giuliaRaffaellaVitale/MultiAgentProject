using UnityEngine;

public class Racket : MonoBehaviour
{
    public P1vs1 agent;
    public GM1vs1 gameManager;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            gameManager.SetRally();
            agent.OnRacketHit(collision);
        }
    }
}
