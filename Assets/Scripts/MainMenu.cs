using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject tutorialCanvas;

    public void OnQuitClicked()
    {
        Application.Quit();
    }

    public void OnStartClicked()
    {
        SceneManager.LoadScene(0, LoadSceneMode.Single);
    }

    public void OnTutorialClick()
    {
        tutorialCanvas.SetActive(true);
    }

    public void OnTutorialBackClick()
    {
        tutorialCanvas.SetActive(false);
    }
}
