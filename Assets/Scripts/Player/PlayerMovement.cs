using UnityEngine;

// Esta clase controla todo lo relacionado con el jugador:
// movimiento, salto, doble salto, animaciones, vida y lanzamiento de bombas.
public class PlayerMovement : MonoBehaviour, IPickupReceiver
{
    [Header("Settings")]
    [SerializeField] private float moveSpeed = 5f; // Velocidad con la que se mueve el jugador hacia la izquierda o derecha.
    [SerializeField] private float jumpForce = 10f; // Fuerza con la que salta el jugador.
    [SerializeField] private float bombInventory = 0;
    [SerializeField] private int maxHealth = 3; // Vida máxima del jugador.
    [SerializeField] private int maxJumps = 2; // Cantidad máxima de saltos permitidos. 2 significa salto normal + doble salto.

    [SerializeField] private Vector3 originalScale; // Guarda la escala original del personaje. Esto sirve para voltearlo a izquierda/derecha sin cambiarle el tamaño.

    [SerializeField] private GameObject bombPrefab; // Prefab de la bomba que el jugador va a lanzar. Se asigna desde el Inspector de Unity.
    [SerializeField] private Transform bombSpawnPoint; // Punto desde donde aparece la bomba cuando se lanza. Normalmente es un objeto hijo llamado BombSpawnPoint.

    [Header("Bomb Charge")]
    [SerializeField] private float minThrowSpeed = 9f;
    [SerializeField] private float maxThrowSpeed = 18f;
    [SerializeField] private float maxChargeTime = 2f;

    [Header("Aim")]
    [SerializeField] private LineRenderer aimLine;
    [SerializeField] private float aimLength = 1f;
    [SerializeField] private float lineWidth = 0.05f;

    private bool chargingBomb = false;
    private float holdTime = 0f;

    private int currentHealth; // Vida actual del jugador. Es private porque solo se maneja desde este script.
    private int jumpsRemaining; // Cantidad de saltos que le quedan al jugador en este momento.
    private int direction = 1; // Dirección hacia donde mira el jugador / 1 = derecha / -1 = izquierda.

    private bool isGrounded; // Indica si el jugador está tocando el piso. True = está en el piso / False = está en el aire.
    private bool isOnPlayer; // Indica si el jugador está tocando a otro jugador (para permitir salto desde la cabeza de otro jugador).
    private bool isOnSurface; // Indica si el jugador está tocando cualquier superficie (piso o jugador).

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

    // Start se ejecuta una sola vez cuando inicia el juego.
    void Start()
    {
        aimLine.startWidth = lineWidth;
        aimLine.endWidth = lineWidth;
        aimLine.enabled = false;
        // Obtiene el Rigidbody2D que está puesto en el jugador.
        rb = GetComponent<Rigidbody2D>();

        // Obtiene el Animator que está puesto en el jugador.
        animator = GetComponent<Animator>();

        // Guarda el tamaño original del personaje.
        // Esto es importante porque después lo volteamos usando escala negativa.
        originalScale = transform.localScale;

        // Al iniciar, la vida actual será igual a la vida máxima.
        currentHealth = maxHealth;

        // Al iniciar, el jugador tiene todos sus saltos disponibles.
        jumpsRemaining = maxJumps;
    }
    // Update se ejecuta una vez por cada frame.
    // Aquí se revisan teclas y acciones constantes del jugador.
    void Update()
    {
        // Variable temporal para saber si el jugador se mueve.
        // -1 = izquierda.
        // 0 = quieto.
        // 1 = derecha.
        float moveInput = 0f;

        // Si se presiona la tecla de izquierda...
        if (Input.GetKey(leftKey))
        {
            // El jugador se moverá hacia la izquierda.
            moveInput = -1f;

            // Guardamos que está mirando hacia la izquierda.
            direction = -1;
        }

        // Si se presiona la tecla de derecha...
        if (Input.GetKey(rightKey))
        {
            // El jugador se moverá hacia la derecha.
            moveInput = 1f;

            // Guardamos que está mirando hacia la derecha.
            direction = 1;
        }

        // Aplicamos el movimiento horizontal al Rigidbody2D.
        // En X usamos la velocidad de movimiento.
        // En Y conservamos la velocidad actual para no afectar salto o caída.
        if (chargingBomb) // Si esta cargando la bomba que el player no se mueva
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            animator.SetBool("isRunning", false);
        }
        else
        {
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
            // Le decimos al Animator si el jugador está corriendo.
            // Si moveInput es diferente de 0, significa que se está moviendo.
            animator.SetBool("isRunning", moveInput != 0);
        }

        isOnSurface = isGrounded || isOnPlayer;

        // Le decimos al Animator si el jugador está saltando.
        // Si NO está en el piso, entonces está en el aire.
        animator.SetBool("isJumping", !isOnSurface);

        // Le decimos al Animator si el jugador está cayendo.
        animator.SetBool("isFalling", rb.linearVelocity.y < 0 && !isOnSurface);

        // Si el jugador se mueve hacia la derecha...
        if (moveInput > 0)
        {
            // Lo ponemos mirando hacia la derecha. Mathf.Abs asegura que X sea positiva.
            transform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        }
        // Si el jugador se mueve hacia la izquierda...
        else if (moveInput < 0)
        {
            // Lo ponemos mirando hacia la izquierda. La X negativa voltea visualmente el sprite.
            transform.localScale = new Vector3(-Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        }

        // Si se presiona la tecla de salto y todavía quedan saltos disponibles...
        if (!chargingBomb && Input.GetKeyDown(jumpKey) && jumpsRemaining > 0)
        {
            isOnPlayer = false;

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

            if (!isGrounded && jumpsRemaining == 1)
            {
                animator.SetBool("isDoubleJumping", true);
                animator.SetBool("isFalling", false);
                animator.SetBool("isJumping", false);
            }

            jumpsRemaining--;
        }

        // LANZMIENTO DE BOMBA CON HOLD DE LANZAMIENTO
        // Si se presiona la tecla de bomba...
        if (Input.GetKeyDown(bombKey) && activeSpecialBomb != null && activeSpecialBomb.isThrown() && activeSpecialBomb.GetBombType() == BombType.Gravity)
        {
            activeSpecialBomb.ToggleGravity();
            return;
        }

        if (bombInventory > 0 || activeSpecialBomb != null) 
        {
            if (Input.GetKeyDown(bombKey))
            {
                StartChargingBomb();
            }

            if (Input.GetKey(bombKey))
            {
                ChargeBomb();
            }

            if (Input.GetKeyUp(bombKey))
            {
                ReleaseBomb();
            }
        }
        else
        {
            if(aimLine != null) // Si no hay bombas y la linea existe, se oculta
            {
                aimLine.enabled = false;
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
        // Restamos el daño recibido a la vida actual.
        currentHealth -= damage;

        // Mostramos la vida actual en la consola.
        Debug.Log("Vida actual: " + currentHealth);

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
        Debug.Log("Player murió");

        animator.SetTrigger("Die");

        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;

        Invoke(nameof(DisablePlayer), 0.6f);
    }
    private void DisablePlayer()
    {
        gameObject.SetActive(false);
    }
    // Función encargada de lanzar la bomba.
    public void ThrowBomb(float throwSpeed)
    {

        if (activeSpecialBomb != null)
        {
            if (activeSpecialBomb.isThrown())
            {
                switch (activeSpecialBomb.GetBombType())
                {
                    case BombType.Gravity:
                        activeSpecialBomb.ToggleGravity();
                        return;

                    case BombType.String:
                        // logica
                        return;

                    case BombType.Sticky:
                        //logica
                        return;

                }

                // Si no esta lanzada, se borra la referencia la bomba
                activeSpecialBomb = null;
            }
        }

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

        if (newBomb != null)
        {
            newBomb.SetBombType(currentBombType); // Se crea un bomba Normal por defecto

            Vector2 throwDirection = GetThrowDirection();
            newBomb.Throw(throwDirection, throwSpeed);

            //newBomb.Throw(new Vector2(direction, 1f), 10f);
            //bombScript.Throw(Vector2.up, 10f);

            if (currentBombType != BombType.Normal)
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
    public void SetBombType(BombType bombType)
    {
        Debug.Log("Tipo de bomba ahora: " + bombType);
        currentBombType = bombType;
    }
    public void AddHealth()
    {
        Debug.Log("Salud aumentada");
        currentHealth += 1;
    }
    public void AddBoomerang()
    {
        Debug.Log("Tipo de bomba ahora: Boomerang");
    }
    public void SetControls(KeyCode left, KeyCode right, KeyCode jump, KeyCode bomb, KeyCode down)
    {
        leftKey = left;
        rightKey = right;
        jumpKey = jump;
        bombKey = bomb;
        downKey = down;
    }
    private Vector2 GetThrowDirection()
    {
        float x = 0f;
        float y = 0f;

        if (Input.GetKey(leftKey))
        {
            x = -1f;
        }

        if (Input.GetKey(rightKey))
        {
            x = 1f;
        }

        if (Input.GetKey(jumpKey))
        {
            y = 1f;
        }

        if (Input.GetKey(downKey))
        {
            y = -1f;
        }

        // Si presiona la x sin ninguna direccion
        if (x == 0f && y == 0f)
        {
            x = direction; // Se lanza hacia direction, hacia donde mira el jugador. Solo en X
        }

        return new Vector2(x, y).normalized;
    }
    public void EnablePhysics(){
        rb.bodyType = RigidbodyType2D.Dynamic;
    }
    private void StartChargingBomb()
    {
        aimLine.enabled = true;
        chargingBomb = true;
        holdTime = 0f;
    }
    private void ChargeBomb()
    {
        if(!chargingBomb) // Si no esta cargando la bomba
            return;

        UpdateAimLine();
        holdTime += Time.deltaTime;

        if (holdTime >= maxChargeTime)
        {
            ReleaseBomb();
        }
    }
    private void ReleaseBomb()
    {
        if (!chargingBomb) // Si no esta cargando la bomba
            return;

        chargingBomb = false;
        aimLine.enabled = false;

        float t = holdTime / maxChargeTime; // Normaliza el tiempo / 0 no cargó nada / 1 carga máxima

        // Da un valor entre minimo y maximo (minThrowForce y maxThrowForce)
        float throwSpeed = Mathf.Lerp(minThrowSpeed, maxThrowSpeed, t);

        ThrowBomb(throwSpeed);
    }
    private void UpdateAimLine()
    {
        aimLine.enabled = true;

        Vector2 direction = GetThrowDirection();

        Vector3 start = bombSpawnPoint.position;
        Vector3 end = start + (Vector3)(direction * aimLength);

        aimLine.SetPosition(0, start);
        aimLine.SetPosition(1, end);

        float t = holdTime / maxChargeTime;

        Color currentColor = Color.Lerp(Color.white, Color.red, t);

        aimLine.startColor = currentColor;
        aimLine.endColor = currentColor;
    }
}