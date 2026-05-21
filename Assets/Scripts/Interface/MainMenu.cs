using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;


public class MainMenu : MonoBehaviour
{
    public void Jugar()

    {
        SceneManager.LoadScene("03_CharacterSelection");
    }

    public void Salir()
    {
        Debug.Log("Saliending...");
        Application.Quit();
    }
}
