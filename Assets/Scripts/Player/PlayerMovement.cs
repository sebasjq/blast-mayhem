using UnityEngine;

// Esta clase controla todo lo relacionado con el jugador:
// movimiento, salto, doble salto, animaciones, vida y lanzamiento de bombas.
public class PlayerMovement : MonoBehaviour, IPickupReceiver
{
    [Header("Settings")]
    [SerializeField] private float moveSpeed = 5f; // Velocidad con la que se mueve el jugador hacia la izquierda o derecha.
    [SerializeField] private float jumpForce = 10f; // Fuerza con la que salta el jugador.
    [SerializeField] private float bombInventory = 0;
    [SerializeField] private int maxHealth = 10; // Vida máxima del jugador.
    [SerializeField] private int maxJumps = 2; // Cantidad máxima de saltos permitidos. 2 significa salto normal + doble salto.

    [SerializeField] private Vector3 originalScale; // Guarda la escala original del personaje. Esto sirve para voltearlo a izquierda/derecha sin cambiarle el tamaño.

    [SerializeField] private GameObject bombPrefab; // Prefab de la bomba que el jugador va a lanzar. Se asigna desde el Inspector de Unity.
    [SerializeField] private Transform bombSpawnPoint; // Punto desde donde aparece la bomba cuando se lanza. Normalmente es un objeto hijo llamado BombSpawnPoint.
    [SerializeField] private Transform aimLineStartPoint;

    [Header("Bomb Charge")] // Variables para el sistema de carga de lanzamiento de bomba
    [SerializeField] private float minThrowSpeed = 9f; // Velocidad mínima con la que se lanza la bomba si el jugador solo presiona y suelta rápidamente la tecla de bomba, sin cargar el lanzamiento.
    [SerializeField] private float maxThrowSpeed = 18f; // Velocidad máxima con la que se lanza la bomba si el jugador carga el lanzamiento manteniendo presionada la tecla de bomba durante el tiempo máximo definido por maxChargeTime.
    [SerializeField] private float maxChargeTime = 2f; // Tiempo máximo que el jugador puede mantener presionada la tecla de bomba para cargar el lanzamiento.

    [Header("Aim")]
    [SerializeField] private LineRenderer aimLine; // Referencia al componente LineRenderer que se utiliza para mostrar la línea de mira mientras el jugador está cargando el lanzamiento de la bomba. Se asigna desde el Inspector de Unity.
    [SerializeField] private float aimLength = 1f; // Longitud de la linea de mira
    [SerializeField] private float lineWidth = 0.1f; // Grosor de la línea de mira

    [Header("UI")]
    [SerializeField] private HealthBarSlider barraDeVida; // Referencia a la barra de vida visual

    private bool chargingBomb = false; // Variable para saber si el jugador está cargando el lanzamiento de la bomba
    private float holdTime = 0f; // Variable para medir cuánto tiempo ha estado el jugador sosteniendo la tecla de bomba para cargar el lanzamiento

    private int currentHealth; // Vida actual del jugador. Es private porque solo se maneja desde este script.
    private int jumpsRemaining; // Cantidad de saltos que le quedan al jugador en este momento.
    private int direction = 1; // Dirección hacia donde mira el jugador / 1 = derecha / -1 = izquierda.

    private bool isGrounded; // Indica si el jugador está tocando el piso. True = está en el piso / False = está en el aire.
    private bool isOnPlayer; // Indica si el jugador está tocando a otro jugador (para permitir salto desde la cabeza de otro jugador).
    private bool isOnSurface; // Indica si el jugador está tocando cualquier superficie (piso o jugador).
    private bool isDead = false; // Indica si el jugador ha muerto. Esto se usa para evitar que el jugador pueda moverse, saltar o lanzar
    private string nombreAsignado;

    private Animator animator; // Referencia al Animator del jugador. Sirve para cambiar entre Idle, Run y Jump.
    private Rigidbody2D rb; // Referencia al Rigidbody2D del jugador. Sirve para moverlo usando físicas.

    private BombType currentBombType = BombType.Normal; // Por default el jugador empieza con el tipo de bomba normal.
    private Bomb activeSpecialBomb;

    // Se asigan desde la funcion SetControls, que es llamada por el PlayerSpawnManager al crear el jugador.
    private KeyCode leftKey;
    private KeyCode rightKey;
    private KeyCode jumpKey;
    private KeyCode downKey;
    private KeyCode bombKey;

    // En Awake se ponen las referencias al Rigidbody2D y al Animator que están en el mismo GameObject del jugador
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>(); // Obtiene el Rigidbody2D que está puesto en el jugador.
        animator = GetComponent<Animator>(); // Obtiene el Animator que está puesto en el jugador.
    }

    // En Start se configura la línea de mira y se inicializan las variables de vida y saltos.
    void Start()
    {
        // Configura la línea de mira para que tenga el grosor deseado y empiece desactivada.
        aimLine.startWidth = lineWidth; 
        aimLine.endWidth = lineWidth;
        aimLine.enabled = false;

        originalScale = transform.localScale; // Guarda el tamaño original del personaje. Esto es importante porque después lo volteamos usando escala negativa.
        currentHealth = maxHealth; // Al iniciar, la vida actual será igual a la vida máxima.
        jumpsRemaining = maxJumps; // Al iniciar, el jugador tiene todos sus saltos disponibles.
    }

    // Update se ejecuta una vez por cada frame. Aquí se revisan teclas y acciones constantes del jugador.
    void Update()
    {
        // Variable temporal para saber si el jugador se mueve. // -1 = izquierda. / 0 = quieto. / 1 = derecha.
        float moveInput = 0f;

        if (isDead) return; // Si el jugador ha muerto, no se ejecuta nada de lo que está abajo

        // Si se presiona la tecla de izquierda...
        if (Input.GetKey(leftKey))
        {
            moveInput = -1f; // El jugador se moverá hacia la izquierda.
            direction = -1; // Guardamos que está mirando hacia la izquierda.
        }

        // Si se presiona la tecla de derecha...
        if (Input.GetKey(rightKey))
        {
            moveInput = 1f; // El jugador se moverá hacia la derecha.
            direction = 1; // Guardamos que está mirando hacia la derecha.
        }

        // Aplicamos el movimiento horizontal al Rigidbody2D,
        // En X usamos la velocidad de movimiento.
        // En Y conservamos la velocidad actual para no afectar salto o caída.
        if (chargingBomb) // Si esta cargando la bomba que el player no se mueva
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); 
            animator.SetBool("isRunning", false); // Si esta cargando la bomba, el jugador no se mueve, por lo tanto no está corriendo.
        }
        else // Si no está cargando la bomba, se mueve normalmente.
        {
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
            animator.SetBool("isRunning", moveInput != 0); // El jugador está corriendo si moveInput no es 0 (es decir, si se está moviendo hacia la izquierda o derecha).
        }

        // El jugador está en una superficie si está tocando el piso o si está tocando a otro jugador.
        isOnSurface = isGrounded || isOnPlayer;

        animator.SetBool("isJumping", !isOnSurface); // Le decimos al Animator si el jugador está saltando (en el aire sin tocar ninguna superficie).

        animator.SetBool("isFalling", rb.linearVelocity.y < 0 && !isOnSurface); // Le decimos al Animator si el jugador está cayendo.

        // Si el jugador se mueve hacia la derecha...
        if (moveInput > 0)
        {
            transform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z); // Lo ponemos mirando hacia la derecha. Mathf.Abs asegura que X sea positiva.
        }

        // Si el jugador se mueve hacia la izquierda...
        else if (moveInput < 0)
        {
            transform.localScale = new Vector3(-Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);  // Lo ponemos mirando hacia la izquierda. La X negativa voltea visualmente el sprite.
        }

        // Si se presiona la tecla de salto, todavía quedan saltos disponibles y el jugador no está cargando una bomba, entonces puede ejecutar el doble salto.
        if (!chargingBomb && Input.GetKeyDown(jumpKey) && jumpsRemaining > 0)
        {
            isOnPlayer = false; // Si el jugador está saltando, ya no está tocando a otro jugador, aunque siga en el aire. Esto evita que pueda seguir saltando al lado de un jugador sin perder sus saltos.

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce); // Aplica una velocidad vertical hacia arriba para hacer que el jugador salte. Conserva la velocidad horizontal actual para no afectar el movimiento lateral.

            if (!isGrounded && jumpsRemaining == 1) // Si el jugador no está en el piso y solo le queda 1 salto, significa que está haciendo el doble salto.
            {
                animator.SetBool("isDoubleJumping", true); // Le decimos al Animator que el jugador está haciendo el doble salto. Esto activa la animación de doble salto.
                animator.SetBool("isFalling", false); // Se asegura de que la animación de caída se desactive al hacer el doble salto
                animator.SetBool("isJumping", false); // Se asegura de que la animación de salto normal se desactive al hacer el doble salto
            }

            jumpsRemaining--; // Resta 1 a los saltos disponibles del jugador cada vez que salta, ya sea salto normal o doble salto.
        }

        // Si se presiona la tecla de bomba y hay una bomba especial activa que ya fue lanzada y es del tipo Gravity, entonces en lugar de lanzar otra bomba, se activa o desactiva la gravedad de esa bomba.
        if (Input.GetKeyDown(bombKey)
            && activeSpecialBomb != null
            && activeSpecialBomb.isThrown())
        {
            if (activeSpecialBomb.GetBombType() == BombType.Gravity)
            {
                activeSpecialBomb.ToggleGravity();
            }

            else if (activeSpecialBomb.GetBombType() == BombType.Spring)
            {
                if (activeSpecialBomb.TryCaptureSpringBomb())
                {
                    AddBomb();
                    currentBombType = BombType.Spring;
                    activeSpecialBomb = null;
                }
            }

            return;
        }

        // Si el jugador tiene bombas en su inventario o tiene una bomba especial activa, puede intentar lanzar una bomba.
        if (bombInventory > 0 || activeSpecialBomb != null)
        {
            if (Input.GetKeyDown(bombKey)) // Si se presiona la tecla de bomba, se inicia el proceso de carga del lanzamiento de la bomba
            {
                StartChargingBomb();
            }

            if (Input.GetKey(bombKey)) // Si se mantiene presionada la tecla de bomba, se continúa el proceso de carga del lanzamiento de la bomba
            {
                ChargeBomb();
            }

            if (Input.GetKeyUp(bombKey)) // Si se suelta la tecla de bomba, se lanza la bomba con la fuerza calculada según el tiempo que se ha estado cargando el lanzamiento
            {
                ReleaseBomb();
            }
        }

        else // Si no tiene bombas en su inventario y no tiene una bomba especial activa, se asegura de que el sistema de carga de lanzamiento de bomba esté reseteado y la línea de mira esté oculta.
        {
            if (aimLine != null) 
            {
                aimLine.enabled = false; // Asegura que la línea de mira esté desactivada si no tiene bombas para lanzar, para evitar confusión al jugador.
            }

            // Resetea el sistema cuando no tiene bombas
            chargingBomb = false;
            holdTime = 0f;
        }
    }

    // Esta función se ejecuta cuando el jugador empieza a tocar otro collider.
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Si el objeto con el que chocó se llama Ground...
        if (collision.gameObject.name == "Ground")
        {
            isGrounded = true;
            jumpsRemaining = maxJumps;
            animator.SetBool("isDoubleJumping", false);
        }

        // Si cayó encima de otro jugador...
        if (collision.gameObject.CompareTag("Player"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                // Verifica que esté encima del otro player.
                if (contact.normal.y > 0.5f)
                {
                    isOnPlayer = true;
                    jumpsRemaining = maxJumps;
                    animator.SetBool("isDoubleJumping", false);
                }
            }
        }
    }

    // Esta función se ejecuta cuando el jugador deja de tocar otro collider.
    private void OnCollisionExit2D(Collision2D collision)
    {
        // Si dejó de tocar el objeto llamado Ground...
        if (collision.gameObject.CompareTag("Ground"))
        {
            // Marcamos que ya no está en el piso.
            isGrounded = false;
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            isOnPlayer = false;
        }
    }

    // Función pública para recibir daño.
    // Otros scripts, como una bomba, pueden llamar esta función.
    public void TakeDamage(int damage)
    {
        if (isDead) return; // Si el jugador ya ha muerto, no recibe más daño ni se ejecuta nada de lo que está en esta función.

        // Restamos el daño recibido a la vida actual.
        currentHealth -= damage;

        // Mostramos la vida actual en la consola.
        Debug.Log("Vida actual: " + currentHealth);

        // Se actualiza la barra visual
        if (barraDeVida != null)
        {
            barraDeVida.CambiarVidaActual(currentHealth);
        }
        // Si la vida llega a 0 o menos...
        if (currentHealth <= 0)
        {
            // El jugador muere.
            Die();
            return; // Salimos de la función para que no ejecute el trigger de "Hit" después de morir.
        }

        animator.SetTrigger("Hit");
    }

    // Función que se ejecuta cuando el jugador muere.
    private void Die()
    {
        if (isDead) return;
        isDead = true;

        // Llamada ultra simple: Si existe el menú, enciéndelo.
        if (GameOverMenu.Instance != null)
        {
            GameOverMenu.Instance.ActivarMenu();
        }

        Debug.Log("Player murió");

        animator.SetTrigger("Die");

        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        Invoke(nameof(DisablePlayer), 0.6f);
    }

    // Función para desactivar el jugador después de morir. Se llama con un delay desde la función Die para que se vea la animación de muerte antes de desaparecer.
    private void DisablePlayer()
    {
        gameObject.SetActive(false);
    }

    // Función encargada de lanzar la bomba.
    public void ThrowBomb(float throwSpeed)
    {
        if (bombInventory <= 0)
        {
            Debug.Log("No hay bombas para lanzar");
            return;
        }

        if (bombPrefab == null || bombSpawnPoint == null)
        {
            return;
        }

        GameObject bomb = Instantiate(bombPrefab, bombSpawnPoint.position, Quaternion.identity);
        Bomb newBomb = bomb.GetComponent<Bomb>();

        Collider2D playerCollider = GetComponent<Collider2D>();
        newBomb.IgnorePlayerCollision(playerCollider, 0.5f); // Hace que la bomba ignore la colisión con el jugador durante 0.5 segundos

        if (newBomb != null)
        {
            newBomb.SetBombType(currentBombType); // Se crea un bomba Normal por defecto

            Vector2 throwDirection = GetThrowDirection();
            newBomb.Throw(throwDirection, throwSpeed, transform);

            //newBomb.Throw(new Vector2(direction, 1f), 10f);
            //bombScript.Throw(Vector2.up, 10f);

            if (currentBombType == BombType.Gravity || currentBombType == BombType.Spring)
            {
                activeSpecialBomb = newBomb; // Guarda la referencia de la bomba especial creada.
                                             // Así, cuando el jugador vuelva a presionar la tecla,
                                             // no se lanzará otra bomba, sino que se controlará
                                             // la misma bomba ya existente (por ejemplo, Gravity).
            }

            currentBombType = BombType.Normal;

        }

        Debug.Log("Bomba lanzada. Bombas restantes: " + bombInventory);
        bombInventory--;
    }

    // Aumentar bombInventory al recoger una bomba
    public void AddBomb()
    {
        bombInventory++;
        Debug.Log("Bombas en inventario: " + bombInventory);
    }

    // Cambiar el tipo de bomba actual al recoger un pickup de bomba especial
    public void SetBombType(BombType bombType)
    {
        Debug.Log("Tipo de bomba ahora: " + bombType);
        currentBombType = bombType;
    }

    // Aumentar la vida del jugador al recoger un pickup de salud
    public void AddHealth()
    {
        if (currentHealth >= maxHealth)
        {
            Debug.Log("Vida ya está al máximo");
            return;
        }

        currentHealth += 1;
        Debug.Log("Salud aumentada. Vida actual: " + currentHealth);

        if (barraDeVida != null)
        {
            barraDeVida.CambiarVidaActual(currentHealth);
        }
    }

    // Método para agregar el tipo de bomba Boomerang
    public void AddBoomerang()
    {
        Debug.Log("Tipo de bomba ahora: Boomerang");
    }

    // Funcion para configurar las teclas de control del jugador. Es llamada por el PlayerSpawnManager al crear el jugador, pasando las teclas correspondientes para cada jugador.
    public void SetControls(KeyCode left, KeyCode right, KeyCode jump, KeyCode bomb, KeyCode down, HealthBarSlider miBarra, string nombre)
    {
        leftKey = left;
        rightKey = right;
        jumpKey = jump;
        bombKey = bomb;
        downKey = down;

        barraDeVida = miBarra; // <-- El manager le entrega su barra asignada

        // Inicializamos la barra de inmediato con la vida máxima de este jugador
        if (barraDeVida != null)
        {
            barraDeVida.InicializarBarraDeVida(maxHealth);
        }
    }

    // Esta función obtiene la dirección de lanzamiento de la bomba según las teclas de dirección que el jugador esté presionando.
    private Vector2 GetThrowDirection()
    {
        float x = 0f;
        float y = 0f;

        if (Input.GetKey(leftKey)) // Si se presiona la tecla de izquierda, la dirección en X será -1
        {
            x = -1f;
        }

        if (Input.GetKey(rightKey)) // Si se presiona la tecla de derecha, la dirección en X será 1
        {
            x = 1f;
        }

        if (Input.GetKey(jumpKey)) // Si se presiona la tecla de salto, la dirección en Y será 1
        {
            y = 1f;
        }

        if (Input.GetKey(downKey)) // Si se presiona la tecla de abajo, la dirección en Y será -1
        {
            y = -1f;
        }

        // Si presiona la x sin ninguna direccion
        if (x == 0f && y == 0f)
        {
            x = direction; // Se lanza hacia direction, hacia donde mira el jugador. Solo en X
        }

        return new Vector2(x, y).normalized; // Normaliza el vector para que su magnitud sea 1, evitando que lanzar en diagonal sea más fuerte que lanzar hacia los lados o hacia arriba.
    }

    // Funciones para el sistema de carga de lanzamiento de bomba
    // La funcion StartChargingBomb se llama cuando el jugador presiona la tecla de bomba. Activa la línea de mira y resetea el tiempo de carga.    
    private void StartChargingBomb()
    {
        aimLine.enabled = true; // Activa la línea de mira para mostrar la dirección y fuerza del lanzamiento.
        chargingBomb = true; // Marca que el jugador está en proceso de cargar el lanzamiento de la bomba.
        holdTime = 0f; // Resetea el tiempo que el jugador ha estado sosteniendo la tecla de bomba. Esto es importante para calcular la fuerza del lanzamiento cuando suelte la tecla.
    }

    // La función ChargeBomb se llama cada frame mientras el jugador mantiene presionada la tecla de bomba.
    // Actualiza la línea de mira y aumenta el tiempo de carga. Si el tiempo de carga alcanza el máximo,
    // se lanza automáticamente la bomba.
    private void ChargeBomb()
    {
        if(!chargingBomb) // Si no esta cargando la bomba, se sale de la función
            return;

        UpdateAimLine(); // Actualiza la línea de mira para reflejar la dirección y fuerza actuales del lanzamiento.
                         // Esto se hace cada frame para que el jugador tenga feedback visual mientras carga el lanzamiento.

        holdTime += Time.deltaTime; // Aumenta el tiempo de carga según el tiempo que ha pasado desde el último frame.

        if (holdTime >= maxChargeTime) // Si el tiempo de carga alcanza o supera el tiempo máximo permitido para cargar el lanzamiento, se lanza automáticamente la bomba.
        {
            ReleaseBomb(); // Llama a la función ReleaseBomb para lanzar la bomba con la fuerza máxima, ya que el jugador ha alcanzado el tiempo máximo de carga.
        }
    }

    // La función ReleaseBomb se llama cuando el jugador suelta la tecla de bomba después de haberla mantenido presionada para cargar el lanzamiento.
    private void ReleaseBomb()
    {
        if (!chargingBomb) // Si no esta cargando la bomba....
            return;

        chargingBomb = false; // Marca que el jugador ya no está cargando el lanzamiento de la bomba, ya que ha soltado la tecla de bomba.
        aimLine.enabled = false; // Desactiva la línea de mira

        float t = holdTime / maxChargeTime; // Normaliza el tiempo / 0 no cargó nada / 1 carga máxima

        // Da un valor entre minimo y maximo (minThrowForce y maxThrowForce)
        float throwSpeed = Mathf.Lerp(minThrowSpeed, maxThrowSpeed, t);

        ThrowBomb(throwSpeed); // Llama a la función ThrowBomb, pasando la velocidad de lanzamiento calculada según el tiempo que
                               // el jugador ha estado cargando el lanzamiento. Esto permite que el lanzamiento sea más fuerte
                               // cuanto más tiempo se haya cargado, hasta un máximo definido por maxThrowSpeed.
    }

    // La función UpdateAimLine actualiza la posición, dirección y color de la línea de mira que se muestra mientras el jugador está cargando el lanzamiento de la bomba.
    private void UpdateAimLine()
    {
        aimLine.enabled = true; // Asegura que la línea de mira esté activa mientras se actualiza.

        Vector2 direction = GetThrowDirection(); // Obtiene la dirección de lanzamiento basada en las teclas de dirección que el jugador está presionando. Esto determina hacia dónde se lanzará la bomba.

        Vector3 start = aimLineStartPoint.position; // El punto de inicio de la línea de mira es la posición del bombSpawnPoint, que es donde aparecerá la bomba cuando se lance.
        Vector3 end = start + (Vector3)(direction * aimLength); // Esta linea basicamente, toma la dirección de lanzamiento,
                                                                // la multiplica por aimLength para determinar la longitud de la
                                                                // línea de mira, y suma eso a la posición de inicio para obtener el punto
                                                                // final de la línea de mira.

        aimLine.SetPosition(0, start); // Establece el punto de inicio de la línea de mira en la posición del bombSpawnPoint.
        aimLine.SetPosition(1, end); // Establece el punto final de la linea de mira en la posición calculada según la dirección y longitud deseada.

        float t = holdTime / maxChargeTime; // Normaliza el tiempo de carga para obtener un valor entre 0 y 1, donde 0 significa que no se ha cargado nada y 1 significa que se ha alcanzado la carga máxima.

        Color currentColor = Color.Lerp(
            new Color(0f, 0.7f, 1f), // Azul celeste
            Color.red, t); // Interpola el color de la línea de mira entre blanco (sin carga) y rojo (carga máxima) según el tiempo de carga.

        aimLine.startColor = currentColor; // Establece el color de inicio de la línea de mira según el tiempo de carga.
        aimLine.endColor = currentColor; // Establece el color de fin de la línea de mira según el tiempo de carga.
    }

    // Esta función se llama como un Animation Event desde la animación del player,
    // esto para se genere la animación de Spawneo del personaje primero, y después se habiliten las físicas para que caiga.
    public void EnablePhysics()
    {
        rb.bodyType = RigidbodyType2D.Dynamic;
    }

}