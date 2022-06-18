using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private Canvas gameOverOverlay;
    [SerializeField] private RessourceSystemManager ressourceSystemMngr;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (ressourceSystemMngr.HP.current <= 0)
        {
            StartGameOver();
        }
    }

    public void OnQuitClicked()
    {
        Application.Quit();
    }

    public void OnRetryClicked()
    {
        SceneManager.LoadScene(0, LoadSceneMode.Single);
    }

    public void StartGameOver()
    {
        gameOverOverlay.enabled = true;
    }
}
