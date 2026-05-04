using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class Enemy : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator animator;
    private PlayerController hedefPlayer;
    public bool Die = false;
    private bool isDead = false;
    public float olmeBekleme = 5f;

    public Transform hedef;

    [Header("Can Ayarları")]
    public int maxCan = 100;
    private int mevcutCan;
    public Image canBarıGorseli;

    private GameObject efektObjesi;

    [Header("Saldırı Ayarları")]
    public float saldiriMesafesi = 2f;
    public int hasar = 11;
    public float saldiriCooldown = 1.5f;

    private float sonrakiSaldiriZamani = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (hedef == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                hedef = playerObj.transform;
            }
        }

        if (hedef != null)
        {
            hedefPlayer = hedef.GetComponentInParent<PlayerController>();
            if (hedefPlayer == null)
            {
                hedefPlayer = hedef.GetComponent<PlayerController>();
            }

            if (hedefPlayer == null)
            {
                Debug.LogWarning("Enemy hedefinde PlayerController bulunamadı: " + hedef.name);
            }
        }

        mevcutCan = maxCan;
        UpdateCanBarı();

        // İlerleme sistemine göre düşman güçlendirme
        if (GameProgressManager.Instance != null)
        {
            maxCan = Mathf.RoundToInt(maxCan * GameProgressManager.Instance.enemyHealthMultiplier);
            hasar = Mathf.RoundToInt(hasar * GameProgressManager.Instance.enemyDamageMultiplier);
            agent.speed *= GameProgressManager.Instance.enemySpeedMultiplier;
        }

        mevcutCan = maxCan;

        // efekt objesi bul
        foreach (Transform child in transform)
        {
            if (child.CompareTag("efekt"))
            {
                efektObjesi = child.gameObject;
                efektObjesi.SetActive(false);
                break;
            }
        }
    }

    void Update()
    {
        if (hedef == null || isDead) return;

        float mesafe = Vector3.Distance(transform.position, hedef.position);

        if (mesafe <= saldiriMesafesi)
        {
            Saldir();
        }
        else
        {
            if (agent != null)
            {
                agent.isStopped = false;
                agent.SetDestination(hedef.position);
            }
        }

        AnimasyonKontrol();
    }

    void AnimasyonKontrol()
    {
        if (animator == null) return;

        float anlikHiz = agent.velocity.magnitude;
        bool yürüyorMu = anlikHiz > 0.1f;
        bool kosuyorMu = anlikHiz > (agent.speed * 0.7f);

        float animationSpeed = 0f;
        if (yürüyorMu) animationSpeed = kosuyorMu ? 1.5f : 1f;

        animator.SetFloat("Speed", animationSpeed, 0.1f, Time.deltaTime);
        animator.SetBool("Walk", yürüyorMu);
        animator.SetBool("Run", kosuyorMu);
    }

    void Saldir()
    {
        if (agent != null)
        {
            agent.isStopped = true;
            agent.SetDestination(transform.position);
        }

        // player'a dön
        Vector3 yon = (hedef.position - transform.position).normalized;
        yon.y = 0;
        transform.forward = yon;

        if (Time.time >= sonrakiSaldiriZamani)
        {
            sonrakiSaldiriZamani = Time.time + saldiriCooldown;

            Debug.Log("Enemy vurdu!");

            // animasyon
            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }

            if (hedefPlayer == null && hedef != null)
            {
                hedefPlayer = hedef.GetComponentInParent<PlayerController>();
                if (hedefPlayer == null)
                {
                    hedefPlayer = hedef.GetComponent<PlayerController>();
                }
            }

            if (hedefPlayer != null)
            {
                hedefPlayer.HasarAl(hasar);
            }
            else
            {
                Debug.LogWarning("Enemy saldırdı ama player component bulunamadı: " + (hedef != null ? hedef.name : "null"));
            }
        }
    }

    public void HasarAl(int miktar)
    {
        if (isDead) return;

        mevcutCan -= miktar;
        Debug.Log("Enemy hasar aldı: " + miktar);

        UpdateCanBarı();

        if (efektObjesi != null)
        {
            StopAllCoroutines();
            StartCoroutine(EfektiGoster());
        }

        if (mevcutCan <= 0)
        {
            isDead = true;
            Die = true;
            if (animator != null)
            {
                animator.SetBool("Die", true);
            }
            StartCoroutine(OlCoroutine());
        }
    }

    void UpdateCanBarı()
    {
        if (canBarıGorseli != null)
        {
            canBarıGorseli.fillAmount = (float)mevcutCan / maxCan;
        }
    }

    IEnumerator EfektiGoster()
    {
        efektObjesi.SetActive(true);
        yield return new WaitForSeconds(0.8f);
        if (efektObjesi != null) efektObjesi.SetActive(false);
    }

    IEnumerator OlCoroutine()
    {
        Debug.Log("Enemy öldü");
        if (agent != null)
        {
            agent.isStopped = true;
        }
        yield return new WaitForSeconds(olmeBekleme);

        // Kazanma: XP kazanma
        if (GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.WinGame();
        }
        else
        {
            SceneManager.LoadScene("winscreen");
        }
    }
}