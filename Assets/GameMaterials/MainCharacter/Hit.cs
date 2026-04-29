using UnityEngine;

public class Hitbox : MonoBehaviour
{
    public int hasar = 10;
    public PlayerController playerController;
    private void OnTriggerEnter(Collider other)
    {

        Enemy enemy = other.GetComponent<Enemy>() ?? other.GetComponentInParent<Enemy>();

        if (enemy != null)
        {
            if (playerController.isAttack)
            {
                enemy.HasarAl(hasar);
                playerController.isAttack = false;
            }
                
            
        }
    }
}