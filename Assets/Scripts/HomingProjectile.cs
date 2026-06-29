using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomingProjectile : MonoBehaviour
{
    [SerializeField] 
    private float _speed = 8f;
    [SerializeField] 
    private float _rotateSpeed = 200f;
    [SerializeField] 
    private float _seekRange = 10f;

    private Transform _target;

    void Start()
    {
        FindClosestTarget();
    }

    void Update()
    {
        if (_target == null)
        {
            FindClosestTarget();
            MoveForward();
            return;
        }

        Vector3 direction = (_target.position - transform.position).normalized;
        float rotateAmount = Vector3.Cross(direction, transform.up).z;

        transform.Rotate(0, 0, -rotateAmount * _rotateSpeed * Time.deltaTime);
        transform.position += transform.up * _speed * Time.deltaTime;
    }

    void FindClosestTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        float closestDistance = Mathf.Infinity;
        Transform closest = null;

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);

            if (distance < closestDistance && distance <= _seekRange)
            {
                closestDistance = distance;
                closest = enemy.transform;
            }
        }

        _target = closest;
    }

    void MoveForward()
    {
        transform.position += transform.up * _speed * Time.deltaTime;
    }

    public enum PowerUpType
    {
        None,
        TripleShot,
        SpeedBoost,
        HomingShots,
        Shield,
    }

}