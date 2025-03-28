using UnityEngine;

public class MovimientoObjeto : MonoBehaviour
{
    public float velocidad = 5f;
    public Vector3 VelocidadActual { get; private set; }

    void Update()
    {
        float movimientoHorizontal = Input.GetAxis("Horizontal") * velocidad * Time.deltaTime;
        float movimientoVertical = Input.GetAxis("Vertical") * velocidad * Time.deltaTime;

        Vector3 movimiento = new Vector3(movimientoHorizontal, 0f, movimientoVertical);
        transform.Translate(movimiento, Space.World);

        // Actualizar la velocidad actual
        VelocidadActual = movimiento / Time.deltaTime;
    }
}



