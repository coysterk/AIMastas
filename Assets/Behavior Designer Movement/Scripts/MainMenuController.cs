using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public GameObject controlsPanel; //Reference to the controls menu.

    public void StartGame() //When start button is clicked.
    {
        SceneManager.LoadScene(1); //Loads the game.
    }

    public void OpenControls() //When controls button is clicked.
    {
        if (controlsPanel != null)
        {
            controlsPanel.SetActive(true); //Goes into the controls overview.
        }
    }

    public void CloseControls() //When the back button is clicked in the controls menu.
    {
        if (controlsPanel != null)
        {
            controlsPanel.SetActive(false); //Goes back to normal title screen.
        }
    }

    public void QuitGame() //When the Quit button is clicked.
    {
        Debug.Log("Quit Game triggered! (Note: This will close the compiled game, but won't stop the Unity Editor)"); //A reminder that you closed the game in Unity.
        Application.Quit(); //Close the application.
    }
}