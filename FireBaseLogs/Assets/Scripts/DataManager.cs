using UnityEngine;

public class DataManager : MonoBehaviour
{
    private int Score;

    public void SetScore(int score)
    {
        Score = score;
    }

    public int GetScore()
    {
        return Score;
    }

    public void AddScore(int score)
    {
        Score += score;
    }
}
