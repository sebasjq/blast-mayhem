using UnityEngine;
using UnityEngine.UI;

public class GlobalBrightness : MonoBehaviour
{
    public Image panelOscuro;
    private static GlobalBrightness instancia;

    void Awake()
    {
        // Si ya hay un brillo sonando de una escena anterior, destruimos la copia
        if (instancia != null)
        {
            Destroy(gameObject);
            return;
        }
        instancia = this;
        DontDestroyOnLoad(gameObject); // Escudo protector al Canvas entero
    }

    // Usamos un pequeño truco: leer el brillo guardado en cada frame
    // No es la forma más ultra-optimizada del mundo, pero para un brillo simple, funciona perfecto y evita errores molestos
    void Update()
    {
        float valorBrillo = PlayerPrefs.GetFloat("Brillo", 0.5f);

        // Matemáticas simples: 0 a 1
        if (valorBrillo < 0.5f)
        {
            float alpha = 0.5f - valorBrillo;
            panelOscuro.color = new Color(0, 0, 0, alpha); // Oscurecer
        }
        else
        {
            float alpha = valorBrillo - 0.5f;
            panelOscuro.color = new Color(1, 1, 1, alpha); // Aclarar
        }
    }
}
