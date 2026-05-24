using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio; // Necesario para controlar el AudioMixer
using UnityEngine.UI;    // Necesario para controlar la interfaz (Sliders, Imágenes)

public class VolumeSlider : MonoBehaviour
{
    // --- REFERENCIAS DE LA INTERFAZ VISUAL ---

    public Slider slider;

    [Tooltip("El valor actual en el que se encuentra el slider (de 0.0001 a 1)")]
    public float sliderValue;

    [Tooltip("La imagen (ej. una 'X' o parlante tachado) que aparece al mutear")]
    public Image imagenMute;

    // --- CONFIGURACIONES DEL MOTOR DE AUDIO ---

    [Header("Configuración Audio")]

    [Tooltip("El cerebro de audio general (tu archivo de MezcladorPrincipal)")]
    public AudioMixer miMezclador;

    [Tooltip("El nombre exacto del parámetro expuesto en el Mixer (ej: VolMusica)")]
    public string nombreParametroMixer;

    [Tooltip("La 'llave' para guardar el dato en la memoria del juego (ej: guardadoMusica)")]
    public string nombrePlayerPrefs;

    // Start se ejecuta una sola vez al cargar la pantalla donde esté este script
    void Start()
    {
        // 1. Carga el volumen guardado de partidas anteriores. Si es la primera vez que juega, usa 0.5 por defecto.
        slider.value = PlayerPrefs.GetFloat(nombrePlayerPrefs, 0.5f);

        // 2. Aplica ese volumen recién cargado al motor de audio real.
        AjustarVolumen(slider.value);

        // 3. Verifica visualmente si el volumen cargado es casi cero para encender el ícono de "mute".
        RevisarSiEstoyMute();
    }

    // Esta función se llama AUTOMÁTICAMENTE cada vez que el jugador arrastra la palanca del slider
    public void ChangeSlider(float valor)
    {
        // 1. Actualiza nuestra variable interna con el nuevo valor de la palanca
        sliderValue = valor;

        // 2. Guarda INMEDIATAMENTE este nuevo valor en la memoria del disco duro (PlayerPrefs)
        PlayerPrefs.SetFloat(nombrePlayerPrefs, sliderValue);

        // 3. Manda el nuevo valor a la fórmula matemática para bajar/subir el volumen real
        AjustarVolumen(valor);

        // 4. Verifica si se bajó la palanca al mínimo para mostrar la imagen de silencio visual
        RevisarSiEstoyMute();
    }

    // La función encargada de traducir la barra visual (0 a 1) al idioma del Mixer (Decibelios)
    private void AjustarVolumen(float valor)
    {
        // Si el valor es extremadamente bajo (casi 0), cortamos el sonido por completo.
        // Hacemos esto porque matemáticamente calcular el logaritmo de 0 causa un error en el juego.
        if (valor <= 0.001f)
        {
            // -80f decibelios es el silencio absoluto en el Mixer de Unity
            miMezclador.SetFloat(nombreParametroMixer, -80f);
        }
        else
        {
            // Transforma el valor lineal del slider (0.0001 a 1) a una curva logarítmica.
            // Multiplicar por 45 hace que la caída del sonido sea más natural y perceptible para el oído humano.
            miMezclador.SetFloat(nombreParametroMixer, Mathf.Log10(valor) * 45);
        }
    }

    // Prende o apaga el ícono visual del silencio (Parlante tachado con x roja)
    private void RevisarSiEstoyMute()
    {
        // Si la barra está en lo más mínimo...
        if (sliderValue <= 0.001f)
        {
            imagenMute.enabled = true;  // Mostramos la imagen
        }
        else
        {
            imagenMute.enabled = false; // Ocultamos la imagen
        }
    }
}