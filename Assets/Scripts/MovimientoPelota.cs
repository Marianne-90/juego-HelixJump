using UnityEngine;

public class MovimientoPelota : MonoBehaviour
{
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private float sensibilidad = 0.02f;
    [SerializeField] private float sensibilidadMaxima = 0.05f;
    [SerializeField] private float incrementoDificultad = 0.0001f;
    [SerializeField] private float velocidadMaximaCaida = 2.5f;

    [Header("Súper velocidad (Tarea 6)")]
    [SerializeField] private float duracionSuperVelocidad = 3f;
    [SerializeField] private float multiplicadorVelocidad = 2.5f;
    [SerializeField] private TrailRenderer estela;
    [SerializeField] private GameObject avisoSuperVelocidad;

    private float tiempoRestanteSuper;
    public bool SuperVelocidadActiva => tiempoRestanteSuper > 0f;

    public void ActivarSuperVelocidad()
    {
        tiempoRestanteSuper = duracionSuperVelocidad;
    }
    

    private Vector2 posicionAnterior;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (estela != null)
        {
            estela.time = Mathf.Min(estela.time, 3f);   // la estela dura máximo 3 segundos
            estela.emitting = false;
        }
        if (avisoSuperVelocidad != null) avisoSuperVelocidad.SetActive(false);
    }

    void Update()
    {
        if (tiempoRestanteSuper > 0f)
            tiempoRestanteSuper -= Time.deltaTime;

        if (estela != null) estela.emitting = SuperVelocidadActiva;
        if (avisoSuperVelocidad != null) avisoSuperVelocidad.SetActive(SuperVelocidadActiva);

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
    float maxima = SuperVelocidadActiva
        ? velocidadMaximaCaida * multiplicadorVelocidad
        : velocidadMaximaCaida;

    // En súper velocidad la pelota cae siempre a la velocidad máxima aumentada
    if (SuperVelocidadActiva || rb.linearVelocity.y < -maxima)
    {
        rb.linearVelocity = new Vector3(
            rb.linearVelocity.x,
            -maxima,
            rb.linearVelocity.z
        );
    }
}
}
