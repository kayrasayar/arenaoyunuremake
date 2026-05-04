using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameProgressManager : MonoBehaviour
{
    public static GameProgressManager Instance;

    [Header("XP Ayarları")]
    public int currentXP = 0;
    public int xpForWin = 20;
    public int xpForLose = -10;
    public int maxXP = 1000; // 10 seviye için

    [Header("Seviye Ayarları")]
    public int currentLevel = 1;
    public int maxLevel = 10;
    public string[] levelNames = { "Çaylak", "Yeni", "Acemi", "Orta", "İyi", "Pro", "Uzman", "Kral", "Efsane", "Tanrı" };

    [Header("UI")]
    public Image xpBar;
    public TextMeshProUGUI xpText;
    public TextMeshProUGUI levelText;

    [Header("Düşman Güçlendirme")]
    public float enemyHealthMultiplier = 1.0f;
    public float enemyDamageMultiplier = 1.0f;
    public float enemySpeedMultiplier = 1.0f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindUIElements();
        UpdateUI();
    }

    void Start()
    {
        LoadProgress();
        FindUIElements();
        UpdateUI();
        UpdateEnemyStats();
    }

    void FindUIElements()
    {
        if (xpBar == null)
        {
            GameObject xpBarObj = GameObject.FindGameObjectWithTag("XPBar");
            if (xpBarObj != null)
            {
                xpBar = xpBarObj.GetComponent<Image>();
            }
        }

        if (xpText == null)
        {
            GameObject xpTextObj = GameObject.FindGameObjectWithTag("XPText");
            if (xpTextObj != null)
            {
                xpText = xpTextObj.GetComponent<TextMeshProUGUI>();
            }
        }

        if (levelText == null)
        {
            GameObject levelTextObj = GameObject.FindGameObjectWithTag("LevelText");
            if (levelTextObj != null)
            {
                levelText = levelTextObj.GetComponent<TextMeshProUGUI>();
            }
        }
    }

    public void WinGame()
    {
        currentXP += xpForWin;
        currentXP = Mathf.Clamp(currentXP, 0, maxXP);
        UpdateLevel();
        SaveProgress();
        UpdateUI();
        UpdateEnemyStats();
        SceneManager.LoadScene("winscreen");
    }

    public void LoseGame()
    {
        currentXP += xpForLose;
        currentXP = Mathf.Clamp(currentXP, 0, maxXP);
        UpdateLevel();
        SaveProgress();
        UpdateUI();
        UpdateEnemyStats();
        SceneManager.LoadScene("losescreen");
    }

    void UpdateLevel()
    {
        int newLevel = Mathf.Clamp((currentXP / 100) + 1, 1, maxLevel);
        if (newLevel != currentLevel)
        {
            currentLevel = newLevel;
            Debug.Log("Yeni seviye: " + levelNames[currentLevel - 1]);
        }
    }

    void UpdateUI()
    {
        if (xpBar != null)
        {
            xpBar.fillAmount = (float)currentXP / maxXP;
        }

        if (xpText != null)
        {
            xpText.text = currentXP + "/" + maxXP;
        }

        if (levelText != null)
        {
            levelText.text = "Seviye " + currentLevel + ": " + levelNames[currentLevel - 1];
        }
    }

    void UpdateEnemyStats()
    {
        // XP'ye göre düşman güçlendirme
        float levelMultiplier = (float)currentLevel / maxLevel;
        enemyHealthMultiplier = 1.0f + levelMultiplier * 2.0f; // 1x - 3x can
        enemyDamageMultiplier = 1.0f + levelMultiplier * 1.5f; // 1x - 2.5x hasar
        enemySpeedMultiplier = 1.0f + levelMultiplier * 0.5f; // 1x - 1.5x hız
    }

    void SaveProgress()
    {
        PlayerPrefs.SetInt("CurrentXP", currentXP);
        PlayerPrefs.SetInt("CurrentLevel", currentLevel);
        PlayerPrefs.Save();
    }

    void LoadProgress()
    {
        currentXP = PlayerPrefs.GetInt("CurrentXP", 0);
        currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
    }

    public void ResetProgress()
    {
        currentXP = 0;
        currentLevel = 1;
        SaveProgress();
        UpdateUI();
        UpdateEnemyStats();
    }
}