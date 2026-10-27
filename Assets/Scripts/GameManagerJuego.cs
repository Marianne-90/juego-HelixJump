using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManagerJuego : MonoBehaviour
{
    [SerializeField] private GameObject panelGameOver;

    void Start()
    {
        panelGameOver.SetActive(false);
    }

    public void Perder()
    {
        panelGameOver.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Reiniciar()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
