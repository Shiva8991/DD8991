using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class MenuManager : MonoBehaviour
{
    public TMP_Dropdown layoutDropdown;
    public GameManager gameManager;
    public GameObject MenunScreen;
    public GameObject GameScreen;
    public GameObject buttonsParent;

    Dictionary<string, Vector2Int> layouts = new()
    {
        { "Select Layout", new Vector2Int(2,2) },
        { "2x2", new Vector2Int(2,2) },
        { "2x3", new Vector2Int(2,3) },
        { "3x3", new Vector2Int(3,3) },
        { "4x4", new Vector2Int(4,4) }
    };

    void Start()
    {
        SetScreen(false);
        toggleButtons(false);
        layoutDropdown.onValueChanged.AddListener(OnLayoutChanged);
        OnLayoutChanged(0);
    }

    private void toggleButtons(bool show)
    {
        foreach (Transform child in buttonsParent.transform)
        {
            child.GetComponent<Button>().interactable = show;
        }
    }

    public void SetScreen(bool showGameScreen)
    {
        MenunScreen.SetActive(!showGameScreen);
        GameScreen.SetActive(showGameScreen);
    }

    void OnLayoutChanged(int index)
    {
        var selected = layoutDropdown.options[index].text;
        Vector2Int size = layouts[selected];
        gameManager.SetGridSize(size.x, size.y);
        toggleButtons(index != 0);

    }

    public void OnPlayClicked()
    {
        Debug.Log("Play clicked!");
        SetScreen(true);
        gameManager.StartGame();
    }
    //public void OnLoadClicked() => gameManager.LoadGame();
    public void OnExitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }
}
