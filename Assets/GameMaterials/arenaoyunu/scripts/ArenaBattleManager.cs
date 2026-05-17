using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Arena sahnesinde çoklu düşman (Kabus modu) zafer koşulunu yönetir.
/// </summary>
public class ArenaBattleManager : MonoBehaviour
{
    public static ArenaBattleManager Instance { get; private set; }

    readonly HashSet<Enemy> aktifDusmanlar = new HashSet<Enemy>();
    bool zaferVerildi;

    public static ArenaBattleManager EnsureForArena()
    {
        if (SceneManager.GetActiveScene().name != "arena")
        {
            return null;
        }

        if (Instance != null)
        {
            return Instance;
        }

        GameObject obj = new GameObject("ArenaBattleManager");
        return obj.AddComponent<ArenaBattleManager>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void KayitEt(Enemy dusman)
    {
        if (dusman == null || zaferVerildi)
        {
            return;
        }

        aktifDusmanlar.Add(dusman);
    }

    public bool TryHandleEnemyDeath(Enemy dusman)
    {
        if (dusman == null || SceneManager.GetActiveScene().name != "arena")
        {
            return false;
        }

        aktifDusmanlar.Remove(dusman);

        if (aktifDusmanlar.Count > 0 || zaferVerildi)
        {
            return true;
        }

        zaferVerildi = true;
        if (GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.WinGame();
        }
        else
        {
            SceneManager.LoadScene("winscreen");
        }

        return true;
    }

    public int AktifDusmanSayisi => aktifDusmanlar.Count;

    public void Sifirla()
    {
        aktifDusmanlar.Clear();
        zaferVerildi = false;
    }
}
