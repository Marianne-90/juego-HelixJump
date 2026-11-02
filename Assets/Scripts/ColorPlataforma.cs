using UnityEngine;

public class ColorPlataforma : MonoBehaviour
{
    public Color colorInicial = Color.white;
    public Color colorFinal = Color.blue;
    public float distanciaCambio = 20f;

    private Renderer plataformaRenderer;
    private Transform pelota;
    private float alturaInicialPelota;

    void Start()
    {
        plataformaRenderer = GetComponent<Renderer>();

        GameObject jugador = GameObject.FindWithTag("Player");

        if (jugador != null)
        {
            pelota = jugador.transform;
            alturaInicialPelota = pelota.position.y;
        }
        else
        {
            Debug.LogWarning("ColorPlataforma: No se encontró ningún objeto con tag Player.");
        }
    }

    void Update()
    {
        if (pelota == null || plataformaRenderer == null)
            return;

        float distanciaDescendida = alturaInicialPelota - pelota.position.y;

        float progreso = Mathf.Clamp01(distanciaDescendida / distanciaCambio);

        plataformaRenderer.material.color =
            Color.Lerp(colorInicial, colorFinal, progreso);
    }
}
