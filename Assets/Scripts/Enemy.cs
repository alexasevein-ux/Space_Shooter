using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField]
    private float _speed = 3f;
    [SerializeField]
    private float _detectionRange = 6f;

    [Header("Ramming")]
    [SerializeField]
    private float _ramSpeed = 8f;
    [SerializeField]
    private float _chargeDelay = 1f;

    private bool _isRamming;
    private bool _isPreparingCharge;

    [Header("Prefabs")]
    [SerializeField]
    private GameObject _laserPrefab;
    [SerializeField]
    private GameObject _enemyLaserPrefab;

    [Header("Shooting")]
    [SerializeField]
    private float _fireRate = 1.5f;

    [Header("Enemy Type")]
    [SerializeField]
    private EnemyType _enemyType;
    [SerializeField]
    private MovementPattern _movementPattern;

    [Header("Shield")]
    [SerializeField]
    private bool _hasShield = false;
    [SerializeField]
    private int _shieldHits = 1;

    private Player _player;
    private Animator _anim;
    private AudioSource _audioSource;

    private float _canFire;
    private float _nextFireTime;
    private bool _isDead;

    public enum EnemyType
    {
        Basic,
        Shooter,
        ZigZag,
        HeatSeeker,
        Rammer,
        Ambusher,
    }

    public enum MovementPattern
    {
        Straight,
        ZigZag,
        SineWave,
    }

    public static System.Action<Vector3> OnEnemyDestroyed;

    void Start()
    {
        _player = GameObject.Find("Player")?.GetComponent<Player>();
        _anim = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (_isDead) return;

        HandleRamming();

        if (_isRamming)
            RamPlayer();
        else
            MoveEnemy();

        HandleShooting();
        HandleAmbusherAttack();

        HandleBoundaryReset();
    }

    private bool IsBehindPlayer()
    {
        if (_player == null) return false;

        float distance = Vector3.Distance(transform.position, _player.transform.position);

        return transform.position.y > _player.transform.position.y && distance < 5f;
    }

    private void HandleAmbusherAttack()
    {
        if (_enemyType != EnemyType.Ambusher) return;

        if (!IsBehindPlayer()) return;

        if (Time.time > _canFire)
        {
            FireBackwardShot();
        }
    }

    private void HandleBackwardShooting()
    {
        if (_enemyType != EnemyType.Shooter) return;

        if (IsBehindPlayer())
        {
            FireBackward();
        }
    }

    private void FireBackward()
    {
        GameObject laser = Instantiate(_laserPrefab, transform.position, Quaternion.identity);

        foreach (Laser l in laser.GetComponentsInChildren<Laser>())
        {
            l.AssignEnemyLaser();
            l.SetDirection(Vector2.down); // or opposite of player direction
        }
    }

    private void FireBackwardShot()
    {
        _canFire = Time.time + Random.Range(1f, 3f);

        GameObject projectile = Instantiate(_enemyLaserPrefab, transform.position, Quaternion.identity);

        Vector2 direction = (_player.transform.position - transform.position).normalized;

        EnemyLaser laser = projectile.GetComponent<EnemyLaser>();

        if (laser != null)
        {
            laser.SetDirection(direction);
        }
    }

    public void SetSpeed(float speed)
    {
        _speed = speed;
    }

    private Transform FindNearestPickup()
    {
        GameObject[] pickups = GameObject.FindGameObjectsWithTag("PowerUp");

        Transform closest = null;
        float closestDistance = Mathf.Infinity;

        foreach (GameObject p in pickups)
        {
            float distance = Vector3.Distance(transform.position, p.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = p.transform;
            }
        }

        return closest;
    }

    private void HandleShooting()
    {
        if (_enemyType != EnemyType.Shooter) return;

        if (Time.time < _canFire) return;

        Transform pickupTarget = FindNearestPickup();

        if (pickupTarget != null)
        {
            ShootAtTarget(pickupTarget);
        }
        else
        {
            ShootPlayer();
        }
    }

    private void ShootAtTarget(Transform target)
    {
        _canFire = Time.time + Random.Range(1.5f, 3f);

        GameObject laser = Instantiate(_enemyLaserPrefab, transform.position, Quaternion.identity);

        Vector2 direction = (target.position - transform.position).normalized;

        EnemyLaser l = laser.GetComponent<EnemyLaser>();

        if (l != null)
        {
            l.SetDirection(direction);
        }
    }

    private void ShootPlayer()
    {
        _canFire = Time.time + Random.Range(2f, 5f);

        if (_player == null) return;

        GameObject laser = Instantiate(_enemyLaserPrefab, transform.position, Quaternion.identity);

        Vector2 direction = (_player.transform.position - transform.position).normalized;

        EnemyLaser l = laser.GetComponent<EnemyLaser>();

        if (l != null)
        {
            l.SetDirection(direction);
        }
    }

    // ---------------- MOVEMENT ----------------
    private void MoveEnemy()
    {
        switch (_movementPattern)
        {
            case MovementPattern.Straight:
                transform.Translate(Vector3.down * _speed * Time.deltaTime);
                break;

            case MovementPattern.ZigZag:
                float zig = Mathf.Sin(Time.time * 5f) * 2f;
                transform.Translate(new Vector3(zig, -1, 0) * _speed * Time.deltaTime);
                break;

            case MovementPattern.SineWave:
                float wave = Mathf.Sin(Time.time * 3f) * 2f;
                transform.position += new Vector3(wave * Time.deltaTime, -_speed * Time.deltaTime, 0);
                break;
        }
    }

    // ---------------- RAMMING ----------------
    private void HandleRamming()
    {
        if (_enemyType != EnemyType.Rammer || _player == null)
            return;

        float distance = Vector3.Distance(transform.position, _player.transform.position);

        if (distance <= _detectionRange)
            _isRamming = true;
    }

    private void RamPlayer()
    {
        Vector3 dir = (_player.transform.position - transform.position).normalized;
        transform.position += dir * _ramSpeed * Time.deltaTime;
    }

    // ---------------- SHOOTING ----------------
    private void FireLaser()
    {
        _canFire = Time.time + Random.Range(2f, 5f);

        GameObject laser = Instantiate(_laserPrefab, transform.position, Quaternion.identity);

        foreach (Laser l in laser.GetComponentsInChildren<Laser>())
            l.AssignEnemyLaser();
    }

    // ---------------- BOUNDARY ----------------
    private void HandleBoundaryReset()
    {
        if (transform.position.y < -5f)
        {
            transform.position = new Vector3(Random.Range(-8f, 8f), 7f, 0);
        }
    }

    // ---------------- COLLISION ----------------
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isDead)
            return;

        if (other.CompareTag("PowerUp"))
        {
            Destroy(other.gameObject);
            return;
        }

        if (other.CompareTag("Laser"))
        {
            Destroy(other.gameObject);

            if (_hasShield && _shieldHits > 0)
            {
                _shieldHits--;

                if (_shieldHits > 0)
                    return;
            }

            Death();
            return;
        }

        if (other.CompareTag("Player"))
        {
            other.GetComponent<Player>()?.Damage();
            Death();
        }
    }

    private void Death()
    {
        _isDead = true;
        _speed = 0;

        _anim?.SetTrigger("OnEnemyDeath");
        _audioSource?.Play();

        GetComponent<Collider2D>().enabled = false;

        Destroy(gameObject, 2.5f);
    }
}