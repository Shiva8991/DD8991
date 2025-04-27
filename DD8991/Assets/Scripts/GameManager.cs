using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject cardPrefab;
    public Transform cardParent;
    public Sprite[] spriteList; // Drag all available sprites here
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI tryText;
    int columns = 4;
    int rows = 4;
    private List<Card> flippedCards = new();
    [SerializeField]
    private List<CardData> runtimePool = new();
    [HideInInspector]
    public int score = 0;
    [HideInInspector]
    public int tries = 0;

    public float aspectRatio = 0.8f;

    private void Start()
    {
        LoadGame();
    }

    public void StartGame()
    {
        ClearCards();
        PrepareRuntimeCardPool();
        CreateLayout();
        StartCoroutine(ShowAllCardsBriefly());
    }

    public void SetGridSize(int newColumns, int newRows)
    {
        columns = newColumns;
        rows = newRows;
    }

    private void PrepareRuntimeCardPool()
    {
        runtimePool.Clear();
        int totalCards = columns * rows;

        // Calculate unique pairs needed (rounding up if odd)
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

        // Generate unique card data (up to maxUniquePairsNeeded)
        for (int i = 0; i < Mathf.Min(spriteList.Length, maxUniquePairsNeeded); i++)
        {
            var data = new CardData
            {
                id = Utils.GenerateUniqueID(), // Ensure unique IDs
                frontSprite = spriteList[i]
            };
            generatedCardData.Add(data);
        }

        // Create pairs (handles both even and odd totalCards)
        for (int i = 0; i < totalCards; i++)
        {
            // Use modulo to loop through available pairs
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
        float padding = 30f; // Padding around the grid


        // Calculate cell size (equal division of parent space)
        float cellWidth = (parentRect.rect.width - 2 * padding) / columns;
        float cellHeight = (parentRect.rect.height - 2 * padding) / rows;


        Debug.Log("columns " + columns + "rows" + rows);
        Debug.Log("Parent Rect: " + parentRect.rect.width + "x" + parentRect.rect.height);
        Debug.Log("cellWidth " + cellWidth + "cellHeight" + cellHeight);


        // Calculate card size (2:3 aspect ratio, fitting within cell)
        float cardWidth, cardHeight;
        if (cellWidth / cellHeight > aspectRatio)
        {
            // Cell is wider than needed → card height matches cell height
            Debug.Log("Cell is wider than needed → card height matches cell height");
            cardHeight = cellHeight - padding;
            cardWidth = cardHeight * aspectRatio;
        }
        else
        {
            // Cell is taller than needed → card width matches cell width
            Debug.Log("Cell is taller than needed → card width matches cell width");
            cardWidth = cellWidth - padding;
            cardHeight = cardWidth * (1 / aspectRatio);
        }

        //cardWidth = cellWidth - padding;
        //cardHeight = cellHeight - padding;
        Debug.Log("cardWidth " + cardWidth + "cardHeight" + cardHeight);

        // Calculate starting position (top-left corner of grid)
        float startX = -parentRect.rect.width / 2f + padding + cellWidth / 2f;
        float startY = parentRect.rect.height / 2f - padding - cellHeight / 2f;

        // Create and position cards
        for (int i = 0; i < runtimePool.Count; i++)
        {
            var cardGO = Instantiate(cardPrefab, cardParent);
            var card = cardGO.GetComponent<Card>();
            card.Setup(runtimePool[i], this);

            // Calculate grid position
            int row = i / columns;
            int col = i % columns;

            // Calculate card position (center of cell)
            float xPos = startX + col * cellWidth;
            float yPos = startY - row * cellHeight;

            // Apply position and size
            RectTransform cardRect = cardGO.GetComponent<RectTransform>();
            cardRect.anchoredPosition = new Vector2(xPos, yPos);
            cardRect.sizeDelta = new Vector2(cardWidth, cardHeight);
        }
    }
    public void RegisterFlip(Card card)
    {
        flippedCards.Add(card);
       
        if (flippedCards.Count == 2)
        {
            tries++;
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
            score++;
            scoreText.text = score.ToString();
            yield return new WaitForSeconds(1f);
            a.Hide();
            b.Hide();
            AudioManager.Instance.PlaySound(SoundType.Match);
        }
        else
        {
            yield return new WaitForSeconds(1f);
            a.Flip(false);
            b.Flip(false);
            AudioManager.Instance.PlaySound(SoundType.Mismatch);
        }

        flippedCards.Clear();
    }

    public bool CanFlip(Card card)
    {
        return flippedCards.Count < 2 && !flippedCards.Contains(card);
    }

    public void SaveGame()
    {
        SaveManager.Save(this);
    }

    public void LoadGame()
    {
        SaveManager.Load(this);
    }
}
