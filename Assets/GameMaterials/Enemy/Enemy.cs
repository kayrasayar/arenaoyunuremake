using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI; // UI işlemleri için gerekli
using System.Collections;
using UnityEngine.SceneManagement;

public class Enemy : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator animator;

    public Transform hedef;
    
    [Header("Can Ayarları")]
    public int maxCan = 100;
    private int mevcutCan;
    public Image canBarıGorseli; // Inspector'da "currentHealth" objesini buraya sürükle

    private GameObject efektObjesi;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        
        // Oyun başında canı fulle
        mevcutCan = maxCan;
        UpdateCanBarı();

        // "efekt" taglı objeyi bulma
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
        if (hedef != null)
        {
            agent.SetDestination(hedef.position);
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

    public void HasarAl(int miktar)
    {
        mevcutCan -= miktar;
        Debug.Log("Enemy hasar aldı: " + miktar);
        
        // Can barını görsel olarak güncelle
        UpdateCanBarı();

        if (efektObjesi != null)
        {
            // Önceki çalışan coroutine varsa durdur (üst üste binmesin)
            StopAllCoroutines(); 
            StartCoroutine(EfektiGoster());
        }
        
        if (mevcutCan <= 0)
        {
            Ol();
        }
    }

    // Can barının fillAmount değerini değiştiren yardımcı fonksiyon
    void UpdateCanBarı()
    {
        if (canBarıGorseli != null)
        {
            // float bölmesi yaparak 0 ile 1 arasında değer veriyoruz
            canBarıGorseli.fillAmount = (float)mevcutCan / maxCan;
        }
    }

    IEnumerator EfektiGoster()
    {
        efektObjesi.SetActive(true);
        yield return new WaitForSeconds(0.8f);
        if (efektObjesi != null) efektObjesi.SetActive(false);
    }

    void Ol()
    {
        Debug.Log("Enemy öldü");
        Destroy(gameObject, 0.2f);
        SceneManager.LoadScene("winscreen");
    }
}