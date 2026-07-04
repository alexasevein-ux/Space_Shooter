using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    [SerializeField] 
    private float _speed = 10f;

    private Vector2 _direction;

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
        Debug.Log(gameObject.name + " direction = " + _direction);
    }

    void Update()
    {
        transform.Translate(_direction * _speed * Time.deltaTime);
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