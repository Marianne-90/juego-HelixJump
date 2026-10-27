using UnityEngine;

public class CamaraSeguir : MonoBehaviour
{
    [SerializeField] private Transform objetivo;
    [SerializeField] private float offsetZ = -10f;
    [SerializeField] private float offsetY = 3f;
    [SerializeField] private float suavizado = 5f;

    void LateUpdate()
    {
        if (objetivo == null) return;

        Vector3 posicionDeseada = new Vector3(
            objetivo.position.x,
            objetivo.position.y + offsetY - 2f,
            objetivo.position.z + offsetZ
        );

        transform.position = Vector3.Lerp(
            transform.position,
            posicionDeseada,
            suavizado * Time.deltaTime
        );
    }
}
