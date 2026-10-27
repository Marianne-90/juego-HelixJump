using UnityEngine;

public class MovimientoPelota : MonoBehaviour
{
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private float sensibilidad = 0.02f;
    [SerializeField] private float sensibilidadMaxima = 0.05f;
    [SerializeField] private float incrementoDificultad = 0.0001f;
    [SerializeField] private float velocidadMaximaCaida = 2.5f;
    

    private Vector2 posicionAnterior;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
if (scoreManager != null)
{
    sensibilidad = 0.02f + scoreManager.ScoreActual * incrementoDificultad;
    sensibilidad = Mathf.Min(sensibilidad, sensibilidadMaxima);
}

        if (Input.GetMouseButtonDown(0))
        {
            posicionAnterior = Input.mousePosition;
        }

        if (Input.GetMouseButton(0))
        {
            Vector2 posicionActual = Input.mousePosition;
            Vector2 delta = posicionActual - posicionAnterior;

            Vector3 movimiento = new Vector3(
                delta.x * sensibilidad,
                0f,
                delta.y * sensibilidad
            );

            rb.MovePosition(rb.position + movimiento);

            posicionAnterior = posicionActual;
        }
    }
    void FixedUpdate()
{
    if (rb.linearVelocity.y < -velocidadMaximaCaida)
    {
        rb.linearVelocity = new Vector3(
            rb.linearVelocity.x,
            -velocidadMaximaCaida,
            rb.linearVelocity.z
        );
    }
}
}
