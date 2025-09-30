using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameContorlerMenu : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject instruccionesPanel; // arrastra tu Panel en el inspector

    private void Update()
    {
        
    }

    // 🔹 Cambiar de escena
    public void LoaderSceneM(string NameScene)
    {
        SceneManager.LoadScene(NameScene);
    }

    // 🔹 Mostrar panel de instrucciones
    public void ShowInstrucciones()
    {
        if (instruccionesPanel != null)
            instruccionesPanel.SetActive(true);
    }

    // 🔹 Ocultar panel de instrucciones
    public void HideInstrucciones()
    {
        if (instruccionesPanel != null)
            instruccionesPanel.SetActive(false);
    }
}
