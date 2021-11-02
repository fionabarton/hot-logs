using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [Header("Set Dynamically")]
    public List<bool>       enemyDead;
    public List<GameObject> enemyGO;
    public List<Vector3>    enemyPositions;
    public List<int>        enemyLevels;

    public List<int>        enemiesBattled; // Added onto when Enemy collides with Player
    public int              currentEnemyNdx; // Enemy currently engaged in battle 
    public eMovement        cachedMovement;

    // Singleton
    private static EnemyManager _S;
    public static EnemyManager  S { get { return _S; } set { _S = value; } }

    void Awake() {
        S = this;
    }

    // Get list of all enemy gameObjects in current scene
    public void GetEnemyGameObjects() {
        // Clear/reset list
        enemyGO.Clear();

        GameObject enemiesGO = GameObject.Find("Enemies");
        if (enemiesGO != null) {
            foreach (Transform child in enemiesGO.transform) {
                enemyGO.Add(child.gameObject);
            }
        }
    }

    // Get list of the positions of all enemies in current scene
    public void GetEnemyPositions() {
        // Clear/reset list
        enemyPositions.Clear();

        GameObject enemiesGO = GameObject.Find("Enemies");
        if (enemiesGO != null) {
            foreach (Transform child in enemiesGO.transform) {
                enemyPositions.Add(child.position);
            }
        } 
    }

    // After battle, set enemy positions back to where they were right before the battle started
    public void SetEnemyPositions() {
        for (int i = 0; i < enemyGO.Count; i++) {
            if (i < enemyPositions.Count) {
                enemyGO[i].transform.position = enemyPositions[i];
            }
        }
    }

    // Get list of the levels of all enemies in current scene
    public void GetEnemyLevels() {
        // Clear/reset list
        enemyLevels.Clear();

        GameObject enemiesGO = GameObject.Find("Enemies");
        if (enemiesGO != null) {
            foreach (Transform child in enemiesGO.transform) {
                Enemy enemy = child.GetComponent<Enemy>();
                if(enemy != null) {
                    enemyLevels.Add(enemy.level);
                }
            }
        }
    }

    // After battle, set enemy levels back to where they were right before the battle started
    public void SetEnemyLevels() {
        for (int i = 0; i < enemyGO.Count; i++) {
            if (i < enemyLevels.Count) {
                Enemy enemy = enemyGO[i].GetComponent<Enemy>();
                if (enemy != null) {
                    enemy.level = enemyLevels[i];
                }
            }
        }
    }

    // Get list of all enemies and whether or not they're dead
    public void GetEnemyDeathStatus() {
        // Clear/reset list
        enemyDead.Clear();

        GameObject enemiesGO = GameObject.Find("Enemies");
        if (enemiesGO != null) {
            enemyDead = new List<bool>(new bool[enemiesGO.transform.childCount]);
        }
    }

    // Based off the elements within enemiesBattled, determine which enemies are dead
    public void SetEnemyDeathStatus() {
        for (int i = 0; i < enemiesBattled.Count; i++) {
            for (int j = 0; j < enemyDead.Count; j++) {
                if (enemiesBattled[i] == j) {
                    enemyDead[j] = true;
                    break;
                }
            }
        }
    }

    // Deactivate enemies that were defeated in Battle 
    public void DeactivateDeadEnemies() {
        for (int i = 0; i < enemyDead.Count - 1; i++) {
            if (enemyDead[i]) {
                if (i < enemyGO.Count) {
                    enemyGO[i].SetActive(false);
                }
            }
        }
    }

    // Caches what the enemy's movement will be set to in overworld if the party has died,
    // or the party or enemies have run
    public void GetEnemyMovement(eMovement movement) {
        cachedMovement = movement;

        // Remove Enemy from List, which prevents it from being deactivated in overworld
        if(enemiesBattled.Count > 0) {
            enemiesBattled.RemoveAt(enemiesBattled.Count - 1);
        }
    }

    // In the overworld, sets the enemy's movement to what was cached in battle 
    // if the party has died, or the party or enemies have run
    public void SetEnemyMovement() {
        if(currentEnemyNdx < enemyGO.Count) {
            EnemyMovement eMovement = enemyGO[currentEnemyNdx].GetComponentInChildren<EnemyMovement>();
            if (eMovement != null) {
                eMovement.onDetectPlayerMode = cachedMovement;
            }
        }
    }
}