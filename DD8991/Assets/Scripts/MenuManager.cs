using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }

    [Header("Level Settings")]
    public List<LayoutData> levels = new(); // Fill via Inspector

    [Header("UI Elements")]
    public TMP_Dropdown layoutDropdown;
    public GameObject menuScreen;
    public GameObject gameScreen;
    public GameObject buttonsParent;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        InitScreen();
    }

    public void InitScreen()
    {
        SetScreen(false);
        PopulateDropdown();
        OnLayoutChanged(0);
    }

    public void PopulateDropdown()
    {
        layoutDropdown.ClearOptions();
        List<string> options = new();

        options.Add("Select Layout"); // Always first option
        foreach (var level in levels)
        {
            options.Add(level.levelName);
        }

        layoutDropdown.AddOptions(options);
        layoutDropdown.onValueChanged.AddListener(OnLayoutChanged);
    }

    private void toggleButtons(bool show)
    {
        foreach (Transform child in buttonsParent.transform)
        {
            if (!child.CompareTag("DontDisable"))
            {
                Button button = child.GetComponent<Button>();
                if (button != null)
                {
                    button.interactable = show;
                }
            }
        }
    }

    public void SetScreen(bool showGameScreen)
    {
        menuScreen.SetActive(!showGameScreen);
        gameScreen.SetActive(showGameScreen);
        GameManager.Instance.commonPopup.HidePopup();
    }

    public void OnLayoutChanged(int index)
    {
        Debug.Log($"index: {index}");
        if (index == 0)
        {
            toggleButtons(false);
            return;
        }
        GameManager.Instance.SetCurrentLevel(index);
        toggleButtons(true);
    }

    public void OnPlayClicked()
    {
        Debug.Log("Play clicked!");
        SetScreen(true);
        GameManager.Instance.StartGame();
    }

    public void OnExitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
