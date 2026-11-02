using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button botonNivel2;

    void Start()
    {
        Time.timeScale = 1f;
        // El nivel 2 solo se habilita cuando ya se completó el nivel 1
        if (botonNivel2 != null)
            botonNivel2.interactable = PlayerPrefs.GetInt("NivelDesbloqueado", 1) >= 2;
    }

    public void Jugar()
    {
        SceneManager.LoadScene("Game");
    }

    public void JugarNivel2()
    {
        SceneManager.LoadScene("Nivel2");
    }

    public void Salir()
    {
        Application.Quit();
    }
}
