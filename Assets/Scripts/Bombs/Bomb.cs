using UnityEngine;
using System.Collections;

// Este script es para la bomba LANZADA, no para la bomba que se recoge.
public class Bomb : MonoBehaviour
{
    [Header("Bomb State")]
    [SerializeField] private BombState currentState = BombState.Pickup; // El estado inicial de la bomba es Pickup, lo que significa que está en el suelo esperando a ser recogida. Luego cambia a Thrown al ser lanzada, Resting si llega a un estado de reposo en el suelo después de ser lanzada, y Exploded al explotar.

    [Header("Bomb Type")]
    [SerializeField] private BombType bombType = BombType.Normal; // El tipo de bomba, que puede ser Normal, Gravity, String o Sticky. Esto se establece al crear la bomba en PlayerMovement y se usa para determinar su comportamiento (por ejemplo, si es del tipo Gravity, se activa el temporizador de explosión y la función ToggleGravity).

    [Header("References")] // Referencias a componentes y objetos necesarios para el funcionamiento de la bomba, tales como Rigidbody2D para controlar la física, Animator para las animaciones, Collider2D para detectar colisiones con el suelo y el jugador, etc.
    [SerializeField] private Rigidbody2D rb; 
    [SerializeField] private Animator animator;
    [SerializeField] private Collider2D groundCollider;
    [SerializeField] private Collider2D playerDetector;

    [Header("Settings")]
    [SerializeField] private float restingVelocityThreshold = 0.2f; // Velocidad mínima para considerar que la bomba está en reposo
    [SerializeField] private int damage = 1; // Daño que la bomba inflige al jugador
    [SerializeField] private float lifeTime = 5f; // Tiempo en segundos antes de que la bomba se destruya automáticamente si es un pickup
    [SerializeField] private SpriteRenderer spriteRenderer; // Referencia al SpriteRenderer para cambiar la apariencia de la bomba si es necesario

    [Header("Movements")]
    private BombParabolicMovement parabolicMovement; // Referencia al script BombParabolicMovement para controlar el movimiento parabólico de la bomba al ser lanzada
    private BombSpringMovement springMovement; // Referencia al script BombSpringMovement para controlar el movimiento de resorte de la bomba al ser lanzada (si es del tipo String)
    private Transform ownerTransform; // Referencia al transform del jugador que lanzó la bomba, para que la bomba pueda moverse hacia el jugador si es del tipo Spring

    [SerializeField] private Color movingColor = new Color(0.8f, 0.3f, 0.3f); // Color para indicar que la bomba está en movimiento (Thown). Se asigna desde el inspector para poder ajustarlo fácilmente

    [Header("World Limits")] // Limites del mundo para forzar la explosión y destrucción de la bomba si se sale de ellos
    [SerializeField] private float minX = -10f;
    [SerializeField] private float maxX = 10f;
    [SerializeField] private float minY = -6f;
    [SerializeField] private float maxY = 6f;

    [Header("Gravity Pickup Settings")]
    [SerializeField] private float gravityExplosionTime = 4f; // Tiempo en segundos antes de que la bomba de gravedad explote automáticamente
    private float gravityTimer = 0f;

    [Header("Explosion")]
    [SerializeField] private float explosionRadius = 1.2f; // Radio de la explosión de la bomba
    [SerializeField] private LayerMask damageLayers; // Capas que pueden recibir daño de la explosión

    private bool collected = false; // Indica si la bomba ha sido recogida por el jugador. Se usa para evitar que se pueda recoger varias veces o que se active la explosión al colisionar con el jugador después de haber sido recogida.
    private bool isGrounded = false; // La bomba debe estar lenta y en el suelo para volver a ser recogida.
    private Color originalColor; // Para almacenar el color original del sprite de la bomba

    // Awake sirve para guardar referencias a todos los componentes
    // que voy a usar muchas veces en el script, para no tener que buscarlos cada vez que los necesito.
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>(); // Referencia al Rigidbody2D para controlar la física de la bomba
        animator = GetComponent<Animator>(); // Referencia al Animator para controlar las animaciones de la bomba (recolección, explosión, etc.)

        parabolicMovement = GetComponent<BombParabolicMovement>(); // Referencia al script BombParabolicMovement para controlar el movimiento parabólico de la bomba al ser lanzada
        springMovement = GetComponent<BombSpringMovement>(); // Referencia al script BombSpringMovement para controlar el movimiento de resorte de la bomba al ser lanzada (si es del tipo String)

        spriteRenderer = GetComponent<SpriteRenderer>(); // Referencia al SpriteRenderer para cambiar el color de la bomba dependiendo de su estado (por ejemplo, para indicar que está en movimiento o en reposo)
        originalColor = spriteRenderer.color; // Guarda el color original del sprite para poder volver a él cuando sea necesario (por ejemplo, después de que la bomba deje de moverse o explote)
    }

    // Start se usa para destruir la bomba después de un tiempo si es un pickup, para evitar que queden bombas infinitas en el suelo.
    private void Start()
    {
        if (currentState == BombState.Pickup) // Si la bomba es un pickup...
        {
            Destroy(gameObject, lifeTime); // Se destruye el objeto de la bomba después de lifeTime segundos para evitar que queden bombas infinitas en el suelo.
        }
    }

    // Update se usa para verificar el estado de la bomba en cada frame
    private void Update()
    {
        if (currentState == BombState.Thrown) // Si la bomba ha sido lanzada...
        {
            CheckIfResting(); // Verifica si la bomba ha llegado a un estado de reposo para permitir que vuelva a ser recogida
            CheckIfOutOfBounds(); // Verifica si la bomba se ha salido de los límites del mundo para forzar su explosión y destrucción
            CheckGravityExplosionTimer(); // Verifica el temporizador de explosión para las bombas tipo Gravity y las hace explotar si el tiempo se ha cumplido
        }
    }

    // Para detectar la colisión con el Player y ejecutar el daño
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        if (currentState == BombState.Thrown)
        {
            PlayerMovement player = collision.GetComponent<PlayerMovement>();

            if (player == null) return;

            Debug.Log("Bomba golpeó al jugador. Infligiendo daño: " + damage);

            Explode(); // Se llama a Explode, que a su vez llama a ApplyExplosionDamage para
                       // aplicar el daño al jugador dentro del radio de explosión.
                       // Esto se hace para que el jugador reciba el daño incluso
                       // si no está exactamente en el centro de la bomba, sino dentro del radio de explosión.

                        // Ademas, evitamos errores al ejecutar el daño en esta función, ya que sino, cada bomba
                        // haría el doble de daño.
            return;
        }

        // Si la bomba es un pickup y esta en resting, entonces se puede volver a recoger
        if (currentState == BombState.Pickup || currentState == BombState.Resting)
        {
            if (collected) return;

            IPickupReceiver receiver = collision.GetComponent<IPickupReceiver>();

            if (receiver == null) return;

            collected = true;

            Debug.Log("Bomba recogida");

            receiver.AddBomb();

            Collect();
        }
    }

    // Para detectar la colisión entre bombas
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Colisión con: " + collision.gameObject.name);
        Bomb otherBomb = collision.gameObject.GetComponentInParent<Bomb>(); 

        if (otherBomb != null)
        {
            Debug.Log("Detectó otra bomba");
            if (currentState == BombState.Thrown && otherBomb.isThrown()) // Si ambas bombas están en Thrown
            { 
                Explode();
                otherBomb.Explode(); // Ambas bombas explotan
                return;
            }
        }

        if (collision.gameObject.CompareTag("Ground")) // Si la bomba colisiona con el suelo, se considera que está en el suelo (grounded).
        {
            isGrounded = true;
        }
    }

    // Función para verificar si la bomba está en estado Thrown. Se usa en Bomb para detectar colisiones entre bombas y hacer que ambas exploten.
    public bool isThrown()
    {
        return currentState == BombState.Thrown;
    }

    // Se fuerza que la bomba explote cuando se sale de los limites del mundo
    private void CheckIfOutOfBounds()
    {
        Vector3 pos = transform.position; // Se obtiene la posición actual de la bomba

        if (pos.x < minX || pos.x > maxX ||
            pos.y < minY || pos.y > maxY) // Condiciones para verificar si la bomba está fuera de los límites definidos
        {
            ForceEndBomb(); // Se llama a la funcion para forzar el fin de la bomba (explosión y destrucción)
        }
    }

    // Esta función se encarga de forzar el fin de la bomba
    private void ForceEndBomb()
    {
        currentState = BombState.Exploded; // Se pone el estado de la bomba a Exploded
        parabolicMovement.StopMovement(); // Se detiene la simulación manual para evitar que la bomba siga moviéndose fuera de los límites
        Destroy(gameObject); // Se destruye el objeto de la bomba para eliminarlo del juego
    }

    // Función para verificar el estado de reposo de la bomba
    private void CheckIfResting()
    {
        if (isGrounded && rb.linearVelocity.magnitude <= restingVelocityThreshold) // Si la bomba está en el suelo y su velocidad es lo suficientemente baja, se considera que está en reposo
        {
            SetRestingState(); // Se llama a SetRestingState para cambiar el estado de la bomba a Resting, lo que permite que vuelva a ser recogida
        }
    }

    // Función para establecer el estado de reposo de la bomba
    private void SetRestingState()
    { 
        currentState = BombState.Resting; // Cambia el estado de la bomba a Resting
        spriteRenderer.color = originalColor; // Cambia el color de la bomba al color original para indicar que está en estado Resting

        parabolicMovement.StopMovement(); // Detenemos la simulación manual
        springMovement.StopMovement();
    }

    // Esta función se encarga de cambiar el tipo de bomba
    public void SetBombType(BombType newBombType) // Se le pasa el nuevo tipo de bomba que se quiere establecer en PlayerMovement al momento crear la bomba
    {
        bombType = newBombType; // Se actualiza el tipo de bomba con el nuevo valor proporcionado
    }

    // Esta función se encarga de obtener el tipo de bomba actual
    public BombType GetBombType() 
    { 
        return bombType; // Devuelve el tipo de bomba actual. Se usa en PlayerMovement para el switch de lanzamiento
    }

    // Esta función se encarga de alternar la gravedad de la bomba si es del tipo Gravity y está en estado Thrown
    public void ToggleGravity()
    {
        if(bombType == BombType.Gravity && currentState == BombState.Thrown)
        {
            parabolicMovement.ToggleGravity(); // Se llama a la función ToggleGravity del script BombParabolicMovement para alternar la gravedad de la bomba.
        }
    }

    // Función para manejar la recolección de la Bomba. Se llama desde PlayerMovement cuando el jugador recoge la bomba.
    public void Collect()
    {
        animator.SetTrigger("Collect"); // Se activa la animación de recolección...

        parabolicMovement.StopMovement(); // Detiene la simulacion manual
        springMovement.StopMovement();

        rb.bodyType = RigidbodyType2D.Kinematic; // Hacer que la bomba no sea afectada por la física
    }

    // Función para manejar el lanzamiento de la Bomba. Se llama desde PlayerMovement cuando el jugador lanza la bomba.
    public void Throw(Vector2 direction, float initialSpeed, Transform playerTransform)
    {
        gravityTimer = 0f; // Reinicia el temporizador de explosión para bombas tipo Gravity cada vez que se lanza la bomba
        collected = false; // La bomba ya no está recogida al ser lanzada
        isGrounded = false; // La bomba no está en el suelo al ser lanzada
        spriteRenderer.color = movingColor; // Cambia el color de la bomba para indicar que está en estado Thrown

        currentState = BombState.Thrown; // Cambia a estado Thrown
        rb.bodyType = RigidbodyType2D.Dynamic;

        ownerTransform = playerTransform;

        Vector2 initialVelocity = direction * initialSpeed; // Como direction viene normalizada en PlayerMovement, se multiplica por la velocidad inicial para obtener la velocidad real que se le va a aplicar a la bomba al ser lanzada.

        if (bombType == BombType.String) 
        {
            parabolicMovement.StopMovement(); 
            springMovement.Launch(initialVelocity, ownerTransform);
        }
        else
        {
            springMovement.StopMovement();
            parabolicMovement.Launch(initialVelocity); // Se llama la función Launch del script BombParabolicMovement para iniciar el movimiento parabólico de la bomba con la velocidad inicial calculada.
        }
    }

    // Esta función se encarga de manejar la explosión de la bomba
    public void Explode()
    {
        Debug.Log("Bomba explotó");

        currentState = BombState.Exploded; // Cambia el estado a Exploded para evitar que se vuelva a activar la explosión o se pueda recoger
        ApplyExplosionDamage(); // Aplica el daño a los jugadores dentro del radio de explosión

        parabolicMovement.StopMovement(); // Detiene la simulacion manual
        springMovement.StopMovement(); // Detiene el movimiento de resorte en caso de que sea del tipo String

        rb.bodyType = RigidbodyType2D.Kinematic; // Hacer que la bomba no sea afectada por la física al explotar
        spriteRenderer.color = originalColor; // Cambia el color de la bomba al color original para que la animación de explosión se vea bien
        animator.SetTrigger("Explode"); // Se activa la animación de explosión
    }

    // Esta función se encarga de aplicar el daño de la explosión a los jugadores dentro del radio de explosión
    private void ApplyExplosionDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll( // El arreglo devuelve los colliders encontrados dentro del círculo de explosión que pertenecen a las capas definidas en damageLayers
            transform.position, // Centro del circulo
            explosionRadius, // Radio de explosión
            damageLayers // Desde el inspector se pone Player como damageLayer para que solo detecte a los jugadores y no a otros objetos como el suelo o otras bombas 
        );

        foreach (Collider2D hit in hits) // Se itera sobre cada collider encontrado dentro del radio de explosión
        {
            PlayerMovement player = hit.GetComponent<PlayerMovement>(); // Se intenta obtener el componente PlayerMovement del collider para verificar si es un jugador

            if (player != null) // Si es un jugador...
            {
                player.TakeDamage(damage); // Aplica daño
            }
        }
    }

    // Función para verificar el temporizador de explosión de las bombas tipo Gravity
    private void CheckGravityExplosionTimer()
    {
        if(bombType != BombType.Gravity) // Si la bomba no es del tipo Gravity, se sale de la función
        {
            return;
        }

        // En otro caso, aumenta el temporizador con el tiempo transcurrido desde el último frame
        gravityTimer += Time.deltaTime;

        // Si el temporizador supero el tiempo definido para la explosión, se llama a la función Explode
        if (gravityTimer >= gravityExplosionTime)
        {
            Explode();
        }
    }

    // Función para dibujar el radio de explosión en el editor de Unity
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red; // Color del gizmo para el radio de explosión
        Gizmos.DrawWireSphere(transform.position, explosionRadius); // Dibuja el circulo, con el centro en la posición de la bomba y el radio definido por explosionRadius
    }

    // Función para ignorar al player cuando la bomba se lanza
    public void IgnorePlayerCollision(Collider2D playerCollider, float duration)
    {
        StartCoroutine(IgnoreCollisionCoroutine(playerCollider, duration)); // Inicia la corrutina para ignorar la colisión con el jugador durante un tiempo determinado
    }

    // Corrutina para ignorar la colisión con el jugador durante un tiempo determinado
    private IEnumerator IgnoreCollisionCoroutine(Collider2D playerCollider, float duration)
    {
        Physics2D.IgnoreCollision(playerDetector, playerCollider, true);
        Physics2D.IgnoreCollision(groundCollider, playerCollider, true);

        yield return new WaitForSeconds(duration); // Espera durante el tiempo especificado

        Physics2D.IgnoreCollision(playerDetector, playerCollider, false);
        Physics2D.IgnoreCollision(groundCollider, playerCollider, false);
    }


}
