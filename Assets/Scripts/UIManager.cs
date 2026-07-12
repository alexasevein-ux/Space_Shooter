using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Boss UI")]
    [SerializeField]
    private Slider _bossHealthSlider;

    [SerializeField]
    private Text _scoreText;
    [SerializeField]
    private Image _livesImg;
    [SerializeField]
    private Sprite[] _livesSprites;

    [SerializeField]
    private Text _gameOverText;
    [SerializeField]
    private Text _restartText;
    [SerializeField]
    private GameManager _gameManager;
    [SerializeField]
    private Slider _thrusterSlider;

    [SerializeField]
    private Slider _shieldSlider;
    [SerializeField]
    private Image _shieldImage;
    [SerializeField]
    private Sprite[] _shieldSprites;
    [SerializeField]
    private TMP_Text _ammoText;
    [SerializeField]
    private TMP_Text _waveText;


    public void UpdateWave(int wave)
    {
        if (_waveText != null)
        {
            _waveText.text = "Wave: " + wave;
        }
    }

    private void Start()
    {
        _gameManager = GameObject.Find("Game_Manager").GetComponent<GameManager>();

        _gameOverText.gameObject.SetActive(false);
        _restartText.gameObject.SetActive(false);

        if (_thrusterSlider == null)
            Debug.LogError("_thrusterSlider is NULL");

        if (_shieldSlider == null)
            Debug.LogError("_shieldSlider is NULL");

        if (_ammoText == null)
            Debug.LogError("_ammoText is NULL");

        Debug.Log("Ammo Text = " + _ammoText);

        if (_livesImg == null)
            Debug.LogError("_livesImg is NULL");

        if (_scoreText == null)
            Debug.LogError("_scoreText is NULL");
    }

    public void UpdateScore(int score)
    {
        _scoreText.text = "Score: " + score;
    }

    public void UpdateLives(int lives)
    {
        if (lives < 0 || lives >= _livesSprites.Length) return;

        _livesImg.sprite = _livesSprites[lives];

        if (lives == 0)
        {
            _gameManager.GameOver();
            _gameOverText.gameObject.SetActive(true);
            _restartText.gameObject.SetActive(true);
            StartCoroutine(GameOverFlickerRoutine());
        }
    }

    public void UpdateThrusterBar(float current, float maximum)
    {
        if (_thrusterSlider == null)
        {
            Debug.LogError("Thruster Slider is NOT assigned in UIManager!");
            return;
        }

        _thrusterSlider.value = current / maximum;
    }

    public void UpdateShieldBar(float current, float maximum)
    {
        _shieldSlider.value = current / maximum;
    }

    public void UpdateShield(int shieldHits)
    {
        if (shieldHits < 0 || shieldHits >= _shieldSprites.Length)
            return;

        _shieldImage.sprite = _shieldSprites[shieldHits];
    }

    public void UpdateAmmo(int current, int maximum)
    {
        _ammoText.text = current + " / " + maximum;
    }

    public void UpdateBossHealth(int currentHealth, int maxHealth)
    {
        if (_bossHealthSlider == null)
            return;

        _bossHealthSlider.maxValue = maxHealth;
        _bossHealthSlider.value = currentHealth;
    }

    private IEnumerator GameOverFlickerRoutine()
    {
        while (true)
        {
            _gameOverText.text = "GAME OVER";
            yield return new WaitForSeconds(0.5f);

            _gameOverText.text = "";
            yield return new WaitForSeconds(0.5f);
        }
    }
}