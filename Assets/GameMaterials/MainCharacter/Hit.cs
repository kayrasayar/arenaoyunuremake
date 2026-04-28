using UnityEngine;

public class Hitbox : MonoBehaviour
{
    public int hasar = 10;

    private void OnTriggerEnter(Collider other)
    {
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy == null)
        {
            enemy = other.GetComponentInParent<Enemy>();
        }

        if (enemy != null)
        {
            enemy.HasarAl(hasar);
        }
    }
}