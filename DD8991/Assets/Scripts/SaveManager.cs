using UnityEngine;
using UnityEngine.UI;
[System.Serializable]
public class GameSaveData
{
    public int score;
    public int tries;
}

public static class SaveManager
{
    private const string SaveKey = "CardMatchSave";

    public static void Save()
    {
        var data = new GameSaveData
        {
            score =  GameManager.Instance.score,
            tries =  GameManager.Instance.tries
        };
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SaveKey, json);
    }

    public static void Load()
    {
        if (!PlayerPrefs.HasKey(SaveKey)) return;

        string json = PlayerPrefs.GetString(SaveKey);
        var data = JsonUtility.FromJson<GameSaveData>(json);

         GameManager.Instance.scoreText.text = $"Score: {data.score}";
         GameManager.Instance.tryText.text = $"Tries: {data.tries}";
         GameManager.Instance.score = data.score;
         GameManager.Instance.tries = data.tries;
    }
}