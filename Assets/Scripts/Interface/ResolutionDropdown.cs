using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResolutionDropdown : MonoBehaviour
{
    public Toggle toggle;
    public TMP_Dropdown resolucionesDropDown;

    private Resolution[] resoluciones;

    void Start()
    {
        // 1. Configurar el Toggle de Pantalla Completa desde los PlayerPrefs
        int pantallaCompletaGuardada = PlayerPrefs.GetInt("PantallaCompleta", 1);
        toggle.isOn = (pantallaCompletaGuardada == 1);
        Screen.fullScreen = toggle.isOn;

        // 2. Configurar y cargar las Resoluciones
        ConfigurarDropdownResoluciones();
    }

    public void ActiveFULLS(bool pantallaCompleta)
    {
        Screen.fullScreen = pantallaCompleta;
        PlayerPrefs.SetInt("PantallaCompleta", pantallaCompleta ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void ConfigurarDropdownResoluciones()
    {
        // Obtener las resoluciones disponibles del monitor actual
        resoluciones = Screen.resolutions;
        resolucionesDropDown.ClearOptions();

        List<string> opciones = new List<string>();

        // Cargamos la resolución guardada por el usuario. 
        // Si no hay nada guardado (primera vez), usamos la resolución actual del monitor por defecto.
        int anchoGuardado = PlayerPrefs.GetInt("ResAncho", Screen.currentResolution.width);
        int altoGuardado = PlayerPrefs.GetInt("ResAlto", Screen.currentResolution.height);

        int indiceResolucionActual = 0;

        for (int i = 0; i < resoluciones.Length; i++)
        {
            // Creamos el texto para el Dropdown (ej: "1920 x 1080")
            string opcion = resoluciones[i].width + " x " + resoluciones[i].height;
            opciones.Add(opcion);

            // Buscamos si esta resolución de la lista coincide con la que guardamos en memoria
            if (resoluciones[i].width == anchoGuardado && resoluciones[i].height == altoGuardado)
            {
                indiceResolucionActual = i;
            }
        }

        // Llenamos el dropdown con las opciones reales del monitor
        resolucionesDropDown.AddOptions(opciones);

        // Marcamos visualmente la opción correcta
        resolucionesDropDown.value = indiceResolucionActual;
        resolucionesDropDown.RefreshShownValue();

        // Aplicamos la resolución real al juego de inmediato al iniciar
        Resolution resElegida = resoluciones[indiceResolucionActual];
        Screen.SetResolution(resElegida.width, resElegida.height, Screen.fullScreen);
    }

    // Esta función se vincula al "On Value Changed (Int32)" del Dropdown en el Inspector
    public void CambiarResolucion(int indiceResolucion)
    {
        // 1. Obtenemos la resolución que seleccionó el jugador
        Resolution resolution = resoluciones[indiceResolucion];

        // 2. Le decimos a la pantalla que cambie de tamaño
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);

        // 3. Guardamos los datos exactos en la memoria
        PlayerPrefs.SetInt("ResAncho", resolution.width);
        PlayerPrefs.SetInt("ResAlto", resolution.height);
        PlayerPrefs.Save(); // Forzamos el guardado
    }
}
