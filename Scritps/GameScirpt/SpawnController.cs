using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpawnController : MonoBehaviour {

    [Header("Spawn settings")]
    [SerializeField] private float spawnRadius;
    [SerializeField] private float startWaveSpawnTime;
    [SerializeField] private float startSpawnAmount;
    [SerializeField] private float spawnDeley;
    [SerializeField] private int waves;
    [SerializeField] private bool endless;

    [SerializeField] private GameObject[] enemyPrefab;
    [SerializeField] private Transform[] players;
    [SerializeField] private Transform hpBarHolder;

    private float waveSpawnTime;
    private float currentSpawnAmount;
    private float actualSpawndelay = 0;

    //void Start() {
    //    NewStart();
    //}

    public void NewStart() {
        StopAllCoroutines();
        waveSpawnTime = startWaveSpawnTime;
        currentSpawnAmount = startSpawnAmount;
        actualSpawndelay = 0;
        StartCoroutine(SpawnEnemy());
    }

    private IEnumerator SpawnEnemy() {

        if (endless) {

            while (true) {
                for (int i = 0; i < currentSpawnAmount; i++) {
                    float randomAngle = Random.Range(0, 360);
                    Vector2 spawnPos = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle)) * spawnRadius;

                    int enemyIndex = (int)Random.Range(0, enemyPrefab.Length);
                    GameObject instance = Instantiate(enemyPrefab[enemyIndex], (Vector2)transform.position + spawnPos, Quaternion.identity, transform);
                    instance.gameObject.GetComponent<EnemyController>().InizialiseEnemey(players, hpBarHolder);

                    yield return new WaitForSeconds(actualSpawndelay);
                    actualSpawndelay = spawnDeley;
                }
                yield return new WaitForSeconds(waveSpawnTime);
            }

        } else {
            for (int j = 0; j < waves; j++) {
                for (int i = 0; i < currentSpawnAmount; i++) {
                    float randomAngle = Random.Range(0, 360);
                    Vector2 spawnPos = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle)) * spawnRadius;

                    int enemyIndex = (int)Random.Range(0, enemyPrefab.Length);
                    GameObject instance = Instantiate(enemyPrefab[enemyIndex], (Vector2)transform.position + spawnPos, Quaternion.identity, transform);
                    instance.gameObject.GetComponent<EnemyController>().InizialiseEnemey(players, hpBarHolder);

                    yield return new WaitForSeconds(spawnDeley);
                }
                yield return new WaitForSeconds(waveSpawnTime);
                currentSpawnAmount++;
            }

            yield return new WaitForSeconds(5);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }

}
