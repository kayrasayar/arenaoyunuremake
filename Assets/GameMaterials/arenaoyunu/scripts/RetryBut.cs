using UnityEngine;
using UnityEngine.SceneManagement;

public class RetryBut : MonoBehaviour
{
    void Start()
    {
        // İmleci görünür yapar
        Cursor.visible = true;

        // İmlecin kilitli kalmasını engeller, serbest bırakır
        Cursor.lockState = CursorLockMode.None;
    }
    public void clicked()
    {
        
        SceneManager.LoadScene("worldscreen");
    }

}
