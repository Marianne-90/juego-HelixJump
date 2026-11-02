using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

// Tarea 5: controla cuándo se completa un nivel y a qué escena se pasa después.
public class ControlNivel : MonoBehaviour
{
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private int puntajeObjetivo = 300;
    [SerializeField] private int numeroNivel = 1;
    [SerializeField] private string siguienteEscena = "Nivel2";   // vacío = último nivel
    [SerializeField] private GameObject panelNivelCompletado;
    [SerializeField] private TMP_Text textoTituloNivelCompletado;
    [SerializeField] private TMP_Text textoBotonSiguiente;
    [SerializeField] private TMP_Text textoProgreso;

    private bool completado;

    void Start()
    {
        Time.timeScale = 1f;
        panelNivelCompletado.SetActive(false);
        bool esUltimo = string.IsNullOrEmpty(siguienteEscena);
        textoTituloNivelCompletado.text = esUltimo ? "¡JUEGO COMPLETADO!" : "¡NIVEL " + numeroNivel + " COMPLETADO!";
        textoBotonSiguiente.text = esUltimo ? "MENÚ" : "SIGUIENTE NIVEL";
    }

    void Update()
    {
        if (completado || scoreManager == null) return;

        int actual = Mathf.Min(scoreManager.ScoreActual, puntajeObjetivo);
        if (textoProgreso != null)
            textoProgreso.text = actual + " / " + puntajeObjetivo;

        if (scoreManager.ScoreActual >= puntajeObjetivo)
            CompletarNivel();
    }

    void CompletarNivel()
    {
        completado = true;
        // Guardamos el progreso para desbloquear el siguiente nivel en el menú
        int desbloqueado = PlayerPrefs.GetInt("NivelDesbloqueado", 1);
        if (numeroNivel + 1 > desbloqueado)
            PlayerPrefs.SetInt("NivelDesbloqueado", numeroNivel + 1);
        PlayerPrefs.Save();

        panelNivelCompletado.SetActive(true);
        Time.timeScale = 0f;
    }

    public void IrAlSiguienteNivel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(string.IsNullOrEmpty(siguienteEscena) ? "MainMenu" : siguienteEscena);
    }

    public void VolverAlMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
