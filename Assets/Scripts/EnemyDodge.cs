using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDodge : MonoBehaviour
{
    [SerializeField] 
    private float _dodgeSpeed = 5f;
    [SerializeField] 
    private float _detectionRadius = 4f;

    private Transform _player;

    void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        DetectAndDodge();
    }

    void DetectAndDodge()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, _detectionRadius);

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Laser"))
            {
                Vector3 dodgeDirection = (transform.position - hit.transform.position).normalized;
                transform.position += dodgeDirection * _dodgeSpeed * Time.deltaTime;
            }
        }
    }
}