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

    public static void Save(GameManager gm)
    {
        var data = new GameSaveData
        {
            score = gm.score,
            tries = gm.tries
        };
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SaveKey, json);
    }

    public static void Load(GameManager gm)
    {
        if (!PlayerPrefs.HasKey(SaveKey)) return;

        string json = PlayerPrefs.GetString(SaveKey);
        var data = JsonUtility.FromJson<GameSaveData>(json);

        gm.scoreText.text = $"Score: {data.score}";
        gm.tryText.text = $"Tries: {data.tries}";
        gm.score = data.score;
        gm.tries = data.tries;
    }
}