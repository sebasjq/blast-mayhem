using UnityEngine;

public class BombParabolicMovement : MonoBehaviour
{
    [Header("Values")]
    [SerializeField] private float g = 9.81f; // Aceleración debido a la gravedad   
    [SerializeField] private float c = 1.2f; // Coeficiente de resistencia del aire
    [SerializeField] private float m = 1f; // Masa de la bomba

    [SerializeField] private float e = 0.6f;
    [SerializeField] private float minBounceVelocity = 0.01f;

    private Rigidbody2D rb;

    private float v_x, v_y;
    private int gravityDirection = -1;
    private bool launched = false;


    // Awake es una función de inicialización que se ejecuta automáticamente una sola
    // vez justo antes de que el juego comience o cuando se instancia un Prefab....
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!launched) return;

        MovimientoParabolico();
    }

    public void Launch(Vector2 initialVelocity)
    {
        rb.gravityScale = 0f; // Desactivar la gravedad para controlar el movimiento manualmente
        rb.linearVelocity = Vector2.zero; // Detener cualquier movimiento previo

        rb.bodyType = RigidbodyType2D.Dynamic;

        // Recibimos las velocidades de lanzamiento iniciales
        v_x = initialVelocity.x;
        v_y = initialVelocity.y;

        gravityDirection = -1;

        // Marcar la bomba como lanzada para iniciar el movimiento parabólico en el Update
        launched = true;
    }

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
        launched = false;

        v_x = 0f;
        v_y = 0f;

        rb.linearVelocity = Vector2.zero; // Detener el movimiento, ya que puede seguir con la ultima velocidad
                                          // con la que venia
    }

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
                    v_y = -e * v_y;
                }
                else
                    v_y = 0f; 
            }

            // Pared
            if (Mathf.Abs(contact.normal.x) > 0.5f)
            {
                if (Mathf.Abs(v_x) > minBounceVelocity)
                    v_x = -e * v_x;
                else
                    v_x = 0f;
            }
        }
        rb.linearVelocity = new Vector2(v_x, v_y); // Se actualiza la posicion de la pelota con los valores de 
                                                    // velocidad con la restitucion aplicada
    }

}
