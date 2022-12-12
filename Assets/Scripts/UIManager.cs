using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager _system;
    public static UIManager UI
    {
        get
        {
            if (_system == null)
            {
                _system = GameObject.FindObjectOfType<UIManager>();

                if (_system == null)
                {
                    GameObject container = new GameObject("UIManager");
                    _system = container.AddComponent<UIManager>();
                }
            }

            return _system;
        }
    }

    public Text CurrentLevelText;
    public Text NextLevelText;
    public Image ProgressBar;

    //public Text coinText;

    void Start()
    {
        CurrentLevelText.text = GameSystem.Sytem.LEVEL.level.ToString();
        Debug.Log("current level is"+GameSystem.Sytem.LEVEL.level.ToString());
        NextLevelText.text = (GameSystem.Sytem.LEVEL.level + 1).ToString();
        Debug.Log("Next level is" + (GameSystem.Sytem.LEVEL.level + 1).ToString());
    }


    float debugProgress;
    public void UpdateProgress(int currentProgress, int totalPoints)
    {
        debugProgress = (currentProgress * 100) / totalPoints;
        ProgressBar.GetComponent<RectTransform>().sizeDelta = new Vector2(debugProgress, 100);
    }

}
