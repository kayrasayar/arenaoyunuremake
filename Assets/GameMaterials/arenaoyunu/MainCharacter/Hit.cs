using UnityEngine;

public class Hitbox : MonoBehaviour
{
    public int hasar = 10;
    public PlayerController playerController;
    private bool hasHitThisAttack = false;

    private void OnTriggerEnter(Collider other)
    {
        TryHit(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryHit(other);
    }

    private void TryHit(Collider other)
    {
        if (playerController == null || !playerController.isAttack || hasHitThisAttack) return;

        Enemy enemy = other.GetComponent<Enemy>() ?? other.GetComponentInParent<Enemy>();
        if (enemy == null) return;

        enemy.HasarAl(hasar);
        hasHitThisAttack = true;
        playerController.isAttack = false;
    }

    public void ResetHit()
    {
        hasHitThisAttack = false;
    }
}