using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Boss : MonoBehaviour
{
    [SerializeField]
    private float _moveSpeed = 3f;

    [SerializeField]
    private GameObject _laserPrefab;

    [SerializeField]
    private Transform _player;

    [SerializeField]
    private int _bossHealth = 50;

    [SerializeField]
    private GameObject _explosionPrefab;

    [SerializeField]
    private UIManager _uiManager;

    [SerializeField]
    private Vector3 _targetPosition = new Vector3(0, 2.5f, 0);

    private bool _hasArrived;
    private bool _inPosition;
    private bool _attacking;

    void AimAtPlayerShot()
    {
        if (_player == null)
            return;

        Vector2 direction = (_player.position - transform.position).normalized;

        FireLaser(direction);
    }

    void Start()
    {
        // Find the Player
        _player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // Find the UIManager
        _uiManager = FindObjectOfType<UIManager>();

        if (_uiManager == null)
        {
            Debug.LogError("Boss could not find UIManager in the scene!");
            return;
        }

        // Initialize the boss health bar
        _uiManager.UpdateBossHealth(_bossHealth, 50);
    }

    void Update()
    {
        Debug.Log("Boss Position: " + transform.position);

        if (!_hasArrived)
        {
            transform.position = Vector3.MoveTowards(transform.position, _targetPosition, _moveSpeed * Time.deltaTime);


            if (Vector3.Distance(transform.position, _targetPosition) < 0.1f)
            {
                _hasArrived = true;

                Debug.Log("Boss Ready!");
            }
        }

        if (!_inPosition)
        {
            transform.position = Vector3.MoveTowards(transform.position, _targetPosition, _moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, _targetPosition) < 0.05f)
            {
                transform.position = _targetPosition;
                _inPosition = true;

                Debug.Log("Boss reached center!");

                StartCoroutine(AttackPattern());
            }
        }
    }

    IEnumerator AttackPattern()
    {
        while (true)
        {
            yield return new WaitForSeconds(2f);

            int attack = Random.Range(0, 3);

            Debug.Log("Boss attack: " + attack);

            switch (attack)
            {
                case 0:
                    StartCoroutine(LaserBurst());
                    break;

                case 1:
                    SpawnSpreadShot();
                    break;

                case 2:
                    AimAtPlayerShot();
                    break;
            }
        }
    }

    private void FireLaser(Vector2 direction)
    {
        GameObject laserObj = Instantiate(_laserPrefab, transform.position, Quaternion.identity);

        Laser laser = laserObj.GetComponent<Laser>();

        if (laser != null)
        {
            laser.Initialize(direction, Laser.LaserOwner.Enemy);
        }
    }

    public void TakeDamage(int damage)
    {
        _bossHealth -= damage;

        if (_bossHealth < 0)
        {
            _bossHealth = 0;
        }

        Debug.Log("Boss Health: " + _bossHealth);

        if (_uiManager != null)
        {
            _uiManager.UpdateBossHealth(_bossHealth, 50);
        }

        if (_bossHealth <= 0)
        {
            Die();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Boss was Hit");
        if (other.CompareTag("Laser"))
        {
            Debug.Log("Boss was Hit By a Laser");
            Laser laser = other.GetComponent<Laser>();

            // Only take damage from player lasers
            if (laser != null && laser.Owner == Laser.LaserOwner.Player)
            {
                Debug.Log("Boss was hit by Player Laser");
                TakeDamage(1);

                Destroy(other.gameObject);
            }
        }
    }

    private void Die()
    {
        Debug.Log("Boss Defeated!");

        if (_explosionPrefab != null)
        {
            Instantiate(_explosionPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    IEnumerator LaserBurst()
    {
        for (int i = 0; i < 5; i++)
        {
            FireLaser(Vector2.down);

            yield return new WaitForSeconds(0.2f);
        }
    }

    void SpawnSpreadShot()
    {
        for (int i = -2; i <= 2; i++)
        {
            Vector2 direction = Quaternion.Euler(0, 0, i * 15f) * Vector2.down;

            FireLaser(direction);
        }
    }
}