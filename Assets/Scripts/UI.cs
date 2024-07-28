using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UI : MonoBehaviour
{
    [SerializeField] private TMP_Text _highScoreText;
    [SerializeField] private TMP_Text _currentScoreText;
    [SerializeField] private GameObject _tapToStart;
    [SerializeField] private GameObject _gameOverUI;
    
    public void GameOver(int currentScore)
    {
        Time.timeScale = 0f;
        _highScoreText.text += Prefs.HighScore;
        _currentScoreText.text += currentScore;
        _gameOverUI.SetActive(true);
    }

    public void OnClickTapToStart()
    {
        _tapToStart.SetActive(false);
        Time.timeScale = 1f;
    }

    public void OnClickExit()
    {
        Application.Quit();
    }

}
