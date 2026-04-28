using UnityEngine;

public class Hitbox : MonoBehaviour
{
    public int hasar = 10;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Hitbox çarpıştı: " + other.name + " (tag=" + other.tag + ")");

        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy == null)
        {
            enemy = other.GetComponentInParent<Enemy>();
        }

        if (enemy != null)
        {
            Debug.Log("Enemy bulundu: " + enemy.name);
            enemy.HasarAl(hasar);
        }
        else
        {
            Debug.Log("Enemy component bulunamadı: " + other.name);
        }
    }
}