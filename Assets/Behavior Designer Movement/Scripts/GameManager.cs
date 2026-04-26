using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    void Update()
    {
        if (Keyboard.current == null) return; //Safety check to make sure a keyboard exists

        if (Keyboard.current.escapeKey.wasPressedThisFrame) //If escape key is pressed, then return to main menu.
        {
            Debug.Log("Escape pressed! Returning to Main Menu...");
            SceneManager.LoadScene(0); //Loads main menu scene.
        }
    }
}