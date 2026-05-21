using UnityEngine;

public class Pickups : MonoBehaviour
{
    [SerializeField] private PickupType pickupType;
    [SerializeField] private float lifeTime = 6f; // Tiempo de duración antes de que el objeto se destruya automáticamente
    [SerializeField] private Animator animator;

    private Rigidbody2D rb;
    private bool collected = false;

    void Start()
    {
        Destroy(gameObject, lifeTime); // Destruye el objeto después de lifeTime segundos
    }

    // Awake se llama antes de Start, lo que garantiza que rb y animator estén disponibles
    // sin tener que llamar a GetComponent en cada frame o en otros métodos, mejorando el rendimiento.
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (collected) return; // Evita recoger el mismo objeto varias veces
            collected = true;

            Debug.Log("Entró trigger con: " + collision.gameObject.name);

            // Basicamente esta linea dice: dame cualquier componente
            // de este GameObject que pueda recibir pickups,
            // es decir, que implemente la interfaz IPlayerPickupReceiver.

            // player por su parte guarda la referencia al componente encontrado,
            // lo que permite acceder a métodos como AddBomb() sin depender
            // de un script específico como PlayerTest.

            // Busca cualquier componente que sepa recibir pickups
            IPickupReceiver receiver = collision.gameObject.GetComponent<IPickupReceiver>();

            if (receiver == null) return; // Si no encuentra un receptor, no hace nada

            switch (pickupType)
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
                    receiver.SetBombType(BombType.String);
                    break;

                case PickupType.StickyPickup:
                    Debug.Log("Sticky recogido");
                    receiver.SetBombType(BombType.Sticky);
                    break;
            }
            Collect();
        }
    }

    public void Collect()
    {
        animator.SetTrigger("Collect");

        // Detener fisicas para que no siga cayendo mientras se anima
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
    }
}
