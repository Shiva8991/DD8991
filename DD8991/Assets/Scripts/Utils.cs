using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    public static void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int rand = Random.Range(0, i + 1);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
    }

    private static int idCounter = 1000; 

    public static int GenerateUniqueID()
    {
        return idCounter++; 
    }
}