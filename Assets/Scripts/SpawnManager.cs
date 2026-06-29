using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField]
    private GameObject _enemyPrefab;
    [SerializeField]
    private GameObject _enemyContainer;
    [SerializeField]
    private GameObject[] _powerups;
    [SerializeField]
    private UIManager _uiManager;
    [SerializeField] 
    private GameObject _healthPickup;
    [SerializeField] 
    private GameObject _ammoPickup;
    [SerializeField]
    private GameObject _shieldPickup;
    [SerializeField]
    private GameObject _homingPowerUpPrefab;

    private Player _player;

    private float _enemySpeed = 3f;
    private float _spawnDelay = 0.5f;
    private int _currentWave = 1;
    private int _enemiesPerWave = 5;
    private bool _stopSpawning = false;

    void Start()
    {
        _player = GameObject.Find("Player").GetComponent<Player>();
    }

    private void TrySpawnDrop(Vector3 position)
    {
        int chance = Random.Range(0, 100);

        if (chance < 20)
        {
            int randomPickup = Random.Range(0, _powerups.Length);
            Instantiate(_powerups[randomPickup], position, Quaternion.identity);
        }
    }

    public void StartSpawning()
    {
        StartCoroutine(SpawnEnemyRoutine());
        StartCoroutine(SpawnPowerupRoutine());
        StartCoroutine(WaveRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (_stopSpawning == false)
        {
            // Spawn enemies

            yield return new WaitForSeconds(5f);
        }
    }

    public void StopSpawning()
    {
        _stopSpawning = true;
    }

    IEnumerator SpawnEnemyRoutine()
    {
        yield return new WaitForSeconds(3.0f);

        while (_stopSpawning == false)
        {
            Vector3 posToSpawn = new Vector3(Random.Range(-8f, 8f), 7, 0);
            GameObject newEnemy = Instantiate(_enemyPrefab, posToSpawn, Quaternion.identity);
            newEnemy.transform.parent = _enemyContainer.transform;
            yield return new WaitForSeconds(5.0f);
        }
    }

    IEnumerator SpawnPowerupRoutine()
    {
        yield return new WaitForSeconds(3.0f);

        while (_stopSpawning == false)
        {
            Vector3 posToSpawn = new Vector3(Random.Range(-8f, 8f), 7, 0);
            int randomPowerUp = Random.Range(0, 3);
            Instantiate(_powerups[randomPowerUp], posToSpawn, Quaternion.identity);
            yield return new WaitForSeconds(Random.Range(3, 8));
        }
    }

    IEnumerator SpawnWave()
    {
        for (int i = 0; i < _enemiesPerWave; i++)
        {
            Vector3 posToSpawn =
                new Vector3(Random.Range(-8f, 8f), 7f, 0);

            GameObject spawnedEnemy =
                Instantiate(_enemyPrefab, posToSpawn, Quaternion.identity);

            Enemy enemyScript = spawnedEnemy.GetComponent<Enemy>();

            if (enemyScript != null)
            {
                enemyScript.SetSpeed(_enemySpeed);
            }

            spawnedEnemy.transform.parent = _enemyContainer.transform;

            yield return new WaitForSeconds(_spawnDelay);
        }

        yield return new WaitUntil(() =>
            _enemyContainer.transform.childCount == 0);
    }

    IEnumerator WaveRoutine()
    {
        while (!_stopSpawning)
        {
            if(_uiManager != null)
            {
                _uiManager.UpdateWave(_currentWave);
            }
            
            Debug.Log("Starting Wave " + _currentWave);

            yield return StartCoroutine(SpawnWave());

            Debug.Log("Wave " + _currentWave + " Complete!");

            _currentWave++;
            _enemiesPerWave += 3;

            _enemySpeed += 0.25f;
            _spawnDelay = Mathf.Max(0.1f, _spawnDelay - 0.05f);

            if (_currentWave % 2 == 0)
            {
                SpawnPickup();
            }

            yield return new WaitForSeconds(3f);

            _enemiesPerWave += 2;
            _enemySpeed += 0.25f;

            float pickupChance = Mathf.Clamp(40f - (_currentWave * 2), 10f, 40f);
        }
    }
    
    private void SpawnFastEnemy()
    {
        Vector3 posToSpawn = new Vector3(Random.Range(-8f, 8f), 7f, 0);
        GameObject enemy = Instantiate(_enemyPrefab, posToSpawn, Quaternion.identity);

        Enemy e = enemy.GetComponent<Enemy>();
        if (e != null)
        {
            e.SetSpeed(6f);
        }
    }

    private void SpawnZigZagEnemy()
    {
        Vector3 posToSpawn = new Vector3(Random.Range(-8f, 8f), 7f, 0);
        Instantiate(_enemyPrefab, posToSpawn, Quaternion.identity);
    }
    public void OnPlayerDeath()
    {
        _stopSpawning = true;
    }

    private void OnEnable()
    {
        Enemy.OnEnemyDestroyed += TrySpawnDrop;
    }

    private void OnDisable()
    {
        Enemy.OnEnemyDestroyed -= TrySpawnDrop;
    }

    private void SpawnPickup()
    {
        if (_player == null)
            return;

        Vector3 spawnPos =
            new Vector3(Random.Range(-8f, 8f), 7f, 0);

        // Low health → higher chance for health
        if (_player.CurrentLives <= 1)
        {
            Instantiate(_healthPickup, spawnPos, Quaternion.identity);
            return;
        }

        // Low ammo → higher chance for ammo
        if (_player.CurrentAmmo < (_player.MaxAmmo * 0.25f))
        {
            Instantiate(_ammoPickup, spawnPos, Quaternion.identity);
            return;
        }

        // Otherwise random
        int random = Random.Range(0, 100);

        if (random < 50)
            Instantiate(_ammoPickup, spawnPos, Quaternion.identity);

        else if (random < 80)
            Instantiate(_shieldPickup, spawnPos, Quaternion.identity);

        else
            Instantiate(_healthPickup, spawnPos, Quaternion.identity);
    }
}