using TMPro;
using UnityEngine;

public class LeadrBoardEntry : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _name;
    [SerializeField] private TextMeshProUGUI _score;

    public void SetName(string name, int score)
    {
        _name.text = name;
        _score.text = score.ToString();
    }
}
