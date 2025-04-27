using UnityEngine;
using System.Collections.Generic;

public enum SoundType
{
    Flip,
    Match,
    Mismatch,
    GameOver,
}

[System.Serializable]
public class SoundEntry
{
    public SoundType type;
    public AudioClip clip;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public List<SoundEntry> soundEntries = new();
    private Dictionary<SoundType, AudioClip> soundMap = new();
    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            audioSource = GetComponent<AudioSource>();

            foreach (var entry in soundEntries)
            {
                if (!soundMap.ContainsKey(entry.type))
                {
                    soundMap.Add(entry.type, entry.clip);
                }
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlaySound(SoundType type)
    {
        if (soundMap.TryGetValue(type, out AudioClip clip))
        {
            audioSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning($"Sound {type} not found in AudioManager!");
        }
    }
}
