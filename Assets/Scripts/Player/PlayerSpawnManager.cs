using UnityEngine;
using UnityEngine.UI;

public class PlayerSpawnManager : MonoBehaviour
{
    [SerializeField] private Transform player1SpawnPoint;
    [SerializeField] private Transform player2SpawnPoint;

    [Header("Barras de Vida de la Interfaz")]
    [SerializeField] private HealthBarSlider barraJugador1; // Barra izquierda
    [SerializeField] private HealthBarSlider barraJugador2; // Barra derecha

    [Header("Retratos de Personajes")]
    [SerializeField] private Image retratoP1;
    [SerializeField] private Image retratoP2;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SpawnPlayers(); // Se llama la funcion en el Start para que se ejecute al inicio del juego
    }

    private void SpawnPlayers()
    {
        // Se crean null porque no se sabe si los jugadores van a ser instanciados o no
        GameObject p1 = null;
        GameObject p2 = null;

        // Se instancian los jugadores solo si se han seleccionado
        if (PlayerSelectionData.player1Prefab != null)
        {
            p1 = Instantiate(PlayerSelectionData.player1Prefab, player1SpawnPoint.position, Quaternion.identity);
        }

        if (PlayerSelectionData.player2Prefab != null)
        {
            p2 = Instantiate(PlayerSelectionData.player2Prefab, player2SpawnPoint.position, Quaternion.identity);
        }

        // Si se crearon correctamente...
        if (p1 != null)
        {
            PlayerMovement p1Movement = p1.GetComponent<PlayerMovement>();
            Animator p1Animator = p1.GetComponent<Animator>();
            Rigidbody2D p1Rb = p1.GetComponent<Rigidbody2D>();

            p1Rb.bodyType = RigidbodyType2D.Kinematic;

            // Al final de los controles, le pasamos la barraJugador1
            p1Movement.SetControls(KeyCode.A, KeyCode.D, KeyCode.W, KeyCode.X, KeyCode.S, barraJugador1, "Jugador 1");

            p1Animator.SetTrigger("Spawn");
        }

        if (p2 != null)
        {
            PlayerMovement p2Movement = p2.GetComponent<PlayerMovement>();
            Animator p2Animator = p2.GetComponent<Animator>();
            Rigidbody2D p2Rb = p2.GetComponent<Rigidbody2D>();

            p2Rb.bodyType = RigidbodyType2D.Kinematic;

            // Al final de los controles, le pasamos la barraJugador2
            p2Movement.SetControls(KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.UpArrow, KeyCode.Space, KeyCode.DownArrow, barraJugador2, "Jugador 2");

            p2Animator.SetTrigger("Spawn");
            if (retratoP1 != null && PlayerSelectionData.player1Sprite != null)
            {
                retratoP1.sprite = PlayerSelectionData.player1Sprite;
            }

            if (retratoP2 != null && PlayerSelectionData.player2Sprite != null)
            {
                retratoP2.sprite = PlayerSelectionData.player2Sprite;
            }
        }
    }
}
