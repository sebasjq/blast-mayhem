using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections; // Necesario para usar temporizadores (Corrutinas)

public class Intro : MonoBehaviour
{
    [Header("Configuración")]
    public float tiempoDeEspera = 3f; // Tiempo en segundos que durará la pantalla
    public string nombreDelMenuPrincipal = "01_MainMenu"; // Nombre correspondiente a escena del menú

    void Start()
    {
        // Inicia el temporizador en cuanto arranca la pantalla
        StartCoroutine(PasarAlMenu());
    }

    IEnumerator PasarAlMenu()
    {
        // Le decimos a Unity que espere la cantidad de segundos indicada
        yield return new WaitForSeconds(tiempoDeEspera);

        // Cargamos la escena del menú principal
        SceneManager.LoadScene(nombreDelMenuPrincipal);
    }
}