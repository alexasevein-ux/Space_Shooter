using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    [SerializeField] private float _radius = 2f;
    [SerializeField] private float _speed = 2f;

    private Vector3 _center;

    void Start()
    {
        _center = transform.position;
    }

    void Update()
    {
        float x = Mathf.Cos(Time.time * _speed) * _radius;
        float y = Mathf.Sin(Time.time * _speed) * _radius;

        transform.position = _center + new Vector3(x, y, 0);
    }
}
