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
        // Not currently working

        // SceneManager.LoadScene(0);
        // GameNetworkManager.instance.Disconnect();
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
