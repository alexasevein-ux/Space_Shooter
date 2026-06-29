using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyLaser : MonoBehaviour
{
    [SerializeField] 
    private float _speed = 8f;

    private Vector2 _direction = Vector2.down;
    private Player _player;

    public void SetDirection(Vector2 direction)
    {
        _direction = direction;
    }

    private void Start()
    {
        _player = GameObject.Find("Player")?.GetComponent<Player>();
    }

    private void Update()
    {
        transform.Translate(_direction * _speed * Time.deltaTime);

        if (Mathf.Abs(transform.position.y) > 10f)
        {
            Destroy(gameObject);
        }
    }

    private bool IsBehindPlayer()
    {
        if (_player == null)
            return false;

        Vector2 toEnemy =
            (transform.position - _player.transform.position).normalized;

        bool abovePlayer =
            transform.position.y > _player.transform.position.y;

        bool alignedHorizontally =
            Mathf.Abs(transform.position.x - _player.transform.position.x) < 1.5f;

        return abovePlayer && alignedHorizontally;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("EnemyLaser"))
        {
            Destroy(other.gameObject);
            Destroy(gameObject);
        }
    }
}