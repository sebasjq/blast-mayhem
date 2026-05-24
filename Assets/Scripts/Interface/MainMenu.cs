using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement; // Librería que le da a Unity el poder de cambiar entre escenas


public class MainMenu : MonoBehaviour
{
    public void Jugar()

    {
        // Le decimos al administrador de escenas que cargue exactamente la escena con este nombre.
        SceneManager.LoadScene("03_CharacterSelection");
    }

    public void Salir()
    {
        // / Texto en la Consola el cual confirma que el botón sí está bien conectado y funcionando.
        Debug.Log("Saliending...");

        // Cierra el juego por completo. 
        // Solo funcionará de verdad cuando el juego esté exportado (Build) e instalado en una PC.
        Application.Quit();
    }
}
