using UnityEngine;

public class BombParabolicMovement : MonoBehaviour
{
    [Header("Values")]
    [SerializeField] private float g = 9.81f; // Aceleración debido a la gravedad   
    [SerializeField] private float c = 1.2f; // Coeficiente de resistencia del aire
    [SerializeField] private float m = 1f; // Masa de la bomba

    [SerializeField] private float e = 0.6f; // Coeficiente de restitución para los rebotes, 0 indica que no rebota nada, 1 indica que rebota
                                             // con la misma velocidad que tenía antes de chocar, y valores entre 0 y 1 indican que rebota perdiendo algo de velocidad.
    [SerializeField] private float minBounceVelocity = 0.01f; // Umbral de velocidad mínima para que la bomba rebote, evitando rebotes infinitos con velocidades muy bajas.

    private Rigidbody2D rb;

    private float v_x, v_y; // Velocidades horizontales y verticales de la bomba, que se actualizan manualmente en el método MovimientoParabolico
    private int gravityDirection = -1; // La gravedad inicialmente apunta hacia abajo, pero puede invertirse con ToggleGravity, lo que afecta la dirección del movimiento parabólico.
    private bool launched = false; // Bandera para indicar si la bomba ha sido lanzada, lo que activa el movimiento parabólico en el FixedUpdate.

    // Awake es una función de inicialización que se ejecuta automáticamente una sola
    // vez justo antes de que el juego comience o cuando se instancia un Prefab....
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>(); // Obtiene la referencia al Rigidbody2D para controlar las físicas de la bomba
    }

    // Se usa FixedUpdate ya que es una simulación de físicas, lo que garantiza que el movimiento parabolico se actualice de manera consistente con el motor de físicas de Unity.
    void FixedUpdate()
    {
        if (!launched) return; // Si la bomba no ha sido lanzada, no hace nada

        MovimientoParabolico(); // Llama al método que calcula el movimiento parabólico de la bomba, actualizando su posición y velocidad en cada frame
    }

    // Este método se llama para lanzar la bomba con una velocidad inicial dada, activando el movimiento parabólico en el FixedUpdate.
    // Se llama en PlayerMovement, cuando el jugador lanza la bomba, pasando la velocidad de lanzamiento calculada en base a la dirección del lanzamiento y la fuerza aplicada.
    public void Launch(Vector2 initialVelocity)
    {
        rb.gravityScale = 0f; // Desactivar la gravedad para controlar el movimiento manualmente
        rb.linearVelocity = Vector2.zero; // Detener cualquier movimiento previo

        rb.bodyType = RigidbodyType2D.Dynamic; // Se hace la simulación manual, pero se mantiene el cuerpo dinámico para que las colisiones sigan funcionando correctamente.

        // Recibimos las velocidades de lanzamiento iniciales
        v_x = initialVelocity.x;
        v_y = initialVelocity.y;

        gravityDirection = -1; // Asegura que la gravedad esté apuntando hacia abajo al lanzar la bomba

        // Marcar la bomba como lanzada para iniciar el movimiento parabólico en el Update
        launched = true;
    }

    // Este método calcula el movimiento parabólico de la bomba, actualizando su velocidad en cada frame considerando la resistencia del aire y la gravedad.
    public void MovimientoParabolico()
    {
        v_x = v_x + (-c / m * v_x) * Time.fixedDeltaTime; // Velocidad horizontal con rozamiento
        v_y = v_y + ((-c / m * v_y) + gravityDirection * g) * Time.fixedDeltaTime; // Velocidad vertical con rozamiento

        // Le envio a Unity la velocidad, y Unity se encarga de mover la bomba en consecuencia.
        // Esto permite controlar el movimiento parabolico manualmente, sin necesidad de desactivar
        // el rigidbody (Kinematic), para que las colisiones sigan funcionando correctamente.
        rb.linearVelocity = new Vector2(v_x, v_y);
    }

    // Detener simuación manual...
    public void StopMovement()
    {
        launched = false; // Detiene el movimiento parabólico, evitando que se actualice la velocidad en el FixedUpdate

        // Detener las velocidades usadas en la simulación manual...
        v_x = 0f;
        v_y = 0f;

        // Al igual que la velocidad del Rigidbody, para evitar que la bomba siga moviéndose con la última velocidad que tenía antes de detener el movimiento manual.
        rb.linearVelocity = Vector2.zero; 
    }

    // Cambiar dirección de la gravedad...
    public void ToggleGravity()
    {
        gravityDirection = gravityDirection * -1;
    }

    // Rebotes...
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // ContactPoint2D representa cada punto exacto donde ocurrió una colisión.
        // collision.contacts guarda todos los puntos de contacto detectados entre ambos colliders.

        // contact.normal devuelve un vector perpendicular a la superficie impactada. Con eso se puede
        // saber desde qué dirección chocó la bomba: normal.y -> suelo o techo / normal.x -> paredes laterales
        foreach (ContactPoint2D contact in collision.contacts)
        {
            // Suelo o techo
            if (Mathf.Abs(contact.normal.y) > 0.5f) // El 0.5f indica cuanto apunta la normal hacia un eje 
            {
                if (Mathf.Abs(v_y) > minBounceVelocity)
                {
                    v_y = -e * v_y; // Aplica la restitución a la velocidad vertical, invirtiendo su dirección y reduciéndola según el coeficiente de restitución e.
                                    // Esto simula un rebote, donde la bomba pierde algo de velocidad cada vez que rebota contra el suelo o el techo.
                }
                else
                    v_y = 0f; // Si la velocidad vertical es menor que el umbral de rebote, se detiene completamente el movimiento vertical, evitando rebotes infinitos con velocidades muy bajas.
            }

            // Pared
            if (Mathf.Abs(contact.normal.x) > 0.5f)
            {
                if (Mathf.Abs(v_x) > minBounceVelocity)
                    v_x = -e * v_x; // Aplica la restitución a la velocidad horizontal, invirtiendo su dirección y reduciéndola según el coeficiente de restitución e.
                else
                    v_x = 0f;
            }
        }

        // Después de calcular los rebotes, se actualiza la velocidad del Rigidbody con los nuevos valores de v_x y v_y, que ya tienen aplicada la restitución.
        rb.linearVelocity = new Vector2(v_x, v_y); 
    }

}
