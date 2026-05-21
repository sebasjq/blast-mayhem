using UnityEngine;

public class PlayerSpawnManager : MonoBehaviour
{
    [SerializeField] private Transform player1SpawnPoint;
    [SerializeField] private Transform player2SpawnPoint;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SpawnPlayers(); // Se llama la funcion en el Start para que se ejecute al inicio del juego
    }

    private void SpawnPlayers()
    {
        // Se crean null porque no se sabe si los jugadores van a ser instanciados o no,
        //dependiendo de si se han seleccionado o no en la pantalla de selección
        GameObject p1 = null; 
        GameObject p2 = null;

        // Se instancian los jugadores solo si se han seleccionado en la pantalla de selección, de lo contrario se quedan como null
        if (PlayerSelectionData.player1Prefab != null)
        {
            // Se instancia el jugador 1 en su punto de spawn
            p1 = Instantiate(PlayerSelectionData.player1Prefab, player1SpawnPoint.position, Quaternion.identity);
        }

        if (PlayerSelectionData.player2Prefab != null)
        {
            p2 = Instantiate(PlayerSelectionData.player2Prefab, player2SpawnPoint.position, Quaternion.identity);
        }

        // Si se crearon correctamente...
        if (p1 != null)
        {
            // Se crea un objeto tipo PlayerMovement para cada jugador, en este caso con la variable p1Movement...
            PlayerMovement p1Movement = p1.GetComponent<PlayerMovement>();
            Animator p1Animator = p1.GetComponent<Animator>();
            Rigidbody2D p1Rb = p1.GetComponent<Rigidbody2D>();

            p1Rb.bodyType = RigidbodyType2D.Kinematic; // Se pone el cuerpo en Kinematic para que se ejecute primero la animación de spawn y luego caiga por la gravedad
            p1Movement.SetControls(KeyCode.A, KeyCode.D, KeyCode.W, KeyCode.X, KeyCode.S); // Se toma la funcion SetControls de la clase PlayerMovement y se le asignan las teclas correspondientes...
            p1Animator.SetTrigger("Spawn");
        }

        if (p2 != null)
        {
            PlayerMovement p2Movement = p2.GetComponent<PlayerMovement>();
            Animator p2Animator = p2.GetComponent<Animator>();
            Rigidbody2D p2Rb = p2.GetComponent<Rigidbody2D>();


            p2Rb.bodyType = RigidbodyType2D.Kinematic;
            p2Movement.SetControls(KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.UpArrow, KeyCode.Space, KeyCode.DownArrow);
            p2Animator.SetTrigger("Spawn");
        }
    }
}
