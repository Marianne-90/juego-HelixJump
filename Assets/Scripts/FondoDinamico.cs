using UnityEngine;

public class FondoDinamico : MonoBehaviour
{
    [SerializeField] private Camera camara;
    [SerializeField] private Color colorInicial = new Color(0.6f, 0.8f, 1f);
    [SerializeField] private Color colorFinal = new Color(0.2f, 0.3f, 0.6f);
    [SerializeField] private float velocidadCambio = 0.2f;

    private float tiempo;

    void Update()
    {
        tiempo += Time.deltaTime * velocidadCambio;

        float t = (Mathf.Sin(tiempo) + 1f) / 2f;

        camara.backgroundColor = Color.Lerp(
            colorInicial,
            colorFinal,
            t
        );
    }
}
