using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinLoseAudioPlayer : MonoBehaviour
{
    public AudioClip winClip;
    public AudioClip loseClip;
    public AudioClip backgroundClip;
    public AudioClip finalEntryClip;
    private AudioSource audioSource;
    private AudioSource backgroundSource;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Initialize()
    {
        GameObject audioPlayer = new GameObject("WinLoseAudioPlayer");
        audioPlayer.hideFlags = HideFlags.HideAndDontSave;
        DontDestroyOnLoad(audioPlayer);
        audioPlayer.AddComponent<WinLoseAudioPlayer>();
    }

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;

        backgroundSource = gameObject.AddComponent<AudioSource>();
        backgroundSource.playOnAwake = false;
        backgroundSource.loop = true;
        backgroundSource.volume = 0.5f; // Lower volume for background

        AudioClip loadedWin = Resources.Load<AudioClip>("win");
        AudioClip loadedLose = Resources.Load<AudioClip>("lose");
        AudioClip loadedBackground = Resources.Load<AudioClip>("hep");
        AudioClip loadedFinal = Resources.Load<AudioClip>("finalgiris");

        if (winClip == null && loadedWin != null)
        {
            winClip = loadedWin;
        }

        if (loseClip == null && loadedLose != null)
        {
            loseClip = loadedLose;
        }

        if (backgroundClip == null && loadedBackground != null)
        {
            backgroundClip = loadedBackground;
        }

        if (finalEntryClip == null && loadedFinal != null)
        {
            finalEntryClip = loadedFinal;
        }

        if (winClip == null)
        {
            Debug.LogWarning("WinLoseAudioPlayer: winClip not assigned and Resources/win.mp3 not found.");
        }

        if (loseClip == null)
        {
            Debug.LogWarning("WinLoseAudioPlayer: loseClip not assigned and Resources/lose.mp3 not found.");
        }

        if (backgroundClip == null)
        {
            Debug.LogWarning("WinLoseAudioPlayer: backgroundClip not assigned and Resources/hep.mp3 not found.");
        }

        if (finalEntryClip == null)
        {
            Debug.LogWarning("WinLoseAudioPlayer: finalEntryClip not assigned and Resources/finalgiris.mp3 not found.");
        }

        // Start background music
        if (backgroundClip != null)
        {
            backgroundSource.clip = backgroundClip;
            backgroundSource.Play();
        }

        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name.Equals("winscreen", StringComparison.OrdinalIgnoreCase) || scene.name.Equals("WinScreen", StringComparison.OrdinalIgnoreCase))
        {
            PlayClip(winClip);
        }
        else if (scene.name.Equals("losescreen", StringComparison.OrdinalIgnoreCase) || scene.name.Equals("LoseScreen", StringComparison.OrdinalIgnoreCase))
        {
            PlayClip(loseClip);
        }
    }

    void PlayClip(AudioClip clip)
    {
        if (clip == null) return;

        AudioSource source = GetComponent<AudioSource>();
        if (source == null)
        {
            source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
        }

        source.PlayOneShot(clip);
    }
}
