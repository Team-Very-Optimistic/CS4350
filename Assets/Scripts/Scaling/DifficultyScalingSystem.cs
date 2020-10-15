﻿using System;
using System.Collections;
using UnityEngine;


public class DifficultyScalingSystem : Singleton<DifficultyScalingSystem>
{
    /// <summary>
    /// Difficulty level 1 is easier, the higher it is the more difficult it gets
    /// </summary>
    public int difficultyLevel = 1;

    public float difficultyIncreaseInterval = 60f;
    public void Awake()
    {
        EnemyBehaviourBase.OnEnemyStart += ManageDifficulty;
        StartCoroutine(IncreaseDifficulty(1, difficultyIncreaseInterval));
    }

    public void OnDestroy()
    {
        StopAllCoroutines();
    }

    private void ManageDifficulty(EnemyBehaviourBase enemyBehaviourBase)
    {
        IncreaseEnemyHealth(enemyBehaviourBase);
        IncreaseEnemyDamage(enemyBehaviourBase);
    }

    private void IncreaseEnemyDamage(EnemyBehaviourBase enemyBehaviourBase)
    {
        var dps = enemyBehaviourBase.GetComponent<Damage>();
        dps.SetDamage(dps.GetDamage() * difficultyLevel / 2);
    }

    public void IncreaseEnemyHealth(EnemyBehaviourBase enemyBehaviourBase)
    {
        var hp = enemyBehaviourBase.GetComponent<HealthScript>();
        hp.SetHealth(hp.maxHealth * difficultyLevel/ 2);
    }

    public IEnumerator IncreaseDifficulty(int amount, float time)
    {
        difficultyLevel += amount;
        yield return new WaitForSeconds(time);
        StartCoroutine(IncreaseDifficulty(amount, time));
    }
}
