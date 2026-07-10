using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asteroid : MonoBehaviour
{
    [SerializeField]
    private float _rotateSpeed = 3f;
    [SerializeField]
    private GameObject _explosionPrefab;
    [SerializeField]
    private AudioClip explosionClip;
    private SpawnManager _spawnManager;
    private bool _isDestroyed = false;

    private void Start()
    {
        _spawnManager = GameObject.Find("Spawn_Manager").GetComponent<SpawnManager>();
    }

    private void Update()
    {
        transform.Rotate(Vector3.forward * _rotateSpeed * Time.deltaTime);
    }

    private void Awake()
    {

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isDestroyed)
            return;

        if (other.CompareTag("Laser"))
        {
            _isDestroyed = true;

            Instantiate(_explosionPrefab, transform.position, Quaternion.identity);

            AudioSource.PlayClipAtPoint(explosionClip, Camera.main.transform.position, 1f);

            Destroy(other.gameObject);

            _spawnManager.StartSpawning();

            Destroy(gameObject);
        }
    }
}