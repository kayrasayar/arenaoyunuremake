using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class ResultScreenManager : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI xpResultText;

    void Start()
    {
        UpdateResultText();
    }

    void UpdateResultText()
    {
        if (GameProgressManager.Instance == null)
            return;

        if (titleText != null)
            titleText.text = "Savaş Sonucu";

        if (descriptionText != null)
            descriptionText.text = "Devam etmek için düğmeye basın.";

        if (xpResultText != null)
            xpResultText.text = "XP: " + GameProgressManager.Instance.currentXP + " (Seviye " + GameProgressManager.Instance.currentLevel + ")";
    }

    public void OnContinueButton()
    {
        SceneManager.LoadScene("worldscene");
    }
}
