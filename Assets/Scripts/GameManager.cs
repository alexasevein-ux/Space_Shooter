using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private bool _isGameOver;
    [SerializeField]
    private int _score = 0;
    [SerializeField]
    private UIManager _uiManager;

    void Start()
    {
        GameObject uiObject = GameObject.Find("Canvas");

        if (uiObject != null)
        {
            _uiManager = uiObject.GetComponent<UIManager>();
        }
    }

    public void AddScore(int points)
    {
        _score += points;
        
        if (_uiManager != null)
        {
            _uiManager.UpdateScore(_score);
        }
    }

    private void OnEnable()
    {
        Enemy.OnEnemyDestroyed += HandleEnemyDestroyed;
    }

    private void OnDisable()
    {
        Enemy.OnEnemyDestroyed -= HandleEnemyDestroyed;
    }

    private void HandleEnemyDestroyed(Vector3 position)
    {
        AddScore(10);
    }

    void Update()
    {
        if (_isGameOver && Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    public void GameOver()
    {
        _isGameOver = true;
    }
}