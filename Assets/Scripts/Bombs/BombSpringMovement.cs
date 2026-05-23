using UnityEngine;

public class BombSpringMovement : MonoBehaviour
{
    [Header("Spring Settings")]
    [SerializeField] private float m = 1f; // Masa
    [SerializeField] private float k = 18f; // Constante de resorte
    [SerializeField] private float b = 5f; // Coeficiente de amortiguamiento

    private Rigidbody2D rb;
    private Transform target; // El objetivo al que el resorte se moverá
    private Vector2 velocity;
    private bool launched = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (!launched || target == null) return;

        MovimientoResorte();
    }

    public void Launch(Vector2 initialVelocity, Transform playerTransform)
    {
        rb.gravityScale = 0; // Desactivar la gravedad
        rb.linearVelocity = Vector2.zero; // Detener cualquier movimiento previo
        rb.bodyType = RigidbodyType2D.Dynamic; // Asegurarse de que el cuerpo es dinámico para que aun pueda colisionar

        target = playerTransform; // Establecer el objetivo
        velocity = initialVelocity; // Establecer la velocidad inicial

        launched = true; // Marcar como lanzado para ejecutar MovimientoResorte en FixedUpdate
    }

    private void MovimientoResorte()
    {
        Vector2 elongacion = rb.position - (Vector2)target.position; // Vector desde el objetivo hasta la bomba.
                                                                     // Esta es la elongaci[on del resorte en 2D.

        Vector2 aceleracion = (-k * elongacion - b * velocity) / m; // Ley de Hooke con amortiguamiento
        
        velocity =  velocity + aceleracion * Time.fixedDeltaTime; // Actualizar la velocidad usando la aceleración
        rb.linearVelocity = velocity; // Aplicar la velocidad al Rigidbody2D para que Unity maneje el movimient
    }

    public void StopMovement()
    {
        launched = false; // Detener el movimiento del resorte
        rb.linearVelocity = Vector2.zero; // Detener cualquier movimiento
        velocity = Vector2.zero; // Reiniciar la velocidad
    }
}
