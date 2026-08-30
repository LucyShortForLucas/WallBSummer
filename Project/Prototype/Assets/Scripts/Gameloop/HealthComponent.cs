using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class HealthComponent : MonoBehaviour, IDamageable
{
    // ----- [LUCY/PROTOTYPE] Strategy pattern for death -----
    public enum DeathStrategy // This is a quick and dirty hardoded implementation, not meant to be permanent
    {
        None,
        DropScrap,
        Respawn 
    }
    private Dictionary<DeathStrategy, Action> strategies = 
        new(new KeyValuePair<DeathStrategy, Action>[]
        {
            new(DeathStrategy.DropScrap, static () =>
            {
                var sc = GameObject.Find("Player").GetComponent<StorageComponent>(); // Never do this normally, kill after prototype 
                var database = GameObject.Find("[BOOTSTRAPPER]").GetComponent<GameBootstrapper>().ResourceHub;
                database.AddResource(sc.StorageID, 1, 1, true);
                if (UnityEngine.Random.Range(0,2) == 1) // 50% 
                    database.AddResource(sc.StorageID, 2, 1, true);
                if (UnityEngine.Random.Range(0,5) == 1) // 25%
                    database.AddResource(sc.StorageID, 3, 1, true);

            }), 
            new(DeathStrategy.None, static () => { }),
            new(DeathStrategy.Respawn, static () => { }),
        });

    [SerializeField] private DeathStrategy deathStrat= DeathStrategy.None;
    [SerializeField] private int maxHealth = 100; 
    [SerializeField] private int currentHealth;   

    public bool IsDead => currentHealth <= 0; // VIOLATES Cs.S.1 - Avoid declaring public fields in a class
                                              // + VIOLATES Cs.S.2 - Avoid auto-implemented properties.

    // Getters and Setters
    public int MaxHealth
    {
        get => maxHealth;
        set => maxHealth = value;
    }
    public int CurrentHealth
    {
        get => currentHealth;
        set => currentHealth = value;
    }

    public void Initialize(int startHealth)
    {
        maxHealth = startHealth;
        currentHealth = startHealth;
    }

    public void TakeDamage(int amount)
    {
        if (IsDead) return; // Lucy: Skipping if healt < 0, but below we destroy the gameobject the moment health falls below 0, so this can never cause an early return

        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            currentHealth = 0;

            strategies[deathStrat]();

            if (deathStrat != DeathStrategy.Respawn)
                Destroy(gameObject);
            else
            {
                currentHealth = maxHealth;
                transform.position = new Vector3(-10, 1, -5);
            }
        }
    }
}