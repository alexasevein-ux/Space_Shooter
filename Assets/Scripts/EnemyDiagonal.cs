using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDiagonal : MonoBehaviour
{
    [SerializeField] private float _speed = 3f;
    [SerializeField] private Vector2 _direction = new Vector2(-1, -1);

    void Update()
    {
        transform.Translate(_direction.normalized * _speed * Time.deltaTime);
    }
}
