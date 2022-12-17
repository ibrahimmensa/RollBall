using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;
using UnityEngine.UI;

public class GameSystem : MonoBehaviour
{
    static GameSystem _system;
    public static GameSystem Sytem
    {
        get
        {
            if (_system == null)
            {
                _system = GameObject.FindObjectOfType<GameSystem>();

                if (_system == null)
                {
                    GameObject container = new GameObject("GameSystem");
                    _system = container.AddComponent<GameSystem>();
                }
            }

            return _system;
        }
    }

    public float timeBeforeBallDisable;
    public float spawnRate;

    public Level LEVEL;
    public Player PLAYER;
    public GameCamera maincam;
    public Animator HoldBtn;
    public GameObject RunningBall;
    public AudioSource CollisionSound;
    public GameObject Timmer;

    public void Awake()
    {

        PLAYER = new Player();
    }
    private void OnEnable()
    {

        if (GoogleAdsManager.Instance)
        {
            Debug.Log("Request Banner");
            GoogleAdsManager.Instance.RequestBanner();
        }
    }
    private void Start()
    {
      // PlayerPrefs.SetInt("Level", 12);
        if (PlayerPrefs.HasKey("Level"))
        {
            if (PlayerPrefs.GetInt("Level") > 15)
            {
                PlayerPrefs.SetInt("Level", 0);
            }
            PlayerPrefs.GetInt("Level");
            LEVEL.LevelManager.transform.GetChild(PlayerPrefs.GetInt("Level")).gameObject.SetActive(true);
        }
        else
        {
            PlayerPrefs.SetInt("Level", 0);
            LEVEL.LevelManager.transform.GetChild(PlayerPrefs.GetInt("Level")).gameObject.SetActive(true);
        }

        //check vibration settings
        if (!PlayerPrefs.HasKey("Vibrate"))
        {
            PlayerPrefs.SetInt("Vibrate", 1);
        }


        //check Sound settings
        if (PlayerPrefs.HasKey("Sound"))
        {
            if (PlayerPrefs.GetInt("Sound") == 1)
            {
                LEVEL.Sound.SetActive(true);
            }
            else
            {
                LEVEL.Sound.SetActive(false);
            }
        }
        else
        {
            PlayerPrefs.SetInt("Sound", 1);
        }
        maincam.target = LEVEL.LevelManager.transform.GetChild(PlayerPrefs.GetInt("Level")).transform.GetChild(2);
        RunningBall = maincam.target.transform.GetChild(1).gameObject;
        LEVEL.Cointxt.text = PlayerPrefs.GetInt("Coins").ToString();
        LEVEL.level = PlayerPrefs.GetInt("Level") + 1;
        LEVEL.Leveltxt.text = "LEVEL " + "- " + LEVEL.level;
        LEVEL.GAME_STATE = GameState.GAME;
    }
    
    public void SetGameState(string state)
    {
        LEVEL.GAME_STATE = (GameState)Enum.Parse(typeof(GameState), state);
    }

    public void levelComplete()
    {
        //AdsInitializer.Instance.ShowAdInterstitial();
        //GoogleAds.Instance.showInterstitial();

        StartCoroutine(LevelCompleted());
    }
    public void levelFail()
    {
        //AdsInitializer.Instance.ShowAdInterstitial();

        StartCoroutine(LevelFail());
    }
    
    public void Restart()
    {
        LEVEL.LevelResart();
    }
    public void NextLevel()
    {
        LEVEL.NextLevel();
    }
    public void Home()
    {
        if(GoogleAdsManager.Instance)
        {
            GoogleAdsManager.Instance.showInterstitial();
        }
        LEVEL.Home();
    }
    public void resume()
    {
        RunningBall.GetComponent<MeshRenderer>().enabled = true;
        RunningBall.GetComponent<Ball>().canMove = true;
        LEVEL.GAME_STATE = GameState.GAME;
        LEVEL.P_LevelFail.SetActive(false);
    }
    public int totl;
    IEnumerator LevelCompleted()
    {
        LEVEL.LevelComplete_Cointxt.text = PlayerPrefs.GetInt("Coins").ToString();
        LEVEL.P_BlackScreen.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        if (GoogleAdsManager.Instance)
        {
            GoogleAdsManager.Instance.showInterstitial();
        }
        LEVEL.P_Levelcomplete.SetActive(true);
        LEVEL.P_BlackScreen.SetActive(false);
        totl = 20 + 5 * (PlayerPrefs.GetInt("Level") + 1);
        LEVEL.dubbleScore.text = totl.ToString();
        if (PlayerPrefs.GetInt("Level") == 0 && PlayerPrefs.GetInt("Coins") == 0)
        {
            PlayerPrefs.SetInt("Coins", totl);
        }
        else
        {
            PlayerPrefs.SetInt("Coins", PlayerPrefs.GetInt("Coins") + totl);
        }
        LEVEL.P_Levelcomplete.transform.Find("Coins").transform.GetChild(0).GetComponent<Text>().text = PlayerPrefs.GetInt("Coins").ToString();
        Debug.Log("score is:" + PlayerPrefs.GetInt("Coins"));
        yield return null;
        StopCoroutine(LevelCompleted());
    }

    IEnumerator LevelFail()
    {
        LEVEL.LevelFailed_Cointxt.text = PlayerPrefs.GetInt("Coins").ToString();
        LEVEL.P_LevelFail.transform.Find("Coins").transform.GetChild(0).GetComponent<Text>().text = PlayerPrefs.GetInt("Coins").ToString();
        LEVEL.P_BlackScreen.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        if (GoogleAdsManager.Instance)
        {
            GoogleAdsManager.Instance.showInterstitial();
        }
        LEVEL.P_LevelFail.SetActive(true);
        LEVEL.P_BlackScreen.SetActive(false);
        yield return null;
        StopCoroutine(LevelFail());
    }

    public void Ads_DubbleReeard()
    {
        if (AdsInitializer.Instance)
            AdsInitializer.Instance.ShowAd(RewardedAdType.DOUBLEREWARD);
    }
    public void Ads_ResumeGame()
    {
        if (AdsInitializer.Instance)
            AdsInitializer.Instance.ShowAd(RewardedAdType.RESUME);
    }
}

[System.Serializable]
public class Level
{
    public event Action OnLevelFinished;
    public event Action OnLevelFailed;
    public event Action OnLevelResart;

    public int CurrentLevel;

    public GameState GAME_STATE;

    [Header("My Variables")]


    public GameObject P_Levelcomplete;
    public GameObject P_LevelFail;
    public GameObject P_BlackScreen;

    //coins
    public Text Cointxt;
    public Text LevelComplete_Cointxt;
    public Text LevelFailed_Cointxt;

    //sound and music
    public GameObject Sound;
    public AudioSource S_Complete;
    public AudioSource S_Failed;


    //levels
    public int level;
    bool levelCom;
    public Text Leveltxt;
    public Text dubbleScore;
    public GameObject LevelManager;
    public Button DubbleReward;

    public void Init()
    {
        GAME_STATE = GameState.IDLE;
        

        SceneManager.LoadScene(2);
        //if (SceneManager.GetActiveScene().buildIndex + 1 != CurrentLevel)
        //    SceneManager.LoadScene("Level_" + CurrentLevel, LoadSceneMode.Single);

    }

    public void LevelFinished()
    {
        GAME_STATE = GameState.COMPLETED;
        levelCom = true;
        if (PlayerPrefs.GetInt("Sound") == 1)
        {
            S_Complete.Play();
        }
        GameSystem.Sytem.levelComplete();
        if (OnLevelFinished != null)
            OnLevelFinished();
    }

    public void LevelFailed()
    {
        GAME_STATE = GameState.LOSE;
        if (PlayerPrefs.GetInt("Sound") == 1)
        {
            S_Failed.Play();
        }
        GameSystem.Sytem.levelFail();
        if (OnLevelFailed != null)
            OnLevelFailed();
    }

    public void LevelResart()
    {
        GAME_STATE = GameState.IDLE;

        Time.timeScale = 1;
        SceneManager.LoadScene(2);

        if (OnLevelResart != null)
            OnLevelResart();
    }

    int rand_level;

    public void NextLevel()
    {
        if (level == 15)
        {
            PlayerPrefs.SetInt("Level", -1);
        }
        PlayerPrefs.SetInt("Level", PlayerPrefs.GetInt("Level") + 1);
        SceneManager.LoadScene(2);

        GAME_STATE = GameState.GAME;
    }

    public void Home()
    {
        //AdsInitializer.Instance.ShowAdInterstitial();
        if (levelCom)
        {
            Debug.Log("LevelComplete Home");

            if (level == 15)
            {
                PlayerPrefs.SetInt("Level", -1);
            }
            PlayerPrefs.SetInt("Level", PlayerPrefs.GetInt("Level") + 1);
            levelCom = false;
            SceneManager.LoadScene(1);
        }
        else
        {
            Time.timeScale = 1;
            Debug.Log("Pause Home");
            SceneManager.LoadScene(1);
        }
    }
    
}

public class Player
{
    int _coin;
    public int coins
    {
        set
        {
            _coin = value;

            PlayerPrefs.SetInt("coins", value);
        }

        get
        {
            return _coin;
        }
    }

    internal void Init()
    {
        //ResetData();
        //PlayerPrefs.SetInt("lvl", 25);
        coins = PlayerPrefs.GetInt("coins");
    }

    void ResetData()
    {
        PlayerPrefs.SetInt("coins", 0);
        PlayerPrefs.SetInt("lvl", 1);
    }
}

public enum GameState
{
    IDLE,
    GAME,
    LOSE,
    COMPLETED
}
