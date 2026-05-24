using UnityEngine;
using UnityEngine.UI; // Necesario para poder usar elementos de interfaz gráfica como el Slider

public class BrightnessSlider : MonoBehaviour
{
    public Slider miSlider;

    // Start se ejecuta una sola vez justo cuando el menú principal aparece en pantalla
    void Start()
    {
        // 1. Buscamos en el disco duro (PlayerPrefs) si el jugador ya había guardado un nivel de "Brillo" antes.
        // 2. Si es la primera vez que abre el juego y no hay nada guardado, usamos 0.5f por defecto (el centro).
        // 3. Movemos la palanca del slider visualmente a esa posición.
        miSlider.value = PlayerPrefs.GetFloat("Brillo", 0.5f);
    }

    // Esta función debe conectarse al evento "On Value Changed" del Slider en el Inspector de Unity.
    // Unity le enviará automáticamente el 'valor' (un número entre 0 y 1) cada vez que el jugador mueva la palanca.
    public void GuardarBrillo(float valor)
    {
        // 1. Escribimos el nuevo número en la memoria bajo el nombre de "Brillo"
        PlayerPrefs.SetFloat("Brillo", valor);

        // 2. Obligamos a Unity a guardar esto en el disco duro INMEDIATAMENTE.
        // Esto evita que el dato se pierda si el juego se cierra de golpe o se cambia de escena muy rápido.
        PlayerPrefs.Save();
    }
}