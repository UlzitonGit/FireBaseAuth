using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] DataManager dataManager;
    [SerializeField] UIManager uiManager;
    public void Click()
    {
        dataManager.AddScore(1);
        uiManager.ShowPlayerScore();
        //_authManager.UpdateScore();
    }
}
