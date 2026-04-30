using UnityEngine;
using UnityEngine.SceneManagement;

public class BordersTrig : MonoBehaviour
{
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene("losescreen");
        }
    }
}
