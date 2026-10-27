using UnityEngine;

public class ZonaPeligrosa : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            GameManagerJuego gameManager =
                FindAnyObjectByType<GameManagerJuego>();

            if (gameManager != null)
            {
                gameManager.Perder();
            }
        }
    }
}
