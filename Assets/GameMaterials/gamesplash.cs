using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class gamesplash : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public string nextScene = "home";
    private bool canSkip = false;

    void Start()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Play();
            canSkip = true;
            Invoke("LoadNextScene", 8f);
        }
    }

    void Update()
    {
        if (canSkip && Input.GetMouseButtonDown(0))
        {
            LoadNextScene();
        }
    }

    void LoadNextScene()
    {
        SceneManager.LoadScene(nextScene);
    }
}
