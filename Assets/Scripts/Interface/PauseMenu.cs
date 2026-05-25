using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [Header("Paneles de Interfaz")]
    [SerializeField] private GameObject botonPausa;
    [SerializeField] private GameObject menuPausa;

    [Header("Navegación")]
    [SerializeField] private string nombreEscenaMenu = "01_MainMenu";

    private bool juegoPausado = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (juegoPausado)
            {
                Reanudar();
            }
            else
            {
                Pausa();
            }
        }
    }

    public void Pausa()
    {
        juegoPausado = true;
        Time.timeScale = 0f; // Congela el juego
        botonPausa.SetActive(false);
        menuPausa.SetActive(true);
    }

    public void Reanudar()
    {
        juegoPausado = false;
        Time.timeScale = 1f; // Descongela el juego
        botonPausa.SetActive(true);
        menuPausa.SetActive(false);
    }

    public void Reiniciar()
    {
        juegoPausado = false;
        Time.timeScale = 1f; // ¡Vital descongelar antes de recargar!
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void IrMenuPrincipal()
    {
        juegoPausado = false;
        Time.timeScale = 1f; // ¡Súper importante para que el menú principal no cargue congelado!
        SceneManager.LoadScene(nombreEscenaMenu);
    }

    public void Cerrar()
    {
        Debug.Log("Cerrando el juego");
        Application.Quit();
    }
}