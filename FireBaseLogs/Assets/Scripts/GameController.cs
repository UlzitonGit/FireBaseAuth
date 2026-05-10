using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] private AuthManager _authManager;
    [SerializeField] DataManager dataManager;
    [SerializeField] UIManager uiManager;
    public void Click()
    {
        dataManager.AddScore(1);
        uiManager.ShowPlayerScore();
        _authManager.UpdateScore();
    }
}
