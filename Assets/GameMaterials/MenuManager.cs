using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void OyunaBasla()
    {
        SceneManager.LoadScene("worldscreen");
    }

    public void ResetXP()
    {
        if (GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.ResetProgress();
        }
        else
        {
            Debug.LogWarning("GameProgressManager bulunamadı, XP sıfırlanamadı.");
        }
    }
}