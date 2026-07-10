using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    [SerializeField]
    private float _speed = 10f;
    [SerializeField]
    private Vector2 _direction;
    [SerializeField]
    private bool _isHoming;
    [SerializeField]
    private Transform _target;

    public enum LaserOwner
    {
        Player,
        Enemy
    }

    public LaserOwner Owner { get; private set; }

    public void Initialize(Vector2 direction, LaserOwner owner)
    {
        _direction = direction.normalized;
        Owner = owner;
    }

    void Start()
    {
        Debug.Log($"{gameObject.name} belongs to {Owner.ToString()} direction = {_direction} and IsHoming = {_isHoming}", this);
    }

    void Update()
    {
        if (_isHoming && _target != null)
        {
            Vector2 direction = (_target.position - transform.position).normalized;
            transform.Translate(direction * _speed * Time.deltaTime, Space.World);
        }
        else
        {
            transform.Translate(_direction * _speed * Time.deltaTime, Space.World);
        }

        if (Mathf.Abs(transform.position.x) > 10f ||
            Mathf.Abs(transform.position.y) > 8f)
        {
            Destroy(gameObject);
        }
    }

    public void SetHoming()
    {
        _isHoming = true;

        Enemy[] enemies = FindObjectsOfType<Enemy>();

        float closestDistance = Mathf.Infinity;

        foreach (Enemy enemy in enemies)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                _target = enemy.transform;
            }
        }

        if (_target != null)
        {
            Debug.Log("Locked onto: " + _target.name);
        }
        else
        {
            Debug.Log("No enemies found for homing missile.");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<Laser>(out Laser otherLaser))
        {
            if (otherLaser.Owner != Owner)
            {
                Destroy(otherLaser.gameObject);
                Destroy(gameObject);
            }
            return;
        }

        if (Owner == LaserOwner.Player && other.CompareTag("Enemy"))
        {
            other.GetComponent<Enemy>()?.Damage();
            Destroy(gameObject);
            return;
        }

        if (Owner == LaserOwner.Enemy && other.CompareTag("Player"))
        {
            other.GetComponent<Player>()?.Damage();
            Destroy(gameObject);
        }

        if (Owner == LaserOwner.Enemy && other.CompareTag("PowerUp"))
        {
            Destroy(other.gameObject);
            Destroy(gameObject);
            return;
        }
    }
}