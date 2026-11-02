using UnityEngine;

public class ZonaPeligrosa : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Con súper velocidad la pelota atraviesa las zonas peligrosas sin perder
            MovimientoPelota pelota = collision.gameObject.GetComponent<MovimientoPelota>();
            if (pelota != null && pelota.SuperVelocidadActiva) return;

            GameManagerJuego gameManager =
                FindAnyObjectByType<GameManagerJuego>();

            if (gameManager != null)
            {
                gameManager.Perder();
            }
        }
    }
}
