using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameObject pauseMenu;

    void Update()
    {
        pauseMenu.SetActive(InputManager.gameIsPaused);
    }

    public void ResumeGame()
    {
        InputManager.gameIsPaused = false;
        InputManager.SwitchActionMap(InputManager.playerControls.Player);
    }

    public void ReturnToMenu()
    {
        GameNetworkManager.instance.Disconnect();

        SceneManager.LoadScene(0);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
