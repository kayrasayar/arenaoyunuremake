using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    [Header("Talim Gereksinimi")]
    public bool needsTraining = false;

    [Header("Kazanılan İlçeler")]
    public System.Collections.Generic.List<string> completedDistricts = new System.Collections.Generic.List<string>();

    public string currentDistrict;

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
        if (scene.name == "worldscreen")
        {
            SanitizeCompletedDistricts();
            ValidateTrainingRequirement();
            SaveProgress();
        }

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
        if (!string.IsNullOrEmpty(currentDistrict)
            && currentDistrict != "Final"
            && !completedDistricts.Contains(currentDistrict))
        {
            completedDistricts.Add(currentDistrict);
        }
        currentXP += xpForWin;
        currentXP = Mathf.Clamp(currentXP, 0, maxXP);
        UpdateLevel();

        if (currentDistrict == "Final")
        {
            // Epic final win - level 99 yap
            currentLevel = 99;
            needsTraining = false;
            SaveProgress();
            UpdateUI();
            UpdateEnemyStats();
            SceneManager.LoadScene("winscreen");
            return;
        }

        needsTraining = false;
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

        needsTraining = true;
        PlayerPrefs.SetInt("HasLostBattle", 1);

        if (currentDistrict == "Final")
        {
            Debug.LogWarning("Final kaybı! Talim alanında kazanmadan devam edemezsin!");
        }

        SaveProgress();
        UpdateUI();
        UpdateEnemyStats();
        SceneManager.LoadScene("losescreen");
    }

    public void CompleteTraining()
    {
        needsTraining = false;
        PlayerPrefs.SetInt("NeedsTraining", 0);
        SaveProgress();
        UpdateUI();
        UpdateEnemyStats();
    }

    public bool AreAllDistrictsCompleted()
    {
        List<string> requiredDistricts = GetRequiredDistrictNames();
        if (requiredDistricts.Count == 0)
        {
            return false;
        }

        foreach (string district in requiredDistricts)
        {
            if (!completedDistricts.Contains(district))
            {
                return false;
            }
        }

        return true;
    }

    public List<string> GetRequiredDistrictNames()
    {
        if (MapCubeSpawner.Instance != null)
        {
            return MapCubeSpawner.Instance.GetPlayableDistrictNames();
        }

        return new List<string>();
    }

    public void SanitizeCompletedDistricts()
    {
        completedDistricts = completedDistricts
            .Where(d => !string.IsNullOrWhiteSpace(d))
            .Select(d => d.Trim())
            .Distinct()
            .ToList();

        completedDistricts.Remove("Final");
        completedDistricts.Remove("Selendi");

        List<string> validDistricts = GetRequiredDistrictNames();
        if (validDistricts.Count > 0)
        {
            completedDistricts = completedDistricts
                .Where(validDistricts.Contains)
                .ToList();
        }
    }

    public void ValidateTrainingRequirement()
    {
        bool hasLostBattle = PlayerPrefs.GetInt("HasLostBattle", 0) == 1;
        if (!hasLostBattle)
        {
            needsTraining = false;
            PlayerPrefs.SetInt("NeedsTraining", 0);
            return;
        }

        needsTraining = PlayerPrefs.GetInt("NeedsTraining", 0) == 1;
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
            if (currentLevel == 99)
            {
                levelText.text = "Seviye 99: ZAFER!";
            }
            else
            {
                levelText.text = "Seviye " + currentLevel + ": " + levelNames[currentLevel - 1];
            }
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

    public void SaveProgress()
    {
        PlayerPrefs.SetInt("CurrentXP", currentXP);
        PlayerPrefs.SetInt("CurrentLevel", currentLevel);
        PlayerPrefs.SetInt("NeedsTraining", needsTraining ? 1 : 0);

        // Kazanılan ilçeleri kaydet
        string districtsString = string.Join(",", completedDistricts);
        PlayerPrefs.SetString("CompletedDistricts", districtsString);

        PlayerPrefs.Save();
    }

    void LoadProgress()
    {
        currentXP = PlayerPrefs.GetInt("CurrentXP", 0);
        currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);

        completedDistricts.Clear();
        string districtsString = PlayerPrefs.GetString("CompletedDistricts", "");
        if (!string.IsNullOrEmpty(districtsString))
        {
            completedDistricts = districtsString
                .Split(',')
                .Select(d => d.Trim())
                .Where(d => !string.IsNullOrEmpty(d))
                .Distinct()
                .ToList();
        }

        ValidateTrainingRequirement();
    }

    public void ResetProgress()
    {
        currentXP = 0;
        currentLevel = 1;
        needsTraining = false;
        completedDistricts.Clear();
        currentDistrict = null;
        PlayerPrefs.SetInt("NeedsTraining", 0);
        PlayerPrefs.SetInt("HasLostBattle", 0);
        PlayerPrefs.DeleteKey("CompletedDistricts");
        SaveProgress();
        UpdateUI();
        UpdateEnemyStats();
    }
}