using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    public int damage = 1;
    public PlayerHealth playerHealth;
    public Player playerMovement;
    public EnemyHurt enemyHurt;
    [SerializeField] int counter_damage = 1;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {

            if (collision.transform.position.x <= transform.position.x)
            {
                playerMovement.KnockFromRight = true;
            }
            else if (collision.transform.position.x > transform.position.x)
            {
                playerMovement.KnockFromRight = false;
            }

            if (!playerHealth.IsInvincible())
            {
                playerHealth.TakeDamage(damage);
                playerMovement.Knockback();
                if (playerMovement.has_counter) enemyHurt.TakeDamage(counter_damage);
            }

        }
    }
}
