using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject loginScreen;
    [SerializeField] private GameObject gameScreen;
    [SerializeField] private GameObject leaderboardScreen;
    [SerializeField] private TextMeshProUGUI playerScore;
    [SerializeField] private DataManager dataManager;
    public void ShowPlayerScore()
    {
        playerScore.text = dataManager.GetScore().ToString() + " your score";
    }

    public void GameScreen()
    {
        gameScreen.gameObject.SetActive(true);
        loginScreen.gameObject.SetActive(false);
        leaderboardScreen.gameObject.SetActive(false);
    }

    public void LeaderboardScreen()
    {
        //authManager.UpdateLeaderboard();
        leaderboardScreen.gameObject.SetActive(true);
        gameScreen.gameObject.SetActive(false);
    }
}
