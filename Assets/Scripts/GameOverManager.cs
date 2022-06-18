using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private GameObject gameOverOverlay;
    [SerializeField] private RessourceSystemManager ressourceSystemMngr;

    public void OnQuitClicked()
    {
        Application.Quit();
    }

    public void OnRetryClicked()
    {
        SceneManager.LoadScene(0, LoadSceneMode.Single);
    }

    public void OnMainMenuClicked()
    {
        SceneManager.LoadScene(1, LoadSceneMode.Single);
    }

    public void StartGameOver()
    {
        gameOverOverlay.SetActive(true);
    }
}
