using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOver : MonoBehaviour
{
    //public GameObject gameOverMenu;
    GameObject _gameOverMenu;
    public GameObject GameOverMenu { get { return _gameOverMenu; } }

    void Awake()
    {
        _gameOverMenu = GameObject.Find("GameOverMenu"); 
    }

    public void EnableGameOverMenu()
    {
        if (_gameOverMenu != null)
        {
            _gameOverMenu.SetActive(true);
        }
    }

    public void DisableGameOverMenu()
    {
        if (_gameOverMenu != null)
        {
            _gameOverMenu.SetActive(false);
        }
    }
}
