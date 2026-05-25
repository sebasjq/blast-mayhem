using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class HealthBarSlider : MonoBehaviour
{
    private Slider slider;

    private void Awake() // Es mejor usar Awake para asegurar que obtenga el Slider antes de que el Player intente usarlo
    {
        slider = GetComponent<Slider>();
    }

    public void CambiarVidaMaxima(float VidaMaxima)
    {
        slider.maxValue = VidaMaxima;
    }

    public void CambiarVidaActual(float CantidadVida)
    {
        slider.value = CantidadVida; 
    }

    public void InicializarBarraDeVida(float CantidadVida)
    {
        CambiarVidaMaxima(CantidadVida);
        CambiarVidaActual(CantidadVida);
    }
}