using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

/// <summary>
/// Sadece ilçe arena sahnesi (arena). Talim ve final sahnelere dokunmaz.
/// </summary>
public class ArenaDistrictVariator : MonoBehaviour
{
    public static ArenaDistrictVariator Instance { get; private set; }

    [Header("Spawn (arena içinde, sınırlar sabit)")]
    public float oyuncuSpawnYaricapMin = 0.5f;
    public float oyuncuSpawnYaricapMax = 4f;
    public float minimumKarsilasmaMesafesi = 7f;
    public float maksimumKarsilasmaMesafesi = 12f;

    Transform oyuncu;
    Transform dusman;
    Light anaIsik;

    Vector3 oyuncuBaslangic;
    Vector3 dusmanBaslangic;
    Vector3 arenaMerkezi;
    Color baslangicAmbient;
    Color baslangicFog;
    float baslangicFogDensity;
    bool fogBaslangictaAcik;
    bool kayitli;

    int sonTohum;

    public static void IlceArenasindaOlustur()
    {
        if (SceneManager.GetActiveScene().name != "arena")
        {
            return;
        }

        if (FindFirstObjectByType<ArenaDistrictVariator>() != null)
        {
            return;
        }

        GameObject obj = new GameObject("ArenaDistrictVariator");
        obj.AddComponent<ArenaDistrictVariator>();
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

        RenderSettings.ambientLight = baslangicAmbient;
        RenderSettings.fogColor = baslangicFog;
        RenderSettings.fogDensity = baslangicFogDensity;
        RenderSettings.fog = fogBaslangictaAcik;
    }

    void Start()
    {
        StartCoroutine(VaryasyonUygulaCoroutine());
    }

    IEnumerator VaryasyonUygulaCoroutine()
    {
        yield return null;

        if (!ReferanslariBul())
        {
            yield break;
        }

        BaslangicKaydet();
        sonTohum = IlceTohumuAl();
        Random.InitState(sonTohum);
        UygulaSpawnVaryasyonu();
        UygulaRenkAtmosferi();
        yield return null;
        AjanlariYerlestir();
    }

    int IlceTohumuAl()
    {
        string anahtar = SceneManager.GetActiveScene().name;
        if (GameProgressManager.Instance != null && !string.IsNullOrEmpty(GameProgressManager.Instance.currentDistrict))
        {
            anahtar = GameProgressManager.Instance.currentDistrict;
        }

        unchecked
        {
            int hash = 17;
            foreach (char c in anahtar)
            {
                hash = hash * 31 + c;
            }

            return hash;
        }
    }

    float T(float min, float max)
    {
        return Mathf.Lerp(min, max, Random.value);
    }

    bool ReferanslariBul()
    {
        GameObject oyuncuObj = GameObject.FindWithTag("Player");
        if (oyuncuObj != null)
        {
            oyuncu = oyuncuObj.transform;
        }

        GameObject dusmanObj = GameObject.Find("enemy");
        if (dusmanObj == null)
        {
            GameObject[] dusmanlar = GameObject.FindGameObjectsWithTag("Enemy");
            if (dusmanlar.Length > 0)
            {
                dusmanObj = dusmanlar[0];
            }
        }

        if (dusmanObj != null)
        {
            dusman = dusmanObj.transform;
        }

        anaIsik = FindFirstObjectByType<Light>();

        return oyuncu != null && dusman != null;
    }

    void BaslangicKaydet()
    {
        if (kayitli)
        {
            return;
        }

        oyuncuBaslangic = oyuncu.position;
        dusmanBaslangic = dusman.position;
        arenaMerkezi = (oyuncuBaslangic + dusmanBaslangic) * 0.5f;

        baslangicAmbient = RenderSettings.ambientLight;
        baslangicFog = RenderSettings.fogColor;
        baslangicFogDensity = RenderSettings.fogDensity;
        fogBaslangictaAcik = RenderSettings.fog;

        kayitli = true;
    }

    void UygulaSpawnVaryasyonu()
    {
        int tip = Mathf.Abs(sonTohum) % 4;

        float oyuncuMin = oyuncuSpawnYaricapMin;
        float oyuncuMax = oyuncuSpawnYaricapMax;
        float dusmanMin = minimumKarsilasmaMesafesi;
        float dusmanMax = maksimumKarsilasmaMesafesi;

        switch (tip)
        {
            case 0:
                oyuncuMax = 3f;
                dusmanMin = 6f;
                dusmanMax = 9f;
                break;
            case 1:
                oyuncuMin = 2f;
                oyuncuMax = 4.5f;
                dusmanMin = 9f;
                dusmanMax = 12f;
                break;
            case 2:
                oyuncuMin = 1f;
                oyuncuMax = 3.5f;
                dusmanMin = 7f;
                dusmanMax = 11f;
                break;
            default:
                oyuncuMin = 1.5f;
                oyuncuMax = 4f;
                dusmanMin = 8f;
                dusmanMax = 13f;
                break;
        }

        float oyuncuAcisi = T(0f, 360f) * Mathf.Deg2Rad;
        float oyuncuMesafe = T(oyuncuMin, oyuncuMax);
        float dusmanMesafe = T(dusmanMin, dusmanMax);
        float aciFarki = T(140f, 220f) * Mathf.Deg2Rad;

        Vector3 oyuncuKonum = arenaMerkezi + new Vector3(Mathf.Cos(oyuncuAcisi), 0f, Mathf.Sin(oyuncuAcisi)) * oyuncuMesafe;
        Vector3 dusmanKonum = arenaMerkezi + new Vector3(
            Mathf.Cos(oyuncuAcisi + aciFarki),
            0f,
            Mathf.Sin(oyuncuAcisi + aciFarki)) * dusmanMesafe;

        oyuncuKonum.y = oyuncuBaslangic.y;
        dusmanKonum.y = dusmanBaslangic.y;

        KarakteriTasi(oyuncu, oyuncuKonum, dusmanKonum);
        KarakteriTasi(dusman, dusmanKonum, oyuncuKonum);
    }

    void UygulaRenkAtmosferi()
    {
        int tip = Mathf.Abs(sonTohum) % 6;
        float fr = (Mathf.Abs(sonTohum) % 997) / 997f;
        float hue = (Mathf.Abs(sonTohum) % 360) / 360f;

        Color isikRenk = Color.HSVToRGB(hue, T(0.1f, 0.28f), T(0.88f, 1.02f));

        if (anaIsik != null)
        {
            anaIsik.color = isikRenk;
            anaIsik.intensity = T(0.9f, 1.2f);
        }

        RenderSettings.ambientLight = Color.Lerp(baslangicAmbient, isikRenk * 0.5f, 0.55f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = Color.Lerp(baslangicFog, isikRenk * 0.4f, 0.5f);
        RenderSettings.fogDensity = Mathf.Lerp(0.0015f, 0.007f, fr);

        if (tip == 0)
        {
            RenderSettings.fogDensity *= 1.2f;
        }
        else if (tip == 1)
        {
            RenderSettings.fogDensity *= 0.75f;
        }
    }

    void KarakteriTasi(Transform karakter, Vector3 konum, Vector3 bakisHedefi)
    {
        if (karakter == null)
        {
            return;
        }

        CharacterController cc = karakter.GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
        }

        karakter.position = konum;

        Vector3 bakis = bakisHedefi - konum;
        bakis.y = 0f;
        if (bakis.sqrMagnitude > 0.01f)
        {
            karakter.rotation = Quaternion.LookRotation(bakis.normalized);
        }

        if (cc != null)
        {
            cc.enabled = true;
        }
    }

    void AjanlariYerlestir()
    {
        NavMeshAgent dusmanAgent = dusman != null ? dusman.GetComponent<NavMeshAgent>() : null;

        if (dusmanAgent != null && dusmanAgent.enabled)
        {
            if (NavMesh.SamplePosition(dusman.position, out NavMeshHit hit, 3f, NavMesh.AllAreas))
            {
                dusmanAgent.Warp(hit.position);
            }
        }
    }
}
