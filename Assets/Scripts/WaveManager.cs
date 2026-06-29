using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [SerializeField] 
    private GameObject _bossPrefab;
    [SerializeField] 
    private SpawnManager _spawnManager;

    private int _currentWave = 0;
    private int _maxWaves = 5;

    public void StartNextWave()
    {
        _currentWave++;

        if (_currentWave >= _maxWaves)
        {
            StartCoroutine(SpawnBoss());
            return;
        }

        _spawnManager.StartSpawning();
    }

    IEnumerator SpawnBoss()
    {
        _spawnManager.StopSpawning();

        yield return new WaitForSeconds(2f);

        Instantiate(_bossPrefab, new Vector3(0, 6, 0), Quaternion.identity);
    }
}