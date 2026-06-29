using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] 
    private float _speed = 3.5f;
    [SerializeField] 
    private float _speedMultiplier = 2f;
    
    private bool _isReversed;

    [Header("Thruster")]
    [SerializeField] 
    private float _maxThrusterCharge = 5f;
    [SerializeField] 
    private float _thrusterDrainRate = 1f;
    [SerializeField] 
    private float _thrusterRechargeRate = 1f;
    private float _currentThrusterCharge;

    [Header("Combat")]
    [SerializeField] 
    private GameObject _laserPrefab;
    [SerializeField] 
    private GameObject _tripleShotPrefab;
    [SerializeField] 
    private float _fireRate = 0.5f;
    private float _canFire = -1f;

    [SerializeField] 
    private int _lives = 3;
    [SerializeField] 
    private int _maxAmmo = 15;
    private int _currentAmmo;

    [SerializeField] 
    private GameObject _homingProjectilePrefab;

    [Header("Shield")]
    [SerializeField] 
    private GameObject _shieldVisualizer;
    [SerializeField] 
    private SpriteRenderer _shieldVisual;
    [SerializeField] 
    private int _shieldHits = 3;
    private int _currentShieldHits;
    private bool _isShieldsActive;

    [Header("Effects")]
    [SerializeField] 
    private GameObject _explosionPrefab;
    [SerializeField] 
    private GameObject _leftEngine;
    [SerializeField] 
    private GameObject _rightEngine;
    private CameraShake _cameraShake;

    [Header("Score")]
    [SerializeField] 
    private int _score;

    [Header("Systems")]
    private SpawnManager _spawnManager;
    private UIManager _uiManager;

    [Header("States")]
    private bool _isTripleShotActive;
    private bool _isSpeedBoostActive;
    private bool _isMagnetActive;
    private bool _isHit;
    private float _magnetTimer;
    [SerializeField] 
    private float _magnetDuration = 2f;

    // Properties
    public int CurrentLives => _lives;
    public int CurrentAmmo => _currentAmmo;
    public int MaxAmmo => _maxAmmo;

    private PowerUpType _currentPowerUp = PowerUpType.None;

    public void TripleShotActive()
    {
        _isTripleShotActive = true;
        StartCoroutine(TripleShotRoutine(5f));
    }

    public void RefillAmmo()
    {
        _currentAmmo = _maxAmmo;
        Debug.Log("Ammo Refilled!");

        if (_uiManager != null)
        {
            _uiManager.UpdateAmmo(_currentAmmo, _maxAmmo);
        }
    }

    public void Heal(int amount)
    {
        _lives += amount;

        // Prevent healing above max lives
        _lives = Mathf.Clamp(_lives, 0, 3);

        // Hide damaged engines if repaired
        if (_lives >= 2)
        {
            _rightEngine.SetActive(false);
        }

        if (_lives >= 3)
        {
            _leftEngine.SetActive(false);
        }

        _uiManager.UpdateLives(_lives);
    }

    public void ApplyDebuff(DebuffType type, float duration)
    {
        StartCoroutine(HandleDebuff(type, duration));
    }

    public void ApplyPowerUp(PowerUpType type, float duration, int value)
    {
        switch (type)
        {
            case PowerUpType.TripleShot:
                StartCoroutine(TripleShotRoutine(duration));
                break;

            case PowerUpType.SpeedBoost:
                StartCoroutine(SpeedBoostRoutine(duration));
                break;

            case PowerUpType.HomingShots:
                StartCoroutine(HomingRoutine(duration));
                break;

            case PowerUpType.Shield:
                ShieldsActive();
                break;

            case PowerUpType.Health:
                Heal(value);
                break;

            case PowerUpType.Ammo:
                RefillAmmo();
                break;
        }
    }

    private IEnumerator HandleDebuff(DebuffType type, float duration)
    {
        switch (type)
        {
            case DebuffType.Slow:
                float originalSpeed = _speed;
                _speed *= 0.5f;
                yield return new WaitForSeconds(duration);
                _speed = originalSpeed;
                break;

            case DebuffType.Freeze:
                float savedSpeed = _speed;
                _speed = 0;
                yield return new WaitForSeconds(duration);
                _speed = savedSpeed;
                break;

            case DebuffType.ReverseControls:
                _isReversed = true;
                yield return new WaitForSeconds(duration);
                _isReversed = false;
                break;
        }
    }

    #region UNITY LIFECYCLE

    void Start()
    {
        _score = 0;

        _currentThrusterCharge = _maxThrusterCharge;
        _currentAmmo = _maxAmmo;
        _currentShieldHits = _shieldHits;

        transform.position = Vector3.zero;

        _spawnManager = GameObject.Find("Spawn_Manager").GetComponent<SpawnManager>();
        _uiManager = GameObject.Find("Canvas").GetComponent<UIManager>();
        _cameraShake = Camera.main.GetComponent<CameraShake>();

        if (_uiManager != null)
        {
            _uiManager.UpdateScore(_score);
        }
    }

    void Update()
    {
        _isHit = false;

        Move();
        HandleShooting();
        HandleMagnet();
    }

    #endregion

    #region MOVEMENT

    void Move()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        if (_isReversed)
        {
            horizontal *= -1;
            vertical *= -1;
        }

        bool usingThrusters = Input.GetKey(KeyCode.LeftShift) && _currentThrusterCharge > 0;

        float speed = _speed;

        if (usingThrusters)
        {
            speed *= _speedMultiplier;
            _currentThrusterCharge -= _thrusterDrainRate * Time.deltaTime;
        }
        else
        {
            _currentThrusterCharge += _thrusterRechargeRate * Time.deltaTime;
        }

        _currentThrusterCharge = Mathf.Clamp(_currentThrusterCharge, 0, _maxThrusterCharge);

        Vector3 direction = new Vector3(horizontal, vertical, 0);
        transform.Translate(direction * speed * Time.deltaTime);

        // Clamp + Wrap
        transform.position = new Vector3(Mathf.Clamp(transform.position.x, -11.3f, 11.3f), Mathf.Clamp(transform.position.y, -3.8f, 0f), 0);

        if (transform.position.x > 11.3f)
            transform.position = new Vector3(-11.3f, transform.position.y, 0);
        else if (transform.position.x < -11.3f)
            transform.position = new Vector3(11.3f, transform.position.y, 0);

        _uiManager.UpdateThrusterBar(_currentThrusterCharge, _maxThrusterCharge);
    }

    #endregion

    #region SHOOTING

    void FireLaser()
    {
        GameObject prefabToSpawn = _laserPrefab;

        if (_isTripleShotActive)
            prefabToSpawn = _tripleShotPrefab;

        if (_isHomingActive)
            prefabToSpawn = _homingProjectilePrefab;

        Instantiate(prefabToSpawn, transform.position, Quaternion.identity);

        _currentAmmo--;
        _uiManager.UpdateAmmo(_currentAmmo, _maxAmmo);
    }

    void HandleShooting()
    {
        if (Input.GetKeyDown(KeyCode.Space) && Time.time > _canFire && _currentAmmo > 0)
        {
            FireLaser();
        }
    }

    #endregion

    #region MAGNET

    void HandleMagnet()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            _isMagnetActive = true;
            _magnetTimer = _magnetDuration;
        }

        if (_isMagnetActive)
        {
            _magnetTimer -= Time.deltaTime;

            if (_magnetTimer <= 0)
                _isMagnetActive = false;
        }
    }

    public bool IsMagnetActive() => _isMagnetActive;

    public float GetMagnetSpeed() => 10f;

    #endregion

    #region DAMAGE SYSTEM

    public void Damage()
    {
        if (_isHit) return;

        _isHit = true;
        _cameraShake.Shake(0.15f, 0.05f);

        if (_isShieldsActive)
        {
            _isShieldsActive = false;
            _shieldVisualizer.SetActive(false);
            return;
        }

        _lives--;

        if (_lives == 2) _leftEngine.SetActive(true);
        if (_lives == 1) _rightEngine.SetActive(true);

        _uiManager.UpdateLives(_lives);

        if (_lives <= 0)
        {
            Instantiate(_explosionPrefab, transform.position, Quaternion.identity);
            _spawnManager.OnPlayerDeath();
            Destroy(gameObject);
        }
    }

    public void TakeDamage()
    {
        if (_currentShieldHits > 0)
        {
            _currentShieldHits--;

            UpdateShieldVisual();

            if (_uiManager != null)
            {
                _uiManager.UpdateShield(_currentShieldHits);
            }

            if (_currentShieldHits == 0)
            {
                _isShieldsActive = false;
                _shieldVisualizer.SetActive(false);
            }

            return;
        }

        Damage();
    }

    public void ShieldsActive()
    {
        _isShieldsActive = true;

        if (_shieldVisualizer != null)
            _shieldVisualizer.SetActive(true);

        _currentShieldHits = _shieldHits;

        UpdateShieldVisual();

        if (_uiManager != null)
            _uiManager.UpdateShield(_currentShieldHits);
    }

    void UpdateShieldVisual()
    {
        if (_shieldVisual == null) return;

        switch (_currentShieldHits)
        {
            case 3:
                _shieldVisual.color = Color.cyan;
                break;

            case 2:
                _shieldVisual.color = Color.yellow;
                break;

            case 1:
                _shieldVisual.color = new Color(1f, 0.5f, 0f);
                break;

            default:
                _shieldVisual.color = Color.clear;
                break;

        }
            _uiManager.UpdateShield(_currentShieldHits);
    }

    #endregion

    #region POWERUPS

    private IEnumerator SpeedBoostRoutine(float duration)
    {
        if (_isSpeedBoostActive) yield break;

        _isSpeedBoostActive = true;

        float originalSpeed = _speed;
        _speed *= _speedMultiplier;

        yield return new WaitForSeconds(duration);

        _speed = originalSpeed;
        _isSpeedBoostActive = false;
    }

    private IEnumerator TripleShotRoutine(float duration)
    {
        _isTripleShotActive = true;

        yield return new WaitForSeconds(duration);

        _isTripleShotActive = false;
    }

    private bool _isHomingActive;

    private IEnumerator HomingRoutine(float duration)
    {
        _isHomingActive = true;

        yield return new WaitForSeconds(duration);

        _isHomingActive = false;
    }

    #endregion

    #region SCORE

    public void AddScore(int points)
    {
        _score += points;
        _uiManager.UpdateScore(_score);
    }

    #endregion
}