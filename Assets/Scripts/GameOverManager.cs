using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverManager : MonoBehaviour {
	// Start is called before the first frame update
	[SerializeField] private GameObject gameOverOverlay;
	[SerializeField] private RessourceSystemManager ressourceSystemMngr;
	[SerializeField] private TextMeshProUGUI gameOverText;

	private float startTime;
	private bool isGameOver = false;

    private void Awake()
    {
		startTime = Time.time;
    }

    public void OnQuitClicked() {
		Application.Quit();
	}

	public void OnRetryClicked() {
		SceneManager.LoadScene("SpaceShip", LoadSceneMode.Single);
	}

	public void OnMainMenuClicked() {
		SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
	}

	public void StartGameOver() {
		if (!isGameOver)
			gameOverText.text = string.Format("You survived: {0} seconds", (int)(Time.time - startTime));
		
		isGameOver = true;
		gameOverOverlay.SetActive(true);
	}
}
