using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NegativePowerUp : MonoBehaviour
{
    [SerializeField]
    private DebuffType _debuffType;

    [SerializeField]
    private float _duration = 5f;

    [SerializeField]
    private float _speed = 3f;

    private void Update()
    {
        transform.Translate(Vector3.down * _speed * Time.deltaTime);

        if (transform.position.y < -6f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Player player = other.GetComponent<Player>();

        if (player != null)
        {
            player.ApplyDebuff(_debuffType, _duration);
            Destroy(gameObject);
        }
    }
}