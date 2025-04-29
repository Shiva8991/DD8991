using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    public int currentLevel = 1;
    [HideInInspector] public int score = 0;
    [HideInInspector] public int tries = 0;
    public bool gameOver = false;

    [Header("Card Settings")]
    public GameObject cardPrefab;
    public float cardAspectRatio = 0.8f;
    public Sprite[] spriteList;
    public Transform cardParent;

    [Header("UI References")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI tryText;
    public CommonPopup commonPopup;

    [Header("Runtime Data")]
    public List<CardData> runtimePool = new();
    public List<Card> flippedCards = new();

    [Header("Board Configuration")]
    private int columns = 4;
    private int rows = 4;

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

    public void StartGame()
    {
        ClearCards();
        InItScreen(currentLevel);
        PrepareRuntimeCardPool();
        CreateLayout();
        StartCoroutine(ShowAllCardsBriefly());
    }
    
    #region Game State Management

    public void ClearCards()
    {
        foreach (Transform child in cardParent)
            Destroy(child.gameObject);
    }

    public void InItScreen(int level, bool isFromSave = false)
    {
        var levelData = MenuManager.Instance.levels[level - 1];
        columns = levelData.columns;
        rows = levelData.rows;

        // Only reset tries/score if NOT loading from a save
        if (!isFromSave)
        {
            tries = levelData.tries;
            score = 0;
        }
        // Update UI elements
        tryText.text = tries.ToString();
        scoreText.text = score.ToString();
    }

    private void PrepareRuntimeCardPool()
    {
        runtimePool.Clear();
        int totalCards = columns * rows;
        int maxUniquePairsNeeded = Mathf.CeilToInt(totalCards / 2f);

        if (spriteList.Length == 0)
        {
            Debug.LogError("No Sprites Assigned for cards!");
            return;
        }

        if (spriteList.Length < maxUniquePairsNeeded)
        {
            Debug.LogWarning("Not enough unique sprites for layout size, repeating sprites!");
        }

        List<CardData> generatedCardData = new();

        for (int i = 0; i < Mathf.Min(spriteList.Length, maxUniquePairsNeeded); i++)
        {
            var data = new CardData
            {
                id = Utils.GenerateUniqueID(),
                frontSprite = spriteList[i]
            };
            generatedCardData.Add(data);
        }

        for (int i = 0; i < totalCards; i++)
        {
            var data = generatedCardData[i % maxUniquePairsNeeded];
            runtimePool.Add(data);
        }
        Utils.Shuffle(runtimePool);
    }

    public void CreateLayout()
    {
        Debug.Log("<color=magenta>ENTER CREATELAYOUT</color>");
        ClearCards();

        RectTransform parentRect = cardParent.GetComponent<RectTransform>();
        float padding = 30f;

        float cellWidth = (parentRect.rect.width - 2 * padding) / columns;
        float cellHeight = (parentRect.rect.height - 2 * padding) / rows;

        float cardWidth, cardHeight;
        if (cellWidth / cellHeight > cardAspectRatio)
        {
            cardHeight = cellHeight - padding;
            cardWidth = cardHeight * cardAspectRatio;
        }
        else
        {
            cardWidth = cellWidth - padding;
            cardHeight = cardWidth * (1 / cardAspectRatio);
        }

        float startX = -parentRect.rect.width / 2f + padding + cellWidth / 2f;
        float startY = parentRect.rect.height / 2f - padding - cellHeight / 2f;

        for (int i = 0; i < runtimePool.Count; i++)
        {
            var cardGO = Instantiate(cardPrefab, cardParent);
            var card = cardGO.GetComponent<Card>();
            card.Setup(runtimePool[i]);

            int row = i / columns;
            int col = i % columns;

            float xPos = startX + col * cellWidth;
            float yPos = startY - row * cellHeight;

            RectTransform cardRect = cardGO.GetComponent<RectTransform>();
            cardRect.anchoredPosition = new Vector2(xPos, yPos);
            cardRect.sizeDelta = new Vector2(cardWidth, cardHeight);
        }
    }

    private IEnumerator ShowAllCardsBriefly()
    {
        foreach (var card in cardParent.GetComponentsInChildren<Card>())
            card.Flip(true);

        yield return new WaitForSeconds(2f);

        foreach (var card in cardParent.GetComponentsInChildren<Card>())
            card.Flip(false);
    }
    #endregion
    public void SetCurrentLevel(int level) => currentLevel = level;

    public void RegisterFlip(Card card)
    {
        if (tries <= 0) return;

        flippedCards.Add(card);

        if (flippedCards.Count == 2)
        {
            tries--;
            tryText.text = tries.ToString();
            StartCoroutine(CheckMatch());
        }
    }

    private IEnumerator CheckMatch()
    {
        Card a = flippedCards[0];
        Card b = flippedCards[1];

        if (a.GetID() == b.GetID())
        {
            AudioManager.Instance.PlaySound(SoundType.Match);
            score++;
            scoreText.text = score.ToString();
            yield return new WaitForSeconds(1f);
            a.Hide();
            b.Hide();
        }
        else
        {
            AudioManager.Instance.PlaySound(SoundType.Mismatch);
            yield return new WaitForSeconds(1f);
            a.Flip(false);
            b.Flip(false);
        }

        flippedCards.Clear();
        ManagePopups();
    }

    /// <summary>
    /// Checks if the level is completed or last level is reached. Shows appropriate popups.
    /// </summary>
    public void ManagePopups()
    {
        if (IsLevelCompleted() && !gameOver)
        {
            Debug.Log("currentLevel: " + currentLevel + " levels: " + MenuManager.Instance.levels.Count);

            if (currentLevel < MenuManager.Instance.levels.Count)
            {
                Debug.Log("Level Completed!");
                commonPopup.ShowPopup(
                    $"Level cleared! Your score: {score}",
                    "TRY NEXT", LoadNextLevel,
                    "EXIT", () => ExitToMainMenu(true)
                );
            }
            else
            {
                Debug.Log("All levels completed!");
                commonPopup.ShowPopup(
                    $"Congrats, all levels cleared!",
                    "RESTART", RestartLevel,
                    "EXIT", () => ExitToMainMenu(false)
                );
            }
        }
        else if (tries <= 0)
        {
            Debug.Log("Tries exhausted - Game Over!");
            StartCoroutine(GameOver());
        }
    }

    public bool IsLevelCompleted()
    {
        int interactableCards = 0;

        foreach (Card card in cardParent.GetComponentsInChildren<Card>())
        {
            // Count cards that are both active AND interactable (not matched)
            if (card.gameObject.activeInHierarchy && card.button.interactable)
                interactableCards++;
        }
        // Win condition: 0 or 1 cards remaining (handles odd-numbered layouts)
        return interactableCards <= 1;
    }

    private IEnumerator GameOver()
    {
        gameOver = true;

        yield return new WaitForSeconds(1f);

        foreach (var card in cardParent.GetComponentsInChildren<Card>())
        {
            card.Flip(true);
        }
        AudioManager.Instance.PlaySound(SoundType.GameOver);

        yield return new WaitForSeconds(1f);

        commonPopup.ShowPopup(
            $"Game Over! Your score: {score}",
            "RETRY", RestartLevel,
            "EXIT", () => ExitToMainMenu(true));
    }

    #region Button Callbacks.
    void Reset()
    {
        gameOver = false;
        commonPopup.HidePopup();
        flippedCards.Clear();
    }

    void RestartLevel()
    {
        SaveSystem.DeleteSave();
        Reset();
        StartGame();
    }

    void LoadNextLevel()
    {
        SaveSystem.DeleteSave();
        Reset();
        currentLevel++;
        StartGame();
    }

    public void ExitToMainMenu(bool saveBeforeExit = true)
    {
        if (saveBeforeExit)
        {
            SaveSystem.SaveGame();
        }
        else
        {
            SaveSystem.DeleteSave();
        }
        ClearCards();
        Reset();
        MenuManager.Instance.InitScreen();
    }
    #endregion


    public bool CanFlip(Card card)
    {
        return flippedCards.Count < 2 && !flippedCards.Contains(card);
    }

    public int GetCurrentLevel()
    {
        for (int i = 1; i < MenuManager.Instance.levels.Count; i++)
        {
            var level = MenuManager.Instance.levels[i];
            if (level.columns == columns && level.rows == rows)
            {
                return i;
            }
        }
        return -1;
    }

    public void SaveGame()
    {
        SaveSystem.SaveGame();
    }

    public bool LoadGame()
    {
        return SaveSystem.LoadGame();
    }

    /* private void OnApplicationQuit()
    {
        Debug.Log("Application is quitting. Saving game state.");
        SaveSystem.SaveGame();
    } */
}
