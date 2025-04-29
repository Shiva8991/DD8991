using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameState
{
    public int currentLevel;
    public int score;
    public int tries;
    public bool gameOver;
    public bool IsLevelCompleted;
    
    // Card data
    public List<int> cardIDs = new List<int>();
    public List<string> spriteNames = new List<string>();
    public List<bool> cardHiddenStates = new List<bool>();
    public List<bool> cardFlippedStates = new List<bool>();
    
    // Flipped cards
    public List<int> flippedCardIndices = new List<int>();
    public DateTime saveTime;
}