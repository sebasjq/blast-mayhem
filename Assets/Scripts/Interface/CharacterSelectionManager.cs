using UnityEngine;
using UnityEngine.UI; // For UI elements like Image
using TMPro; // For TextMeshPro elements
using UnityEngine.SceneManagement; // For scene management

public class CharacterSelectionManager : MonoBehaviour
{
    [Header("Characters")]
    [SerializeField] private GameObject[] playerPrefabs;
    [SerializeField] private Sprite[] playerSprites;
    [SerializeField] private string[] playerNames;

    [Header("Player 1 UI")]
    [SerializeField] private Image player1Image;
    [SerializeField] private TMP_Text player1NameText;

    [Header("Player 2 UI")]
    [SerializeField] private Image player2Image;
    [SerializeField] private TMP_Text player2NameText;

    [Header("Scene")]
    [SerializeField] private string gameSceneName = "02_BombTest";

    private int player1Index = 0;
    private int player2Index = 1;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateUI(); // Actualiza la UI para mostrar los personajes iniciales seleccionados
    }
    
    public void NextPlayer1()
    {
        Debug.Log("Click derecha Player 1");

        // Incrementa el índice de Player 1 y lo envuelve alrededor del número de personajes disponibles
        player1Index = (player1Index + 1) % playerPrefabs.Length; // El % asegura que vuelva al inicio del array después de llegar al final
        
        UpdateUI();
    }

    public void PreviousPlayer1()
    {
        Debug.Log("Click izquierda Player 1");

        // Decrementa el índice de Player 1 y lo envuelve alrededor del número de personajes disponibles
        player1Index--;

        if(player1Index < 0)
            player1Index = playerPrefabs.Length - 1;

        UpdateUI();
    }

    public void NextPlayer2()
    {
        Debug.Log("Click derecha Player 2");

        // Incrementa el índice de Player 1 y lo envuelve alrededor del número de personajes disponibles
        player2Index = (player2Index + 1) % playerPrefabs.Length; // El % asegura que vuelva al inicio del array después de llegar al final
        
        UpdateUI();
    }

    public void PreviousPlayer2()
    {
        Debug.Log("Click izquierda Player 2");

        // Decrementa el índice de Player 1 y lo envuelve alrededor del número de personajes disponibles
        player2Index--;

        if (player2Index < 0)
            player2Index = playerPrefabs.Length - 1;

        UpdateUI();
    }

    public void StartGame()
    {
        PlayerSelectionData.player1Prefab = playerPrefabs[player1Index]; // Asigna el prefab del personaje seleccionado para Player 1
        PlayerSelectionData.player2Prefab = playerPrefabs[player2Index]; // Asigna el prefab del personaje seleccionado para Player 2

        SceneManager.LoadScene(gameSceneName); // Carga la escena del juego
    }

    private void UpdateUI()
    {
        Debug.Log("UpdateUI ejecutado");

        // Update Player 1 UI
        player1Image.sprite = playerSprites[player1Index]; // Pone el sprite del personaje seleccionado en el Image de Player 1
        player1NameText.text = playerNames[player1Index]; // Pone el nombre del personaje seleccionado en el Text de Player 1

        player2Image.sprite = playerSprites[player2Index]; // Pone el sprite del personaje seleccionado en el Image de Player 2
        player2NameText.text = playerNames[player2Index]; // Pone el nombre del personaje seleccionado en el Text de Player 2
    }
}
