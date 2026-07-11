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

    private bool _isRamming;

    [Header("Prefabs")]
    [SerializeField]
    private GameObject _laserPrefab;
    [SerializeField]
    private GameObject _enemyLaserPrefab;

    [Header("Effects")]
    [SerializeField]
    private GameObject _explosionPrefab;

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

        _canFire = Time.time + Random.Range(1f, 3f);
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
    }

    private void FireBackwardShot()
    {
        _canFire = Time.time + Random.Range(1f, 3f);

        Vector3 spawnPos = transform.position;

        GameObject laserObj = Instantiate(_laserPrefab, spawnPos, Quaternion.identity);

        Laser laser = laserObj.GetComponent<Laser>();

        if (laser != null)
        {
            laser.Initialize(Vector2.up, Laser.LaserOwner.Player);
        }
    }

    public void SetSpeed(float speed)
    {
        _speed = speed;
    }

    public void SetEnemyType(EnemyType type)
    {
        _enemyType = type;
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
        if (_enemyType != EnemyType.Shooter && _enemyType != EnemyType.HeatSeeker && _enemyType != EnemyType.ZigZag && _enemyType != EnemyType.Ambusher)
        {
            return;
        }

        if (Time.time < _canFire)
            return;

        FireLaser();
    }

    private void ShootAtTarget(Transform target)
    {
        _canFire = Time.time + Random.Range(1.5f, 3f);

        GameObject laser = Instantiate(_enemyLaserPrefab, transform.position, Quaternion.identity);

        Vector2 direction = Vector2.down;

        Laser l = laser.GetComponent<Laser>();

        if (l != null)
        {
            l.Initialize(direction, Laser.LaserOwner.Enemy);
        }
    }

    private void ShootPlayer()
    {
        _canFire = Time.time + Random.Range(1f, 4f);

        if (_player == null) return;

        Vector3 spawnPos = transform.position;

        GameObject laserObj = Instantiate(_laserPrefab, spawnPos, Quaternion.identity);

        Laser laser = laserObj.GetComponent<Laser>();

        if (laser != null)
        {
            Vector2 direction;

            if (_player != null && _player.IsMisfireActive())
            {
                float angle = Random.Range(-180f, 180f);
                direction = Quaternion.Euler(0, 0, angle) * Vector2.down;

                Debug.Log("Enemy misfired! Angle: " + angle);
            }
            else
            {
                direction = Vector2.down;
            }

            laser.Initialize(direction, Laser.LaserOwner.Enemy);
        }
    }

    // ---------------- MOVEMENT ----------------
    private void MoveEnemy()
    {
        switch (_enemyType)
        {
            case EnemyType.Basic:
                MoveStraight();
                break;

            case EnemyType.Shooter:
                MoveStraight();
                break;

            case EnemyType.ZigZag:
                MoveZigZag();
                break;

            case EnemyType.HeatSeeker:
                HeatSeekPlayer();
                break;

            case EnemyType.Rammer:
                MoveStraight();
                break;

            case EnemyType.Ambusher:
                AmbushPlayer();
                break;
        }
    }
    private void MoveStraight()
    {
        transform.Translate(Vector3.down * _speed * Time.deltaTime);
    }

    private void MoveZigZag()
    {
        transform.position += new Vector3(Mathf.Sin(Time.time * 5f) * 2f * Time.deltaTime, -_speed * Time.deltaTime, 0f);
    }

    // ---------------- HEAT SEEKER ----------------
    private void HeatSeekPlayer()
    {
        if (_player == null)
        {
            transform.Translate(Vector3.down * _speed * Time.deltaTime);
            return;
        }

        Vector2 direction = (_player.transform.position - transform.position).normalized;

        transform.Translate(direction * _speed * Time.deltaTime, Space.World);
    }

    // ---------------- AMBUSH ----------------
    private void AmbushPlayer()
    {
        if (_player == null)
        {
            transform.Translate(Vector3.down * _speed * Time.deltaTime);
            return;
        }

        float distance = Vector3.Distance(transform.position, _player.transform.position);

        if (distance < 4f)
        {
            Vector2 direction = (_player.transform.position - transform.position).normalized;

            transform.Translate(direction * (_speed * 2f) * Time.deltaTime, Space.World);
        }
       
        else
        {
            transform.Translate(Vector3.down * _speed * Time.deltaTime);
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

        // Misfire disables enemy weapons
        if (_player != null && _player.IsMisfireActive())
        {
            Debug.Log("Enemy weapon misfired!");
            return;
        }

        GameObject laser = Instantiate(_laserPrefab, transform.position, Quaternion.identity);

        Laser l = laser.GetComponent<Laser>();

        if (l != null)
        {
            l.Initialize(Vector2.down, Laser.LaserOwner.Enemy);
        }
    }

    // ---------------- BOUNDARY ----------------
    private void HandleBoundaryReset()
    {
        if (transform.position.y < -6f)
        {
            Destroy(gameObject);
        }
    }

    // ---------------- COLLISION ----------------
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isDead)
            return;

        Laser laser = other.GetComponent<Laser>();

        // Ignore enemy lasers completely
        if (laser != null && laser.Owner == Laser.LaserOwner.Enemy)
        {
            return;
        }

        // Only player lasers can damage enemies
        if (laser != null && laser.Owner == Laser.LaserOwner.Player)
        {
            Debug.Log("Enemy hit by player projectile");

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
            Player player = other.GetComponent<Player>();

            if (player != null)
            {
                player.DamageShield();
            }

            Death();
        }
    }

    public void Damage()
    {
        Death();
    }

    public void BombDamage()
    {
        Death();
    }

    private void Death()
    {
        if (_isDead)
            return;

        _isDead = true;
        _speed = 0;

        GetComponent<Collider2D>().enabled = false;

        if (_anim != null)
            _anim.SetTrigger("OnEnemyDeath");

        if (_explosionPrefab != null)
            Instantiate(_explosionPrefab, transform.position, Quaternion.identity);

        if (_audioSource != null)
            _audioSource.Play();

        OnEnemyDestroyed?.Invoke(transform.position);

        Destroy(gameObject, 2.5f);
    }
}