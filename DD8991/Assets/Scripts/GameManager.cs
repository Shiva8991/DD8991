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

    public GameObject cardPrefab;
    public CommonPopup commonPopup;
    public Transform cardParent;
    public Sprite[] spriteList;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI tryText;
    [HideInInspector] public int score = 0;
    [HideInInspector] public int tries = 0;

    [SerializeField] private List<CardData> runtimePool = new();
    private List<Card> flippedCards = new();

    private int columns = 4;
    private int rows = 4;
    public float cardAspectRatio = 0.8f;
    bool gameOver = false;
    int currentLevel = 1;

    void Awake()
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
        LoadGame();
    }

    public void StartGame()
    {
        ClearCards();
        InItScreen(currentLevel);
        PrepareRuntimeCardPool();
        CreateLayout();
        StartCoroutine(ShowAllCardsBriefly());
    }

    public void SetCurrentLevel(int level) => currentLevel = level;

    void InItScreen(int level)
    {
        Debug.Log("Initializing screen for level: " + level);
        var levelData = MenuManager.Instance.levels[level - 1];
        Debug.Log("Level Data: " + levelData.levelName + " " + levelData.columns + " " + levelData.rows + " " + levelData.tries);
        columns = levelData.columns;
        rows = levelData.rows;
        tries = levelData.tries;
        tryText.text = tries.ToString();
        score = 0;
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

    private IEnumerator ShowAllCardsBriefly()
    {
        foreach (var card in cardParent.GetComponentsInChildren<Card>())
            card.Flip(true);

        yield return new WaitForSeconds(2f);

        foreach (var card in cardParent.GetComponentsInChildren<Card>())
            card.Flip(false);
    }

    private void ClearCards()
    {
        foreach (Transform child in cardParent)
            Destroy(child.gameObject);
    }

    private void CreateLayout()
    {
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

    public void RegisterFlip(Card card)
    {
        Debug.Log("ENTER REGISTERFLIP, tries: " + tries);

        if (tries <= 0) return;

        flippedCards.Add(card);

        if (flippedCards.Count == 2)
        {
            tries--;
            tryText.text = tries.ToString();
            Debug.Log("tries2 :: " + tries);

            StartCoroutine(CheckMatch());
        }
    }


    private IEnumerator CheckMatch()
    {
        Debug.Log("ENTER CHECKMATCH");

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

        // 🔥 After flipping check game state correctly
        if (IsGameWon() && !gameOver)
        {
            Debug.Log("currentLevel: " + currentLevel + " levels: " + MenuManager.Instance.levels.Count);

            if (currentLevel < MenuManager.Instance.levels.Count)
            {
                Debug.Log("Level Completed!");
                commonPopup.ShowPopup(
                    $"Level cleared! Your score: {score}",
                    "TRY NEXT", LoadNextLevel,
                    "EXIT", ExitToMainMenu
                );
            }
            else
            {
                Debug.Log("All levels completed!");
                commonPopup.ShowPopup(
                    $"Congrats, all levels cleared!",
                    "RESTART", RestartLevel,
                    "EXIT", ExitToMainMenu
                );
            }
        }
        else if (tries <= 0 && !gameOver)
        {
            Debug.Log("Tries exhausted - Game Over!");
            StartCoroutine(GameOver());
        }
    }

    private bool IsGameWon()
    {
        int activeCards = 0;

        foreach (Transform child in cardParent)
        {
            if (child.gameObject.activeInHierarchy)
                activeCards++;
        }

        return activeCards <= 1;
    }


    private IEnumerator GameOver()
    {
        Debug.Log("Game Over!");
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
            "EXIT", ExitToMainMenu);
    }

    void Reset()
    {
        gameOver = false;
        commonPopup.HidePopup();
        flippedCards.Clear();
    }

    void RestartLevel()
    {
        Reset();
        StartGame();
    }

    void LoadNextLevel()
    {
        Reset();
        currentLevel++;
        StartGame();
    }

    public void ExitToMainMenu()
    {
        Reset();
        MenuManager.Instance.InitScreen();
    }



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
        SaveManager.Save();
    }

    public void LoadGame()
    {
        SaveManager.Load();
    }
}
