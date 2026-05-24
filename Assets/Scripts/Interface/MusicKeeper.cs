using UnityEngine;

public class MusicKeeper : MonoBehaviour
{
    // Esta variable estática guardará la única copia permitida de la música
    private static MusicKeeper instanciaUnica;

    void Awake()
    {
        // 1. REVISAR SI YA EXISTE OTRA MÚSICA SONANDO
        if (instanciaUnica != null && instanciaUnica != this)
        {
            // Si ya hay una música de fondo sonando, destruimos esta nueva para que no se duplique
            Destroy(gameObject);
            return;
        }

        // 2. SI SOMOS LA PRIMERA, NOS CONVERTIMOS EN LA OFICIAL
        instanciaUnica = this;

        // 3. LE PONEMOS EL ESCUDO PROTECTOR PARA QUE APAREZCA EN LAS OTRAS SCENES
        DontDestroyOnLoad(gameObject);
    }
}
