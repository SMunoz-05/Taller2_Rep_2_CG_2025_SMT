using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameContorlerMenu : MonoBehaviour
{
    private void Update()
    {
    }
    public void LoaderSceneM(string NameScene)
    {
        SceneManager.LoadScene(NameScene);
    }
}