using UnityEngine;
public class ScoreManager : MonoBehaviour
{
    [SerializeField] private UITextUpdater scoreUI;
    private int score;
    public void AddScore(int value)
    {
        score += value;
        scoreUI.Display(score.ToString());
    }
}