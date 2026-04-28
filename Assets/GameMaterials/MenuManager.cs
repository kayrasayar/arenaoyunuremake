using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void OyunaBasla()
    {
        SceneManager.LoadScene("arena");
    }
}