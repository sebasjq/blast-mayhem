using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FullScreenToggle : MonoBehaviour
{
    public Toggle toggle;

    void Start()
    {
        // 1. Buscamos en la memoria si ya había una preferencia guardada.
        // Como PlayerPrefs no guarda valores "bool" (true/false) directamente, 
        // usamos: 1 significa True, 0 significa False.
        // Si es la primera vez que abre el juego, por defecto asumimos que arranca en Pantalla Completa (1).
        int pantallaCompletaGuardada = PlayerPrefs.GetInt("PantallaCompleta", 1);

        // 2. Aplicamos el estado visual al cuadrito de la interfaz (Toggle)
        if (pantallaCompletaGuardada == 1)
        {
            toggle.isOn = true;
        }
        else
        {
            toggle.isOn = false;
        }

        // 3. Forzamos a Unity a aplicar el estado real a la pantalla física de inmediato
        Screen.fullScreen = toggle.isOn;
    }

    // Esta función se vincula al evento "On Value Changed" del Toggle en el Inspector
    public void ActiveFULLS(bool fullscreen)
    {
        // 1. Cambiamos el estado de la pantalla en tiempo real
        Screen.fullScreen = fullscreen;

        // 2. Guardamos la elección del jugador en los PlayerPrefs usando nuestro truco numérico
        if (fullscreen == true)
        {
            PlayerPrefs.SetInt("PantallaCompleta", 1); // 1 = Pantalla Completa
        }
        else
        {
            PlayerPrefs.SetInt("PantallaCompleta", 0); // 0 = Modo Ventana
        }

        // 3. Forzamos el guardado inmediato en el disco duro
        PlayerPrefs.Save();
    }
}