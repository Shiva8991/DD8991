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
    public Button continueButton;

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

    /// <summary>
    /// Switches between menu and game screens, hides popups, and updates continue button state.
    /// </summary>
    /// <param name="showGameScreen">True to show game screen, false for menu</param>
    public void SetScreen(bool showGameScreen)
    {
        menuScreen.SetActive(!showGameScreen);
        gameScreen.SetActive(showGameScreen);
        GameManager.Instance.commonPopup.HidePopup();
        continueButton.interactable = SaveSystem.HasSavedGame();
    }

    /// <summary>
    /// Populates the dropdown with level names as filled by user in Inspector.
    /// </summary>
    public void PopulateDropdown()
    {
        layoutDropdown.ClearOptions();
        List<string> options = new();

        options.Add("Select Layout");
        foreach (var level in levels)
        {
            options.Add(level.levelName);
        }

        layoutDropdown.AddOptions(options);
        layoutDropdown.onValueChanged.AddListener(OnLayoutChanged);
    }

    public void OnLayoutChanged(int index)
    {
        if (index == 0)
        {
            toggleButtons(false);
            return;
        }
        GameManager.Instance.SetCurrentLevel(index);
        toggleButtons(true);
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

    #region Button Callbacks
    public void OnPlayClicked()
    {
        SetScreen(true);
        GameManager.Instance.StartGame();
    }

    public void OnContinueClicked()
    {
        Debug.Log("<color=white>Continue clicked! </color>");
        SetScreen(true);
        if (!GameManager.Instance.LoadGame())
        {
            OnPlayClicked();
        }
    }

    public void OnExitClicked()
    {
        Debug.Log("Exit clicked!");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    #endregion
}