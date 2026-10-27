using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] private Transform pelota;
    [SerializeField] private TMP_Text textoScore;
    [SerializeField] private TMP_Text textoHighScore;

    private float yInicial;
    private int score;
    private int highScore;
    public int ScoreActual => score;

    void Start()
    {
        yInicial = pelota.position.y;

        highScore = PlayerPrefs.GetInt("HighScore", 0);

        textoHighScore.text = "MEJOR: " + highScore;
    }

    void Update()
    {
        float descenso = yInicial - pelota.position.y;

        score = Mathf.Max(0, Mathf.FloorToInt(descenso * 10f));

        textoScore.text = score.ToString();

        if (score > highScore)
        {
            highScore = score;

            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();

            textoHighScore.text = "MEJOR: " + highScore;
        }
    }
}
