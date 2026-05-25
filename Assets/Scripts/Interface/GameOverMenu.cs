using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverMenu : MonoBehaviour
{
    public static GameOverMenu Instance { get; private set; }

    [Header("Referencias de UI")]
    [SerializeField] private GameObject panelGameOver;
    [SerializeField] private GameObject[] elementosAocultar;

    [Header("Configuración de Tiempo")]
    [Tooltip("Tiempo en segundos que esperará antes de mostrar el menú de Game Over.")]
    [SerializeField] private float tiempoDeEspera = 1.5f; // Puedes cambiarlo a tu gusto en Unity

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Esta función ahora inicia la secuencia con espera
    public void ActivarMenu()
    {
        StartCoroutine(SecuenciaGameOver());
    }

    // La corrutina que maneja los tiempos
    private IEnumerator SecuenciaGameOver()
    {
        // 1. Ocultamos inmediatamente la UI del juego (barras de vida, pausa, etc.)
        foreach (GameObject elemento in elementosAocultar)
        {
            if (elemento != null)
            {
                elemento.SetActive(false);
            }
        }

        // 2. Aquí el código se "pausa" durante los segundos que le asignaste
        // Mientras tanto, el usuario ve cómo desaparece el jugador en el fondo
        yield return new WaitForSeconds(tiempoDeEspera);

        // 3. Pasado el tiempo, encendemos por fin el panel de Game Over
        if (panelGameOver != null)
        {
            panelGameOver.SetActive(true);
        }
    }

    public void Reiniciar()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void MenuInicial(string Nombre)
    {
        SceneManager.LoadScene(Nombre);
    }

    public void Salir()
    {
        Debug.Log("Cerrando el juego");
        Application.Quit();
    }
}