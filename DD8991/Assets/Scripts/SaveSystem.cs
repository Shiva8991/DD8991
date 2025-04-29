using System;
using UnityEngine;

public static class SaveSystem
{
    private const string SAVE_KEY = "CardGameSaveData";

    public static void SaveGame()
    {
        GameState state = new GameState();

        // Basic game state
        state.currentLevel = GameManager.Instance.currentLevel;
        state.score = GameManager.Instance.score;
        state.tries = GameManager.Instance.tries;
        state.gameOver = GameManager.Instance.gameOver;
        state.IsLevelCompleted = GameManager.Instance.IsLevelCompleted();
        state.saveTime = DateTime.Now;

        // Card states
        Card[] allCards = GameManager.Instance.cardParent.GetComponentsInChildren<Card>();
        foreach (Card card in allCards)
        {
            state.cardIDs.Add(card.GetID());
            state.spriteNames.Add(card.GetFrontSpriteName());
            state.cardHiddenStates.Add(!card.gameObject.activeSelf);
            state.cardFlippedStates.Add(card.isFlipped);
        }

        // Flipped cards (store indices)
        foreach (Card flippedCard in GameManager.Instance.flippedCards)
        {
            for (int i = 0; i < allCards.Length; i++)
            {
                if (allCards[i] == flippedCard)
                {
                    state.flippedCardIndices.Add(i);
                    break;
                }
            }
        }

        // Convert to JSON and save
        string json = JsonUtility.ToJson(state);
        Debug.Log("Saved game ::  " + json);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
    }

    public static bool LoadGame()
    {
        if (!PlayerPrefs.HasKey(SAVE_KEY)) return false;

        string json = PlayerPrefs.GetString(SAVE_KEY);
        GameState state = JsonUtility.FromJson<GameState>(json);

        // Restore basic game state
        GameManager.Instance.currentLevel = state.currentLevel;
        GameManager.Instance.score = state.score;
        GameManager.Instance.tries = state.tries;
        GameManager.Instance.gameOver = state.gameOver;

        // Update UI
        GameManager.Instance.scoreText.text = GameManager.Instance.score.ToString();
        GameManager.Instance.tryText.text = GameManager.Instance.tries.ToString();

        // Clear existing cards
        GameManager.Instance.ClearCards();

        // Recreate the card layout
        GameManager.Instance.InItScreen(GameManager.Instance.currentLevel, true);

        // Prepare runtime pool from saved IDs and sprite names
        GameManager.Instance.runtimePool.Clear();
        for (int i = 0; i < state.cardIDs.Count; i++)
        {
            Sprite frontSprite = null;
            foreach (Sprite sprite in GameManager.Instance.spriteList)
            {
                if (sprite.name == state.spriteNames[i])
                {
                    frontSprite = sprite;
                    break;
                }
            }

            GameManager.Instance.runtimePool.Add(new CardData
            {
                id = state.cardIDs[i],
                frontSprite = frontSprite
            });
        }

        // Create the layout
        GameManager.Instance.CreateLayout();

        // Restore card states
        Card[] allCards = GameManager.Instance.cardParent.GetComponentsInChildren<Card>();
        for (int i = 0; i < allCards.Length; i++)
        {
            allCards[i].gameObject.SetActive(!state.cardHiddenStates[i]);
            allCards[i].ForceFlip(state.cardFlippedStates[i]);
        }

        // Restore flipped cards
        GameManager.Instance.flippedCards.Clear();
        foreach (int index in state.flippedCardIndices)
        {
            if (index < allCards.Length)
            {
                GameManager.Instance.flippedCards.Add(allCards[index]);
            }
        }

        // Manage Popups.
        GameManager.Instance.ManagePopups();

        return true;
    }

    public static bool HasSavedGame()
    {
        return PlayerPrefs.HasKey(SAVE_KEY);
    }

    public static void DeleteSave()
    {
        PlayerPrefs.DeleteKey(SAVE_KEY);
        PlayerPrefs.Save();
    }
}