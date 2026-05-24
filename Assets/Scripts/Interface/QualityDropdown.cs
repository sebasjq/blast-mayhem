using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QualityDropdown : MonoBehaviour
{
    // Se arrastra el componente TMP_Dropdown desde el Inspector
    public TMP_Dropdown dropdown;

    void Start()
    {
        // 1. Cargamos la calidad guardada. Si es la primera vez, ponemos la opción 3 por defecto (ej. Alta/Ultra)
        int calidadGuardada = PlayerPrefs.GetInt("numeroDeCalidad", 3);

        // 2. Movemos el Dropdown visualmente a la opción correcta
        dropdown.value = calidadGuardada;

        // 3. Aplicamos esa calidad al motor gráfico de inmediato al iniciar la escena
        QualitySettings.SetQualityLevel(calidadGuardada);
    }

    // Esta función se conectará al evento del Dropdown. 
    // Recibe automáticamente un "int" que representa el índice de la opción elegida (0, 1, 2, 3...)
    public void CambiarCalidadGrafica(int indiceCalidad)
    {
        // 1. Le decimos a Unity que cambie la calidad de los gráficos en tiempo real
        QualitySettings.SetQualityLevel(indiceCalidad);

        // 2. Guardamos el número de la opción en la memoria
        PlayerPrefs.SetInt("numeroDeCalidad", indiceCalidad);

        // 3. Forzamos el guardado inmediato en el disco duro para que no se pierda al cambiar de escena
        PlayerPrefs.Save();

        Debug.Log("Calidad cambiada a: " + QualitySettings.names[QualitySettings.GetQualityLevel()]);
    }

}
