using UnityEngine;

public class ColorPelota : MonoBehaviour
{
    [SerializeField] private Renderer rendererPelota;
    [SerializeField] private Color colorA = Color.magenta;
    [SerializeField] private Color colorB = Color.cyan;
    [SerializeField] private float velocidad = 1f;

    private float tiempo;

    void Update()
    {
        tiempo += Time.deltaTime * velocidad;

        float t = (Mathf.Sin(tiempo) + 1f) / 2f;

        Color colorActual = Color.Lerp(colorA, colorB, t);

        rendererPelota.material.color = colorActual;
    }
}
