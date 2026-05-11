using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHurt : MonoBehaviour
{
    const int WEAPON_LAYER = 8;
    [SerializeField] int MaxHealth = 2;
    [SerializeField] Player player;
    [SerializeField] List<GameObject> ItemSpawns = new List<GameObject>();
    int health;
    bool killed = false;

    private void Awake()
    {
        health = MaxHealth;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == WEAPON_LAYER)
        {
            if (collision.gameObject.CompareTag("Weapon")) TakeDamage(player.GetWeaponStrength());
            if (collision.gameObject.CompareTag("Slide")) TakeDamage(player.GetSlideStrength());
        }
    }

    public void TakeDamage(int damage)
    {
        if (killed) return;
        AudioManager.instance.PlaySoundFXClip(AudioManager.instance.Enemyhurt, transform);
        health -= damage;
        Debug.Log("Enemy remaining health: " + health);
        if (health <= 0)
        {
            killed = true;
            SpawnItem();
            Events.EnemyDied();
            Destroy(this.transform.parent.parent.gameObject);
        }
    }

    private void SpawnItem()
    {
        int ItemNum = UnityEngine.Random.Range(0, ItemSpawns.Count);
        Instantiate(ItemSpawns[ItemNum], transform.position, Quaternion.identity);
    }

}
