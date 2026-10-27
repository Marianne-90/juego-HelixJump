using System.Collections.Generic;
using UnityEngine;

public class NivelGenerador : MonoBehaviour
{
    [SerializeField] private GameObject prefabAnillo;
    [SerializeField] private Transform pelota;

    [SerializeField] private int cantidadInicial = 15;
    [SerializeField] private float distanciaVertical = 2.5f;
    [SerializeField] private float distanciaReciclaje = 8f;

    private List<GameObject> anillos = new List<GameObject>();

    void Start()
    {
        GenerarNivelInicial();
    }

    void Update()
    {
        ReciclarAnillos();
    }

    void GenerarNivelInicial()
    {
        for (int i = 0; i < cantidadInicial; i++)
        {
            Vector3 posicion = new Vector3(
                0f,
                -i * distanciaVertical,
                0f
            );

            float rotacionY = Random.Range(0f, 360f);

            GameObject nuevoAnillo = Instantiate(
                prefabAnillo,
                posicion,
                Quaternion.Euler(0f, rotacionY, 0f)
            );

            anillos.Add(nuevoAnillo);
        }
    }

    void ReciclarAnillos()
    {
        foreach (GameObject anillo in anillos)
        {
            if (anillo.transform.position.y >
                pelota.position.y + distanciaReciclaje)
            {
                float yMasBaja = ObtenerYMasBaja();

                anillo.transform.position = new Vector3(
                    0f,
                    yMasBaja - distanciaVertical,
                    0f
                );

                // Cambiamos la rotación cuando reciclamos el anillo
                anillo.transform.rotation = Quaternion.Euler(
                    0f,
                    Random.Range(0f, 360f),
                    0f
                );
            }
        }
    }

    float ObtenerYMasBaja()
    {
        float yMasBaja = float.MaxValue;

        foreach (GameObject anillo in anillos)
        {
            if (anillo.transform.position.y < yMasBaja)
            {
                yMasBaja = anillo.transform.position.y;
            }
        }

        return yMasBaja;
    }
}
