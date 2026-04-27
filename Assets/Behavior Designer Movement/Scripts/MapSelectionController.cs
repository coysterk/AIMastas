using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MapSelectionController : MonoBehaviour
{
    //Custom Size UI
    public TMP_InputField widthInputField;
    public TMP_InputField heightInputField;

    // --- Predefined Sizes ---
    public void SelectSmall()
    {
        SaveAndLoad(20, 10);
    }

    public void SelectMedium()
    {
        SaveAndLoad(40, 20);
    }

    public void SelectLarge()
    {
        SaveAndLoad(60, 30);
    }

    public void SelectCustom()
    {
        //Default fallbacks in case the player leaves the boxes blank.
        int chosenWidth = 40;
        int chosenHeight = 20;

        //int.TryParse tries to turn the text into a number. If it works, it saves it!
        if (int.TryParse(widthInputField.text, out int parsedW)) chosenWidth = parsedW;
        if (int.TryParse(heightInputField.text, out int parsedH)) chosenHeight = parsedH;

        // Keep the map from being way too small or way too big
        chosenWidth = Mathf.Clamp(chosenWidth, 10, 150);
        chosenHeight = Mathf.Clamp(chosenHeight, 10, 150);

        SaveAndLoad(chosenWidth, chosenHeight);
    }

    // --- The Transition Logic ---
    private void SaveAndLoad(int width, int height)
    {
        // 1. Put the numbers in the static backpack
        GameSettings.mapWidth = width;
        GameSettings.mapHeight = height;
        GameSettings.isDataLoaded = true;

        // 2. Load Scene Index 2 (Your Main Game)
        SceneManager.LoadScene(2);
    }
}