using UnityEngine;

public class Pickups : MonoBehaviour
{
    [SerializeField] private PickupType pickupType; // Tipo de pickup que este objeto representa
    [SerializeField] private float lifeTime = 6f; // Tiempo de duración antes de que el objeto se destruya automáticamente
    [SerializeField] private Animator animator; // Referencia al componente Animator para controlar las animaciones del pickup

    private Rigidbody2D rb; 
    private bool collected = false; // Bandera para evitar recoger el mismo objeto varias veces

    void Start()
    {
        Destroy(gameObject, lifeTime); // Destruye el pickup después de lifeTime segundos
    }

    // Awake se llama antes de Start, lo que garantiza que rb y animator estén disponibles
    // sin tener que llamar a GetComponent en cada frame o en otros métodos, mejorando el rendimiento.
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>(); // Obtiene la referencia al Rigidbody2D para controlar las físicas del pickup
        animator = GetComponent<Animator>(); // Obtiene la referencia al Animator para controlar las animaciones del pickup
    }

    // Función que se llama automáticamente cuando otro collider entra en contacto con el collider de este objeto
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player")) // Si el objeto que colisiona tiene la etiqueta "Player"...
        {
            if (collected) return; // Evita recoger el mismo objeto varias veces
            collected = true; // Marca el pickup como recogido para evitar múltiples activaciones

            Debug.Log("Entró trigger con: " + collision.gameObject.name); // Debug para verificar que el trigger se activa correctamente

            // Basicamente esta linea dice: dame cualquier componente
            // de este GameObject que pueda recibir pickups,
            // es decir, que implemente la interfaz IPlayerPickupReceiver.

            // player por su parte guarda la referencia al componente encontrado,
            // lo que permite acceder a métodos como AddBomb() sin depender
            // de un script específico como PlayerTest.

            // Busca cualquier componente que sepa recibir pickups
            IPickupReceiver receiver = collision.gameObject.GetComponent<IPickupReceiver>();

            if (receiver == null) return; // Si no encuentra un receptor, no hace nada

            switch (pickupType) // Dependiendo del tipo de pickup, se llama al método correspondiente en el receptor, osea, el Player u otro objeto que implemente la interfaz IPickupReceiver
            {
                case PickupType.Health:
                    Debug.Log("Vida recogida");
                    receiver.AddHealth();
                    break;

                case PickupType.GravityPickup:
                    Debug.Log("Gravity recogido");
                    receiver.SetBombType(BombType.Gravity);
                    break;

                case PickupType.StringPickup:
                    Debug.Log("String recogido");
                    receiver.SetBombType(BombType.Spring);
                    break;

                case PickupType.StickyPickup:
                    Debug.Log("Sticky recogido");
                    receiver.SetBombType(BombType.Sticky);
                    break;
            }

            Collect();
        }
    }

    // Método para manejar la animación de recogida del pickup y detener las físicas para que el objeto no siga cayendo mientras se anima
    public void Collect()
    {
        animator.SetTrigger("Collect"); // Activa la animación de recogida utilizando el Animator

        // Detener fisicas para que no siga cayendo mientras se anima
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
    }
}
