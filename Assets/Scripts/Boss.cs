using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : MonoBehaviour
{
    [SerializeField]
    private float _moveSpeed = 2f;
    [SerializeField]
    private GameObject _laserPrefab;

    private Vector3 _targetPosition = new Vector3(0, 2.5f, 0);
    private bool _inPosition;

    [SerializeField]
    private Transform _player;

    void AimAtPlayerShot()
    {
        if (_player == null)
            _player = GameObject.FindGameObjectWithTag("Player").transform;

        Vector3 direction = (_player.position - transform.position).normalized;
        Quaternion rotation = Quaternion.LookRotation(Vector3.forward, direction);

        Instantiate(_laserPrefab, transform.position, rotation);
    }

    void Start()
    {
        StartCoroutine(AttackPattern());
    }

    IEnumerator AttackPattern()
    {
        while (true)
        {
            yield return new WaitForSeconds(2f);

            int attack = Random.Range(0, 3);

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

    IEnumerator LaserBurst()
    {
        for (int i = 0; i < 5; i++)
        {
            Instantiate(_laserPrefab, transform.position, Quaternion.identity);
            yield return new WaitForSeconds(0.2f);
        }
    }

    void SpawnSpreadShot()
    {
        for (int i = -2; i <= 2; i++)
        {
            Quaternion rot = Quaternion.Euler(0, 0, i * 15f);
            Instantiate(_laserPrefab, transform.position, rot);
        }
    }

    void Update()
    {
        if (!_inPosition)
        {
            transform.position = Vector3.MoveTowards(transform.position, _targetPosition, _moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, _targetPosition) < 0.05f)
            {
                _inPosition = true;
            }
        }
    }
}