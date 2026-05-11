using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int baseMaxHealth = 3;
    public int maxHealth = 3;           // default max health
    public int health;                  // updated health
    private Player player;              // player reference
    public GameObject gameOverMenuUI;   // game over menu
    private int enemies_killed = 0;
    [SerializeField] private float InvincibilityTime = 1.5f;
    private float iframeTimer = 0;

    // for HP
    public GameObject[] healthPoints;
    public GameObject[] healthPointOutlines;
    [SerializeField] private int min_enemies_drain = 5;
    [SerializeField] private int drain_value = 1;

    // for armor
    [SerializeField] private int leather_armor_health = 1;
    [SerializeField] private int iron_armor_health = 2;

    void Awake()
    {
        health = maxHealth;
        player = GetComponent<Player>();
        UpdateHealthPoints();
    }

    private void OnEnable()
    {
        Events.Enemydied += TrackEnemiesKilled;
        Events.Gameover += GameOver;
    }

    private void OnDisable()
    {
        Events.Enemydied -= TrackEnemiesKilled;
        Events.Gameover -= GameOver;

    }

    private void FixedUpdate()
    {
        HandleTimers();
    }

    private void HandleTimers()
    {
        if (iframeTimer > 0) iframeTimer -= Time.deltaTime;
    }

    public bool IsInvincible()
    {
        return (iframeTimer > 0) || player.IsSliding() || player.IsDying();
    }

    public void TakeDamage(int damage)
    {
        if (IsInvincible()) return;
        iframeTimer = InvincibilityTime;
        health -= damage;
        UpdateHealthPoints();
        if (health <= 0)
        {
            AudioManager.instance.PlaySoundFXClip(AudioManager.instance.Death, transform);
            AudioManager.instance.StopBackground();
            player.Death();
        }
        else AudioManager.instance.PlaySoundFXClip(AudioManager.instance.PlayerHurt, transform);
    }

    private void GameOver()
    {
        gameOverMenuUI.SetActive(true);
        Debug.Log("Game over!");
        Time.timeScale = 0f;
    }

    private void TrackEnemiesKilled()
    {
        if (!player.HasHPDrain())
        {
            enemies_killed = 0;
        }
        else
        {
            enemies_killed++;
            if (enemies_killed >= min_enemies_drain)
            {
                enemies_killed -= min_enemies_drain;
                health += drain_value;
                health = Mathf.Min(health, maxHealth);
                print("Health drained: " + drain_value);
            }
        }

    }

    public void HandleArmorHealth(string armorName, bool has_hp_up)
    {
        if (armorName == "Leather Armor") maxHealth = baseMaxHealth + leather_armor_health;
        else if (armorName == "Iron Armor") maxHealth = baseMaxHealth + iron_armor_health;
        else maxHealth = baseMaxHealth;
        if (has_hp_up) maxHealth++;
        health = Mathf.Min(health, maxHealth);
        UpdateHealthPoints();
    }

    public void RestoreHealth()
    {
        health = maxHealth;
        UpdateHealthPoints();
    }

    public void UpdateHealthPoints()
    {
        foreach (var hpOut in healthPointOutlines)
        {
            hpOut.SetActive(false);
        }

        foreach (var hp in healthPoints)
        {
            hp.SetActive(false);
        }

        for (int i = 0; i < maxHealth; i++)
        {
            healthPointOutlines[i].SetActive(true);
        }

        for (int j = 0; j < health; j++)
        {
            healthPoints[j].SetActive(true);
        }
    }
}
