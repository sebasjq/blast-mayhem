using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

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

    [Header("Sistema Pantalla de Carga")]
    [SerializeField] private GameObject fondoDeCarga;
    [SerializeField] private GameObject panelControles;
    [SerializeField] private GameObject panelBombas;
    [SerializeField] private Slider barraDeCarga;
    [SerializeField] private float tiempoTotalDeLectura = 6f;

    private int player1Index = 0;
    private int player2Index = 1;

    void Start()
    {
        UpdateUI(); // Actualiza la UI para mostrar los personajes iniciales seleccionados

        // Por seguridad, aseguramos que el panel de carga empiece apagado al iniciar la escena
        if (fondoDeCarga != null) fondoDeCarga.SetActive(false);
    }

    public void NextPlayer1()
    {
        player1Index = (player1Index + 1) % playerPrefabs.Length;
        UpdateUI();
    }

    public void PreviousPlayer1()
    {
        player1Index--;
        if (player1Index < 0) player1Index = playerPrefabs.Length - 1;
        UpdateUI();
    }

    public void NextPlayer2()
    {
        player2Index = (player2Index + 1) % playerPrefabs.Length;
        UpdateUI();
    }

    public void PreviousPlayer2()
    {
        player2Index--;
        if (player2Index < 0) player2Index = playerPrefabs.Length - 1;
        UpdateUI();
    }

    // ESTA FUNCIÓN LA TIENE TU BOTÓN "INICIAR" (EL DE PIXEL ART)
    public void StartGame()
    {
        Debug.Log("¡Guardando personajes y activando pantalla de carga!");

        // 1. PRIMERO: Guardamos los personajes seleccionados para que aparezcan en el juego
        PlayerSelectionData.player1Prefab = playerPrefabs[player1Index];
        PlayerSelectionData.player2Prefab = playerPrefabs[player2Index];

        // 2. SEGUNDO: Encendemos los paneles de tutoriales en la cara del jugador
        if (fondoDeCarga != null) fondoDeCarga.SetActive(true);
        if (panelControles != null) panelControles.SetActive(true);
        if (panelBombas != null) panelBombas.SetActive(false);

        // 3. TERCERO: Reseteamos la barrita visual
        if (barraDeCarga != null) barraDeCarga.value = 0f;

        // 4. CUARTO: Iniciamos la carga asíncrona en segundo plano del nivel "02_BombTest"
        StartCoroutine(CargarJuegoYActualizarBarra());
    }

    IEnumerator CargarJuegoYActualizarBarra()
    {
        // Ponemos a Unity a cargar la escena real de pruebas en la RAM
        AsyncOperation cargaFondo = SceneManager.LoadSceneAsync(gameSceneName);
        cargaFondo.allowSceneActivation = false; // No dejes entrar al jugador todavía

        float tiempoPasado = 0f;

        // Bucle automático mientras dure el tiempo de lectura o el juego termine de cargar
        while (tiempoPasado < tiempoTotalDeLectura || cargaFondo.progress < 0.9f)
        {
            tiempoPasado += Time.deltaTime;

            // Calculamos el porcentaje visual (de 0 a 1)
            float progresoVisual = Mathf.Clamp01(tiempoPasado / tiempoTotalDeLectura);
            if (barraDeCarga != null) barraDeCarga.value = progresoVisual;

            // Si pasa de la mitad (50%), ocultamos controles y mostramos las bombas
            if (progresoVisual >= 0.5f && panelControles.activeSelf)
            {
                panelControles.SetActive(false);
                panelBombas.SetActive(true);
            }

            yield return null; // Espera al siguiente frame
        }

        // Aseguramos que se llene al 100% la barra
        if (barraDeCarga != null) barraDeCarga.value = 1f;

        // ¡Misión cumplida! Viajamos a la escena de juego con los personajes cargados
        cargaFondo.allowSceneActivation = true;
    }

    private void UpdateUI()
    {
        player1Image.sprite = playerSprites[player1Index];
        player1NameText.text = playerNames[player1Index];

        player2Image.sprite = playerSprites[player2Index];
        player2NameText.text = playerNames[player2Index];
    }
}