using UnityEngine;

// Tarea 6: orbe que, al tocarlo la pelota, activa la súper velocidad.
public class Potenciador : MonoBehaviour
{
    [SerializeField] private float velocidadGiro = 120f;

    void Update()
    {
        transform.Rotate(0f, velocidadGiro * Time.deltaTime, 0f, Space.World);
    }

    void OnTriggerEnter(Collider otro)
    {
        if (!otro.CompareTag("Player")) return;

        MovimientoPelota pelota = otro.GetComponent<MovimientoPelota>();
        if (pelota != null)
        {
            pelota.ActivarSuperVelocidad();
            gameObject.SetActive(false);
        }
    }
}
