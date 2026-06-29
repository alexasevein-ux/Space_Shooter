using UnityEngine;

public class Powerup : MonoBehaviour
{
    [SerializeField] 
    private float _speed = 3f;
    [SerializeField] 
    private PowerUpType _powerUpType;
    [SerializeField] 
    private float _duration = 6f;
    [SerializeField] 
    private int _value = 1;

    void Update()
    {
        transform.Translate(Vector3.down * _speed * Time.deltaTime);

        if (transform.position.y < -6f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent(out Player player))
            return;

        ApplyPowerUp(player);
        Destroy(gameObject);
    }

    private void ApplyPowerUp(Player player)
    {
        switch (_powerUpType)
        {
            case PowerUpType.TripleShot:
                player.ApplyPowerUp(PowerUpType.TripleShot, _duration, _value);
                break;

            case PowerUpType.SpeedBoost:
                player.ApplyPowerUp(PowerUpType.SpeedBoost, _duration, _value);
                break;

            case PowerUpType.HomingShots:
                player.ApplyPowerUp(PowerUpType.HomingShots, _duration, _value);
                break;

            case PowerUpType.Shield:
                player.ShieldsActive();
                break;

            case PowerUpType.Health:
                player.Heal(_value);
                break;

            case PowerUpType.Ammo:
                player.RefillAmmo();
                break;
        }
    }
}